using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VKMatcher.Core
{
    class MultithreadingServer
    {
        List<Task> tasks = new List<Task>();

        public int ThreadCount { get; }
        public Action WorkerAction { get; }

        public MultithreadingServer(Action worker)
        {
            WorkerAction = worker;
            ThreadCount = Environment.ProcessorCount * 2;
        }

        public MultithreadingServer(Action worker, int threadCount)
        {
            WorkerAction = worker;
            ThreadCount = threadCount;
        }

        public void Run()
        {
            // Create muiltithreading
            Enumerable.Range(0, ThreadCount).All(_ =>
            {
                var task = Task.Run(() =>
                {
                    WorkerAction();
                });

                tasks.Add(task);

                return true;
            });

            // Infinity loop
            Task.WaitAll(tasks.ToArray());
        }
    }
}
