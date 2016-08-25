using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TasksShould
{
    public class ContinueWithShould
    {
#region delay
        [Test]
        public async Task Simple()
        {
            var t = Task.Delay(TimeSpan.FromSeconds(1))
             .ContinueWith( _ => Task.Delay(TimeSpan.FromSeconds(5)))
             .Unwrap();
            await t;
        }


        [Test]
        public async Task Simpler()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        #endregion

#region console write
        [Test]
        public async Task Continuation()
        {
            var t = Task.Factory
                .StartNew(() => new { Tete = "some value" })
                .ContinueWith(v => new { Tato = $"contitnuation was here...{v.Result.Tete}" })
                .ContinueWith(v => new { pepe = $"other contitnuation was here...{v.Result.Tato}" })
                .ContinueWith(v => Console.Out.WriteLineAsync(v.Result.pepe));

            await t;
        }

        [Test]
        public async Task AsyncAwait()
        {
            var first = Task.FromResult(new { Tete = "some value" });
            var second = Task.FromResult("contitnuation was here...");
            var third = Task.FromResult("other contitnuation was here...");

            var actress = $"{await third} {await second} { (await first).Tete}";

            await Console.Out.WriteLineAsync(actress);
        }
#endregion
    }
}
