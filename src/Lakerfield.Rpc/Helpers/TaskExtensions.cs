using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lakerfield.Rpc.Helpers
{
  public static class TaskExtensions
  {
    public static async Task<bool> AwaitWithTimeout(this Task task, TimeSpan timeout)
    {
      var timeoutTask = Task.Delay(timeout);
      var completedTask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(false);

      if (completedTask != task)
        return false; // Timeout

      await task.ConfigureAwait(false); // Ensure exceptions are thrown if task failed
      return true; // Task completed in time
    }

  }
}
