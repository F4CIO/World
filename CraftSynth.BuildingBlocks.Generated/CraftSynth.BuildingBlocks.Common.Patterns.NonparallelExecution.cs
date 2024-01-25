using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace CraftSynth.BuildingBlocks.Common.Patterns
{
	/// <summary>
	/// =======Instead using:=====================================	
	/// 
	/// 
	/// private static object _lock = new object();
	///  
	/// if(Monitor.TryEnter(_lock))
	/// {
	///		try
	///		{
	///			...
	///			...
	///		}
	///		finally
	///		{
	///			Monitor.Exit(_lock);
	///		}
	/// } 
	/// 
	/// 
	/// you can use:----------------------------------------------
	///  
	/// 
	/// using (NonparallelExecution.Lock("myLock1"))
	/// {
	/// 	Console.WriteLine("Doing work synchronously...");
	/// 	Task.Delay(1000); //you can also call async here
	/// }
	/// 
	/// 
	/// =======And during async coding instead using:=============
	/// 
	/// 
	/// static SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1); 
	/// 
	/// await semaphoreSlim.WaitAsync();
	/// try
	/// {
	///     await Task.Delay(1000);
	/// }
	/// finally
	/// {
	/// 	semaphoreSlim.Release();
	/// }
	///  
	/// 
	/// you can use:----------------------------------------------
	/// 
	/// 
	///	using (await NonparallelExecution.LockAsync("myLock1"))
	/// {
	///     Console.WriteLine("Doing work asynchronously...");
	///     await Task.Delay(1000);
	/// }
	///
	///===========================================================
	///
	/// Source:
	/// https://blog.cdemi.io/async-waiting-inside-c-sharp-locks/
	/// https://chat.openai.com/share/c9f540cc-b664-45a5-bc38-c77b11405ed1
	/// 
	/// </summary>
	public sealed class NonparallelExecution : IDisposable
	{
		private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
		private string _lockName;

		private NonparallelExecution() { }

		public static async Task<NonparallelExecution> LockAsync(string lockName)
		{
			var nonparallelExecution = new NonparallelExecution { _lockName = lockName };
			var semaphore = _locks.GetOrAdd(lockName, _ => new SemaphoreSlim(1, 1));
			await semaphore.WaitAsync();
			return nonparallelExecution;
		}

		public static NonparallelExecution Lock(string lockName)
		{
			var nonparallelExecution = new NonparallelExecution { _lockName = lockName };
			var semaphore = _locks.GetOrAdd(lockName, _ => new SemaphoreSlim(1, 1));
			semaphore.Wait();
			return nonparallelExecution;
		}

		public void Dispose()
		{
			if (_locks.TryGetValue(_lockName, out var semaphore))
			{
				semaphore.Release();
			}
		}
	}




	/////Retired version that has not async support:
	/////
	///// <summary>
	///// Instead using:--------------------------------------------
	///// 
	///// 
	///// private static object _lock = new object();
	/////  
	///// if(Monitor.TryEnter(_lock))
	///// {
	/////		try
	/////		{
	/////			...
	/////			...
	/////		}
	/////		finally
	/////		{
	/////			Monitor.Exit(_lock);
	/////		}
	///// }
	///// 
	///// 
	///// you can use:----------------------------------------------
	///// 
	///// 
	///// using(var a = new NonparallelExecution("someLockObject"))
	///// {
	/////		if(a.IsNotExecuting)
	/////		{
	/////			...
	/////			...
	/////		}
	///// }
	///// </summary>
	//public class NonparallelExecution:IDisposable
	//{
	//	#region Private Members
	//	private static object _lock = new object();
	//	private static Dictionary<string, bool> _codePartNamesAndIsExecutingState = new Dictionary<string, bool>();

	//	private string _currentCodePartName;
	//	#endregion

	//	#region Properties
	//	public bool IsNotExecuting
	//	{
	//		get
	//		{
	//			lock (_lock)
	//			{
	//				return !_codePartNamesAndIsExecutingState[_currentCodePartName];
	//			}
	//		}
	//	}
	//	#endregion

	//	#region Public Methods
	//	#endregion

	//	#region Constructors And Initialization

	//	public NonparallelExecution(string codePartName)
	//	{
	//		lock (_lock)
	//		{
	//			_currentCodePartName = codePartName;
	//			if (_codePartNamesAndIsExecutingState.ContainsKey(codePartName))
	//			{
	//				_codePartNamesAndIsExecutingState[codePartName] = true;
	//			}
	//			else
	//			{
	//				_codePartNamesAndIsExecutingState.Add(codePartName, true);
	//			}
	//		}
	//	}
	//	#endregion

	//	#region Deinitialization And Destructors
	//	public void Dispose()
	//	{
	//		this.Dispose(true);
	//		GC.SuppressFinalize(this);       
	//	}

	//	bool _disposed = false;

	//	protected virtual void Dispose(bool disposing)
	//	{
	//		lock (_lock)
	//		{
	//			if (!_disposed)
	//			{

	//				if (disposing)
	//				{
	//					// Free any managed objects here. 
	//					//
	//				}

	//				// Free any unmanaged objects here. 
	//				//

	//				try
	//				{
	//					if (_codePartNamesAndIsExecutingState[_currentCodePartName] == false)
	//					{
	//						throw new Exception("Invalid usage of NonparallelExecution class. Please see class description.");
	//					}
	//					else
	//					{
	//						_codePartNamesAndIsExecutingState[_currentCodePartName] = false;
	//					}
	//				}
	//				catch (KeyNotFoundException)
	//				{
	//					throw new Exception("Invalid usage of NonparallelExecution class. Please see class description.");
	//				}

	//				_disposed = true;
	//			}
	//		}
	//	}

	//	~NonparallelExecution()
	//	{
	//		 this.Dispose(false);
	//	}
	//	#endregion

	//	#region Event Handlers
	//	#endregion

	//	#region Private Methods
	//	#endregion

	//	#region Helpers
	//	#endregion
	//}
}
