using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mantenimiento.API.Swagger;

public class SwaggerOperationMetadataFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
        var actionName = context.ApiDescription.ActionDescriptor.RouteValues["action"];

        if (string.IsNullOrWhiteSpace(controllerName) || string.IsNullOrWhiteSpace(actionName))
        {
            return;
        }

        operation.Tags = new List<OpenApiTag>
        {
            new() { Name = SwaggerMetadata.GetTagName(controllerName) }
        };

        var operationInfo = SwaggerMetadata.GetOperationInfo(controllerName, actionName);
        operation.Summary = operationInfo.Summary;
        operation.Description = operationInfo.Description;
        operation.OperationId = $"{controllerName}_{actionName}";

        foreach (var response in operation.Responses)
        {
            response.Value.Description = SwaggerMetadata.GetResponseDescription(response.Key);
        }
    }
}
