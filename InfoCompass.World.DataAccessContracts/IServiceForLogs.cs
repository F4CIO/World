//using Microsoft.Extensions.Options;

namespace MyCompany.World.DataAccessContracts;

public interface IServiceForLogs:IServiceForEntities
{
	Task Create();
}