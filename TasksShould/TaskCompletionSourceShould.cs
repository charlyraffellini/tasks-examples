using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using System.Threading.Tasks;
using Tasks;

namespace TasksShould
{
    public class TaskCompletionSourceShould
    {
#region delay

        [Test]
        public async Task Run10KOfTasks()
        {
            var tasks = new List<Task>();
            for (var i = 0; i < 10000; i++)
                tasks.Add(Delay_TaskCompletionSource(5000)
                .ContinueWith(o => Console.Write($"{o.Id} | ")));

            await Task.WhenAll(tasks);
        }

        public Task Delay_TaskCompletionSource(int milliseconds)
        {
            var tcs = new TaskCompletionSource<object>();
            var timer = new System.Timers.Timer(milliseconds) { AutoReset = false };
            timer.Elapsed += delegate { timer.Dispose(); tcs.SetResult(null); };
            timer.Start();
            return tcs.Task;
        }

#endregion

#region http

        [Test]
        public async Task TaskCompletionSource()
        {
            //this test run 15 or 20 seconds or less that 5 sec hiting google.com
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(3, 3);

            var listener = new HttpListener_WithTaskCompletionSource();
            var requestBinId = "uppmjfup";

            var res = await 1000.Times(i => listener.ValidateUrlAsync($"http://requestb.in/{requestBinId}?pepe={i}"));

            Assert.IsTrue(res.All(x => x));

        }

        private static void Validate(int millisecondsDelay, Action action)
        {
            if (millisecondsDelay < 0)
                throw new ArgumentOutOfRangeException("millisecondsDelay");
            if (action == null) throw new ArgumentNullException("action");
        }
        #endregion






        public static Task Unused_StartNewDelayed(int millisecondsDelay, Action action)
        {
            Validate(millisecondsDelay, action);

            var tcs = new TaskCompletionSource<object>();

            var timer = new Timer(
                _ => tcs.SetResult(null), null, millisecondsDelay, Timeout.Infinite);

            return tcs.Task.ContinueWith(_ =>
            {
                timer.Dispose();
                action();
            });
        }

    }
}