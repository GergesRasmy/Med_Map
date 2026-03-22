using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Med_Map.Filters
{
    public class MultipleResponseTypesOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var grouped = context.MethodInfo
                .GetCustomAttributes(typeof(ProducesResponseTypeAttribute), true)
                .Cast<ProducesResponseTypeAttribute>()
                .GroupBy(a => a.StatusCode)
                .Where(g => g.Count() > 1);

            foreach (var group in grouped)
            {
                var statusCode = group.Key.ToString();
                if (!operation.Responses.ContainsKey(statusCode)) continue;

                var schemas = group
                    .Select(a => a.Type)
                    .Where(t => t != null && t != typeof(void))
                    .Select(t => context.SchemaGenerator.GenerateSchema(t, context.SchemaRepository))
                    .ToList();

                if (schemas.Count < 2) continue;

                var oneOfSchema = new OpenApiSchema { OneOf = schemas };

                foreach (var content in operation.Responses[statusCode].Content)
                    content.Value.Schema = oneOfSchema;

                foreach (var key in new[] { "application/json", "text/plain", "text/json" })
                    if (!operation.Responses[statusCode].Content.ContainsKey(key))
                        operation.Responses[statusCode].Content[key] = new OpenApiMediaType { Schema = oneOfSchema };
            }
        }
    }
}
