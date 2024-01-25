using CraftSynth.BuildingBlocks.Common.Patterns;
using InfoCompass.World.DataAccessContracts;

namespace InfoCompass.World.DataAccessByFileSystem;

public class ServiceForEntities:ServiceBase, IServiceForEntities
{
	#region Private Members
	#endregion

	#region Properties
	protected readonly ServiceForCOE _c;

	private const string IDS_COUNTERS_FILENAME = "idsCounters.json";
	protected string _tableName<T>() where T : IEntity
	{
		//string r = this.GetType().Name;
		//if(r.StartsWith("ServiceFor"))
		//{
		//	r = r.Substring("ServiceFor".Length);
		//}
		string r = typeof(T).Name;
		return r;
	}

	protected string _tableFilePath<T>() where T : IEntity
	{
		string r = CraftSynth.BuildingBlocks.IO.FileSystem.PathCombine(_c.Configuration.DataFolderPathAbsolute, _tableName<T>() + ".json");
		return r;
	}
	#endregion

	#region Public Methods
	/// <summary>
	/// This is a place to start transation for example
	/// </summary>
	/// <returns></returns>
	public async Task<UnitOfWork> CreateUnitOfWork()
	{
		var r = new UnitOfWork(null, null, (UnitOfWork disposingUnitOfWork) =>
		{
			//here you can commit transation for example
		});
		return r;
	}

	public async Task<List<T>> GetAll<T>() where T : IEntity
	{
		List<T> r = null;

		using(await NonparallelExecution.LockAsync(_tableFilePath<T>()))
		{
			r = await this.LoadAll<T>();
		}

		return r;
	}

	public async Task<T> GetById<T>(long id) where T : IEntity
	{
		var r = default(T);

		using(await NonparallelExecution.LockAsync(_tableFilePath<T>()))
		{
			List<T> entities = await this.LoadAll<T>();
			r = entities.SingleOrDefault(e => e.Id == id);
		}

		return r;
	}

	public async Task<List<T>> Get<T>(Func<T, bool> filter) where T : IEntity
	{
		List<T> r = null;

		using(await NonparallelExecution.LockAsync(_tableFilePath<T>()))
		{
			List<T> entities = await this.LoadAll<T>();
			r = entities.Where(filter).ToList();
		}

		return r;
	}

	public async Task<T> Insert<T>(T entity) where T : IEntity
	{
		using(await NonparallelExecution.LockAsync(_tableFilePath<T>()))
		{
			List<T> entities = await this.LoadAll<T>();
			//check for duplicates: var entityFromDb = entities.Where(e => e.email == entity.email);
			entity.Id = entities.Count == 0 ? 1 : entities.Max(e => e.Id) + 1;//await this.GetNextAvailableId();
			entities.Add(entity);
			await this.SaveAll(entities);
		}

		return entity;
	}

	public async Task Update<T>(T entity) where T : IEntity
	{
		using(await NonparallelExecution.LockAsync(_tableFilePath<T>()))
		{
			List<T> entities = await this.LoadAll<T>();
			T entityFromDb = entities.Single(e => e.Id == entity.Id);//check uniqueness
			entities.Remove(entityFromDb);
			entities.Add(entity);
			await this.SaveAll(entities);
		}
	}

	public async Task<List<T>> SaveAll1<T>(List<T> entities) where T : IEntity
	{
		List<T> r = null;

		using(await NonparallelExecution.LockAsync(_tableFilePath<T>()))
		{
			await this.SaveAll(entities);

			r = await this.LoadAll<T>();
		}

		return r;
	}
	#endregion

	#region Constructors And Initialization
	public ServiceForEntities(ServiceForCOE c)
	{
		_c = c;
	}
	#endregion

	#region Deinitialization And Destructors
	#endregion

	#region Event Handlers
	#endregion

	#region Private Methods		
	private async Task<long> GetNextAvailableId<T>() where T : IEntity
	{
		long r;

		var counters = new Dictionary<string, long>();
		string tableFilePath = CraftSynth.BuildingBlocks.IO.FileSystem.PathCombine(_c.Configuration.DataFolderPathAbsolute, IDS_COUNTERS_FILENAME);
		using(await NonparallelExecution.LockAsync(IDS_COUNTERS_FILENAME))
		{
			if(!File.Exists(tableFilePath))
			{
				await CraftSynth.BuildingBlocks.IO.Json.SerializeAndWriteToFile(counters, tableFilePath);
			}
			counters = await CraftSynth.BuildingBlocks.IO.Json.ReadFileAndDeserializeAsync<Dictionary<string, long>>(tableFilePath);

			long? lastCounter = null;
			if(counters.ContainsKey(_tableName<T>()))
			{
				lastCounter = counters[_tableName<T>()];
			}

			r = lastCounter == null ? 1 : lastCounter.Value + 1;

			counters[_tableName<T>()] = r;
			await CraftSynth.BuildingBlocks.IO.Json.SerializeAndWriteToFile(counters, tableFilePath);
		}

		return r;
	}

	private async Task<List<T>> LoadAll<T>() where T : IEntity
	{
		List<T> r;

		if(!File.Exists(_tableFilePath<T>()))
		{
			r = new List<T>();
			await CraftSynth.BuildingBlocks.IO.Json.SerializeAndWriteToFile(r, _tableFilePath<T>());
		}

		r = await CraftSynth.BuildingBlocks.IO.Json.ReadFileAndDeserializeAsync<List<T>>(_tableFilePath<T>(), null, null, false);

		if(typeof(T) == typeof(LogEntry))
		{
			if(CraftSynth.BuildingBlocks.IO.FileSystem.GetFileSizeInBytes(_tableFilePath<T>()) > 10485760)
			{
				MarkFileAsCorrupted<T>();
				r = await CraftSynth.BuildingBlocks.IO.Json.ReadFileAndDeserializeAsync<List<T>>(_tableFilePath<T>(), null, null, false);
			}
		}

		if(r == null && CraftSynth.BuildingBlocks.IO.FileSystem.GetFileSizeInBytes(_tableFilePath<T>()) > 0)
		{
			MarkFileAsCorrupted<T>();
			r = await CraftSynth.BuildingBlocks.IO.Json.ReadFileAndDeserializeAsync<List<T>>(_tableFilePath<T>(), null, null, false);
		}

		return r;
	}

	private async void MarkFileAsCorrupted<T>() where T : IEntity
	{
		string newFilePath = CraftSynth.BuildingBlocks.IO.FileSystem.PathCombine(Path.GetDirectoryName(_tableFilePath<T>()), Path.GetFileNameWithoutExtension(_tableFilePath<T>()) + "." + DateTime.Now.ToDDateAndTimeAs_YYY_MM_DD__HH_MM_SS__ForFileSystem() + Path.GetExtension(_tableFilePath<T>()));
		File.Move(_tableFilePath<T>(), newFilePath);
		File.WriteAllText(_tableFilePath<T>(), "[]");
	}

	private async Task SaveAll<T>(List<T> entities) where T : IEntity
	{
		await CraftSynth.BuildingBlocks.IO.Json.SerializeAndWriteToFile(entities, _tableFilePath<T>(), true);
	}
	#endregion

	#region Helpers
	#endregion
}

public class MockedServiceForEntities:ServiceBase, IServiceForEntities
{
	public async Task<UnitOfWork> CreateUnitOfWork()
	{
		throw new NotImplementedException();
	}

	public Task<List<T>> Get<T>(Func<T, bool> filter) where T : IEntity
	{
		throw new NotImplementedException();
	}

	public Task<List<T>> GetAll<T>() where T : IEntity
	{
		throw new NotImplementedException();
	}

	public Task<T> GetById<T>(long id) where T : IEntity
	{
		throw new NotImplementedException();
	}

	public Task<T> Insert<T>(T entity) where T : IEntity
	{
		throw new NotImplementedException();
	}

	public Task<List<T>> SaveAll1<T>(List<T> entities) where T : IEntity
	{
		throw new NotImplementedException();
	}

	public Task Update<T>(T entity) where T : IEntity
	{
		throw new NotImplementedException();
	}
}