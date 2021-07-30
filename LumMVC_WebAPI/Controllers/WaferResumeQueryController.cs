using LumMVC_WebAPI.Models;
using LumMVC_WebAPI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/WaferResumeQuery")]
    public class WaferResumeQueryController : ApiController
    {
        [HttpGet]
        [Route("GetOnHoldList")]
        public HttpResponseMessage GetOnHoldList()
        {
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            grading_devEntities db = new grading_devEntities();
            try
            {
                var list = (from header in db.TBL_WAFER_RESUME
                              join isHold in db.TBL_WAFER_RESUME_ITEM
                              on new { header.Id, Key = "IsHold(Y/N)" }
                              equals new { Id = isHold.Header_Id, isHold.Key }
                              join comment in db.TBL_WAFER_RESUME_ITEM
                              on new { header.Id, Key = "Comment" }
                              equals new { Id = comment.Header_Id, comment.Key }
                              where header.Type == "Hold" && isHold.Value == "Y"
                              select new { header.Wafer_Id, header.RW_Wafer_Id, IsHold = isHold.Value,
                                  Comment = comment.Value, header.Created_By, header.Creation_Date }).
                                  OrderByDescending(r=> r.Creation_Date).ToList();

                resp = IOHelper.LogAndResponse(new ObjectContent<dynamic>(list, new JsonMediaTypeFormatter()));
            }
            catch (Exception ex)
            {
                resp = IOHelper.LogAndResponse(null, HttpStatusCode.ExpectationFailed, string.Format(@"Message: {0},
                StackTrace: {1} ", ex.Message, ex.StackTrace));
            }

            return resp;
        }
    }
}
