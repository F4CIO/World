//using Microsoft.Extensions.Options;

namespace MyCompany.World.DataAccessContracts;

public interface IServiceForEntities
{
	Task<UnitOfWork> CreateUnitOfWork();
	Task<List<T>> GetAll<T>() where T : IEntity;
	Task<T> GetById<T>(long id) where T : IEntity;
	Task<List<T>> Get<T>(Func<T, bool> filter) where T : IEntity;

	Task<T> Insert<T>(T entity) where T : IEntity;

	Task Update<T>(T entity) where T : IEntity;

	Task<List<T>> SaveAll1<T>(List<T> entities) where T : IEntity;
}