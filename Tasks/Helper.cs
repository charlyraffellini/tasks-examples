using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tasks
{
    public static class Helper
    {
        public static void Times(this int count, Action action)
        {
            for (int i = 0; i < count; i++)
            {
                action();
            }
        }

        public static void Times(this int count, Action<int> action)
        {
            for (int i = 0; i < count; i++)
            {
                action(i);
            }
        }

        public static async Task<int[]> Times(this int count, Func<int, Task<int>> action)
        {
            ICollection<Task<int>> tasks = new List<Task<int>>();

            for (int i = 0; i < count; i++)
            {
                tasks.Add(action(i));
            }

            return await Task.WhenAll(tasks);
        }

        public static async Task Times(this int count, Func<int, Task> action)
        {
            ICollection<Task> tasks = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(action(i));
            }

            await Task.WhenAll(tasks);
        }
        public static async Task<bool[]> Times(this int count, Func<int, Task<bool>> action)
        {
            ICollection<Task<bool>> tasks = new List<Task<bool>>();
            for (var i = 0; i < count; i++)
            {
                tasks.Add(action(i));
            }

            return await Task.WhenAll(tasks);
        }
    }
}