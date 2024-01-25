//using Microsoft.Extensions.Options;

namespace InfoCompass.World.DataAccessContracts;

public interface IServiceForLogs:IServiceForEntities
{
	Task Create();
}