using MyCompany.World.DataAccessContracts;

namespace MyCompany.World.DataAccessByFileSystem;

public sealed class ServiceForJwts:ServiceForEntities, IServiceForJwts
{
	private readonly ServiceForCOE _c;

	public ServiceForJwts(ServiceForCOE c) : base(c)
	{
		_c = c;
	}
}

public class MockedServiceForJwts:ServiceForEntities, IServiceForJwts
{
	private readonly ServiceForCOE _c;

	public MockedServiceForJwts(ServiceForCOE c) : base(c)
	{
		_c = c;

		throw new NotImplementedException();
	}

	public async Task<List<Jwt>> Get(Jwt logFilter)
	{
		throw new NotImplementedException();
	}

	public async Task<List<Jwt>> Get(Func<Jwt, bool> filter)
	{
		throw new NotImplementedException();
	}

	public async Task<Jwt> Insert(Jwt log)
	{
		throw new NotImplementedException();
	}

	public async Task Update(Jwt log)
	{
		throw new NotImplementedException();
	}
}