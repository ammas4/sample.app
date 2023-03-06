namespace AppSample.CoreTools.Redis;

/// <summary>
/// Интерфейс для отметки классов, которые настроены для сериализации с помощью MessagePack.
/// Класс должен быть отмечен атрибутом MessagePackObject, его свойства - атрибутом Key или IgnoreMember.
/// </summary>
public interface IMessagePackedObject
{
}