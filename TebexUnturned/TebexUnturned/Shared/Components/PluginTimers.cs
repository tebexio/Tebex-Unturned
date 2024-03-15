using System;
using System.Threading;
using Tebex.Adapters;

namespace Tebex.Shared.Components
{
    public class PluginTimers
    {
        private static BaseTebexAdapter _adapter;
        public PluginTimers(BaseTebexAdapter adapter)
        {
            _adapter = adapter;
        }
        
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
        
        /// <summary>
        /// Executes the specified action once after a delay.
        /// </summary>
        /// <param name="delayInSeconds">The delay in seconds after which to execute the action.</param>
        /// <param name="action">The action to execute.</param>
        /// <returns>A System.Threading.Timer object that can be used to cancel the execution.</returns>
        public Timer Once(double delayInSeconds, Action action)
        {
            // Convert delay from seconds to milliseconds
            var delayInMilliseconds = (int)(delayInSeconds * 1000);

            // Create a timer that invokes the action once after the specified delay
            var timer = new Timer(_ =>
            {
                action();
                // Since the action should only occur once, dispose of the timer after execution
            }, null, delayInMilliseconds, Timeout.Infinite);

            // Return the timer to allow for external control (e.g., disposing of the timer early)
            return timer;
        }
    }
}