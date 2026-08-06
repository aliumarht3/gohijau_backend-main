namespace GoHijauBackend.Infrastructure.External.Clients
{
    public class ApiResponse
    {
        public string Body { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int StatusCode { get; set; }
    }
}
