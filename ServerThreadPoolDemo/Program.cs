using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;

namespace ServerThreadPoolDemo
{
    class Program
    {
        static readonly Process process = Process.GetCurrentProcess();

        static void Main(string[] args)
        {
            string baseAddress = "http://localhost:9000/";

            //ThreadPool.SetMinThreads(1000, 1000);
            //ThreadPool.SetMaxThreads(12, 12);

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {
                Timer t = new Timer(OutputThreadPoolValues, null, 0, 500);

                Console.ReadLine();
            }
        }


        
        private static void OutputThreadPoolValues(object state)
        {
            ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);

            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);

            ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int avaliableCompletionPortThreads);

            int processThreads = process.Threads.Count;

            Console.WriteLine( $"Min: {minWorkerThreads}  Max: {maxWorkerThreads}     Available_W: {availableWorkerThreads}  Current_W: {maxWorkerThreads - availableWorkerThreads}    Available_IO: {avaliableCompletionPortThreads}  Current_IO: {maxCompletionPortThreads - avaliableCompletionPortThreads}");
            //Console.WriteLine( $"Min: {minCompletionPortThreads}  Max: {maxCompletionPortThreads}     Available: {avaliableCompletionPortThreads}  Current: {maxCompletionPortThreads - avaliableCompletionPortThreads}");
            Console.WriteLine($"Process Threads: {processThreads} ");

        }
    }
}
