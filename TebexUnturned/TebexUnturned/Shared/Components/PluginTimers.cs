using System;
using System.Threading;

namespace Tebex.Shared.Components
{
    public class PluginTimers
    {
        /// <summary>
        /// Executes the specified action at a fixed interval.
        /// </summary>
        /// <param name="intervalInSeconds">The interval in seconds at which to execute the action.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>A System.Threading.Timer object that can be used to cancel the repeated execution.</returns>
        public Timer Every(double intervalInSeconds, Action action)
        {
            // Convert interval from seconds to milliseconds
            var intervalInMilliseconds = (int)(intervalInSeconds * 1000);

            // Create a timer that invokes the action at the specified interval
            var timer = new Timer(_ =>
            {
                action();
            }, null, intervalInMilliseconds, intervalInMilliseconds);

            // Return the timer to allow for external control (e.g., stopping the timer)
            return timer;
        }
    }
}