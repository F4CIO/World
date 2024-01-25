using System.Diagnostics;
using InfoCompass.World.Public.Api;
using Microsoft.AspNetCore.Mvc;

namespace InfoCompass.World.UiWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OtherController:BaseController
{
	private readonly ServiceForCOE _c;

	public OtherController(ServiceForCOE c)
	{
		_c = c;
	}

	//// GET: api/<BooksController>
	//[HttpGet]
	//public IEnumerable<string> Get()
	//{
	//	return new string[] { "value1", "value2" };
	//}

	//// GET api/<BooksController>/5
	//[HttpGet("{id}")]
	//public string Get(int id)
	//{
	//	return "value";
	//}

	/// <summary>
	/// Runs RegenerateApiClientGeneratedTsFile.bat which re-creates InfoCompass.World.UiAngular\src\app\shared\api-client.generated.ts
	/// That .bat must be run while this api is running.
	/// Check actual .bat because it has absolute path to api-client.generated.ts
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	[Route("RegenerateApiClientGeneratedTsFile")]
	//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
	public string RegenerateApiClientGeneratedTsFile()
	{
		string r;

		//using(_c.LogBeginScope($"Executing api call GetByUserId on url:{this.CurrentRequestUri}... "))
		//{
		//try
		//{
		//---api method body--
		if(Debugger.IsAttached)
		{
			string jsonUri = $"{CraftSynth.BuildingBlocks.UI.Web.UriHandler.GetProtocol(this.CurrentRequestUri)}://{CraftSynth.BuildingBlocks.UI.Web.UriHandler.GetAuthority(this.CurrentRequestUri, false)}/swagger/v1/swagger.json";

			string solutionFolderPath = CraftSynth.BuildingBlocks.Common.Misc.ApplicationRootFolderPath;
			solutionFolderPath = Path.GetDirectoryName(solutionFolderPath);
			solutionFolderPath = Path.GetDirectoryName(solutionFolderPath);
			solutionFolderPath = Path.GetDirectoryName(solutionFolderPath);
			solutionFolderPath = Path.GetDirectoryName(solutionFolderPath);
			string? targetFilePath = CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths(solutionFolderPath, true, "api-client.generated.ts").FirstOrDefault();//should find something like: d:\\Projects\\InfoCompass.World\\SourceCode\\InfoCompass.World.UiAngular\\src\\app\\shared\\api-client.generated.ts
			if(targetFilePath == null)
			{
				throw new Exception($"Could not find api-client.generated.ts in {solutionFolderPath}");
			}

			string nswagFilePath = CraftSynth.BuildingBlocks.IO.FileSystem.GetFilePaths("c:\\Users", true, "nswag.exe", true).FirstOrDefault();//should find something like: C:\Users\Administrator\.dotnet\tools\nswag.exe
			if(targetFilePath == null)
			{
				throw new Exception($"Could not find nswag.exe in c:\\Users");
			}

			//for this nswag command to work you initially need to install tool with this line:  dotnet tool install -g NSwag.ConsoleCore
			string arguments = $"openapi2tsclient /input:{jsonUri} /output:{targetFilePath} /template:\"angular\" /useHttpClient:true";
			string command = nswagFilePath + " " + arguments;
			int exitCode = -1;
			try
			{
				exitCode = CraftSynth.BuildingBlocks.UI.Console.ExecuteCommand(nswagFilePath, arguments, 5000);
			}
			catch
			{
				throw;
			}
			if(exitCode != 0)
			{
				throw new Exception("exitCode is not 0");
			}

			string content = System.IO.File.ReadAllText(targetFilePath);
			content = $"//Generation moment:{DateTime.Now.ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS()}\r\n" + content;
			content = content.Replace("OpaqueToken", "InjectionToken");//fix for latest angular version 
			content = content.Replace("this.baseUrl = baseUrl !== undefined && baseUrl !== null ? baseUrl : \"\";", "this.baseUrl = environment.API_BASE_URL;");//insure base url is read from environment.ts file
			content = content.Replace("export const API_BASE_URL = new InjectionToken('API_BASE_URL');", "import { environment } from 'src/environments/environment';\r\n\r\nexport const API_BASE_URL = new InjectionToken('API_BASE_URL');");//insure base url is read from environment.ts file			
			content = content.Replace("@Injectable()", "@Injectable({\r\n  providedIn: 'root'\r\n})");
			content = content.Replace("message: string;", "//message: string;");//some dummy error 
			System.IO.File.WriteAllText(targetFilePath, content);
			r = $"Ok. File that was regenerated: {targetFilePath} Command executed: {command}";
		}
		else
		{
			r = "This endpoint is for internal use only. (No debugger attached)";
		}
		//--------------------
		//}
		//catch(Exception ex)
		//{
		//	//r = await this.HandleExceptionOnThisLayer<List<BookGenerationJob>>(ex);
		//}
		//}

		return r;
	}


	//// POST api/<BooksController>
	//[HttpPost]
	//public void Post([FromBody] string value)
	//{		
	//}

	//// PUT api/<BooksController>/5
	//[HttpPut("{id}")]
	//public void Put(int id, [FromBody] string value)
	//{
	//}

	//// DELETE api/<BooksController>/5
	//[HttpDelete("{id}")]
	//public void Delete(int id)
	//{
	//}
}
