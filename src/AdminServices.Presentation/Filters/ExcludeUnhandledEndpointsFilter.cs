using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdminServices.Presentation.Filters;

/// <summary>
/// Filter to exclude unhandled endpoints from Swagger
/// </summary>
public class ExcludeUnhandledEndpointsFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathsToRemove = swaggerDoc.Paths
            .Where(p => p.Key.Contains("/_") || p.Key.Contains("/swagger"))
            .Select(p => p.Key)
            .ToList();

        foreach (var path in pathsToRemove)
        {
            swaggerDoc.Paths.Remove(path);
        }
    }
}
