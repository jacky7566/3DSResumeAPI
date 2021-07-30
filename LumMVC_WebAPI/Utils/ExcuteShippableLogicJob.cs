using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Utils
{
    public class ExcuteShippableLogicJob : IJob
    {
        private static string _IsExcuteJob = ConfigurationManager.AppSettings["ExeJob"];
        public Task Execute(IJobExecutionContext context)
        {
            //LogHelper.WriteLine("Test");
            if (string.IsNullOrEmpty(_IsExcuteJob) == false && _IsExcuteJob.Trim().ToUpper().Equals("Y"))
            {
                ShippableLogicUtil shipLogic = new ShippableLogicUtil();
                shipLogic.StartShippableLogic();
            }
            return null;
        }
    }
}