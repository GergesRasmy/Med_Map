using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;

namespace Med_Map.Controllers
{
    [ApiController]
    [Route("api/schema")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SchemaController : ControllerBase
    {
        private readonly ISwaggerProvider _swagger;

        public SchemaController(ISwaggerProvider swagger)
        {
            _swagger = swagger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var doc = _swagger.GetSwagger("v1");

            var endpoints = doc.Paths
                .SelectMany(path => path.Value.Operations.Select(op => new
                {
                    method = op.Key.ToString().ToUpper(),
                    path = path.Key,
                    auth = op.Value.Security?.Count > 0,
                    body = ResolveBody(op.Value.RequestBody, doc),
                    response = ResolveResponse(op.Value.Responses, doc)
                }))
                .ToList();

            return Ok(endpoints);
        }

        private object? ResolveBody(OpenApiRequestBody? body, OpenApiDocument doc)
        {
            var schema = body?.Content.Values.FirstOrDefault()?.Schema;
            return schema is null ? null : ResolveSchema(schema, doc, []);
        }

        private object? ResolveResponse(OpenApiResponses responses, OpenApiDocument doc)
        {
            var success = responses.FirstOrDefault(r => r.Key.StartsWith("2")).Value;
            var schema = success?.Content?.Values.FirstOrDefault()?.Schema;
            return schema is null ? null : ResolveSchema(schema, doc, []);
        }

        private object ResolveSchema(OpenApiSchema schema, OpenApiDocument doc, HashSet<string> visited)
        {
            if (schema.Reference != null)
            {
                var id = schema.Reference.Id;
                if (visited.Contains(id)) return $"ref:{id}";
                if (doc.Components.Schemas.TryGetValue(id, out var resolved))
                    return ResolveSchema(resolved, doc, [.. visited, id]);
            }

            if (schema.AllOf?.Count > 0)
            {
                var merged = new Dictionary<string, object>();
                foreach (var sub in schema.AllOf)
                    if (ResolveSchema(sub, doc, visited) is Dictionary<string, object> d)
                        foreach (var kv in d) merged[kv.Key] = kv.Value;
                if (schema.Properties?.Count > 0)
                    foreach (var p in schema.Properties)
                        merged[p.Key] = ResolveSchema(p.Value, doc, visited);
                return merged;
            }

            if (schema.Properties?.Count > 0)
                return schema.Properties.ToDictionary(
                    p => p.Key,
                    p => (object)ResolveSchema(p.Value, doc, visited));

            if (schema.Type == "array" && schema.Items != null)
                return new object[] { ResolveSchema(schema.Items, doc, visited) };

            if (schema.Enum?.Count > 0)
            {
                var values = schema.Enum
                    .OfType<OpenApiString>().Select(e => e.Value)
                    .Concat(schema.Enum.OfType<OpenApiInteger>().Select(e => e.Value.ToString()));
                return $"enum({string.Join("|", values)})";
            }

            return schema.Format is not null ? $"{schema.Type}({schema.Format})" : schema.Type ?? "object";
        }
    }
}
