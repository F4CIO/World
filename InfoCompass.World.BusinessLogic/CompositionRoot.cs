using Microsoft.Extensions.Configuration;

namespace InfoCompass.World.BusinessLogic;

public class CompositionRoot
{
	public static void Compose(Microsoft.Extensions.Hosting.HostBuilderContext context, IServiceCollection services, bool isTesting = false)
	{
		Configuration? configuration = context.Configuration.GetSection("Main").Get<Configuration>();

		//this is the only point to change when changing underlying data provider 
		InfoCompass.World.DataAccessByFileSystem.CompositionRoot.Compose(context, services, isTesting);

		if(isTesting)
		{
			services.AddScoped<IServiceForUsers, InfoCompass.World.BusinessLogic.MockedServiceForUsers>();
			services.AddScoped<IServiceForLogs, InfoCompass.World.BusinessLogic.MockedServiceForLogs>();
			services.AddScoped<IServiceForJwts, InfoCompass.World.BusinessLogic.MockedServiceForJwts>();
			services.AddScoped<InfoCompass.World.BusinessLogic.IServiceForSettings, InfoCompass.World.BusinessLogic.ServiceForSettings>();
			//services.AddOpenAIService(s => { s.ApiKey = settings.OpenAiApiKey; s.ApiRequestTimeout = TimeSpan.FromMinutes(settings.OpenAiRequestTimeoutInMinutes);});	
			services.AddScoped<InfoCompass.World.BusinessLogic.IServiceForEMails, InfoCompass.World.BusinessLogic.MockedServiceForEMails>();
			services.AddScoped<InfoCompass.World.BusinessLogic.IServiceForCities, InfoCompass.World.BusinessLogic.MockedServiceForJobs>();
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
			services.AddScoped<InfoCompass.World.BusinessLogic.IServiceForSettings, InfoCompass.World.BusinessLogic.ServiceForSettings>();
			//services.AddOpenAIService(s =>{	s.ApiKey = settings2.OpenAiApiKey; s.ApiRequestTimeout = TimeSpan.FromMinutes(settings2.OpenAiRequestTimeoutInMinutes);});		
			//
			services.AddScoped<InfoCompass.World.BusinessLogic.IServiceForEMails, InfoCompass.World.BusinessLogic.ServiceForEMails>();
			services.AddScoped<InfoCompass.World.BusinessLogic.IServiceForCities, InfoCompass.World.BusinessLogic.ServiceForCities>();
		}
	}
}
