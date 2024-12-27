using MyCompany.World.DataAccessContracts;

namespace MyCompany.World.DataAccessByFileSystem;

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