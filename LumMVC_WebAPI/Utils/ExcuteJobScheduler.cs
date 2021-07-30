
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace LumMVC_WebAPI.Utils
{
    public class ExcuteJobScheduler
    {
        public static IScheduler _Schedular = null;        
        private static string _CronJobSchedule = ConfigurationManager.AppSettings["ExeShipLgcSchdule"];

        public static void Application_Start()
        {
            // 建立簡單的、以 RAM 為儲存體的排程器
            var schedulerFactory = new Quartz.Impl.StdSchedulerFactory();
            _Schedular = schedulerFactory.GetScheduler().Result;

            // 建立工作
            IJobDetail job = JobBuilder.Create<ExcuteShippableLogicJob>()
                                .WithIdentity("ExcuteShippableLogicJob")
                                .Build();

            // 建立觸發器
            ITrigger trigger = TriggerBuilder.Create()
                                    // Default 每六小時觸發一次，不然看Config
                                    .WithCronSchedule(string.IsNullOrEmpty(_CronJobSchedule) ? "0 0 0/6 * * ?": _CronJobSchedule)                                 
                                    .WithIdentity("ExcuteShippableLogicTrigger")
                                    .Build();

            // 把工作加入排程
            _Schedular.ScheduleJob(job, trigger);

            // 啟動排程器
            _Schedular.Start();
        }

        protected void Application_End()
        {
            _Schedular.Shutdown(false);
        }
    }
}