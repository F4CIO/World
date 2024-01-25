using System.Diagnostics;
using InfoCompass.World.BusinessLogic;
using InfoCompass.World.UiWebApi.Logic;
using InfoCompass.World.UiWebApi.Middleware;
using Serilog;

namespace InfoCompass.World.UiWebApi;

public class Program
{
	public static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		builder.Host.ConfigureAppConfiguration((context, config) =>
		{
			//source: https://blog.christian-schou.dk/how-to-use-ioptions-to-bind-configurations
			IHostEnvironment env = context.HostingEnvironment;
			config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"InfoCompass.World.json", optional: false, reloadOnChange: true)
				.AddJsonFile($"InfoCompass.World.json.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();
		});

		builder.Host.ConfigureServices((context, services) =>
		{
			CompositionRoot.Compose(context, services, false);
			builder.Services.AddControllers();
			builder.Services.AddEndpointsApiExplorer();// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddSwaggerGen();
		});

		Configuration? configuration = builder.Configuration.GetSection("Main").Get<Configuration>();
		string allowedOriginsCsv = configuration.AllowedOriginsCsv;
		string[] allowedOrigins = allowedOriginsCsv.ParseCSV(new char[] { ',' }, new char[] { ' ' }, false, null, null, new List<string>()).ToArray();
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("MyAllowSpecificOrigins",
							  policy =>
							  {
								  policy.WithOrigins(allowedOrigins)
										.AllowAnyHeader()
										.AllowAnyMethod();
							  });
		});

		WebApplication app = builder.Build();

		ServiceForLogSink_FilePerUserPerJob serviceForLogSink_FilePerUserPerJob = app.Services.GetRequiredService<ServiceForLogSink_FilePerUserPerJob>();
		ServiceForLogSink_Db serviceForLogSink_Db = app.Services.GetRequiredService<ServiceForLogSink_Db>();
		Log.Logger = new LoggerConfiguration()
			.Enrich.FromLogContext()
			.WriteTo.Console()
			.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // Adjust this to filter out noise
			.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
			.WriteTo.Sink(serviceForLogSink_FilePerUserPerJob)
			.WriteTo.Sink(serviceForLogSink_Db)
			.CreateLogger();

		using(IServiceScope scope = app.Services.CreateScope())
		{//test at startup that all services are resolvable:
			IHttpContextAccessor httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
			ServiceForCOE serviceForCOE = scope.ServiceProvider.GetRequiredService<ServiceForCOE>();
			IServiceForLogs serviceForLogs = scope.ServiceProvider.GetRequiredService<InfoCompass.World.BusinessLogic.IServiceForLogs>();
			ServiceForCOE serviceForC = scope.ServiceProvider.GetRequiredService<ServiceForCOE>();
			IServiceForBase serviceForBase = scope.ServiceProvider.GetRequiredService<IServiceForBase>();
			IServiceForEMails serviceForEMails = scope.ServiceProvider.GetRequiredService<IServiceForEMails>();
			IServiceForUsers serviceForUsers = scope.ServiceProvider.GetRequiredService<IServiceForUsers>();

			using(serviceForC.LogBeginScope("sss11"))
			{
				using(serviceForC.LogBeginScope("sss22"))
				{
					serviceForC.Log("aaaa");
					serviceForC.Log(LogCategoryAndTitle.InternalEvent__Web_Site_Started, CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath);
				}
			}
		}

		Microsoft.AspNetCore.Builder.SwaggerBuilderExtensions.UseSwagger(app);
		app.UseSwaggerUI();

		app.UseCors("MyAllowSpecificOrigins");

		app.UseAuthentication();
		app.UseAuthorization();
		app.UseSerilogUserContext();

		app.MapControllers();

		//app.MapGenerateInitialRequestEndpoints();

		if(Debugger.IsAttached)
		{
			app.UseDeveloperExceptionPage();
		}

		app.Run();
	}
}