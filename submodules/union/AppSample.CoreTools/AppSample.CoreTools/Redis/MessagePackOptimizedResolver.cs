using MessagePack;

namespace AppSample.CoreTools.Redis;

/// <summary>
/// Оптимизированные настройки resolver'а для MessagePack - нестандартная сериализация (в бинарном формате .NET) для DateTime и Guid
/// </summary>
public static class MessagePackOptimizedResolver
{
    static readonly IFormatterResolver StandardResolver = MessagePack.Resolvers.CompositeResolver.Create(
        // resolver custom types first
        MessagePack.Resolvers.NativeGuidResolver.Instance,
        //MessagePack.Resolvers.NativeDecimalResolver.Instance,
        MessagePack.Resolvers.NativeDateTimeResolver.Instance,
        // finally use standard resolver
        MessagePack.Resolvers.StandardResolver.Instance
    );

    public static readonly MessagePack.MessagePackSerializerOptions StandardOptions = MessagePackSerializerOptions.Standard.WithResolver(StandardResolver);

    static readonly IFormatterResolver ContractlessResolver = MessagePack.Resolvers.CompositeResolver.Create(
        // resolver custom types first
        MessagePack.Resolvers.NativeGuidResolver.Instance,
        //MessagePack.Resolvers.NativeDecimalResolver.Instance,
        MessagePack.Resolvers.NativeDateTimeResolver.Instance,
        // finally use contractless resolver
        MessagePack.Resolvers.ContractlessStandardResolver.Instance
    );

    public static readonly MessagePack.MessagePackSerializerOptions ContractlessOptions = MessagePackSerializerOptions.Standard.WithResolver(ContractlessResolver);

}