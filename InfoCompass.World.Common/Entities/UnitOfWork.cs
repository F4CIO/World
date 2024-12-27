namespace MyCompany.World.Common.Entities;

public class UnitOfWork:IDisposable
{
	public UnitOfWork ParentUnitOfWork;
	public object DbContext;
	public delegate void UnitOfWorkOnDispose_Delegate(UnitOfWork disposingUnitOfWork);
	public UnitOfWorkOnDispose_Delegate _onDispose;

	public UnitOfWork(UnitOfWork parentUnitOfWork, object dbContext, UnitOfWorkOnDispose_Delegate onDispose)
	{
		ParentUnitOfWork = parentUnitOfWork;
		DbContext = dbContext;
		_onDispose = onDispose;
	}

	public static UnitOfWork UseParentIfExistOrNew(UnitOfWork parentUnitOfWork, UnitOfWork newUnitOfWork)
	{
		UnitOfWork r;

		if(parentUnitOfWork != null)
		{
			r = new UnitOfWork(parentUnitOfWork, parentUnitOfWork.DbContext, newUnitOfWork._onDispose);
		}
		else
		{
			r = newUnitOfWork;
		}

		return r;
	}

	// Public implementation of Dispose pattern callable by consumers. 
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	// Flag: Has Dispose already been called? 
	bool _disposed = false;

	// Protected implementation of Dispose pattern. 
	protected virtual void Dispose(bool disposing)
	{
		if(!_disposed)
		{

			if(disposing)
			{
				// Free any managed objects here. 
				//
			}

			// Free any unmanaged objects here. 
			//
			if(_onDispose != null)
			{
				_onDispose.Invoke(this);
				_onDispose = null;
			}

			_disposed = true;
		}
	}

	~UnitOfWork()
	{
		this.Dispose(false);
	}
}
