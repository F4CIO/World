using InfoCompass.World.DataAccessContracts;

namespace InfoCompass.World.DataAccessByFileSystem;

public sealed class ServiceForPasswordResetRequests:ServiceForEntities, IServiceForPasswordResetRequests
{
	public ServiceForPasswordResetRequests(ServiceForCOE c) : base(c)
	{
	}
}

public class MockedServiceForPasswordResetRequests:ServiceForEntities, IServiceForPasswordResetRequests
{
	public MockedServiceForPasswordResetRequests(ServiceForCOE c) : base(c)
	{
		throw new NotImplementedException();
	}

	public async Task<List<PasswordResetRequest>> Get(PasswordResetRequest logFilter)
	{
		throw new NotImplementedException();
	}

	public async Task<List<PasswordResetRequest>> Get(Func<PasswordResetRequest, bool> filter)
	{
		throw new NotImplementedException();
	}

	public async Task<PasswordResetRequest> Insert(PasswordResetRequest log)
	{
		throw new NotImplementedException();
	}

	public async Task Update(PasswordResetRequest log)
	{
		throw new NotImplementedException();
	}
}