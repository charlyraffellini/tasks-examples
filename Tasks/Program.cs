
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace Tasks
{
    public class Program
    {
        public static readonly string ConnectionString = "ServiceBusCnnString";

        //public static readonly string StorageConnectionString =
        //    "AzureStorageCnnString";

        static void Main(string[] args)
        {
            Functions.FeedServiceBus().Wait();

            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(6, 100);

            Environment.SetEnvironmentVariable("AzureWebJobsEnv", "Development");
            var config = new JobHostConfiguration
            {
                DashboardConnectionString = "UseDevelopmentStorage=true;",
                StorageConnectionString = "UseDevelopmentStorage=true;",
                Queues = { BatchSize = 16}
            };

            config.UseServiceBus(new ServiceBusConfiguration
            {
                ConnectionString = ConnectionString,
                PrefetchCount = 100,//Prefetch and keep in cache SubscriptionClient.PrefetchCount
                MessageOptions = { MaxConcurrentCalls = 32},
            });

            config.UseDevelopmentSettings();

            JobHost host = new JobHost(config);
            host.Start();
            Console.ReadLine();
        }

        public static HashSet<int> ThreadHashes = new HashSet<int>();
        public static ConcurrentDictionary<int,bool> ConcurrencyDictionary = new ConcurrentDictionary<int,bool>();
        public static ConcurrentDictionary<int,bool> ParallelismDictionary = new ConcurrentDictionary<int, bool>();

        public static int ConcurrentCount()
        {
            return ConcurrencyDictionary.Sum(kv => kv.Value ? 1 : 0);
        }

        public static int ParallelismCount()
        {
            return ParallelismDictionary.Sum(kv => kv.Value ? 1 : 0);
        }

        public static void InsertData(int threadHashCode, int messageId, bool isStart)
        {            
            ConcurrencyDictionary.AddOrUpdate(messageId, isStart, (i, b) => isStart);
            ParallelismDictionary.AddOrUpdate(threadHashCode, isStart, (i, b) => isStart);
            ThreadHashes.Add(threadHashCode);
        }

        public static string GetLabel(int messageId, int threadHashCode)
        {
            return $"Concurrency {ConcurrentCount()} and Parallelism {ParallelismCount()}. Message {messageId} is being processed on Thread {threadHashCode}";
        }
    }

    public static class Functions
    {
        private static HttpClient _client = new HttpClient();
        private static string  requestBinId = "uz4799uz";
        private static async Task ConsumeSubscriptionByTasks([ServiceBusTrigger("TestTopic", "all")] BrokeredMessage message)
        {
            var body = message.GetBody<string>();
            int messageId;
            int.TryParse(body.Split('|').First(), out messageId);
            var threadHashCode = Thread.CurrentThread.GetHashCode();
            Program.InsertData(threadHashCode, messageId, true);
            await Console.Out.WriteLineAsync(Program.GetLabel(messageId, threadHashCode));
            await Task.Delay(3000); //Needed in order to force rescheduless
            await _client.GetAsync($"http://requestb.in/{requestBinId}?messageBody={body}");
            Program.InsertData(threadHashCode, messageId, false);
        }


        public static void ConsumeSubscriptionByThreads([ServiceBusTrigger("TestTopic", "all")] BrokeredMessage message)
        {
            var body = message.GetBody<string>();
            int messageId;
            int.TryParse(body.Split('|').First(), out messageId);
            var threadHashCode = Thread.CurrentThread.GetHashCode();
            Program.InsertData(threadHashCode, messageId, true);
            Console.WriteLine(Program.GetLabel(messageId, threadHashCode));
            Thread.Sleep(3000);//Needed in order to force reschedule
            Program.InsertData(threadHashCode, messageId, false);
        }

        public static async Task FeedServiceBus()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString(Program.ConnectionString);
            if (await namespaceManager.TopicExistsAsync("TestTopic"))
            {
                await namespaceManager.DeleteTopicAsync("TestTopic");
                await namespaceManager.CreateTopicAsync("TestTopic");
                namespaceManager.CreateSubscription("TestTopic", "all");
            }

            TopicClient client = TopicClient.CreateFromConnectionString(Program.ConnectionString, "TestTopic");

            Func<int, Task> createMessage = async i => await client.SendAsync(new BrokeredMessage($"{i}|{Guid.NewGuid()}"));

            await 100.Times(createMessage);
        }
    }
}
