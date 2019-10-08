﻿using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;

namespace ServerThreadPoolDemo
{
    [RoutePrefix("api/asyncserver")]
    public class AsyncServerController : ApiController
    {

        #region   #  Async Over Sync  #


        [Route("hello-sync")]
        [HttpGet]
        public string Hello()
        {
            Thread.Sleep(4000);

            return "Hello World Sync";
        }


        #endregion


        #region   #  Async Blocking  #


        [Route("hello-async-blocking")]
        [HttpGet]
        public Task<string> HelloAsyncBlocking()
        {
            Thread.Sleep(4000);

            return Task.FromResult( "Hello World Async" );
        }


        #endregion


        #region   #  Async Over Sync  #


        [Route("hello-async-over-sync")]
        [HttpGet]
        public async Task<string> HelloAsyncOverSync()
        {
            await Task.Run( () => Thread.Sleep(4000));

            return "Hello World AsyncOverSync";
        }


        #endregion


        #region   #  Async Over Sync - No ThreadPool  #


        [Route("hello-async-over-sync-no-threadpool")]
        [HttpGet]
        public async Task<string> HelloAsyncOverSyncNoThreadPool()
        {
            await Task.Factory.StartNew(() => Thread.Sleep(4000), TaskCreationOptions.LongRunning);

            return "Hello World AsyncOverSync";
        }


        #endregion


        #region   #  Sync Over Aync  #


        [Route("hello-sync-over-async")]
        [HttpGet]
        public string HelloSyncOverAync()
        {
            Task<string> task = AsyncMethod();

            return task.Result.ToString();
        }


        public static async Task<string> AsyncMethod()
        {
            using (HttpClient client = new HttpClient())
            {
                var jsonString = await client.GetStringAsync("http://deelay.me/4000/www.microsoft.com");
            }

            return "Hello World SyncOverAsync";
        }


        #endregion


        #region   #  Async  #


        [Route("hello-async")]
        [HttpGet]
        public async Task<string> HelloAsync()
        {
            await Task.Delay(4000);

            return "Hello World Async";
        }


        #endregion

    }
}