using Google.Protobuf;
using Google.Protobuf.Reflection;
using System.Text.Json;

namespace Ledon.BerryNet.Grpc.Dynamic.Messages;

/// <summary>
/// 动态 Protobuf 消息，支持运行时创建和操作
/// </summary>
public class DynamicMessage : IMessage
{
    private readonly MessageDescriptor _descriptor;
    private readonly Dictionary<FieldDescriptor, object?> _fields;

    public DynamicMessage(MessageDescriptor descriptor)
    {
        _descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        _fields = new Dictionary<FieldDescriptor, object?>();
    }

    public MessageDescriptor Descriptor => _descriptor;

    /// <summary>
    /// 设置字段值
    /// </summary>
    public DynamicMessage SetField(string fieldName, object? value)
    {
        var field = _descriptor.FindFieldByName(fieldName);
        if (field == null)
            throw new ArgumentException($"Field '{fieldName}' not found in message '{_descriptor.FullName}'");

        _fields[field] = ConvertValue(field, value);
        return this;
    }

    /// <summary>
    /// 获取字段值
    /// </summary>
    public T? GetField<T>(string fieldName)
    {
        var field = _descriptor.FindFieldByName(fieldName);
        if (field == null)
            throw new ArgumentException($"Field '{fieldName}' not found in message '{_descriptor.FullName}'");

        if (_fields.TryGetValue(field, out var value))
        {
            return (T?)value;
        }

        return default;
    }

    /// <summary>
    /// 从 JSON 字符串创建消息
    /// </summary>
    public static DynamicMessage FromJson(MessageDescriptor descriptor, string json)
    {
        var message = new DynamicMessage(descriptor);
        var jsonDoc = JsonDocument.Parse(json);
        
        foreach (var property in jsonDoc.RootElement.EnumerateObject())
        {
            var field = descriptor.FindFieldByName(property.Name);
            if (field != null)
            {
                var value = ConvertJsonValue(field, property.Value);
                message._fields[field] = value;
            }
        }

        return message;
    }

    /// <summary>
    /// 转换为 JSON 字符串
    /// </summary>
    public string ToJson()
    {
        var jsonObject = new Dictionary<string, object?>();
        
        foreach (var kvp in _fields)
        {
            jsonObject[kvp.Key.Name] = ConvertToJsonValue(kvp.Key, kvp.Value);
        }

        return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });
    }

    private static object? ConvertValue(FieldDescriptor field, object? value)
    {
        if (value == null) return null;

        return field.FieldType switch
        {
            FieldType.Double => Convert.ToDouble(value),
            FieldType.Float => Convert.ToSingle(value),
            FieldType.Int64 => Convert.ToInt64(value),
            FieldType.UInt64 => Convert.ToUInt64(value),
            FieldType.Int32 => Convert.ToInt32(value),
            FieldType.Fixed64 => Convert.ToUInt64(value),
            FieldType.Fixed32 => Convert.ToUInt32(value),
            FieldType.Bool => Convert.ToBoolean(value),
            FieldType.String => value.ToString(),
            FieldType.Bytes => value is byte[] bytes ? ByteString.CopyFrom(bytes) : ByteString.CopyFromUtf8(value.ToString() ?? ""),
            FieldType.UInt32 => Convert.ToUInt32(value),
            FieldType.SFixed32 => Convert.ToInt32(value),
            FieldType.SFixed64 => Convert.ToInt64(value),
            FieldType.SInt32 => Convert.ToInt32(value),
            FieldType.SInt64 => Convert.ToInt64(value),
            _ => value
        };
    }

    private static object? ConvertJsonValue(FieldDescriptor field, JsonElement jsonValue)
    {
        if (jsonValue.ValueKind == JsonValueKind.Null) return null;

        return field.FieldType switch
        {
            FieldType.Double => jsonValue.GetDouble(),
            FieldType.Float => jsonValue.GetSingle(),
            FieldType.Int64 => jsonValue.GetInt64(),
            FieldType.UInt64 => jsonValue.GetUInt64(),
            FieldType.Int32 => jsonValue.GetInt32(),
            FieldType.Fixed64 => jsonValue.GetUInt64(),
            FieldType.Fixed32 => jsonValue.GetUInt32(),
            FieldType.Bool => jsonValue.GetBoolean(),
            FieldType.String => jsonValue.GetString(),
            FieldType.Bytes => ByteString.CopyFromUtf8(jsonValue.GetString() ?? ""),
            FieldType.UInt32 => jsonValue.GetUInt32(),
            FieldType.SFixed32 => jsonValue.GetInt32(),
            FieldType.SFixed64 => jsonValue.GetInt64(),
            FieldType.SInt32 => jsonValue.GetInt32(),
            FieldType.SInt64 => jsonValue.GetInt64(),
            _ => jsonValue.GetString()
        };
    }

    private static object? ConvertToJsonValue(FieldDescriptor field, object? value)
    {
        if (value == null) return null;

        return field.FieldType switch
        {
            FieldType.Bytes when value is ByteString bytes => bytes.ToStringUtf8(),
            _ => value
        };
    }

    #region IMessage Implementation
    
    public int CalculateSize()
    {
        // 简化实现，实际使用中可能需要更精确的计算
        return ToByteArray().Length;
    }

    public void MergeFrom(CodedInputStream input)
    {
        throw new NotImplementedException("Dynamic message merge from coded input is not supported");
    }

    public void WriteTo(CodedOutputStream output)
    {
        foreach (var kvp in _fields)
        {
            WriteField(output, kvp.Key, kvp.Value);
        }
    }

    public IMessage Clone()
    {
        var clone = new DynamicMessage(_descriptor);
        foreach (var kvp in _fields)
        {
            clone._fields[kvp.Key] = kvp.Value;
        }
        return clone;
    }

    public bool Equals(IMessage other)
    {
        if (other is not DynamicMessage otherDynamic) return false;
        if (!_descriptor.Equals(otherDynamic._descriptor)) return false;
        
        return _fields.SequenceEqual(otherDynamic._fields);
    }

    public void MergeFrom(IMessage other)
    {
        if (other is DynamicMessage otherDynamic && _descriptor.Equals(otherDynamic._descriptor))
        {
            foreach (var kvp in otherDynamic._fields)
            {
                _fields[kvp.Key] = kvp.Value;
            }
        }
    }

    public byte[] ToByteArray()
    {
        using var stream = new MemoryStream();
        using var output = new CodedOutputStream(stream);
        WriteTo(output);
        output.Flush();
        return stream.ToArray();
    }

    private static void WriteField(CodedOutputStream output, FieldDescriptor field, object? value)
    {
        if (value == null) return;

        var tag = WireFormat.MakeTag(field.FieldNumber, GetWireType(field.FieldType));
        output.WriteTag(tag);

        switch (field.FieldType)
        {
            case FieldType.Double:
                output.WriteDouble((double)value);
                break;
            case FieldType.Float:
                output.WriteFloat((float)value);
                break;
            case FieldType.Int64:
                output.WriteInt64((long)value);
                break;
            case FieldType.UInt64:
                output.WriteUInt64((ulong)value);
                break;
            case FieldType.Int32:
                output.WriteInt32((int)value);
                break;
            case FieldType.Bool:
                output.WriteBool((bool)value);
                break;
            case FieldType.String:
                output.WriteString((string)value);
                break;
            case FieldType.Bytes:
                output.WriteBytes((ByteString)value);
                break;
        }
    }

    private static WireFormat.WireType GetWireType(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.Double or FieldType.Fixed64 or FieldType.SFixed64 => WireFormat.WireType.Fixed64,
            FieldType.Float or FieldType.Fixed32 or FieldType.SFixed32 => WireFormat.WireType.Fixed32,
            FieldType.String or FieldType.Bytes or FieldType.Message => WireFormat.WireType.LengthDelimited,
            _ => WireFormat.WireType.Varint
        };
    }

    #endregion
}
