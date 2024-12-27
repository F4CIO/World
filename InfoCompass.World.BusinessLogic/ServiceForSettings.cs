namespace MyCompany.World.BusinessLogic;

public interface IServiceForSettings
{
	Task<Settings> GetCachedOrFromDbForUserDefault();
	Task<Settings> GetByUserId(long userId);
}

public sealed class ServiceForSettings:IServiceForSettings
{
	//readonly ServiceForCOE _c;
	readonly MyCompany.World.DataAccessContracts.IServiceForSettings _serviceForSettings;

	public ServiceForSettings(/*ServiceForCOE c, */MyCompany.World.DataAccessContracts.IServiceForSettings serviceForSettings)
	{
		//_c = c;
		_serviceForSettings = serviceForSettings;
	}

	public async Task<Settings> GetCachedOrFromDbForUserDefault()
	{
		Settings r = await _serviceForSettings.GetCachedOrFromDbForUserDefault();
		return r;
	}

	public async Task<Settings> GetByUserId(long userId)
	{
		Settings r = await _serviceForSettings.GetByUserId(userId);
		return r;
	}
}

public class MockedServiceForSettings:IServiceForSettings
{
	readonly MyCompany.World.DataAccessContracts.IServiceForSettings _serviceForSettings;

	public MockedServiceForSettings(MyCompany.World.DataAccessContracts.IServiceForSettings serviceForSettings)
	{
		_serviceForSettings = serviceForSettings;
	}

	public Task<Settings> GetCachedOrFromDbForUserDefault()
	{
		throw new NotImplementedException();
	}

	public Task<Settings> GetByUserId(long userId)
	{
		throw new NotImplementedException();
	}
}