//using Microsoft.Extensions.Options;

namespace MyCompany.World.DataAccessContracts;

public interface IServiceForUsers:IServiceForEntities
{
	public Task<User> GetByEMail(string eMail);
}