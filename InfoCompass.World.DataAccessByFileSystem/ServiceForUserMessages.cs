using InfoCompass.World.DataAccessContracts;

namespace InfoCompass.World.DataAccessByFileSystem;

public sealed class ServiceForUserMessages:ServiceForEntities, IServiceForUserMessages
{
	public ServiceForUserMessages(ServiceForCOE c) : base(c)
	{
	}
}

public class MockedServiceForUserMessages:ServiceForEntities, IServiceForUserMessages
{
	public MockedServiceForUserMessages(ServiceForCOE c) : base(c)
	{
		throw new NotImplementedException();
	}

	public async Task<List<UserMessage>> Get(UserMessage logFilter)
	{
		throw new NotImplementedException();
	}

	public async Task<List<UserMessage>> Get(Func<UserMessage, bool> filter)
	{
		throw new NotImplementedException();
	}

	public async Task<UserMessage> Insert(UserMessage log)
	{
		throw new NotImplementedException();
	}

	public async Task Update(UserMessage log)
	{
		throw new NotImplementedException();
	}
}