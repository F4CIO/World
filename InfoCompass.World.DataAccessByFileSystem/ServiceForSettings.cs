using InfoCompass.World.DataAccessContracts;

namespace InfoCompass.World.DataAccessByFileSystem;

public sealed class ServiceForSettings:ServiceForEntities, IServiceForSettings
{
	private readonly ServiceForCOE _c;

	private static Settings _settings;

	public ServiceForSettings(ServiceForCOE c) : base(c)
	{
		_c = c;
	}

	public async Task<Settings> GetCachedOrFromDbForUserDefault()
	{
		Settings r;
		if(_settings == null)
		{
			List<Settings> rr = await this.Get<Settings>(s => s.UserId == 1);
			_settings = rr.Single();
		}
		r = _settings;
		return r;
	}

	public async Task<Settings> GetByUserId(long userId)
	{
		Settings r = (await this.Get<Settings>(s => s.UserId == userId)).SingleOrDefault();
		return r;
	}
}

public class MockedServiceForSettingss:ServiceForEntities, IServiceForSettings
{
	private readonly ServiceForCOE _c;

	public MockedServiceForSettingss(ServiceForCOE c) : base(c)
	{
		_c = c;
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