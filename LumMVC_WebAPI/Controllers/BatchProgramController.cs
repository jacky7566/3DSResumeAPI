using LumMVC_WebAPI.Models;
using LumMVC_WebAPI.Utils;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class BatchProgramController : ApiController
    {        
        [Route("api/ShippableLogicChecker")]
        [HttpGet]
        public HttpResponseMessage ShippableLogicChecker()
        {
            try
            {
                ShippableLogicUtil slu = new ShippableLogicUtil();
                slu.StartShippableLogic();
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine(ShippableLogicUtil.GetAllFootprints(ex));
                MailHelper.SendMail(string.Empty, null, string.Empty, ShippableLogicUtil.GetAllFootprints(ex), true, null, false);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        [Route("api/ABCChecker")]
        [HttpGet]
        public HttpResponseMessage ABCChecker()
        {
            ShippableLogicUtil slu = new ShippableLogicUtil();

            return null;
        }

        [HttpGet]
        [Route("api/ExpireRWChecker")]
        public HttpResponseMessage ExpireRWChecker()
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            grading_devEntities db = new grading_devEntities();
            StringBuilder sb = new StringBuilder();
            try
            {
                ShippableLogicUtil shipLogicUtil = new ShippableLogicUtil();
                var shippedList = (from header in db.TBL_WAFER_RESUME
                                   join ship in db.TBL_WAFER_RESUME_ITEM
                                   on new { header.Id, Key = "oship_flag", Value = "0" }
                                   equals new { Id = ship.Header_Id, ship.Key, ship.Value }
                                   join exp in db.TBL_WAFER_RESUME_ITEM
                                   on new { header.Id, Key = "exp_date" }
                                   equals new { Id = exp.Header_Id, exp.Key }
                                   where header.Type == "WIN" && header.Status == 2
                                   select new { header.RW_Wafer_Id, header.Wafer_Id, exp.Value })
                                   .OrderBy(r => r.RW_Wafer_Id).OrderBy(r => r.Wafer_Id).ToList();

                if (shippedList != null && shippedList.Count() > 0)
                {
                    var waferList = shippedList.Select(r => r.Wafer_Id).Distinct();
                    ////Check Wafer Is Released
                    var releasedWaferList = (from header in db.TBL_WAFER_RESUME
                                             join isHold in db.TBL_WAFER_RESUME_ITEM
                                             on new { header.Id, Key = "IsHold(Y/N)" }
                                             equals new { Id = isHold.Header_Id, isHold.Key }
                                             where header.Type == "Hold" && isHold.Value == "N" && waferList.Contains(header.Wafer_Id)
                                             select new { header.RW_Wafer_Id, header.Wafer_Id, header.Creation_Date }).ToList();

                    sb.Append("<table border='1'>");
                    sb.Append("<tr><th>RW Wafer Id</th><th>Wafer Id</th><th>Expired Date</th></tr>");
                    foreach (var item in shippedList)
                    {
                        var expDate = DateTime.Parse(item.Value);
                        if (DateTime.Now.Date.CompareTo(expDate) > 0)
                        {
                            if (releasedWaferList.Where(r => r.Wafer_Id == item.Wafer_Id
                                && r.RW_Wafer_Id == item.RW_Wafer_Id && r.Creation_Date.CompareTo(expDate) >= 0).Count() > 0)
                                continue;
                            //shipLogicUtil.UpdateResumeStatus(item.RW_Wafer_Id, 2, 1);
                            sb.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                                item.RW_Wafer_Id, item.Wafer_Id, item.Value);
                        }
                    }
                    sb.Append("</table>");
                    if (sb.Length > 0)
                    {
                        MailHelper.SendMail(string.Empty, null, "In Stocks RW Expired List (Shippable = Y)", sb.ToString(), true, null, false);
                    }
                }
                else
                {
                    LogHelper.WriteLine("No Shippable = 'Y' RWs expired");
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine(ShippableLogicUtil.GetAllFootprints(ex) + " ,RWList: " + sb.ToString());
                MailHelper.SendMail(string.Empty, null, string.Empty, ShippableLogicUtil.GetAllFootprints(ex), true, null, false);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            
            return resp;
        }
    }
}
