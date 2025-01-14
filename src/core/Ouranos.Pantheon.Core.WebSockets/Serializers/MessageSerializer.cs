namespace Ouranos.Pantheon.Core.WebSockets.Serializers;

public sealed class MessageSerializer : IMessageSerializer
{
    private readonly IMessageConverter _messageConverter;
    private readonly ITypeResolver _typeResolver;

    public MessageSerializer(
        ITypeResolver typeResolver,
        IMessageConverter messageConverter
    )
    {
        ArgumentNullException.ThrowIfNull(messageConverter);
        ArgumentNullException.ThrowIfNull(typeResolver);

        _typeResolver = typeResolver;
        _messageConverter = messageConverter;
    }

    public byte[] Serialize<T>(T message)
    {
        return message is null ? [] : _messageConverter.Serialize(message);
    }

    public T Deserialize<T>(byte[] data)
    {
        var targetType = _typeResolver.ResolveType(data);
        if (!typeof(T).IsAssignableFrom(targetType))
        {
            throw new InvalidOperationException($"Resolved type '{targetType}' is not assignable to '{typeof(T)}'.");
        }

        var instance = _messageConverter.Deserialize(data, targetType);
        if (instance is not IList<object> list)
        {
            return (T)instance;
        }

        object resolvedList = list
            .Select(item => _messageConverter.Serialize(item))
            .Select(Deserialize<object>)
            .ToList();

        return (T)resolvedList;
    }
}