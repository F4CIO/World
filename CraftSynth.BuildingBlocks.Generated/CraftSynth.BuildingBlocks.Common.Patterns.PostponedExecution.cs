using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace CraftSynth.BuildingBlocks.Common.Patterns
{
	/// <summary>
	/// Suitable for scheduling some work. One usage example is when one value changes frequently and we want to insure that once changing stops last value is saved on disk.
    /// There are two stages once work is scheduled: 1) waiting and 2) executing work.
    /// if another request is made during first phase work will be rescheduled.
    /// If another request is made during second phase that phase will be finished and both waiting and work will repeated.
	/// </summary>
	public class PostponedExecution
	{
		#region Private Members
		private PostponedWorkDelegate _workToExecute;
		private static object _lock = new object();
		private int _requests = 0;
        private int _timeToWaitBeforeWorkInMilliseconds;
        private ISynchronizeInvoke _controlForInvoke = null;        
        private int? _maxPostponingTimeInMiliseconds;
        #endregion

        #region Properties
        public delegate void PostponedWorkDelegate(PostponedExecution sender);
        public object Tag;
        #endregion

        #region Public Methods
        public void RequestWaitAndWork()
		{
			_requests++;
		}
		
		public void WaitAndDoWork(bool ifAlreadyExecutingJustMakeRequest, bool ifWaitingOrWorkingAndAnotherWasRequestedInMeantimeRepeatAllAtEnd, bool waitAndDoWorkEvenIfNoneRequested = true, bool inNewThread = true)
        {
            if (!inNewThread)
            {
                this.WaitAndDoWork_Inner(ifAlreadyExecutingJustMakeRequest, ifWaitingOrWorkingAndAnotherWasRequestedInMeantimeRepeatAllAtEnd, waitAndDoWorkEvenIfNoneRequested);
            }
            else
            {
                Thread thread = new Thread(() => {
                    this.WaitAndDoWork_Inner(ifAlreadyExecutingJustMakeRequest, ifWaitingOrWorkingAndAnotherWasRequestedInMeantimeRepeatAllAtEnd, waitAndDoWorkEvenIfNoneRequested);
                });
                thread.Name = "WaitAndDoWork_Inner(...)";
                thread.Start();
            }
        }

        private void WaitAndDoWork_Inner(bool ifAlreadyExecutingJustMakeRequest, bool ifWaitingOrWorkingAndAnotherWasRequestedInMeantimeRepeatAllAtEnd, bool waitAndDoWorkEvenIfNoneRequested)
        {
            if (!Monitor.TryEnter(_lock))
            {
                if (ifAlreadyExecutingJustMakeRequest)
                {
                    _requests++;
                }
            }
            else
            {
                try
                {
                    if (waitAndDoWorkEvenIfNoneRequested)
                    {
                        _requests++;
                    }
                    if (!ifWaitingOrWorkingAndAnotherWasRequestedInMeantimeRepeatAllAtEnd)
                    {
                        if (_requests > 0)
                        {
                            _requests = 0;
                            Thread.Sleep(_timeToWaitBeforeWorkInMilliseconds);

                            if (_controlForInvoke == null)
                            {
                                _workToExecute.Invoke(this);
                            }
                            else
                            {
                                //TODO: port to DotNet6
                                throw new NotImplementedException();
                                //PostponedExecution.InvokeIfRequired(_controlForInvoke, () =>
                                //{
                                //    _workToExecute.Invoke(this);
                                //});
                            }
                        }
                    }
                    else
                    {
                        while (_requests > 0)
                        {
                            DateTime momentWhenPostponingStartedAsUtc = DateTime.UtcNow;
                            do
                            {
                                _requests = 0;

                                TimeSpan periodToPospone = TimeSpan.FromMilliseconds(_timeToWaitBeforeWorkInMilliseconds);
                                if (_maxPostponingTimeInMiliseconds != null)
                                {
                                    DateTime now = DateTime.UtcNow;
                                    DateTime furthestExecutionMoment = momentWhenPostponingStartedAsUtc.AddMilliseconds(_maxPostponingTimeInMiliseconds.Value);
                                    if (now.Ticks > furthestExecutionMoment.Ticks)
                                    {
                                        periodToPospone = new TimeSpan(0);
                                    }
                                    else
                                    {
                                        TimeSpan periodToPostponeForFurthestExecutionMoment = furthestExecutionMoment.Subtract(now);
                                        if (periodToPostponeForFurthestExecutionMoment.Ticks < periodToPospone.Ticks)
                                        {
                                            periodToPospone = periodToPostponeForFurthestExecutionMoment;
                                        }
                                    }
                                }

                                Thread.Sleep(periodToPospone);
                            } while (_requests > 0);

                            if (_controlForInvoke == null)
                            {
                                _workToExecute.Invoke(this);
                            }
                            else
                            {
                                //TODO: port to DotNet6
                                throw new NotImplementedException(); 
                                //PostponedExecution.InvokeIfRequired(_controlForInvoke, () =>
                                //{
                                //    _workToExecute.Invoke(this);
                                //});
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        public void ExecuteIfAnyRequested(bool requestAnotherIfExecuting, bool repeatExecutionIfAnyRequestedWhileExecuting, bool inNewThread = true)
		{
			this.WaitAndDoWork(requestAnotherIfExecuting, repeatExecutionIfAnyRequestedWhileExecuting, false, inNewThread);
		}
		#endregion

		#region Constructors And Initialization

		public PostponedExecution(int timeToWaitBeforeWorkingInMiliseconds, ISynchronizeInvoke controlForInvokeIfWorkShouldExecuteOnUiThread_onlyNullIsAllowed, PostponedWorkDelegate workToExecute, int? maxPostponingTimeInMilisecondsIfRequestsAreTooFrequent = null, object tag = null)
		{
            _timeToWaitBeforeWorkInMilliseconds = timeToWaitBeforeWorkingInMiliseconds;
			_workToExecute = workToExecute;
            _controlForInvoke = controlForInvokeIfWorkShouldExecuteOnUiThread_onlyNullIsAllowed;
            _maxPostponingTimeInMiliseconds = maxPostponingTimeInMilisecondsIfRequestsAreTooFrequent;
            this.Tag = tag;
		}
        #endregion

        #region Deinitialization And Destructors

        #endregion

        #region Event Handlers

        #endregion

        #region Private Methods

        #endregion

        #region Helpers
        //TODO: port to DotNet6
        ///// <summary>
        ///// 
        ///// Usage example:
        ///// 
        ///// InvokeIfRequired(someControlOrForm, () =>
        ///// {
        /////     
        ///// });
        ///// 
        ///// Source: https://stackoverflow.com/questions/2367718/automating-the-invokerequired-code-pattern
        ///// </summary>
        ///// <param name="obj"></param>
        ///// <param name="action"></param>
        //private static void InvokeIfRequired(ISynchronizeInvoke control, MethodInvoker action)
        //{
        //    if (control.InvokeRequired)
        //    {
        //        var args = new object[0];
        //        control.Invoke(action, args);
        //    }
        //    else
        //    {
        //        action();
        //    }
        //}
        #endregion
    }
}
