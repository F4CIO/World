using InfoCompass.World.BusinessLogic;
using InfoCompass.World.Public.Api;
using InfoCompass.World.UiWebApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfoCompass.World.UiWebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CitiesController:BaseController
{
	private readonly ServiceForCOE _c;
	private readonly IServiceForCities _serviceForCities;

	public CitiesController(ServiceForCOE c, IServiceForCities serviceForCities)
	{
		_c = c;
		_serviceForCities = serviceForCities;
	}

	//// GET: api/<CitiesController>
	//[HttpGet]
	//public IEnumerable<string> Get()
	//{
	//	return new string[] { "value1", "value2" };
	//}

	//// GET api/<CitiesController>/5
	//[HttpGet("{id}")]
	//public string Get(int id)
	//{
	//	return "value";
	//}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="value">
	/// {
	///  "firstName": "Nenad",
	///  "lastName": "Curcin",
	///  "bookTitle": "10 interesting things from every country",
	///  "chaptersCount": 5,
	///  "bookLanguage": "En",
	///  "exportTypesCsv": "Pdf",
	///  "iAgreeToTerms": true,
	///  "subscribeMe": true,
	///  "eMail": "f4cio1@gmail.com"
	/// }
	/// </param>
	/// <returns></returns>
	[HttpPost]
	[Route("GetByUserId")]
	[AllowAnonymous]//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
	public async Task<ServerResponseForUI<List<City>>> GetByUserId(int id)
	{
		ServerResponseForUI<List<City>> r;

		using(_c.LogBeginScope($"Executing api call GetByUserId on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--
				ServerResponse<List<City>> serverResponse = await _serviceForCities.GetByUserId(id);
				r = new ServerResponseForUI<List<City>>(_c, serverResponse);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<List<City>>(ex);
			}
		}

		return r;
	}

	[HttpPost]
	[Route("SaveAll")]
	[AllowAnonymous]//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme + "," + CookieAuthenticationDefaults.AuthenticationScheme)]
	public async Task<ServerResponseForUI<List<City>>> SaveAll([FromBody] List<City> cities)
	{
		ServerResponseForUI<List<City>> r;

		using(_c.LogBeginScope($"Executing api call SaveAll on url:{this.CurrentRequestUri}... "))
		{
			try
			{
				//---api method body--
				ServerResponse<List<City>> serverResponse = await _serviceForCities.SaveAll(cities);
				r = new ServerResponseForUI<List<City>>(_c, serverResponse);
				//--------------------
			}
			catch(Exception ex)
			{
				r = await this.HandleExceptionOnThisLayer<List<City>>(ex);
			}
		}

		return r;
	}


	//// POST api/<CitiesController>
	//[HttpPost]
	//public void Post([FromBody] string value)
	//{		
	//}

	//// PUT api/<CitiesController>/5
	//[HttpPut("{id}")]
	//public void Put(int id, [FromBody] string value)
	//{
	//}

	//// DELETE api/<CitiesController>/5
	//[HttpDelete("{id}")]
	//public void Delete(int id)
	//{
	//}
}
