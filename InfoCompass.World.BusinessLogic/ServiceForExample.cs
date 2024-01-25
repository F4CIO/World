namespace InfoCompass.World.BusinessLogic;

//public interface HandlerForYouTrackIssuesInterface: GenericHandlerInterface<YouTrackSharp.Issues.Issue>
//{
//	Task<List<YouTrackSharp.Issues.Issue>> GetAll(ContextOfExecution c);
//	Task<T> GetByScriptName<T>(ContextOfExecution c, string scriptName, List<object> parameters = null);
//}

//public class ServiceForExample: GenericHandler<YouTrackSharp.Issues.Issue>, HandlerForYouTrackIssuesInterface
//{
//	public Task<List<YouTrackSharp.Issues.Issue>> GetAll(ContextOfExecution c){
//		throw new NotImplementedException();
//	}

//	public async Task<T> GetByScriptName<T>(ContextOfExecution c, string scriptName, List<object> parameters = null)
//	{
//		T r = default(T);
//		this.InsureContextOfExecution(c);
//		using (c.Log().LogScope("Executing GetByScriptName... "))
//		{
//			try
//			{
//				#region --- business layer method body ---
//				r = await YouTrend.BusinessLogic.HandlersFactory.HandlerForScripts.LoadAndExecute<T>(c, scriptName, parameters);
//				#endregion
//			}
//			catch (Exception ex)
//			{
//				this.HandleExceptionOnThisLayer(ex, $"Error occurred while executing GetByScriptName({c},{scriptName.ToNonNullString("null")},{parameters.Count}).", null, null, null, null, c);
//			}
//		}
//		return r;
//	}

//}