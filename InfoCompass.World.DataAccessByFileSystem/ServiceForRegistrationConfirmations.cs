using InfoCompass.World.DataAccessContracts;

namespace InfoCompass.World.DataAccessByFileSystem;

public sealed class ServiceForRegistrationConfirmations:ServiceForEntities, IServiceForRegistrationConfirmations
{
	public ServiceForRegistrationConfirmations(ServiceForCOE c) : base(c)
	{
	}
}

public class MockedServiceForRegistrationConfirmations:ServiceForEntities, IServiceForRegistrationConfirmations
{
	public MockedServiceForRegistrationConfirmations(ServiceForCOE c) : base(c)
	{
		throw new NotImplementedException();
	}

	public async Task<List<RegistrationConfirmation>> Get(RegistrationConfirmation logFilter)
	{
		throw new NotImplementedException();
	}

	public async Task<List<RegistrationConfirmation>> Get(Func<RegistrationConfirmation, bool> filter)
	{
		throw new NotImplementedException();
	}

	public async Task<RegistrationConfirmation> Insert(RegistrationConfirmation log)
	{
		throw new NotImplementedException();
	}

	public async Task Update(RegistrationConfirmation log)
	{
		throw new NotImplementedException();
	}
}