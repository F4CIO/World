using InfoCompass.World.DataAccessContracts;

namespace InfoCompass.World.DataAccessByFileSystem;

public sealed class ServiceForCities:ServiceForEntities, IServiceForCities
{
	public ServiceForCities(ServiceForCOE c) : base(c)
	{
	}
}

public class MockedServiceForJobs:ServiceForEntities, IServiceForCities
{
	public MockedServiceForJobs(ServiceForCOE c) : base(c)
	{
	}
}