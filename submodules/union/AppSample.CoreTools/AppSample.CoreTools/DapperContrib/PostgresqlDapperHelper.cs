using AppSample.CoreTools.Helpers;

namespace AppSample.CoreTools.DapperContrib;

public static class PostgresqlDapperHelper
{
    public static void Configure()
    {
        //настройки Dapper'а
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapperExtensions.TableNameMapper = (x) => (TableNameConverter(x.Name));
        SqlMapperExtensions.ColumnNameMapper = ColumnNameConverter;
    }

    static string TableNameConverter(string className)
    {
        return StringHelper.ToSnakeCase(className).ToLowerInvariant().Replace("_entity", "") + "s";
    }

    static string ColumnNameConverter(string propertyName)
    {
        return StringHelper.ToSnakeCase(propertyName).ToLowerInvariant();
    }
}