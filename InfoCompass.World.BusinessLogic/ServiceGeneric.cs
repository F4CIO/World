namespace YouTrend.BusinessLogic;

public interface GenericHandlerInterface<TEntity>
{
	UnitOfWork CreateUnitOfWork(string dbConnectionString = null);
	List<TEntity> GetBySqlQuery(string sqlQuery);
	TEntity GetById(int id);
	List<TEntity> Where<TEntity>(System.Linq.Expressions.Expression<Func<TEntity, bool>> condition, string orderBy = null, System.ComponentModel.ListSortDirection? orderDirection = null, int? count = null, int? offset = null) where TEntity : class;
	List<TEntity> GetAll<TEntity>(string orderBy = null, System.ComponentModel.ListSortDirection? orderDirection = null) where TEntity : class;
	TEntity SingleOrDefault<TEntity>(System.Linq.Expressions.Expression<Func<TEntity, bool>> condition) where TEntity : class;
	void Insert(ref TEntity entity);
	void Delete(TEntity entity);
	void Update(TEntity entity);
	void Save(ref TEntity entity);
}

//   public class GenericHandler<TEntity> : HandlerBase, GenericHandlerInterface<TEntity> where TEntity : class
//   {
//       public UnitOfWork CreateUnitOfWork(string dbConnectionString = null)
//       {
//           return YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().CreateUnitOfWork(dbConnectionString);
//       }

//	public List<TEntity> GetBySqlQuery(string sqlQuery)
//	{
//		return YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().GetBySqlQuery(sqlQuery, c);
//	}

//	public TEntity GetById(int id)
//	{
//		return YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().GetById(id, c);
//	}

//	public List<TEntity> Where<TEntity>(System.Linq.Expressions.Expression<Func<TEntity, bool>> condition, string orderBy = null, System.ComponentModel.ListSortDirection? orderDirection = null, int? count = null, int? offset = null) where TEntity : class
//	{
//		List<TEntity> entities = null;
//		entities = YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().Where<TEntity>(condition, c, orderBy, orderDirection, count, offset);
//		return entities;
//	}

//	public List<TType> Where<TEntity, TType>(System.Linq.Expressions.Expression<Func<TEntity, bool>> condition, System.Linq.Expressions.Expression<Func<TEntity, TType>> select, string orderBy = null, System.ComponentModel.ListSortDirection? orderDirection = null, int? count = null, int? offset = null) where TEntity : class where TType : class
//	{
//		List<TType> entities = null;
//		entities = YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().Where<TEntity, TType>(condition, select, c, orderBy, orderDirection, count, offset);
//		return entities;
//	}

//	public List<TEntity> GetAll<TEntity>(string orderBy = null, System.ComponentModel.ListSortDirection? orderDirection = null) where TEntity : class
//	{
//		List<TEntity> entities = null;
//		entities = YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().GetAll<TEntity>(c, orderBy, orderDirection);
//		return entities;
//	}

//	public TEntity SingleOrDefault<TEntity>(System.Linq.Expressions.Expression<Func<TEntity, bool>> condition, ContextOfExecution c = null) where TEntity : class
//	{
//		return YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().SingleOrDefault<TEntity>(condition, c);
//	}

//	public void Insert(ref TEntity entity)
//	{
//		YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().Insert(ref entity, c);
//	}

//	public void Delete(TEntity entity)
//	{
//		YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().Delete(entity, c);
//	}

//	public void Update(TEntity entity)
//	{
//		YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().Update(entity, c);
//	}

//	public void Save(ref TEntity entity)
//	{
//		YouTrend.DataRepository.HandlersFactory.GetGenericHandler<TEntity>().Save(ref entity, c);
//	}
//}
