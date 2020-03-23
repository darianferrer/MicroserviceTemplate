using Microsoft.OpenApi.Models;

namespace MicroserviceName.Host.AppSettings
{
    public class SwaggerSettings
    {
        public string? Description { get; set; }
        public OpenApiContact? Contact { get; set; }
    }
}
