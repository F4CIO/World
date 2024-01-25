//using Microsoft.Extensions.Options;

namespace InfoCompass.World.DataAccessContracts;

public interface IServiceForUsers:IServiceForEntities
{
	public Task<User> GetByEMail(string eMail);
}