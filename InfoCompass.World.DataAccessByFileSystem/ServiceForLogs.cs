using MyCompany.World.DataAccessContracts;

namespace MyCompany.World.DataAccessByFileSystem;

public sealed class ServiceForLogs:ServiceForEntities, IServiceForLogs
{
	private readonly ServiceForCOE _c;

	public ServiceForLogs(ServiceForCOE c) : base(c)
	{
		_c = c;
	}

	/// <summary>
	/// TODO: is it needed?
	/// </summary>
	/// <returns></returns>
	public async Task Create()
	{

	}
}

public class MockedServiceForLogs:ServiceForEntities, IServiceForLogs
{
	private readonly ServiceForCOE _c;

	public MockedServiceForLogs(ServiceForCOE c) : base(c)
	{
		_c = c;

		throw new NotImplementedException();
	}

	public Task Create()
	{
		throw new NotImplementedException();
	}

	public async Task<List<LogEntry>> Get(LogEntry logFilter)
	{
		throw new NotImplementedException();
	}

	public async Task<List<LogEntry>> Get(Func<LogEntry, bool> filter)
	{
		throw new NotImplementedException();
	}

	public async Task<LogEntry> Insert(LogEntry log)
	{
		throw new NotImplementedException();
	}

	public async Task Update(LogEntry log)
	{
		throw new NotImplementedException();
	}
}