using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Tasks;

namespace TasksShould
{
    public class ConcurrencyIssue
    {
        [Test]
        public async Task ShouldHaveConcurrencyIssue()
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(2, 2);
            int computeThis = 0;
            var random = new Random();
            
            var computations = 1000.Times(i => Task.Run(async () =>
            {
                var managedThreadId = Thread.CurrentThread.ManagedThreadId;
                await Task.Delay(random.Next(1, 10000));
                var pre = computeThis;
                await Task.Delay(random.Next(10, 800));
                await Console.Out.WriteAsync($"Task {i} after delay: {pre} in thread {managedThreadId} | ");
                computeThis = pre + 1;
                return managedThreadId;
            }));

            var threads = await computations;

            Assert.IsTrue(threads.ToList().All(t => t == threads.First()));
            Assert.IsTrue(computeThis < 100);
        }

        [Test]
        public async Task ShouldHaveConcurrencyIssue_BlockedVersion()
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(2, 2);

            var threads = new List<int>();
            var random = new Random();
            var computeThis = 0;
            
            //This version runs slowly, hint: reduce the amount of iterations or increase the amount of threads
            for (var i = 0; i < 1000; i++)
            {
                var managedThreadId = Thread.CurrentThread.ManagedThreadId;
                await Task.Delay(random.Next(1, 10000));
                var pre = computeThis;
                await Task.Delay(random.Next(10, 800));
                await Console.Out.WriteAsync($"Task {i} after delay: {pre} in thread {managedThreadId} | ");
                computeThis = pre + 1;
                threads.Add(managedThreadId);
            }

            Assert.IsTrue(threads.ToList().All(t => t == threads.First()));
            Assert.IsTrue(computeThis < 100);
        }

    }
}