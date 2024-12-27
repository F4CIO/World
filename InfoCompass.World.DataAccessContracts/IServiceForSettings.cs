//using Microsoft.Extensions.Options;

namespace MyCompany.World.DataAccessContracts;

public interface IServiceForSettings
{
	public Task<Settings> GetCachedOrFromDbForUserDefault();
	public Task<Settings> GetByUserId(long userId);
}