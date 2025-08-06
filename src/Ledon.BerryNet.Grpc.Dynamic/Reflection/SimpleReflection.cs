using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Net.Client;

namespace Ledon.BerryNet.Grpc.Dynamic.Reflection
{
    /// <summary>
    /// 简化的 gRPC 反射服务响应
    /// </summary>
    public class ServerReflectionResponse : IMessage
    {
        public FileDescriptorResponse? FileDescriptorResponse { get; set; }
        public ListServiceResponse? ListServicesResponse { get; set; }
        public ErrorResponse? ErrorResponse { get; set; }

        public MessageDescriptor Descriptor => throw new NotImplementedException();

        public int CalculateSize() => 0;
        public IMessage Clone() => new ServerReflectionResponse();
        public bool Equals(IMessage other) => false;
        public void MergeFrom(CodedInputStream input) { }
        public void MergeFrom(IMessage other) { }
        public byte[] ToByteArray() => Array.Empty<byte>();
        public void WriteTo(CodedOutputStream output) { }
    }

    /// <summary>
    /// 简化的 gRPC 反射服务请求
    /// </summary>
    public class ServerReflectionRequest : IMessage
    {
        public string? FileContainingSymbol { get; set; }
        public string? ListServices { get; set; }

        public MessageDescriptor Descriptor => throw new NotImplementedException();

        public int CalculateSize() => 0;
        public IMessage Clone() => new ServerReflectionRequest();
        public bool Equals(IMessage other) => false;
        public void MergeFrom(CodedInputStream input) { }
        public void MergeFrom(IMessage other) { }
        public byte[] ToByteArray() => Array.Empty<byte>();
        public void WriteTo(CodedOutputStream output) { }
    }

    /// <summary>
    /// 文件描述符响应
    /// </summary>
    public class FileDescriptorResponse
    {
        public List<ByteString> FileDescriptorProto { get; set; } = new();
    }

    /// <summary>
    /// 服务列表响应
    /// </summary>
    public class ListServiceResponse
    {
        public List<ServiceResponse> Service { get; set; } = new();
    }

    /// <summary>
    /// 服务响应
    /// </summary>
    public class ServiceResponse
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 错误响应
    /// </summary>
    public class ErrorResponse
    {
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 简化的服务器反射客户端（模拟）
    /// </summary>
    public class ServerReflectionClient
    {
        public ServerReflectionClient(GrpcChannel channel)
        {
            // 模拟实现，实际项目中需要真正的反射客户端
        }

        public AsyncDuplexStreamingCall<ServerReflectionRequest, ServerReflectionResponse> ServerReflectionInfo(CallOptions options)
        {
            // 返回一个模拟的流，实际实现需要真正的 gRPC 调用
            throw new NotSupportedException("Server reflection is not available in this simplified implementation. Please use pre-defined message descriptors.");
        }
    }
}

namespace Grpc.Reflection.V1Alpha
{
    // 提供兼容的命名空间别名
    public class ServerReflection
    {
        public class ServerReflectionClient : Ledon.BerryNet.Grpc.Dynamic.Reflection.ServerReflectionClient
        {
            public ServerReflectionClient(GrpcChannel channel) : base(channel) { }
        }
    }
}
