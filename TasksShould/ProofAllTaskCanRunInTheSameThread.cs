using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tasks;

namespace TasksShould
{
    public class ProofAllTaskCanRunInTheSameThread
    {
        [Test]
        public async Task AllTasksRunInTheSameThread_Lambda()
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(2, 2);

            var random = new Random();

            Func<int, long, Task> func = async (index, creationTime) =>
            {
                var millisecondsDelay = random.Next(500, 1000); 
                await Task.Delay(millisecondsDelay);//Force to reschedule
                var threadNum = Thread.CurrentThread.ManagedThreadId;
                await Console.Out.WriteLineAsync(
                    $"Task #{index} created at {creationTime} on thread #{threadNum}. Delayed in {millisecondsDelay} millisec");
            };

            var taskArray = 10.Times(i => func(i, DateTime.Now.Ticks));

            await Task.WhenAll(taskArray);
        }
    }
}