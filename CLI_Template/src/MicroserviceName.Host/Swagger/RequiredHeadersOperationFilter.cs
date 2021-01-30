using System.Collections.Generic;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MicroserviceName.Host.Swagger
{
    public class RequiredHeadersOperationFilter : IOperationFilter
    {
        private readonly IEnumerable<OpenApiParameter> _requiredHeaders = new List<OpenApiParameter>
        {
            new OpenApiParameter
            {
                Name = "ConversationId",
                In = ParameterLocation.Header,
                Required = true,
                Example = new OpenApiString("SwaggerConversationId"),
            }
        };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            foreach (var parameter in _requiredHeaders)
            {
                operation.Parameters.Add(parameter);
            }
        }
    }
}
