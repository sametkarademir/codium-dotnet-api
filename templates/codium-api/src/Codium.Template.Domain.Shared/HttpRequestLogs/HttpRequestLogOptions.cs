namespace Codium.Template.Domain.Shared.HttpRequestLogs;

public class HttpRequestLogOptions
{
    public const string SectionName = "HttpRequestLog";

    public bool Enabled { get; set; } = true;
    public string MaskPattern { get; set; } = "***MASKED***";

    public List<string> ExcludedPaths { get; set; } = ["/health", "/metrics", "/favicon.ico"];
    public List<string> ExcludedHttpMethods { get; set; } = [];
    public List<string> ExcludedContentTypes { get; set; } = ["application/octet-stream", "application/pdf", "image/", "video/", "audio/"];

    public bool LogRequestBody { get; set; } = false;
    public int MaxRequestBodyLength { get; set; } = 5000;
    public bool LogOnlySlowRequests { get; set; } = false;
    public long SlowRequestThresholdMs { get; set; } = 10;

    public List<string> RequestBodySensitiveProperties { get; set; } = ["Password", "Token", "Secret", "Key", "Credential", "Ssn", "Credit", "Card"];
    public List<string> QueryStringSensitiveProperties { get; set; } = ["Password", "Token", "Secret", "ApiKey", "Key"];
    public List<string> HeaderSensitiveProperties { get; set; } = ["Authorization", "Cookie", "X-Api-Key"];
}
