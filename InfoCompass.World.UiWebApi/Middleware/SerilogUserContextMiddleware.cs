using Serilog.Context;

namespace InfoCompass.World.UiWebApi.Middleware;

public class SerilogUserContextMiddleware
{
	private readonly RequestDelegate _next;

	public SerilogUserContextMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context, ServiceForCOE serviceForCOE)
	{
		long? userId = null;

		try
		{
			userId = serviceForCOE?.CurrentlyLoggedInUser?.Id;
		}
		catch { }

		if(userId == null)
		{
			await _next(context);
		}
		else
		{
			// Push the user ID to the Serilog context
			using(LogContext.PushProperty("UserId", userId))
			{
				await _next(context);
			}
		}
	}
}

// Extension method for adding the middleware
public static class SerilogUserContextMiddlewareExtensions
{
	public static IApplicationBuilder UseSerilogUserContext(
		this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<SerilogUserContextMiddleware>();
	}
}