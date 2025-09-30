using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Orders.Api.Http;

public sealed class HttpClientLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpClientLoggingHandler> _log;
    public HttpClientLoggingHandler(ILogger<HttpClientLoggingHandler> log) => _log = log;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        _log.LogInformation("HTTP → {Method} {Url}", request.Method, request.RequestUri);

        var resp = await base.SendAsync(request, ct);

        sw.Stop();
        _log.LogInformation("HTTP ← {Status} {Method} {Url} in {Elapsed} ms",
            (int)resp.StatusCode, request.Method, request.RequestUri, sw.ElapsedMilliseconds);

        return resp;
    }
}
