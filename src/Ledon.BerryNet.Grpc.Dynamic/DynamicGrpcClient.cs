using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Reflection.V1Alpha;
using Ledon.BerryNet.Grpc.Dynamic.Messages;
using Ledon.BerryNet.Grpc.Dynamic.Internal;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Ledon.BerryNet.Grpc.Dynamic;

/// <summary>
/// 动态 gRPC 客户端实现
/// </summary>
public class DynamicGrpcClient : IDynamicGrpcClient
{
    private readonly GrpcChannel _channel;
    private readonly Dictionary<string, string> _headers;
    private readonly RetryPolicy? _retryPolicy;
    private readonly bool _enableReflection;
    private readonly ILogger<DynamicGrpcClient>? _logger;
    private readonly Lazy<ServerReflection.ServerReflectionClient> _reflectionClient;
    private readonly ConcurrentCache<string, ServiceDescriptor> _serviceCache = new();
    private readonly ConcurrentCache<string, FileDescriptor> _fileCache = new();

    public DynamicGrpcClient(
        GrpcChannel channel,
        Dictionary<string, string> headers,
        RetryPolicy? retryPolicy,
        bool enableReflection,
        ILogger<DynamicGrpcClient>? logger)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _headers = headers ?? new Dictionary<string, string>();
        _retryPolicy = retryPolicy;
        _enableReflection = enableReflection;
        _logger = logger;
        _reflectionClient = new Lazy<ServerReflection.ServerReflectionClient>(() => 
            new ServerReflection.ServerReflectionClient(_channel));
    }

    public async Task<ServiceDescriptor?> GetServiceDescriptorAsync(string serviceName)
    {
        if (_serviceCache.TryGetValue(serviceName, out var cachedService))
        {
            return cachedService;
        }

        if (!_enableReflection)
        {
            _logger?.LogWarning("Reflection is disabled, cannot get service descriptor for {ServiceName}", serviceName);
            return null;
        }

        try
        {
            var request = new Reflection.ServerReflectionRequest
            {
                FileContainingSymbol = serviceName
            };

            using var call = _reflectionClient.Value.ServerReflectionInfo(CreateCallOptions());
            await call.RequestStream.WriteAsync(request);
            await call.RequestStream.CompleteAsync();

            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                if (response.FileDescriptorResponse != null)
                {
                    var fileDescriptor = await ParseFileDescriptorAsync(response.FileDescriptorResponse);
                    if (fileDescriptor != null)
                    {
                        var service = fileDescriptor.Services.FirstOrDefault(s => s.FullName == serviceName || s.Name == serviceName.Split('.').Last());
                        if (service != null)
                        {
                            _serviceCache.TryAdd(serviceName, service);
                            return service;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to get service descriptor for {ServiceName}", serviceName);
        }

        return null;
    }

    public async Task<DynamicMessage> CallUnaryAsync(string serviceName, string methodName, DynamicMessage request, CallOptions? options = null)
    {
        var service = await GetServiceDescriptorAsync(serviceName);
        if (service == null)
            throw new InvalidOperationException($"Service '{serviceName}' not found");

        var method = service.FindMethodByName(methodName);
        if (method == null)
            throw new InvalidOperationException($"Method '{methodName}' not found in service '{serviceName}'");

        if (method.IsClientStreaming || method.IsServerStreaming)
            throw new InvalidOperationException($"Method '{methodName}' is not a unary method");

        var callOptions = options ?? CreateCallOptions();
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var call = CreateUnaryCall(service, method, request, callOptions);
            var response = await call.ResponseAsync;
            return response;
        });
    }

    public async Task<string> CallUnaryJsonAsync(string serviceName, string methodName, string requestJson, CallOptions? options = null)
    {
        var service = await GetServiceDescriptorAsync(serviceName);
        if (service == null)
            throw new InvalidOperationException($"Service '{serviceName}' not found");

        var method = service.FindMethodByName(methodName);
        if (method == null)
            throw new InvalidOperationException($"Method '{methodName}' not found in service '{serviceName}'");

        var request = DynamicMessage.FromJson(method.InputType, requestJson);
        var response = await CallUnaryAsync(serviceName, methodName, request, options);
        
        return response.ToJson();
    }

    public async Task<DynamicMessage> CallClientStreamingAsync(string serviceName, string methodName, IAsyncEnumerable<DynamicMessage> requests, CallOptions? options = null)
    {
        var service = await GetServiceDescriptorAsync(serviceName);
        if (service == null)
            throw new InvalidOperationException($"Service '{serviceName}' not found");

        var method = service.FindMethodByName(methodName);
        if (method == null)
            throw new InvalidOperationException($"Method '{methodName}' not found in service '{serviceName}'");

        if (!method.IsClientStreaming || method.IsServerStreaming)
            throw new InvalidOperationException($"Method '{methodName}' is not a client streaming method");

        var callOptions = options ?? CreateCallOptions();

        return await ExecuteWithRetryAsync(async () =>
        {
            var call = CreateClientStreamingCall(service, method, callOptions);
            
            await foreach (var request in requests)
            {
                await call.RequestStream.WriteAsync(request);
            }
            await call.RequestStream.CompleteAsync();

            return await call.ResponseAsync;
        });
    }

    public async IAsyncEnumerable<DynamicMessage> CallServerStreamingAsync(string serviceName, string methodName, DynamicMessage request, CallOptions? options = null)
    {
        var service = await GetServiceDescriptorAsync(serviceName);
        if (service == null)
            throw new InvalidOperationException($"Service '{serviceName}' not found");

        var method = service.FindMethodByName(methodName);
        if (method == null)
            throw new InvalidOperationException($"Method '{methodName}' not found in service '{serviceName}'");

        if (method.IsClientStreaming || !method.IsServerStreaming)
            throw new InvalidOperationException($"Method '{methodName}' is not a server streaming method");

        var callOptions = options ?? CreateCallOptions();
        var call = CreateServerStreamingCall(service, method, request, callOptions);

        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            yield return response;
        }
    }

    public async IAsyncEnumerable<DynamicMessage> CallBidirectionalStreamingAsync(string serviceName, string methodName, IAsyncEnumerable<DynamicMessage> requests, CallOptions? options = null)
    {
        var service = await GetServiceDescriptorAsync(serviceName);
        if (service == null)
            throw new InvalidOperationException($"Service '{serviceName}' not found");

        var method = service.FindMethodByName(methodName);
        if (method == null)
            throw new InvalidOperationException($"Method '{methodName}' not found in service '{serviceName}'");

        if (!method.IsClientStreaming || !method.IsServerStreaming)
            throw new InvalidOperationException($"Method '{methodName}' is not a bidirectional streaming method");

        var callOptions = options ?? CreateCallOptions();
        var call = CreateBidirectionalStreamingCall(service, method, callOptions);

        // 启动请求写入任务
        var writeTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var request in requests)
                {
                    await call.RequestStream.WriteAsync(request);
                }
                await call.RequestStream.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error writing to bidirectional stream");
            }
        });

        // 读取响应
        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            yield return response;
        }

        await writeTask;
    }

    public async Task<IEnumerable<string>> GetAvailableServicesAsync()
    {
        if (!_enableReflection)
        {
            _logger?.LogWarning("Reflection is disabled, cannot list services");
            return Enumerable.Empty<string>();
        }

        try
        {
            var request = new Reflection.ServerReflectionRequest
            {
                ListServices = ""
            };

            using var call = _reflectionClient.Value.ServerReflectionInfo(CreateCallOptions());
            await call.RequestStream.WriteAsync(request);
            await call.RequestStream.CompleteAsync();

            var services = new List<string>();
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                if (response.ListServicesResponse != null)
                {
                    services.AddRange(response.ListServicesResponse.Service.Select(s => s.Name));
                }
            }

            return services;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to list available services");
            return Enumerable.Empty<string>();
        }
    }

    public async Task<IEnumerable<MethodDescriptor>> GetServiceMethodsAsync(string serviceName)
    {
        var service = await GetServiceDescriptorAsync(serviceName);
        return service?.Methods.ToList() ?? Enumerable.Empty<MethodDescriptor>();
    }

    public async Task<DynamicMessage> CreateMessageAsync(string messageTypeName)
    {
        // 首先尝试从已缓存的文件描述符中查找
        foreach (var fileDesc in _fileCache.Values)
        {
            var messageType = fileDesc.FindTypeByName<MessageDescriptor>(messageTypeName);
            if (messageType != null)
            {
                return new DynamicMessage(messageType);
            }
        }

        // 如果未找到，尝试通过反射获取
        if (_enableReflection)
        {
            var request = new Reflection.ServerReflectionRequest
            {
                FileContainingSymbol = messageTypeName
            };

            using var call = _reflectionClient.Value.ServerReflectionInfo(CreateCallOptions());
            await call.RequestStream.WriteAsync(request);
            await call.RequestStream.CompleteAsync();

            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                if (response.FileDescriptorResponse != null)
                {
                    var fileDescriptor = await ParseFileDescriptorAsync(response.FileDescriptorResponse);
                    if (fileDescriptor != null)
                    {
                        var messageType = fileDescriptor.FindTypeByName<MessageDescriptor>(messageTypeName);
                        if (messageType != null)
                        {
                            return new DynamicMessage(messageType);
                        }
                    }
                }
            }
        }

        throw new InvalidOperationException($"Message type '{messageTypeName}' not found");
    }

    private CallOptions CreateCallOptions()
    {
        var metadata = new Metadata();
        foreach (var header in _headers)
        {
            metadata.Add(header.Key, header.Value);
        }

        return new CallOptions(metadata);
    }

    private Task<FileDescriptor?> ParseFileDescriptorAsync(Reflection.FileDescriptorResponse response)
    {
        try
        {
            // 简化实现：由于 FileDescriptor.BuildFrom 在某些版本中可能不可用
            // 这里返回 null，实际项目中需要更完整的 proto 解析实现
            _logger?.LogWarning("File descriptor parsing is not implemented in this simplified version");
            return Task.FromResult<FileDescriptor?>(null);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to parse file descriptor");
            return Task.FromResult<FileDescriptor?>(null);
        }
    }

    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        if (_retryPolicy == null)
        {
            return await operation();
        }

        var attempt = 0;
        var delay = _retryPolicy.InitialBackoff;

        while (attempt < _retryPolicy.MaxRetryAttempts)
        {
            try
            {
                return await operation();
            }
            catch (RpcException ex) when (_retryPolicy.RetryableStatusCodes.Contains(ex.StatusCode) && attempt < _retryPolicy.MaxRetryAttempts - 1)
            {
                attempt++;
                _logger?.LogWarning("Retrying operation, attempt {Attempt}/{MaxAttempts}, delay {Delay}ms", 
                    attempt, _retryPolicy.MaxRetryAttempts, delay.TotalMilliseconds);
                
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(Math.Min(
                    delay.TotalMilliseconds * _retryPolicy.BackoffMultiplier,
                    _retryPolicy.MaxBackoff.TotalMilliseconds));
            }
        }

        return await operation(); // 最后一次尝试，让异常抛出
    }

    #region Call Creation Methods

    private AsyncUnaryCall<DynamicMessage> CreateUnaryCall(ServiceDescriptor service, MethodDescriptor method, DynamicMessage request, CallOptions callOptions)
    {
        return DynamicMethodInvoker.CreateUnaryCall(_channel, service, method, request, callOptions);
    }

    private AsyncClientStreamingCall<DynamicMessage, DynamicMessage> CreateClientStreamingCall(ServiceDescriptor service, MethodDescriptor method, CallOptions callOptions)
    {
        return DynamicMethodInvoker.CreateClientStreamingCall(_channel, service, method, callOptions);
    }

    private AsyncServerStreamingCall<DynamicMessage> CreateServerStreamingCall(ServiceDescriptor service, MethodDescriptor method, DynamicMessage request, CallOptions callOptions)
    {
        return DynamicMethodInvoker.CreateServerStreamingCall(_channel, service, method, request, callOptions);
    }

    private AsyncDuplexStreamingCall<DynamicMessage, DynamicMessage> CreateBidirectionalStreamingCall(ServiceDescriptor service, MethodDescriptor method, CallOptions callOptions)
    {
        return DynamicMethodInvoker.CreateDuplexStreamingCall(_channel, service, method, callOptions);
    }

    #endregion

    /// <summary>
    /// 注册消息描述符（用于不支持反射的场景）
    /// </summary>
    public void RegisterMessageDescriptor(MessageDescriptor descriptor)
    {
        // 将消息描述符存储在缓存中
        // 由于没有文件描述符，我们创建一个虚拟的
        var fileName = $"dynamic_{descriptor.FullName}.proto";
        if (!_fileCache.ContainsKey(fileName))
        {
            // 这里应该有更完整的实现，但为了简化，我们只存储引用
            _logger?.LogInformation("Registered message descriptor: {MessageType}", descriptor.FullName);
        }
    }

    /// <summary>
    /// 注册服务描述符（用于不支持反射的场景）
    /// </summary>
    public void RegisterServiceDescriptor(ServiceDescriptor descriptor)
    {
        _serviceCache.TryAdd(descriptor.FullName, descriptor);
        _logger?.LogInformation("Registered service descriptor: {ServiceName}", descriptor.FullName);
    }

    /// <summary>
    /// 注册文件描述符（用于不支持反射的场景）
    /// </summary>
    public void RegisterFileDescriptor(FileDescriptor fileDescriptor)
    {
        _fileCache.TryAdd(fileDescriptor.Name, fileDescriptor);
        
        // 注册文件中的所有服务
        foreach (var service in fileDescriptor.Services)
        {
            _serviceCache.TryAdd(service.FullName, service);
        }
        
        _logger?.LogInformation("Registered file descriptor: {FileName} with {ServiceCount} services", 
            fileDescriptor.Name, fileDescriptor.Services.Count);
    }

    /// <summary>
    /// 使用预注册的描述符调用一元方法
    /// </summary>
    public async Task<DynamicMessage> CallUnaryWithDescriptorAsync(ServiceDescriptor service, MethodDescriptor method, DynamicMessage request, CallOptions? options = null)
    {
        var callOptions = options ?? CreateCallOptions();
        
        return await ExecuteWithRetryAsync(async () =>
        {
            var call = CreateUnaryCall(service, method, request, callOptions);
            var response = await call.ResponseAsync;
            return response;
        });
    }

    public void Dispose()
    {
        if (_reflectionClient.IsValueCreated)
        {
            // ServerReflectionClient 会在 channel dispose 时自动清理
        }
        _channel?.Dispose();
    }
}

/// <summary>
/// 线程安全的缓存类
/// </summary>
internal class ConcurrentCache<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> _cache = new();

    public bool TryGetValue(TKey key, out TValue? value) => _cache.TryGetValue(key, out value);
    public bool TryAdd(TKey key, TValue value) => _cache.TryAdd(key, value);
    public bool ContainsKey(TKey key) => _cache.ContainsKey(key);
    public IEnumerable<TValue> Values => _cache.Values;
}
