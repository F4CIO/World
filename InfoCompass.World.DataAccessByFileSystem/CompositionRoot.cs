using InfoCompass.World.DataAccessContracts;
using Microsoft.Extensions.Configuration;

namespace InfoCompass.World.DataAccessByFileSystem;

public class CompositionRoot
{
	public static void Compose(Microsoft.Extensions.Hosting.HostBuilderContext context, IServiceCollection services, bool isTesting = false)
	{
		Configuration? configuration = context.Configuration.GetSection("Main").Get<Configuration>();

		if(isTesting)
		{
			services.AddScoped<IServiceForEntities, MockedServiceForEntities>();
			services.AddScoped<IServiceForSettings, MockedServiceForSettingss>();
			services.AddScoped<IServiceForLogs, MockedServiceForLogs>();
			services.AddScoped<IServiceForUsers, MockedServiceForUsers>();
			services.AddScoped<IServiceForJwts, MockedServiceForJwts>();
			services.AddScoped<IServiceForRegistrationConfirmations, MockedServiceForRegistrationConfirmations>();
			services.AddScoped<IServiceForCities, MockedServiceForJobs>();
			services.AddScoped<IServiceForPasswordResetRequests, MockedServiceForPasswordResetRequests>();
			services.AddScoped<IServiceForUserMessages, MockedServiceForUserMessages>();
		}
		else
		{
			services.AddScoped<IServiceForEntities, ServiceForEntities>();
			services.AddScoped<IServiceForSettings, ServiceForSettings>();
			services.AddScoped<IServiceForLogs, ServiceForLogs>();
			services.AddScoped<IServiceForUsers, ServiceForUsers>();
			services.AddScoped<IServiceForJwts, ServiceForJwts>();
			services.AddScoped<IServiceForRegistrationConfirmations, ServiceForRegistrationConfirmations>();
			services.AddScoped<IServiceForCities, ServiceForCities>();
			services.AddScoped<IServiceForPasswordResetRequests, ServiceForPasswordResetRequests>();
			services.AddScoped<IServiceForUserMessages, ServiceForUserMessages>();
		}
	}
}
