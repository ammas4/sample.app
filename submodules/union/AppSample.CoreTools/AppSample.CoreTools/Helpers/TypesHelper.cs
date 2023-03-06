namespace AppSample.CoreTools.Helpers;

public static class TypesHelper
{
    /// <summary>
    /// Получение списка всех наследников типа в коде сборок Beeline.*
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<Type> GetAllDescendantsInBeelineAssemblies<T>() where T : class
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (name != null && name.StartsWith("Beeline.", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var type in assembly.GetTypes()
                             .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
                {
                    yield return type;
                }
            }
        }
    }
}