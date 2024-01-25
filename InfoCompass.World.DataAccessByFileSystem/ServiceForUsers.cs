using InfoCompass.World.DataAccessContracts;

namespace InfoCompass.World.DataAccessByFileSystem;

public sealed class ServiceForUsers:ServiceForEntities, IServiceForUsers
{
	private readonly ServiceForCOE _c;
	public ServiceForUsers(ServiceForCOE c) : base(c)
	{
		_c = c;
	}

	public async Task<User> GetByEMail(string eMail)
	{
		List<User> rr = await this.Get<User>(s => string.Compare(s.EMail, eMail, StringComparison.OrdinalIgnoreCase) == 0);
		User r = rr.SingleOrDefault();
		return r;
	}
}

public class MockedServiceForUsers:ServiceForEntities, IServiceForUsers
{
	private readonly ServiceForCOE _c;

	public MockedServiceForUsers(ServiceForCOE c) : base(c)
	{
		_c = c;

		throw new NotImplementedException();
	}

	public async Task<List<User>> Get(User userFilter)
	{
		throw new NotImplementedException();
	}

	public async Task<List<User>> Get(Func<User, bool> filter)
	{
		throw new NotImplementedException();
	}

	public Task<User> GetByEMail(string eMail)
	{
		throw new NotImplementedException();
	}

	public async Task<User> Insert(User user)
	{
		throw new NotImplementedException();
	}

	public async Task Update(User user)
	{
		throw new NotImplementedException();
	}
}