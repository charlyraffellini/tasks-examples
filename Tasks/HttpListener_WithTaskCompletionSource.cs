using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tasks
{
    public class HttpListener_WithTaskCompletionSource
    {
        public Task<bool> ValidateUrlAsync(string url)
        {
            var tcs = new TaskCompletionSource<bool>();
            var request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                request.BeginGetResponse(iar =>
                {
                    HttpWebResponse response = null;
                    try
                    {
                        response = (HttpWebResponse)request.EndGetResponse(iar);
                        tcs.SetResult(response.StatusCode == HttpStatusCode.OK);
                    }
                    catch (Exception exc) { tcs.SetException(exc); }
                    finally { if (response != null) response.Close(); }
                }, null);
            }
            catch (Exception exc) { tcs.SetException(exc); }
            return tcs.Task;

        }
    }
}
