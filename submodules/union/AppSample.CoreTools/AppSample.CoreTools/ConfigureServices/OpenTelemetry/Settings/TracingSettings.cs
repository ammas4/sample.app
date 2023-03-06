namespace AppSample.CoreTools.ConfigureServices.OpenTelemetry.Settings
{
    /// <summary>
    /// Настройки трассировки
    /// </summary>
    public class TracingSettings
    {
        /// <summary>
        /// Изменять название HttpClient Span с "HTTP GET"/"HTTP POST" на URL запроса 
        /// </summary>
        public bool RewriteHttpClientSpanName { get; set; }
        
        /// <summary>
        /// Включить трассировку HttpClient
        /// </summary>
        public bool HttpClientInstrumentation { get; set; }
        
        /// <summary>
        /// Включить трассировку AspNetCore
        /// </summary>
        public bool AspNetCoreInstrumentation { get; set; }
        
        /// <summary>
        /// Включить трассировку SqlClient
        /// </summary>
        public bool SqlClientInstrumentation { get; set; }
        
        /// <summary>
        /// Добавлять в трейсы текст исключения
        /// </summary>
        public bool RecordException { get; set; }
        
        /// <summary>
        /// Добавлять в трейсы SqlClient запрос
        /// </summary>
        public bool SetDbStatementForText { get; set; }

        /// <summary>
        /// Трассировка включена
        /// </summary>
        public bool IsEnabled { get; set; }
        
        /// <summary>
        /// Список названий эндпоинтов исключений из трассировки AspNetCore
        /// </summary>
        public string[] AspNetCoreInstrumentationFilter { get; set; }
        
        /// <summary>
        /// Список названий эндпоинтов исключений из трассировки HttpClient
        /// </summary>
        public string[] HttpClientInstrumentationFilter { get; set; }
        
        public JaegerSettings Jaeger { get; set; }
    }
}
