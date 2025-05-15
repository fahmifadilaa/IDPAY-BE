using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Ekr.EnrollmentThirdParty.Filters
{
    public class DefaultHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Type-Aplikasi",
                In = ParameterLocation.Header,
                Required = true,
                Example = new OpenApiString("")
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Apps-Agent",
                In = ParameterLocation.Header,
                Required = true,
                Example = new OpenApiString("")
            });
        }
    }
}
