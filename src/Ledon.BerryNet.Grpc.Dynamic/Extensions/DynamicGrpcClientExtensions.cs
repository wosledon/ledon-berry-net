using Google.Protobuf.Reflection;
using Grpc.Core;
using Ledon.BerryNet.Grpc.Dynamic.Messages;

namespace Ledon.BerryNet.Grpc.Dynamic.Extensions;

/// <summary>
/// 动态 gRPC 客户端扩展方法
/// </summary>
public static class DynamicGrpcClientExtensions
{
    /// <summary>
    /// 注册消息描述符，用于不支持反射的场景
    /// </summary>
    public static void RegisterMessageDescriptor(this IDynamicGrpcClient client, MessageDescriptor descriptor)
    {
        if (client is DynamicGrpcClient dynamicClient)
        {
            dynamicClient.RegisterMessageDescriptor(descriptor);
        }
    }

    /// <summary>
    /// 注册服务描述符，用于不支持反射的场景
    /// </summary>
    public static void RegisterServiceDescriptor(this IDynamicGrpcClient client, ServiceDescriptor descriptor)
    {
        if (client is DynamicGrpcClient dynamicClient)
        {
            dynamicClient.RegisterServiceDescriptor(descriptor);
        }
    }

    /// <summary>
    /// 从 FileDescriptor 批量注册描述符
    /// </summary>
    public static void RegisterFileDescriptor(this IDynamicGrpcClient client, FileDescriptor fileDescriptor)
    {
        if (client is DynamicGrpcClient dynamicClient)
        {
            dynamicClient.RegisterFileDescriptor(fileDescriptor);
        }
    }

    /// <summary>
    /// 使用预定义的消息类型创建动态消息
    /// </summary>
    public static DynamicMessage CreateMessage(this IDynamicGrpcClient client, MessageDescriptor descriptor)
    {
        return new DynamicMessage(descriptor);
    }

    /// <summary>
    /// 简化的一元调用，不依赖反射
    /// </summary>
    public static async Task<DynamicMessage> CallAsync(
        this IDynamicGrpcClient client,
        ServiceDescriptor service,
        string methodName,
        DynamicMessage request,
        CallOptions? options = null)
    {
        var method = service.FindMethodByName(methodName);
        if (method == null)
            throw new ArgumentException($"Method '{methodName}' not found in service '{service.FullName}'");

        // 由于反射不可用，我们需要使用预注册的描述符
        if (client is DynamicGrpcClient dynamicClient)
        {
            return await dynamicClient.CallUnaryWithDescriptorAsync(service, method, request, options);
        }

        throw new NotSupportedException("This client type does not support direct descriptor calls");
    }
}

/// <summary>
/// 动态 gRPC 客户端构建器扩展
/// </summary>
public static class DynamicGrpcClientBuilderExtensions
{
    /// <summary>
    /// 禁用反射（当服务器不支持反射时使用）
    /// </summary>
    public static IDynamicGrpcClientBuilder WithoutReflection(this IDynamicGrpcClientBuilder builder)
    {
        return builder.WithReflection(false);
    }

    /// <summary>
    /// 添加预定义的文件描述符
    /// </summary>
    public static IDynamicGrpcClientBuilder WithFileDescriptor(this IDynamicGrpcClientBuilder builder, FileDescriptor fileDescriptor)
    {
        if (builder is DynamicGrpcClientBuilder dynamicBuilder)
        {
            dynamicBuilder.AddFileDescriptor(fileDescriptor);
        }
        return builder;
    }
}
