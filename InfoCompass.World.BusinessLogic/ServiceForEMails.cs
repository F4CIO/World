using System.Net.Mail;

namespace InfoCompass.World.BusinessLogic;

public interface IServiceForEMails
{
	Task SendAfterGenerateInitial(City job);
}

public sealed class ServiceForEMails:IServiceForEMails
{
	readonly ServiceForCOE _c;
	readonly IServiceForCities _serviceForCities;

	public ServiceForEMails(ServiceForCOE c, IServiceForCities serviceForCities)
	{
		_c = c;
		_serviceForCities = serviceForCities;
	}

	public async Task SendAfterGenerateInitial(City job)
	{
		var eventHandler = new SendCompletedEventHandler((sender, e) =>
		{
			var job = (City)e.UserState;

			if(e.Error != null)
			{
				//var deepestError = CraftSynth.BuildingBlocks.Common.Misc.GetDeepestException(e.Error);
				throw new Exception($"Error: {e.Error.Message}", e.Error);
			}
			else if(e.Cancelled)
			{
				//log?.AddLine("Send canceled.");
			}
			else
			{
				//log?.AddLine($"Mail sent successfully to {job.UserEMailHint}.");
			}
		});

	}
}

public class MockedServiceForEMails:IServiceForEMails
{
	readonly ServiceForCOE _c;

	public MockedServiceForEMails(ServiceForCOE c)
	{
		_c = c;
	}

	public Task SendAfterGenerateInitial(City job)
	{
		throw new NotImplementedException();
	}
}