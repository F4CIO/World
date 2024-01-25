namespace InfoCompass.World.BusinessLogic;

public interface IServiceForCities
{
	Task<ServerResponse<List<City>>> GetByUserId(long userId);

	Task<ServerResponse<List<City>>> SaveAll(List<City> cities);
	Task UpdateCityIntoDb(City job);
}

public sealed class ServiceForCities:InfoCompass.World.BusinessLogic.ServiceBase, IServiceForCities
{
	private readonly InfoCompass.World.DataAccessContracts.IServiceForCities _serviceForCities;
	public ServiceForCities(ServiceForCOE c, IServiceForLogs serviceForLogs, InfoCompass.World.DataAccessContracts.IServiceForUsers serviceForUsers, InfoCompass.World.DataAccessContracts.IServiceForCities serviceForCities)
		: base(c, serviceForLogs, serviceForUsers)
	{
		_serviceForCities = serviceForCities;
	}

	public async Task<ServerResponse<List<City>>> GetByUserId(long userId)
	{
		ServerResponse<List<City>> r = null;

		try
		{
			//User jobUser = await _serviceForUsers.GetById<User>(userId);
			//if(jobUser == null)
			//{
			//	r = new ServerResponse<List<City>>(false, $"User with id {userId} not found.", null, null, null, null, System.Net.HttpStatusCode.NotFound);
			//}
			//else if(jobUser.IsRegistered && _c?.CurrentlyLoggedInUser?.Id != userId)
			//{
			//	r = new ServerResponse<List<City>>(false, "Not authorized", null, null, null, null, System.Net.HttpStatusCode.Unauthorized);
			//}
			//else
			{
				List<City> jobs = await _serviceForCities.Get<City>(j => j.UserId == userId);
				if(jobs != null)
				{

					jobs = jobs.OrderBy(j => j.MomentOfCreation.Ticks).ToList();
				}
				r = new ServerResponse<List<City>>(true, "Ok", null, jobs, null, null, System.Net.HttpStatusCode.OK);
			}
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing GetByUserId({userId})");
		}

		return r;
	}

	public async Task<ServerResponse<List<City>>> SaveAll(List<City> cities)
	{
		ServerResponse<List<City>> r = null;

		try
		{
			List<City> jobs = await _serviceForCities.SaveAll1<City>(cities);
			r = new ServerResponse<List<City>>(true, "Ok", null, jobs, null, null, System.Net.HttpStatusCode.OK);
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing SaveAll(...)");
		}

		return r;
	}


	public async Task UpdateCityIntoDb(City job)
	{
		job.MomentOfLastUpdate = DateTime.Now;

	}
}

public class MockedServiceForJobs:IServiceForCities
{
	readonly ServiceForCOE _c;
	readonly InfoCompass.World.DataAccessContracts.IServiceForUsers _serviceForUsers;

	public MockedServiceForJobs(ServiceForCOE c, InfoCompass.World.DataAccessContracts.IServiceForUsers serviceForUsers, InfoCompass.World.DataAccessContracts.IServiceForCities serviceForCities)
	{
		_c = c;
		_serviceForUsers = serviceForUsers;
	}

	public Task<ServerResponse<List<City>>> GetByUserId(long userId)
	{
		throw new NotImplementedException();
	}

	public Task<ServerResponse<List<City>>> SaveAll(List<City> cities)
	{
		throw new NotImplementedException();
	}

	public Task UpdateCityIntoDb(City job)
	{
		throw new NotImplementedException();
	}
}