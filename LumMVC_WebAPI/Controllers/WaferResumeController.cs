using LumMVC_WebAPI.Models;
using LumMVC_WebAPI.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using SystemLibrary.Utility;
using LumMVC_WebAPI.Models.Classes;

namespace LumMVC_WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class WaferResumeController : ApiController
    {

        string format = "yyyyMMddHHmmss";
        grading_devEntities _db = new grading_devEntities();
        TBL_WAFER_TXN_LOGController _log = new TBL_WAFER_TXN_LOGController();


        private DateTime ParseFromTime(string date)
        {
            date = date.Trim();
            string time = "000000";
            DateTime fromDt = new DateTime();
            if (string.IsNullOrEmpty(date))
            {
                fromDt = DateTime.ParseExact("100101010"+ time, format, null);
            }
            else
            {
                fromDt = DateTime.ParseExact(date + time, format, null);
                
            }
            return fromDt;
        }

        private DateTime ParseEndTime(string date)
        {
            date = date.Trim();
            string time = "235959";
            DateTime endDt = new DateTime();
            if (string.IsNullOrEmpty(date))
            {
                endDt = DateTime.ParseExact("22001231"+ time, format, null);                
            }
            else
            {
                endDt = DateTime.ParseExact(date + time, format, null);
            }
            return endDt;
        }

        /// <summary>
        /// Get Wafer Resume Result
        /// </summary>
        /// <param name="boxId">Box ID</param>
        /// <param name="waferId">Wafer ID</param>
        /// <param name="rwWaferId">RW Wafer ID</param>
        /// <param name="shipable">Y or N</param>
        /// <param name="nonShipable">boolean for none shippable only</param>
        /// <param name="updatedRange">Last_Updated_Date from to, ex: 2020/08/17-2020/11/16</param>
        /// <param name="pageNumber">Current Page Index</param>
        /// <param name="globalFilter"></param>
        /// <param name="sortField"></param>
        /// <param name="sortOrder"></param>
        /// <returns></returns>
        // GET: api/WaferResume
        [Route("api/WaferResume/{boxId}/{waferId}/{rwWaferId}/{shipable}/{nonShipable}/{updatedRange}/{pageNumber}/{globalFilter}/{sortField}/{sortOrder}")]
        public object Get(string boxId, string waferId, string rwWaferId, string shipable, bool nonShipable, string updatedRange, int pageNumber, string globalFilter, string sortField, int sortOrder)
        {
            int totalRecords = 0;
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();//引用stopwatch物件
            sw.Reset();//碼表歸零
            sw.Start();//碼表開始計時

            //string[] bypassPi = new string[] { "list", "Details", "Box_Id", "ErrorReason" };
            var winShipList = (from header in _db.TBL_WAFER_RESUME
                               join item in _db.TBL_WAFER_RESUME_ITEM on header.Id equals item.Header_Id
                               where item.Key == "oship_flag" && item.Type == "WIN" && item.Value == "1"
                               select header.RW_Wafer_Id);

            //DateTime createdFromDt = ParseFromTime(createdFrom);
            //DateTime createdToDt = ParseEndTime(createdTo);
            DateTime updatedFromDt = ParseFromTime(updatedRange.Split('-')[0]);
            DateTime updatedToDt = ParseEndTime(updatedRange.Split('-')[1]);

            List<TBL_WAFER_RESUME> list = (from r in _db.TBL_WAFER_RESUME
                                           where r.Status == 9 && !winShipList.Contains(r.RW_Wafer_Id)
                                           //&& r.Creation_Date >= createdFromDt
                                           //&& r.Creation_Date <= createdToDt
                                           && r.Last_Updated_Date >= updatedFromDt
                                           && r.Last_Updated_Date <= updatedToDt
                                           && (waferId.Trim() == "NA" ? true : r.Wafer_Id.Contains(waferId))
                                           && (rwWaferId.Trim() == "NA" ? true : r.RW_Wafer_Id.Contains(rwWaferId))
                                           select r
                                           ).ToList();

            List<string> waferList = new List<string>();
            List<WaferResumeResult> resultList = new List<WaferResumeResult>();

            if (list.Count() > 0)
            {
                waferList = (from item in list
                             select item.Wafer_Id).Distinct().ToList();

                List<WaferResumeItem> itemList = this.getWaferResumeItems(waferList);

                try
                {
                    resultList = (from head in list
                                  join item in itemList on new { head.RW_Wafer_Id, head.Wafer_Id } equals new { item.RW_Wafer_Id, item.Wafer_Id }
                                  //into item_join
                                  //from item in item_join.DefaultIfEmpty()
                                  group new { head, item } by new
                                  {
                                      item.Box_Id,
                                      head.Wafer_Id,
                                      head.RW_Wafer_Id,
                                      item.Good_Die_Qty
                                  } into g
                                  select new WaferResumeResult
                                  {
                                      Box_Id = g.Key.Box_Id,
                                      Good_Die_Qty = g.Key.Good_Die_Qty,
                                      Wafer_Id = g.Key.Wafer_Id,
                                      RW_Wafer_Id = g.Key.RW_Wafer_Id,
                                      Creation_Date = g.Select(X => X.head.Creation_Date).FirstOrDefault(),
                                      Shipable = g.Select(X => X.head.Attribute1).FirstOrDefault(),
                                      // Attribute2 = g.Select(X => X.head.Attribute2).FirstOrDefault(),
                                      // Created_By = g.Select(X => X.head.Created_By).FirstOrDefault(),
                                      // LastUpdated_By = g.Select(X => X.head.LastUpdated_By).FirstOrDefault(),
                                      Last_Updated_Date = g.Select(X => X.head.Last_Updated_Date).FirstOrDefault(),
                                      // Sys_Updated_Date = g.Select(X => X.head.Sys_Updated_Date).FirstOrDefault(),
                                      // Status = g.Select(X => X.head.Status).FirstOrDefault(),
                                      Details = g.Select(X => X.item.Details).FirstOrDefault(),
                                      ErrorReason = g.Select(X => JsonConvert.DeserializeObject<Dictionary<string, string>>(X.head.Attribute2)).FirstOrDefault()
                                  }).ToList();
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLine(ex.InnerException.ToString());
                    throw ex;
                }

                if (boxId.Trim() != "NA")
                {
                    resultList = resultList.Where(x => string.IsNullOrEmpty(x.Box_Id) ? false : x.Box_Id.Contains(boxId.Trim())).ToList();
                }
                if (shipable.Trim() != "NA")
                {
                    resultList = resultList.Where(x => string.IsNullOrEmpty(x.Shipable) ? false : x.Shipable == shipable.Trim()).ToList();
                }
                if (nonShipable)
                {
                    resultList = resultList.Where(x => x.ErrorReason.ContainsKey("IsHold(Y/N)") ? x.ErrorReason["IsHold(Y/N)"] == "Y": false).ToList();
                }

                if (sortField.Trim() != "NA")
                {
                    PropertyInfo property = typeof(WaferResumeResult).GetProperty(sortField);
                    if (sortOrder > 0)
                        resultList = resultList.OrderBy(x => property.GetValue(x)).ToList();
                    else
                        resultList = resultList.OrderByDescending(x => property.GetValue(x)).ToList();
                }

                totalRecords = resultList.Count;
                if (globalFilter.Trim() != "NA")
                {
                    string[] propertyNames = { "Box_Id", "Wafer_Id", "RW_Wafer_Id", "Shipable", "Last_Updated_Date" };
                    List<WaferResumeResult> tmpResult = new List<WaferResumeResult>();
                    PropertyInfo[] properties = typeof(WaferResumeResult).GetProperties().Where(x => propertyNames.Contains(x.Name)).ToArray();
                    foreach (var property in properties)
                    {                        
                        switch(property.PropertyType.Name)
                        {
                            case "String":
                                tmpResult.AddRange(resultList.Where(x => property.GetValue(x) == null ? false : property.GetValue(x).ToString().Contains(globalFilter.Trim())).ToList());
                                break;
                            case "DateTime":
                                tmpResult.AddRange(resultList.Where(x => property.GetValue(x) == null ? false : DateTime.Parse(property.GetValue(x).ToString()).ToString("yyyy-MM-dd").Contains(globalFilter.Trim())).ToList());
                                break;
                        }
                    }

                    tmpResult.Distinct();
                    resultList = tmpResult;
                }
            }

            sw.Stop();//碼錶停止
            //印出所花費的總豪秒數
            string totalSeconds = sw.Elapsed.TotalSeconds.ToString();
            // int pageNumber = 1;
            int numberOfObjectsPerPage = 10;
            var queryResultPage = numberOfObjectsPerPage < resultList.Count() ? resultList.Skip(numberOfObjectsPerPage * pageNumber).Take(numberOfObjectsPerPage) : resultList;

            return new { totalRecords , records= resultList.Count(), queryResultPage , totalSeconds };
        }

        [ResponseType(typeof(List<TBL_WAFER_RESUME_HIS>))]
        [Route("api/GetWaferResHisById/{rw_waferId}")]
        [HttpGet]
        public List<TBL_WAFER_RESUME_HIS> GetWaferResHisById(string rw_waferId)
        {
            List<TBL_WAFER_RESUME_HIS> list = _db.TBL_WAFER_RESUME_HIS.Where(r => r.RW_Wafer_Id == rw_waferId
            && r.Status == 9)
                .OrderByDescending(r => r.Last_Updated_Date).ToList();
            return list;
        }

        // GET: api/WaferResume/5
        public string Get(int id)
        {
            //LogHelper.WriteLine("Test123");
            return "value";
        }

        [Route("api/WaferResumeBatchSave")]
        public async System.Threading.Tasks.Task WaferResumeBatchSave([FromBody]List<JObject> data)
        {
            try
            {
                var chunkList = Caculater.ChunkBy(data, 30);
                foreach (var chunk in chunkList)
                {
                    using (var context = new grading_devEntities())
                    {
                        context.Configuration.AutoDetectChangesEnabled = false;
                        foreach (var obj in chunk)
                        {
                            await SaveWaferResumeItems(obj, context);
                            //await PostWaferResumeAsync(obj, context);
                        }
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception e)
            {
                string exStr = string.Format("StackTrace: {0} \n\r Inner Exception: {1}", e.StackTrace, e.InnerException);
                LogHelper.WriteLine(exStr);
                MailHelper.SendMail(string.Empty, null, "Wafer Resume Error!", exStr, true, null, false);
            }
        }

        // POST: api/WaferResume
        public async System.Threading.Tasks.Task PostAsync([FromBody]JObject data)
        {
            using (var context = new grading_devEntities())
            {
                context.Configuration.AutoDetectChangesEnabled = false;
                await SaveWaferResumeItems(data, context);
                context.SaveChanges();
            }
        }

        private void UpdateEntity<T>(T entity, grading_devEntities context) where T : class
        {
            context.Set<T>().Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        private void DeleteEntity<T>(T entity, grading_devEntities context) where T : class
        {
            context.Set<T>().Attach(entity);
            context.Entry(entity).State = EntityState.Deleted;
        }

        private async System.Threading.Tasks.Task SaveWaferResumeItems(JObject data, grading_devEntities context)
        {
            var now = DateTime.Now;
            TBL_WAFER_RESUME header = new TBL_WAFER_RESUME();
            //Deal With Header Data
            try
            {
                header = data["wafer_header"].ToObject<TBL_WAFER_RESUME>();
                List<TBL_WAFER_RESUME_ITEM> inputList = data["wafer_item_list"].ToObject<List<TBL_WAFER_RESUME_ITEM>>();
                //LogHelper.WriteLine("wafer_header: " + data["wafer_header"].ToString());

                JSONHelper jsHelper = new JSONHelper();
                string attribute1 = string.Empty;
                if (inputList != null && inputList.Count > 0)
                {
                    attribute1 = jsHelper.TransferHiveFormat(inputList);
                }
                int status = 1; //Default Status = 1
                status = header.Status == null ? 1 : header.Status.Value;
                var inputRWID = string.IsNullOrEmpty(header.RW_Wafer_Id) ? string.Empty : header.RW_Wafer_Id;

                var res = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id
                                && r.Level == header.Level && r.Type == header.Type && r.RW_Wafer_Id == header.RW_Wafer_Id).ToList();

                var isNewHeader = (res.Count() == 0);
                //if (!isNewHeader)
                //    header = res.First();

                if (isNewHeader == false)
                {
                    header = res.FirstOrDefault();
                    //Check if update data is same
                    if (header.Attribute1 == attribute1 && header.RW_Wafer_Id == inputRWID)
                        return;
                }
                
                header.Attribute1 = attribute1;
                header.Last_Updated_Date = now;
                header.Sys_Updated_Date = now;
                header.Status = status;

                if (isNewHeader)
                {
                    header.Id = Guid.NewGuid();
                    header.Creation_Date = now;
                    header.Last_Updated_Date = now;
                    context.TBL_WAFER_RESUME.Add(header);
                }
                else
                {                    
                    string lastupdated_by = header.Created_By;            
                    header.LastUpdated_By = lastupdated_by;
                    UpdateEntity(header, context);
                }

                //if can found the exist item, then update others to set status into initial 1
                List<TBL_WAFER_RESUME> resAllList = null;

                if (header.Level == "WF")
                {
                    if (string.IsNullOrEmpty(header.RW_Wafer_Id))
                    {
                        var rwIdList = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id && r.RW_Wafer_Id != "")
                            .Select(r=>r.RW_Wafer_Id).Distinct().ToList();
                        if (rwIdList != null && rwIdList.Count() > 0)
                        {
                            resAllList = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id
                            && rwIdList.Contains(r.RW_Wafer_Id) && r.Status == 2).ToList();
                        }
                    }
                }
                else
                {
                    resAllList = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id
                    && r.RW_Wafer_Id == header.RW_Wafer_Id && r.Status == 2).ToList();
                }

                if (resAllList != null && resAllList.Count() > 0)
                {
                    foreach (var resItem in resAllList)
                    {
                        resItem.Status = 1;
                        resItem.Last_Updated_Date = now;
                        resItem.Sys_Updated_Date = now;
                        UpdateEntity(resItem, context);
                    }
                }
                
                var resItems = context.TBL_WAFER_RESUME_ITEM.Where(r => r.Header_Id == header.Id).ToList();
                foreach (TBL_WAFER_RESUME_ITEM item in inputList)
                {
                    var resItem = resItems.Where(r => r.Key == item.Key && r.Type == item.Type);
                    if (resItem.Any())
                    {
                        var tmp = resItem.FirstOrDefault();
                        tmp.Last_Updated_Date = now;
                        tmp.Value = item.Value;
                        tmp.Status = status;

                        UpdateEntity(tmp, context);
                    }
                    else
                    {
                        item.Id = Guid.NewGuid();
                        item.Header_Id = header.Id;
                        item.Creation_Date = now;
                        item.Last_Updated_Date = now;
                        item.Status = status;
                        context.TBL_WAFER_RESUME_ITEM.Add(item);
                    }
                    //context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                string exStr = string.Format("StackTrace: {0} \n\r Inner Exception: {1}", e.StackTrace, e.InnerException);
                LogHelper.WriteLine(exStr);
                MailHelper.SendMail(string.Empty, null, "Wafer Resume Error!", exStr, true, null, false);

                await _log.PostTBL_WAFER_TXN_LOG(new TBL_WAFER_TXN_LOG()
                {
                    Id = Guid.NewGuid(),
                    Level = "WaferResume",
                    Type = "Exception",
                    Wafer_Id = header.Wafer_Id,
                    RW_Wafer_Id = header.RW_Wafer_Id,
                    Created_By = header.Created_By,
                    Log = string.Format("Exception: [{0}], Attribute1: [{1}]", exStr, data.ToString()),
                    Creation_Date = DateTime.Now
                });
            }
        }

       
        private async System.Threading.Tasks.Task SaveWaferResumeItemsNew(JObject data, grading_devEntities context)
        {
            var now = DateTime.Now;
            TBL_WAFER_RESUME header = new TBL_WAFER_RESUME();
            //Deal With Header Data
            try
            {
                header = data["wafer_header"].ToObject<TBL_WAFER_RESUME>();
                List<TBL_WAFER_RESUME_ITEM> inputList = data["wafer_item_list"].ToObject<List<TBL_WAFER_RESUME_ITEM>>();
                //LogHelper.WriteLine("wafer_header: " + data["wafer_header"].ToString());

                JSONHelper jsHelper = new JSONHelper();
                string attribute1 = string.Empty;
                if (inputList != null && inputList.Count > 0)
                {
                    attribute1 = jsHelper.TransferHiveFormat(inputList);
                }
                int status = 1; //Default Status = 1
                status = header.Status == null ? 1 : header.Status.Value;
                var inputRWID = string.IsNullOrEmpty(header.RW_Wafer_Id) ? string.Empty : header.RW_Wafer_Id;

                var res = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id
                                && r.Level == header.Level && r.Type == header.Type && r.RW_Wafer_Id == header.RW_Wafer_Id).ToList();

                var isNewHeader = (res.Count() == 0);
                //if (!isNewHeader)
                //    header = res.First();

                if (isNewHeader == false)
                {
                    header = res.FirstOrDefault();
                    //Check if update data is same
                    if (header.Attribute1 == attribute1 && header.RW_Wafer_Id == inputRWID)
                        return;
                }

                header.Attribute1 = attribute1;
                header.Last_Updated_Date = now;
                header.Sys_Updated_Date = now;
                header.Status = status;

                if (isNewHeader)
                {
                    header.Id = Guid.NewGuid();
                    header.Creation_Date = now;
                    header.Last_Updated_Date = now;
                    context.TBL_WAFER_RESUME.Add(header);
                }
                else
                {
                    string lastupdated_by = header.Created_By;
                    header.LastUpdated_By = lastupdated_by;
                    UpdateEntity(header, context);
                }

                //if can found the exist item, then update others to set status into initial 1
                List<TBL_WAFER_RESUME> resAllList = null;

                if (header.Level == "WF")
                {
                    if (string.IsNullOrEmpty(header.RW_Wafer_Id))
                    {
                        var rwIdList = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id && r.RW_Wafer_Id != "")
                            .Select(r => r.RW_Wafer_Id).Distinct().ToList();
                        if (rwIdList != null && rwIdList.Count() > 0)
                        {
                            resAllList = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id
                            && rwIdList.Contains(r.RW_Wafer_Id) && r.Status == 2).ToList();
                        }
                    }
                }
                else
                {
                    resAllList = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id
                    && r.RW_Wafer_Id == header.RW_Wafer_Id && r.Status == 2).ToList();
                }

                if (resAllList != null && resAllList.Count() > 0)
                {
                    foreach (var resItem in resAllList)
                    {
                        resItem.Status = 1;
                        resItem.Last_Updated_Date = now;
                        resItem.Sys_Updated_Date = now;
                        UpdateEntity(resItem, context);
                    }
                }

                var resItems = context.TBL_WAFER_RESUME_ITEM.Where(r => r.Header_Id == header.Id).ToList();
                foreach (TBL_WAFER_RESUME_ITEM item in inputList)
                {
                    var resItem = resItems.Where(r => r.Key == item.Key && r.Type == item.Type);
                    if (resItem.Any())
                    {
                        var tmp = resItem.FirstOrDefault();
                        tmp.Last_Updated_Date = now;
                        tmp.Value = item.Value;
                        tmp.Status = status;

                        UpdateEntity(tmp, context);
                    }
                    else
                    {
                        item.Id = Guid.NewGuid();
                        item.Header_Id = header.Id;
                        item.Creation_Date = now;
                        item.Last_Updated_Date = now;
                        item.Status = status;
                        context.TBL_WAFER_RESUME_ITEM.Add(item);
                    }
                    //context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                string exStr = string.Format("StackTrace: {0} \n\r Inner Exception: {1}", e.StackTrace, e.InnerException);
                LogHelper.WriteLine(exStr);
                MailHelper.SendMail(string.Empty, null, "Wafer Resume Error!", exStr, true, null, false);

                await _log.PostTBL_WAFER_TXN_LOG(new TBL_WAFER_TXN_LOG()
                {
                    Id = Guid.NewGuid(),
                    Level = "WaferResume",
                    Type = "Exception",
                    Wafer_Id = header.Wafer_Id,
                    RW_Wafer_Id = header.RW_Wafer_Id,
                    Created_By = header.Created_By,
                    Log = string.Format("Exception: [{0}], Attribute1: [{1}]", exStr, data.ToString()),
                    Creation_Date = DateTime.Now
                });
            }
        }

        private async System.Threading.Tasks.Task PostWaferResumeAsync(JObject data, grading_devEntities context)
        {
            var now = DateTime.Now;
            TBL_WAFER_RESUME waferResumeHeader = new TBL_WAFER_RESUME();
            ShippableLogicUtil shipLogicUtil = new ShippableLogicUtil();
            //Deal With Header Data
            try
            {
                waferResumeHeader = data["wafer_header"].ToObject<TBL_WAFER_RESUME>();
                List<TBL_WAFER_RESUME_ITEM> waferResumeItemList = data["wafer_item_list"].ToObject<List<TBL_WAFER_RESUME_ITEM>>();
                LogHelper.WriteLine("wafer_header: " + data["wafer_header"].ToString());

                JSONHelper jsHelper = new JSONHelper();
                string attribute1 = string.Empty;
                if (waferResumeItemList != null && waferResumeItemList.Count > 0)
                {
                    attribute1 = jsHelper.TransferHiveFormat(waferResumeItemList);
                }
                int status = waferResumeHeader.Status == null ? 1 : waferResumeHeader.Status.Value;

                List<TBL_WAFER_RESUME> existList = new List<TBL_WAFER_RESUME>();
                if (string.IsNullOrEmpty(waferResumeHeader.Wafer_Id) && string.IsNullOrEmpty(waferResumeHeader.RW_Wafer_Id))
                {
                    MailHelper.SendMail(string.Empty, null, "Wafer Resume Error!", "Missing Wafer_Id/RW_Wafer_Id!", true, null, false);
                    return;
                }
                //Delete exist header and items
                shipLogicUtil.DeleteWRHeadNItems(waferResumeHeader.Id, context);
            }
            catch (Exception e)
            {
                string exStr = string.Format("StackTrace: {0} \n\r Inner Exception: {1}", e.StackTrace, e.InnerException);
                LogHelper.WriteLine(exStr);
                MailHelper.SendMail(string.Empty, null, "Wafer Resume Error!", exStr, true, null, false);

                await _log.PostTBL_WAFER_TXN_LOG(new TBL_WAFER_TXN_LOG()
                {
                    Id = Guid.NewGuid(),
                    Level = "WaferResume",
                    Type = "Exception",
                    Wafer_Id = waferResumeHeader.Wafer_Id,
                    RW_Wafer_Id = waferResumeHeader.RW_Wafer_Id,
                    Created_By = waferResumeHeader.Created_By,
                    Log = string.Format("Exception: [{0}], Attribute1: [{1}]", exStr, data.ToString()),
                    Creation_Date = DateTime.Now
                });
            }
        }

        // PUT: api/WaferResume/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/WaferResume/5
        public void Delete(int id)
        {
        }

        private List<WaferResumeItem> getWaferResumeItems(List<string> waferList)
        {
            List<WaferResumeItem> itemList = new List<WaferResumeItem>();
            try
            {
                var shipLogicList = _db.TBL_SHIPLOGIC_CONFIG.Where(r => r.Status == 1 && r.IsDisplay == 1).ToList();
                var lookupList = _db.TBL_WAFER_RESUME_LOOKUP.Where(r => r.Type == "ShipLogicGroup").ToList();

                itemList = (from head in _db.TBL_WAFER_RESUME
                            join item in _db.TBL_WAFER_RESUME_ITEM.Where(x => x.Status == 1) on new { head.Id } equals new { Id = item.Header_Id }
                            //join lkup in lookupList on
                            //new { Key = head.Wafer_Id.Substring(0, 5), Attribute1 = head.RW_Wafer_Id.Substring(0, 1) } 
                            //equals new { lkup.Key, lkup.Attribute1 }
                            //join cfg in shipLogicList
                            //on new { item.Type, item.Key, lkup.Value } equals new { Type = cfg.SLCType, Key = cfg.SLCKey, Value = cfg.GroupName }
                            where waferList.Contains(head.Wafer_Id) && string.IsNullOrEmpty(head.RW_Wafer_Id) == false
                            select new WaferResumeItem()
                            {
                                Value = item.Value,
                                Type = item.Type,
                                Key = item.Key,
                                Wafer_Id = head.Wafer_Id,
                                RW_Wafer_Id = head.RW_Wafer_Id
                                //RW_Prefix = lkup.Attribute1
                            }).OrderBy(x => x.Type).ToList();

                var result = (from item in itemList
                           join lkup in lookupList on
                           new { Key = item.Wafer_Id.Substring(0, 5), Attribute1 = item.RW_Wafer_Id.Substring(0, 1) }
                           equals new { lkup.Key, lkup.Attribute1 }
                           join cfg in shipLogicList
                           on new { item.Type, item.Key, lkup.Value } equals new { Type = cfg.SLCType, Key = cfg.SLCKey, Value = cfg.GroupName }
                           select new WaferResumeItem()
                           {
                               Value = item.Value,
                               Type = item.Type,
                               Key = item.Key,
                               Wafer_Id = item.Wafer_Id,
                               RW_Wafer_Id = item.RW_Wafer_Id,
                               RW_Prefix = lkup.Attribute1
                           }).OrderBy(x => x.Type).ToList();


               itemList = (from item in result
                           group new { item } by new
                           {
                               item.Wafer_Id,
                               item.RW_Wafer_Id,
                           } into g
                           select new WaferResumeItem
                           {
                               Wafer_Id = g.Key.Wafer_Id,
                               RW_Wafer_Id = g.Key.RW_Wafer_Id,
                               Box_Id = g.Where(x => x.item.Key == "box_id" && x.item.Type == "WIN").Select(x => x.item.Value).FirstOrDefault(),
                               Good_Die_Qty = g.Where(x => x.item.Key == "good_die_qty" && x.item.Type == "eDoc").Select(x => x.item.Value).FirstOrDefault(),
                               Details = g.ToDictionary(x => x.item.Type + "_" + x.item.Key + "_" + x.item.RW_Prefix, x => x.item.Value)
                           }).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

            return itemList;
        }

        /// <summary>
        /// Once type = "Hold"
        /// </summary>
        /// <param name="inputObjects"></param>
        /// <returns></returns>
        private List<JObject> WaferResumeHoldDataCheck(List<JObject> inputObjects)
        {
            grading_devEntities context = new grading_devEntities();
            try
            {
                foreach (var input in inputObjects)
                {
                    TBL_WAFER_RESUME header = input["wafer_header"].ToObject<TBL_WAFER_RESUME>();
                    List<TBL_WAFER_RESUME_ITEM> inputList = input["wafer_item_list"].ToObject<List<TBL_WAFER_RESUME_ITEM>>();
                    //Get all Wafer Resume items
                    var waferResumeList = context.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == header.Wafer_Id).ToList();
                    //Get RWId List
                    var rwIdList = waferResumeList.Where(r => string.IsNullOrEmpty(r.RW_Wafer_Id) == false).Select(r => r.RW_Wafer_Id).Distinct().ToList();

                    if (waferResumeList.Count() == 0)
                        continue;

                    if (header.Level == "WF") //WF Hold
                    {
                        //get all exist Hold list
                        var existHoldItems = waferResumeList.Where(r => r.Type == "Hold").ToList();
                        if (existHoldItems.Count() == 0)
                        {
                            continue;
                        }
                        else
                        {
                            
                        }
                    }
                    else
                    {
                        //Wafer Hold Item remove
                        var wfHoldItem = waferResumeList.Where(r => r.Wafer_Id == header.Wafer_Id 
                        && string.IsNullOrEmpty(r.RW_Wafer_Id) && r.Type == "Hold").ToList();
                    }
                }
            }
            catch (Exception e)
            {
                string exStr = string.Format("StackTrace: {0} \n\r Inner Exception: {1}", e.StackTrace, e.InnerException);
                LogHelper.WriteLine(exStr);
                MailHelper.SendMail(string.Empty, null, "Wafer Resume Hold Error!", exStr, true, null, false);
            }
            return inputObjects;
        }
    }
}
