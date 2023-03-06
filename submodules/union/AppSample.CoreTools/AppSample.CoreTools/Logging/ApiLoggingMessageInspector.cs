using System.Diagnostics;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace AppSample.CoreTools.Logging;

public class ApiLoggingMessageInspector : IClientMessageInspector
{
    readonly IRequestResponseLogger _requestResponseLogger;
    readonly string _clientName;

    public ApiLoggingMessageInspector(IRequestResponseLogger requestResponseLogger, string clientName)
    {
        _requestResponseLogger = requestResponseLogger;
        _clientName = clientName;
    }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        var requestBody = ToString(ref request, false);
        var context = new LoggingContext();

        context.LogItem.Api = _clientName;
        context.LogItem.RequestBody = requestBody;
        context.LogItem.RequestMethod = HttpMethod.Post.Method;
        context.LogItem.RequestUrl = channel.RemoteAddress.Uri.ToString();

        return context;
    }

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        var context = (LoggingContext) correlationState;
        context.StopWatch.Stop();
        string responseBody = ToString(ref reply, true);

        var logItem = context.LogItem;
        logItem.RequestBody = responseBody;
        logItem.ResponseStatusCode = reply.IsFault ? HttpStatusCode.InternalServerError : HttpStatusCode.OK;
        logItem.Time = context.StopWatch.Elapsed;
        _requestResponseLogger.Log(logItem);
    }

    static string ToString(ref Message request, bool streamed)
    {
        var buffer = request.CreateBufferedCopy(int.MaxValue);
        request = buffer.CreateMessage();

        if (!streamed)
            return (buffer.CreateMessage()).ToString();

        // Message.ToString() may have the Body contents as "... stream ...".
        // This means that the Message was created using an XmlRead or XmlDictionaryReader that was created from a Stream that hasn't been read yet.
        // https://stackoverflow.com/a/10759660/12007032
        using var memoryStream = new MemoryStream();
        var writer = XmlWriter.Create(memoryStream, new XmlWriterSettings {OmitXmlDeclaration = true});
        buffer.CreateMessage().WriteMessage(writer);
        writer.Flush();

        memoryStream.Position = 0;
        return new StreamReader(memoryStream).ReadToEnd();
    }

    class LoggingContext
    {
        public Stopwatch StopWatch { get; }
        public LogItem LogItem { get; }

        public LoggingContext()
        {
            StopWatch = Stopwatch.StartNew();
            LogItem = new LogItem();
        }
    }
}