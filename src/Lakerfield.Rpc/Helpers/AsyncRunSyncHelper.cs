using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Lakerfield.Rpc.Helpers
{
  /// <summary>
  /// Helper to run async code sync in oa wpf application, without deadlocking the ui-thread
  ///
  /// Code from:
  /// http://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
  ///
  /// Original from:
  /// http://social.msdn.microsoft.com/Forums/en-US/163ef755-ff7b-4ea5-b226-bbe8ef5f4796/is-there-a-pattern-for-calling-an-async-method-synchronously
  /// </summary>
  public static class AsyncRunSyncHelper
  {
    /// <summary>
    /// Execute's an async Task<T> method which has a void return value synchronously
    /// </summary>
    /// <param name="task">Task<T> method to execute</param>
    [DebuggerStepThrough]
    public static void RunSync(Func<Task> task)
    {
      var oldContext = SynchronizationContext.Current;
      var synch = new ExclusiveSynchronizationContext();
      SynchronizationContext.SetSynchronizationContext(synch);
      synch.Post(async _ =>
      {
        try
        {
          await task();
        }
        catch (Exception e)
        {
          synch.InnerException = e;
          //throw;
        }
        finally
        {
          synch.EndMessageLoop();
        }
      }, null);
      synch.BeginMessageLoop();

      SynchronizationContext.SetSynchronizationContext(oldContext);
    }

    /// <summary>
    /// Execute's an async Task&lt;T&gt; method which has a T return type synchronously
    /// </summary>
    /// <typeparam name="T">Return Type</typeparam>
    /// <param name="task">Task&lt;T&gt; method to execute</param>
    /// <returns></returns>
    [DebuggerStepThrough]
    public static T RunSync<T>(Func<Task<T>> task)
    {
      var oldContext = SynchronizationContext.Current;
      var synch = new ExclusiveSynchronizationContext();
      SynchronizationContext.SetSynchronizationContext(synch);
      T ret = default(T);
      synch.Post(async _ =>
      {
        try
        {
          ret = await task();
        }
        catch (Exception e)
        {
          synch.InnerException = e;
          //throw;
        }
        finally
        {
          synch.EndMessageLoop();
        }
      }, null);
      synch.BeginMessageLoop();
      SynchronizationContext.SetSynchronizationContext(oldContext);
      return ret;
    }

    private class ExclusiveSynchronizationContext : SynchronizationContext
    {
      private bool done;
      public Exception InnerException { get; set; }
      readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
      readonly Queue<Tuple<SendOrPostCallback, object>> items =
          new Queue<Tuple<SendOrPostCallback, object>>();

      public override void Send(SendOrPostCallback d, object state)
      {
        throw new NotSupportedException("We cannot send to our same thread");
      }

      public override void Post(SendOrPostCallback d, object state)
      {
        lock (items)
        {
          items.Enqueue(Tuple.Create(d, state));
        }
        workItemsWaiting.Set();
      }

      public void EndMessageLoop()
      {
        Post(_ => done = true, null);
      }

      [DebuggerStepThrough]
      public void BeginMessageLoop()
      {
        while (!done)
        {
          Tuple<SendOrPostCallback, object> task = null;
          lock (items)
          {
            if (items.Count > 0)
            {
              task = items.Dequeue();
            }
          }
          if (task != null)
          {
            task.Item1(task.Item2);
            if (InnerException != null) // the method threw an exeption
            {
              throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
            }
          }
          else
          {
            workItemsWaiting.WaitOne();
          }
        }
      }

      public override SynchronizationContext CreateCopy()
      {
        return this;
      }
    }
  }

}
