using Microsoft.Extensions.Configuration;

namespace MyCompany.World.BusinessLogic;

public class CompositionRoot
{
	public static void Compose(Microsoft.Extensions.Hosting.HostBuilderContext context, IServiceCollection services, bool isTesting = false)
	{
		Configuration? configuration = context.Configuration.GetSection("Main").Get<Configuration>();

		//this is the only point to change when changing underlying data provider 
		MyCompany.World.DataAccessByFileSystem.CompositionRoot.Compose(context, services, isTesting);

		if(isTesting)
		{
			services.AddScoped<IServiceForUsers, MyCompany.World.BusinessLogic.MockedServiceForUsers>();
			services.AddScoped<IServiceForLogs, MyCompany.World.BusinessLogic.MockedServiceForLogs>();
			services.AddScoped<IServiceForJwts, MyCompany.World.BusinessLogic.MockedServiceForJwts>();
			services.AddScoped<MyCompany.World.BusinessLogic.IServiceForSettings, MyCompany.World.BusinessLogic.ServiceForSettings>();
			//services.AddOpenAIService(s => { s.ApiKey = settings.OpenAiApiKey; s.ApiRequestTimeout = TimeSpan.FromMinutes(settings.OpenAiRequestTimeoutInMinutes);});	
			services.AddScoped<MyCompany.World.BusinessLogic.IServiceForEMails, MyCompany.World.BusinessLogic.MockedServiceForEMails>();
			services.AddScoped<MyCompany.World.BusinessLogic.IServiceForCities, MyCompany.World.BusinessLogic.MockedServiceForJobs>();
		}
		else
		{
			services.AddScoped<IServiceForUsers, ServiceForUsers>();
			services.AddScoped<IServiceForLogs, ServiceForLogs>();
			services.AddScoped<IServiceForJwts, ServiceForJwts>();
			//s =>
			//{
			//	var settins = s.GetRequiredService<InfoCompass.World.DataAccessContracts.IServiceForSettings>();
			//	return new ServiceForBetalgoOpenAI(s, context.Configuration.GetSection("Main").Get<IOptions<Configuration>>(), settins, new OpenAIService())
			//});				
			services.AddScoped<MyCompany.World.BusinessLogic.IServiceForSettings, MyCompany.World.BusinessLogic.ServiceForSettings>();
			//services.AddOpenAIService(s =>{	s.ApiKey = settings2.OpenAiApiKey; s.ApiRequestTimeout = TimeSpan.FromMinutes(settings2.OpenAiRequestTimeoutInMinutes);});		
			//
			services.AddScoped<MyCompany.World.BusinessLogic.IServiceForEMails, MyCompany.World.BusinessLogic.ServiceForEMails>();
			services.AddScoped<MyCompany.World.BusinessLogic.IServiceForCities, MyCompany.World.BusinessLogic.ServiceForCities>();
		}
	}
}
