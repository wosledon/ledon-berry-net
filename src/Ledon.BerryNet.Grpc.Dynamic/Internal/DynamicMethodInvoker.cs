using Google.Protobuf;
using Google.Protobuf.Reflection;
using Grpc.Core;
using Grpc.Net.Client;
using Ledon.BerryNet.Grpc.Dynamic.Messages;
using System.Runtime.CompilerServices;

namespace Ledon.BerryNet.Grpc.Dynamic.Internal;

/// <summary>
/// 动态方法调用器
/// </summary>
internal static class DynamicMethodInvoker
{
    /// <summary>
    /// 创建一元调用
    /// </summary>
    public static AsyncUnaryCall<DynamicMessage> CreateUnaryCall(
        GrpcChannel channel,
        ServiceDescriptor service,
        MethodDescriptor method,
        DynamicMessage request,
        CallOptions callOptions)
    {
        var methodDef = CreateMethodDefinition(service, method);
        var callInvoker = channel.CreateCallInvoker();
        
        return callInvoker.AsyncUnaryCall(methodDef, null, callOptions, request);
    }

    /// <summary>
    /// 创建客户端流式调用
    /// </summary>
    public static AsyncClientStreamingCall<DynamicMessage, DynamicMessage> CreateClientStreamingCall(
        GrpcChannel channel,
        ServiceDescriptor service,
        MethodDescriptor method,
        CallOptions callOptions)
    {
        var methodDef = CreateClientStreamingMethodDefinition(service, method);
        var callInvoker = channel.CreateCallInvoker();
        
        return callInvoker.AsyncClientStreamingCall(methodDef, null, callOptions);
    }

    /// <summary>
    /// 创建服务端流式调用
    /// </summary>
    public static AsyncServerStreamingCall<DynamicMessage> CreateServerStreamingCall(
        GrpcChannel channel,
        ServiceDescriptor service,
        MethodDescriptor method,
        DynamicMessage request,
        CallOptions callOptions)
    {
        var methodDef = CreateServerStreamingMethodDefinition(service, method);
        var callInvoker = channel.CreateCallInvoker();
        
        return callInvoker.AsyncServerStreamingCall(methodDef, null, callOptions, request);
    }

    /// <summary>
    /// 创建双向流式调用
    /// </summary>
    public static AsyncDuplexStreamingCall<DynamicMessage, DynamicMessage> CreateDuplexStreamingCall(
        GrpcChannel channel,
        ServiceDescriptor service,
        MethodDescriptor method,
        CallOptions callOptions)
    {
        var methodDef = CreateDuplexStreamingMethodDefinition(service, method);
        var callInvoker = channel.CreateCallInvoker();
        
        return callInvoker.AsyncDuplexStreamingCall(methodDef, null, callOptions);
    }

    private static Method<DynamicMessage, DynamicMessage> CreateMethodDefinition(
        ServiceDescriptor service,
        MethodDescriptor method)
    {
        return new Method<DynamicMessage, DynamicMessage>(
            MethodType.Unary,
            $"{service.FullName}",
            method.Name,
            CreateMarshaller(method.InputType),
            CreateMarshaller(method.OutputType));
    }

    private static Method<DynamicMessage, DynamicMessage> CreateClientStreamingMethodDefinition(
        ServiceDescriptor service,
        MethodDescriptor method)
    {
        return new Method<DynamicMessage, DynamicMessage>(
            MethodType.ClientStreaming,
            $"{service.FullName}",
            method.Name,
            CreateMarshaller(method.InputType),
            CreateMarshaller(method.OutputType));
    }

    private static Method<DynamicMessage, DynamicMessage> CreateServerStreamingMethodDefinition(
        ServiceDescriptor service,
        MethodDescriptor method)
    {
        return new Method<DynamicMessage, DynamicMessage>(
            MethodType.ServerStreaming,
            $"{service.FullName}",
            method.Name,
            CreateMarshaller(method.InputType),
            CreateMarshaller(method.OutputType));
    }

    private static Method<DynamicMessage, DynamicMessage> CreateDuplexStreamingMethodDefinition(
        ServiceDescriptor service,
        MethodDescriptor method)
    {
        return new Method<DynamicMessage, DynamicMessage>(
            MethodType.DuplexStreaming,
            $"{service.FullName}",
            method.Name,
            CreateMarshaller(method.InputType),
            CreateMarshaller(method.OutputType));
    }

    private static Marshaller<DynamicMessage> CreateMarshaller(MessageDescriptor messageDescriptor)
    {
        return new Marshaller<DynamicMessage>(
            message => message.ToByteArray(),
            bytes => DeserializeDynamicMessage(messageDescriptor, bytes));
    }

    private static DynamicMessage DeserializeDynamicMessage(MessageDescriptor descriptor, byte[] bytes)
    {
        var message = new DynamicMessage(descriptor);
        
        // 这里需要实现从字节数组反序列化到动态消息的逻辑
        // 由于这是一个复杂的实现，我们先提供一个简化版本
        using var stream = new MemoryStream(bytes);
        using var input = new CodedInputStream(stream);
        
        // 简化的反序列化逻辑
        while (!input.IsAtEnd)
        {
            var tag = input.ReadTag();
            if (tag == 0) break;

            var fieldNumber = WireFormat.GetTagFieldNumber(tag);
            var wireType = WireFormat.GetTagWireType(tag);
            
            var field = descriptor.Fields.InFieldNumberOrder().FirstOrDefault(f => f.FieldNumber == fieldNumber);
            if (field != null)
            {
                var value = ReadFieldValue(input, field, wireType);
                if (value != null)
                {
                    message.SetField(field.Name, value);
                }
            }
            else
            {
                // 跳过未知字段
                input.SkipLastField();
            }
        }

        return message;
    }

    private static object? ReadFieldValue(CodedInputStream input, FieldDescriptor field, WireFormat.WireType wireType)
    {
        return field.FieldType switch
        {
            FieldType.Double => input.ReadDouble(),
            FieldType.Float => input.ReadFloat(),
            FieldType.Int64 => input.ReadInt64(),
            FieldType.UInt64 => input.ReadUInt64(),
            FieldType.Int32 => input.ReadInt32(),
            FieldType.Fixed64 => input.ReadFixed64(),
            FieldType.Fixed32 => input.ReadFixed32(),
            FieldType.Bool => input.ReadBool(),
            FieldType.String => input.ReadString(),
            FieldType.Bytes => input.ReadBytes(),
            FieldType.UInt32 => input.ReadUInt32(),
            FieldType.SFixed32 => input.ReadSFixed32(),
            FieldType.SFixed64 => input.ReadSFixed64(),
            FieldType.SInt32 => input.ReadSInt32(),
            FieldType.SInt64 => input.ReadSInt64(),
            _ => null
        };
    }
}
