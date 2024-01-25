namespace InfoCompass.World.BusinessLogic;

public interface IServiceForJwts
{
	Task<Jwt?> GetByUserId(long userId);
	Task<Jwt> Create(long userId, string rawData, DateTime momentOfExpirationAsUts);
	Task<bool> IsPresentAndActive(string rawData);
	Task<Jwt?> Deactivate(string rawData, JwtReasonForDeactivation jwtReasonForDeactivation, bool throwExceptionIfNotFound);
	Task<List<Jwt>> DeactivateAllTokens(long userId, JwtReasonForDeactivation jwtReasonForDeactivation, bool throwExceptionIfNotFound);
}

public sealed class ServiceForJwts:InfoCompass.World.BusinessLogic.ServiceBase, IServiceForJwts
{
	readonly ServiceForCOE _c;
	readonly InfoCompass.World.DataAccessContracts.IServiceForJwts _serviceForJwts;

	public ServiceForJwts(ServiceForCOE c, IServiceForLogs serviceForLogs, InfoCompass.World.DataAccessContracts.IServiceForUsers serviceForUsers, InfoCompass.World.DataAccessContracts.IServiceForJwts serviceForJwts)
		: base(c, serviceForLogs, serviceForUsers)
	{
		_c = c;
		_serviceForJwts = serviceForJwts;
	}

	public async Task<Jwt?> GetByUserId(long userId)
	{
		Jwt? r = null;

		try
		{
			r = (await _serviceForJwts.Get<Jwt>(j => j.UserId == userId)).FirstOrDefault();
		}
		catch(Exception e)
		{
			await this.HandleExceptionOnThisLayer(e, $"Error occurred while executing GetByUserId({userId})");
		}

		return r;
	}

	public async Task<Jwt> Create(long userId, string rawData, DateTime momentOfExpirationAsUts)
	{
		DateTime now = DateTime.Now;
		var r = new Jwt()
		{
			UserId = userId,
			MomentOfCreation = now,
			MomentOfLastUpdate = now,
			RawData = rawData,
			MomentOfExpirationAsUtc = momentOfExpirationAsUts,
			IsActive = true,
			ReasonForDeactivation = null
		};
		r = await _serviceForJwts.Insert(r);

		return r;
	}

	public async Task<bool> IsPresentAndActive(string rawData)
	{
		bool r;
		Jwt? jwt = null;

		//using(_c.LogBeginScope($"Checking token '{rawData.Bubble(10, "...")}' for presence and active state"))
		{
			jwt = (await _serviceForJwts.Get<Jwt>(j => j.RawData == rawData)).FirstOrDefault();
			if(jwt != null && jwt.IsActive && DateTime.Compare(DateTime.UtcNow, jwt.MomentOfExpirationAsUtc) >= 0)
			{
				jwt = await this.Deactivate(rawData, JwtReasonForDeactivation.Expired, true);
			}
			r = jwt != null && jwt.IsActive;
		}

		return r;
	}

	public async Task<Jwt?> Deactivate(string rawData, JwtReasonForDeactivation jwtReasonForDeactivation, bool throwExceptionIfNotFound)
	{
		Jwt? jwt = null;

		using(_c.LogBeginScope($"Deactivating token '{rawData.Bubble(10, "...")}' with reason {jwtReasonForDeactivation}"))
		{
			jwt = (await _serviceForJwts.Get<Jwt>(j => j.RawData == rawData)).FirstOrDefault();
			if(jwt == null)
			{
				if(throwExceptionIfNotFound)
				{
					throw new Exception($"Jwt '{rawData.Bubble(10, "...")}' not found.");
				}
			}
			else
			{
				jwt.IsActive = false;
				jwt.ReasonForDeactivation = jwtReasonForDeactivation.ToString();
				jwt.MomentOfLastUpdate = DateTime.Now;
				await _serviceForJwts.Update(jwt);

				string message = $"Deactivated token '{rawData.Bubble(10, "...")}' with reason {jwtReasonForDeactivation}";
				long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__Jwt_Deactivated, _c.CurrentlyLoggedInUser, message, LogExtraColumnNames.JwtId, jwt.Id);
				_c.Log(message + $"For more details see log with id={logId}");
			}
		}

		return jwt;
	}

	public async Task<List<Jwt>> DeactivateAllTokens(long userId, JwtReasonForDeactivation jwtReasonForDeactivation, bool throwExceptionIfNotFound)
	{
		var jwts = new List<Jwt>();

		using(_c.LogBeginScope($"Deactivating all tokens for user with id='{userId}' with reason {jwtReasonForDeactivation}"))
		{
			jwts = (await _serviceForJwts.Get<Jwt>(j => j.UserId == userId));
			if(jwts.Count == 0)
			{
				if(throwExceptionIfNotFound)
				{
					throw new Exception($"Jwts for user with id '{userId}' not found.");
				}
			}
			else
			{
				foreach(Jwt jwt in jwts)
				{
					jwt.IsActive = false;
					jwt.ReasonForDeactivation = jwtReasonForDeactivation.ToString();
					jwt.MomentOfLastUpdate = DateTime.Now;
					await _serviceForJwts.Update(jwt);

					string message = $"Deactivated token '{jwt.RawData.Bubble(10, "...")}' for user with id {userId} with reason {jwtReasonForDeactivation}";
					long logId = await _serviceForLogs.Write(LogCategoryAndTitle.UserAction__Jwt_Deactivated, _c.CurrentlyLoggedInUser, message, LogExtraColumnNames.JwtId, jwt.Id, LogExtraColumnNames.UserId, userId);
					_c.Log(message + $"For more details see log with id={logId}");
				}
			}
		}

		return jwts;
	}
}

public class MockedServiceForJwts:IServiceForJwts
{
	readonly ServiceForCOE _c;
	readonly InfoCompass.World.DataAccessContracts.IServiceForJwts _serviceForJwts;

	public MockedServiceForJwts(ServiceForCOE c, InfoCompass.World.DataAccessContracts.IServiceForJwts serviceForJwts)
	{
		_c = c;
		_serviceForJwts = serviceForJwts;
	}

	public Task<Jwt> Create(long userId, string rawData, DateTime momentOfExpirationAsUts)
	{
		throw new NotImplementedException();
	}

	public Task<Jwt?> Deactivate(string rawData, JwtReasonForDeactivation jwtReasonForDeactivation, bool throwExceptionIfNotFound)
	{
		throw new NotImplementedException();
	}

	public Task<List<Jwt>> DeactivateAllTokens(long userId, JwtReasonForDeactivation jwtReasonForDeactivation, bool throwExceptionIfNotFound)
	{
		throw new NotImplementedException();
	}

	public Task<Jwt?> GetByUserId(long userId)
	{
		throw new NotImplementedException();
	}

	public Task<bool> IsPresentAndActive(string rawData)
	{
		throw new NotImplementedException();
	}
}