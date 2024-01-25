using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace InfoCompass.World.UiWebApi.Logic;

/// <summary>
/// Source: https://www.seeleycoder.com/blog/optional-route-parameters-with-swagger-asp-net-core/
/// </summary>
public class ReApplyOptionalRouteParameterOperationFilter:IOperationFilter
{
	private const string captureName = "routeParameter";

	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		IEnumerable<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute> httpMethodAttributes = context.MethodInfo
			.GetCustomAttributes(true)
			.OfType<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>();

		Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute? httpMethodWithOptional = httpMethodAttributes?.FirstOrDefault(m => m.Template?.Contains("?") ?? false);
		if(httpMethodWithOptional == null)
		{
			return;
		}

		string regex = $"{{(?<{captureName}>\\w+)\\?}}";

		System.Text.RegularExpressions.MatchCollection matches = System.Text.RegularExpressions.Regex.Matches(httpMethodWithOptional.Template, regex);

		foreach(System.Text.RegularExpressions.Match match in matches)
		{
			string name = match.Groups[captureName].Value;

			OpenApiParameter? parameter = operation.Parameters.FirstOrDefault(p => p.In == ParameterLocation.Path && p.Name == name);
			if(parameter != null)
			{
				parameter.AllowEmptyValue = true;
				parameter.Description = "Must check \"Send empty value\" or Swagger passes a comma for empty values otherwise";
				parameter.Required = false;
				//parameter.Schema.Default = new OpenApiString(string.Empty);
				parameter.Schema.Nullable = true;
			}
		}
	}
}
