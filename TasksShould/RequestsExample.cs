using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Tasks;

namespace TasksShould
{
    public class RequestsExample
    {
        [Test]
        public async Task HttpRequest()
        {
            ThreadPool.SetMinThreads(1, 1);
            ThreadPool.SetMaxThreads(6, 6);

            var client = new HttpClient();
            const string requestBinId = "uz4799uz";

            var requests = 100.Times(i => client.PostAsync($"http://requestb.in/{requestBinId}",
                new FormUrlEncodedContent(new Dictionary<string, string> {{$"Tete {i}", "Eleven by Eleven"}})));

            await requests;
        }
    }
}