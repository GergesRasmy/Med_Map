using System.Text.Json.Serialization;

namespace Med_Map.DTO
{
    public class ResponseDTO<T>
    {
        public bool success { get; set; }
        public string code { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public T? data { get; set; }
        public object? error { get; set; }
    }
}
