using LumMVC_WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Utils
{
    public class ShippableLogicUtil
    {
        grading_devEntities _DB = new grading_devEntities();
        List<TBL_SHIPLOGIC_CONFIG> _ShipLogicList = new List<TBL_SHIPLOGIC_CONFIG>();
        List<TBL_WAFER_RESUME_LOOKUP> _ProductSLGroupList = new List<TBL_WAFER_RESUME_LOOKUP>();
        JSONHelper _JsonHelper = new JSONHelper();

        public ShippableLogicUtil()
        {
            this.Initial();
        }

        #region Initial
        private void Initial()
        {
            //Initial Shippable Logic List
            this.GetShipLogicGroup();
            //Get MaskGroup List
            var pList = _DB.TBL_WAFER_RESUME_LOOKUP.Where(r => r.Level == "Product" && r.Type == "ShipLogicGroup" && r.Status == 1);
            if (pList != null && pList.Count() > 0) _ProductSLGroupList = pList.ToList();
            //Update GET_DATE Key
            var getDates = _DB.TBL_SHIPLOGIC_CONFIG.Where(r => r.CmpType == "GET_DATE" && r.Status == 1).ToList();
            string dtimeStr = DateTime.Now.ToString();
            foreach (var item in getDates)
            {
                item.CmpKey = dtimeStr;
                _DB.Entry(item);
            }
            _DB.SaveChanges();
        }
        public void GetShipLogicGroup()
        {
            var list = from item in _DB.TBL_SHIPLOGIC_CONFIG
                       where item.Status == 1 && item.IsCompare == 1
                       select item;
            if (list != null && list.Count() > 0)

                _ShipLogicList = list.ToList();
            else
            {
                MailHelper.SendMail(string.Empty, null, "Shippable Logic not found", "Shippable Logic not found", true, null, false);
            }
        }
        #endregion

        #region GetRawData
        /// <summary>
        /// Get Wafer Resume List
        /// </summary>
        /// <param name="status">0 Disable/1 In-Process/2 Triggered to Oracle/3 Shipped</param>
        /// <returns></returns>
        private List<TBL_WAFER_RESUME> GetWaferResumeListByStatus(int status, string groupType)
        {
            List<string> testList = new List<string>() { "P62111301202" };
            //DateTime dt = DateTime.Now.AddDays(-3);
            var res = (from item in _DB.TBL_WAFER_RESUME
                       join lkItem in _DB.TBL_WAFER_RESUME_LOOKUP on
                       new { Key = item.Wafer_Id.Substring(0, 5), Attribute1 = item.RW_Wafer_Id.Substring(0, 1) } equals new { lkItem.Key, lkItem.Attribute1 }
                       where item.Status == status && lkItem.Value == groupType && item.Level == "RW"
                       //&& item.RW_Wafer_Id.StartsWith("P")
                       //&& item.Last_Updated_Date > dt 
                       && testList.Contains(item.RW_Wafer_Id)
                       orderby item.RW_Wafer_Id
                       select item).ToList();

            LogHelper.WriteLine(string.Format("Status: {0}, Group: {1}, Count: {2}", status, groupType, res.Count()));
            return res;
        }
        #endregion

        #region Main Start
        public void StartShippableLogic()
        {
            List<TBL_SHIPLOGIC_CONFIG> shipLogicCfgList = new List<TBL_SHIPLOGIC_CONFIG>();
            try
            {
                var groupTypes = (from lkItem in _DB.TBL_WAFER_RESUME_LOOKUP
                                  where lkItem.Level == "Product" && lkItem.Type == "ShipLogicGroup" && lkItem.Status == 1
                                  select lkItem.Value).Distinct().ToList(); //Athens-P...

                foreach (var groupType in groupTypes)
                {
                    //Get specific group (ex: WIN/TMapRes/eDoc...)
                    shipLogicCfgList = _ShipLogicList.Where(r => r.GroupName == groupType).Distinct()
                        .OrderBy(r => r.ErrorSeq).OrderBy(r => r.ErrorGroup).ToList();
                    if (shipLogicCfgList.Count() > 0)
                        this.CheckResumeListByGroup(shipLogicCfgList, groupType);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Check Function
        private void CheckResumeListByGroup(List<TBL_SHIPLOGIC_CONFIG> shipLogicCfgList, string groupType)
        {
            List<TBL_WAFER_RESUME> list = null;
            Dictionary<string, string> resultDic = new Dictionary<string, string>();

            try
            {
                list = GetWaferResumeListByStatus(1, groupType); //Get validate list by group type (Athens-P)type's check items
                var rwList = list.Select(x => x.RW_Wafer_Id).Distinct().ToList(); //Distinc RW_Wafer_List
                string curEcGroup = string.Empty;
                string curEcSeq = string.Empty;
                TBL_SHIPLOGIC_CONFIG slConfig = null;
                //bool isFutureHold;
                if (list != null && list.Count() > 0)
                {
                    foreach (string rwId in rwList)
                    {
                        //isFutureHold = false;
                        resultDic = new Dictionary<string, string>(); //Default rw result log
                        var waferId = list.Where(r => r.RW_Wafer_Id == rwId && r.Wafer_Id != "").FirstOrDefault().Wafer_Id;
                        for (int i = 0; i < shipLogicCfgList.Count(); i++)
                        {
                            slConfig = shipLogicCfgList[i];
                            //Looking for Result Log (Check Tbl_WaferResume by RW Base)
                            var item = list.Where(r => r.RW_Wafer_Id == rwId && r.Type == slConfig.SLCType).FirstOrDefault();
                            if (slConfig.Attribute1 == "WF") //item == null && 
                            {
                                //Check if exist data in Wafer Level
                                //waferId = GetWaferIdByRwWaferIdFromExistList(rwId, list);
                                if (string.IsNullOrEmpty(waferId) == false)
                                {
                                    //Get By Wafer First
                                    var wfItem = _DB.TBL_WAFER_RESUME.Where(r => r.Wafer_Id == waferId && r.Level == "WF"
                                    && r.Type == slConfig.SLCType).FirstOrDefault();

                                    if (wfItem != null)
                                    {
                                        if (item == null ? true : item.Last_Updated_Date < wfItem.Last_Updated_Date)
                                        {
                                            item = wfItem;
                                        }
                                        //if (slConfig.SLCKey == "IsHold(Y/N)")
                                        //    isFutureHold = true;
                                    }
                                }
                            }
                            //Check Tbl_WaferResume_Item
                            //CheckWaferResumeItems(item, slConfig.SLCType, groupType, ref resultDic);

                            if (resultDic.Count() > 0
                                && (
                                (slConfig.ErrorGroup.Equals(curEcGroup) == false)
                                || (slConfig.ErrorGroup.Equals(curEcGroup) && slConfig.ErrorSeq.Equals(curEcSeq) == false)
                                ))
                            { break; }
                                
                            curEcGroup = slConfig.ErrorGroup;
                            curEcSeq = slConfig.ErrorSeq;

                            CheckWaferResumeItem(item, slConfig, groupType, ref resultDic);
                        }                        
                        if (this.SaveItemResult(rwId, waferId, resultDic)) //Is Passed
                        {
                            this.UpdateResumeStatus(rwId, 1, 2);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //LogHelper.WriteLine(ex.ToString());
                throw ex;
            }
            
        }
        //private void CheckWaferResumeItems(TBL_WAFER_RESUME queryItem, string checkType, string groupType, ref Dictionary<string, string> dic)
        //{
        //    try
        //    {
        //        //Get Check List (ex: GroupName = 'Athens-P' And SLCType = 'WIN')
        //        var resItemChkList = _ShipLogicList.Where(r => r.GroupName == groupType && r.SLCType == checkType).ToList();
        //        //Get Resume Item by Header (keep need to check list by lookup list)
        //        List<TBL_WAFER_RESUME_ITEM> resItemList = new List<TBL_WAFER_RESUME_ITEM>();
        //        if (queryItem != null) resItemList = _DB.TBL_WAFER_RESUME_ITEM.Where(r => r.Header_Id == queryItem.Id).ToList();

        //        if (resItemChkList != null && resItemChkList.Count() > 0) //Exsits items to check
        //        {
        //            foreach (var chkItem in resItemChkList)
        //            {
        //                var resItem = resItemList.Where(r => r.Type == chkItem.SLCType && r.Key == chkItem.SLCKey).FirstOrDefault();
        //                if (resItem != null)
        //                {
        //                    if (ValidateShipLogic(chkItem, resItem, queryItem.RW_Wafer_Id, queryItem.Wafer_Id) == false)
        //                    {
        //                        if (chkItem.CmpType != null && chkItem.CmpType != "NA" && chkItem.CmpType != "GETDATE")
        //                        {
        //                            dic.Add(chkItem.CmpType + "_" + chkItem.CmpKey, "NA");
        //                        }
        //                        else
        //                            dic.Add(resItem.Type + "_" + resItem.Key, resItem.Value);
        //                    }
        //                }
        //                else
        //                {
        //                    if (chkItem.Attribute2 != "?") //Alert for missing resume item (ex: exp_date missing.)
        //                    {
        //                        dic.Add(chkItem.SLCType + "_" + chkItem.SLCKey, "Missing");
        //                    }
        //                }                            
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.WriteLine(ex.ToString());
        //        throw ex;
        //    }
            
        //}

        private void CheckWaferResumeItem(TBL_WAFER_RESUME queryItem, TBL_SHIPLOGIC_CONFIG slConfig, string groupType, ref Dictionary<string, string> dic)
        {
            try
            {
                //Get Check List (ex: GroupName = 'Athens-P' And SLCType = 'WIN')
                //var resItemChkList = _ShipLogicList.Where(r => r.GroupName == groupType && r.SLCType == checkType).ToList();
                //Get Resume Item by Header (keep need to check list by lookup list)
                TBL_WAFER_RESUME_ITEM resItem = null;
                if (queryItem != null) resItem = _DB.TBL_WAFER_RESUME_ITEM.Where(r => r.Header_Id == queryItem.Id 
                && r.Type == slConfig.SLCType && r.Key == slConfig.SLCKey).FirstOrDefault();

                string errCode = string.Empty;
                //var ecArry = string.IsNullOrEmpty(slConfig.ErrorSeq) ? null : slConfig.ErrorSeq.Split(';');
                errCode = slConfig.SLCType + "_" + slConfig.SLCKey;
                var errReasonArry = string.IsNullOrEmpty(slConfig.DisplayName) ? null : slConfig.DisplayName.Split(';');
                
                if (resItem != null)
                {
                    if (ValidateShipLogic(slConfig, resItem, queryItem.RW_Wafer_Id, queryItem.Wafer_Id) == false)
                    {                        
                        string value = string.Empty;
                        //if (slConfig.CmpType != null && slConfig.CmpType != "NA" && slConfig.CmpType != "GET_DATE")
                        //{
                        //    if (errReasonArry != null && errReasonArry.Count() > 1)
                        //    {
                        //        errCode = errReasonArry[1].ToString();
                        //        //errCode = string.Format("{0}{1} ({2}_{3})", slConfig.ErrorGroup, errReasonArry[1].ToString(), slConfig.SLCType, slConfig.SLCKey);
                        //        //errCode = string.Format("{0}{1}", slConfig.ErrorGroup, ecArry[1].ToString());
                        //    }
                        //    //dic.Add(slConfig.CmpType + "_" + slConfig.CmpKey, "NA");
                        //    value = "NA";
                        //}
                        //else
                        //{
                        //    if (errReasonArry != null && errReasonArry.Count() > 0)
                        //    {
                        //        errCode = errReasonArry[0].ToString();
                        //        //errCode = string.Format("{0}{1} ({2}_{3})", slConfig.ErrorGroup, ecArry[0].ToString(), resItem.Type, resItem.Key);
                        //        //errCode = string.Format("{0}{1}", slConfig.ErrorGroup, ecArry[0].ToString());
                        //    }
                        //    //dic.Add(resItem.Type + "_" + resItem.Key, resItem.Value);
                        //    value = resItem.Value;
                        //}

                        value = string.IsNullOrEmpty(resItem.Value) ? string.Format("Missing {0} info", slConfig.SLCType) : resItem.Value;
                        if (errReasonArry != null && errReasonArry.Count() > 0)
                            errCode = errReasonArry[0].ToString();
                        if (dic.ContainsKey(errCode) == false)
                            dic.Add(errCode, value);
                    }
                }
                else
                {
                    if (slConfig.Attribute2 != "?") //Alert for missing resume item (ex: exp_date missing.)
                    {

                        if (errReasonArry != null)
                        {
                            if (errReasonArry.Count() > 1)
                                errCode = errReasonArry[1].ToString();
                            else
                                errCode = errReasonArry[0].ToString();
                        } 
                        
                        //errCode = string.Format("{0}{1}",slConfig.ErrorGroup, ecArry[0].ToString());
                        //errCode = string.Format("{0} ({1}_{2})", ecArry[0].ToString(), slConfig.SLCType, slConfig.SLCKey);

                        if (dic.ContainsKey(errCode) == false)
                            dic.Add(errCode, string.Format("Missing {0} info", slConfig.SLCType));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("RW Wafer Id: {0}, ex: {1}", queryItem.RW_Wafer_Id, GetAllFootprints(ex)));
            }

        }
        #endregion

        #region Transaction Functions
        private bool SaveItemResult(string rwId, string waferId, Dictionary<string, string> resDic)
        {
            string result = _JsonHelper.JsonTextBuilder(resDic);
            string isPassed = "N";
            try
            {

                if (resDic.Count() == 0 ||
                    (resDic.ContainsKey("IsHold(Y/N)") && resDic.ContainsValue("N")))
                {
                    isPassed = "Y";
                    //result = string.Empty;
                }
                var res = (from item in _DB.TBL_WAFER_RESUME
                           where item.Level == "Ship" && item.Type == "Result"
                           && item.RW_Wafer_Id == rwId && item.Status == 9
                           select item).ToList().FirstOrDefault();

                if (res == null)
                {
                    TBL_WAFER_RESUME resHeaderItem = new TBL_WAFER_RESUME()
                    {
                        Id = Guid.NewGuid(),
                        RW_Wafer_Id = rwId,
                        Wafer_Id = waferId,
                        Level = "Ship",
                        Type = "Result",
                        Attribute1 = isPassed,
                        Attribute2 = result,
                        Status = 9,
                        Creation_Date = DateTime.Now,
                        Last_Updated_Date = DateTime.Now
                    };
                    _DB.TBL_WAFER_RESUME.Add(resHeaderItem);
                }
                else
                {

                    TBL_WAFER_RESUME resHeaderItem = res;
                    if (res.Attribute1 != isPassed)
                    {
                        resHeaderItem.Last_Updated_Date = DateTime.Now;
                        resHeaderItem.Attribute1 = isPassed;
                    }
                    resHeaderItem.Attribute2 = result;
                    resHeaderItem.Sys_Updated_Date = DateTime.Now;
                    _DB.Entry(resHeaderItem);

                }
                int retVal = _DB.SaveChanges();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine(ex.ToString());
                return false;
            }
            
            if (isPassed == "Y") return true;
            else return false;
        }
        public void UpdateResumeStatus(string rwId, int checkStatus, int updateStatus)
        {
            try
            {
                var updateList = _DB.TBL_WAFER_RESUME.Where(r => r.RW_Wafer_Id == rwId && r.Status == checkStatus);
                foreach (var item in updateList)
                {
                    item.Status = updateStatus;
                    _DB.Entry(item);
                }
                _DB.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public void PostWaferResumeItems(Guid headerId, List<TBL_WAFER_RESUME_ITEM> list, grading_devEntities context, int status)
        {
            DateTime now = new DateTime();
            var resItems = context.TBL_WAFER_RESUME_ITEM.Where(r => r.Header_Id == headerId).ToList();
            foreach (TBL_WAFER_RESUME_ITEM item in list)
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
                    item.Header_Id = headerId;
                    item.Creation_Date = now;
                    item.Last_Updated_Date = now;
                    item.Status = status;
                    context.TBL_WAFER_RESUME_ITEM.Add(item);
                }
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

        public void DeleteWRHeadNItems(Guid headerId, grading_devEntities context)
        {
            try
            {
                string sql = string.Format(@"Delete from TBL_WAFER_RESUME WHERE ID = '{0}';
                                            Delete from TBL_WAFER_RESUME_ITEM WHERE HEADER_ID = '{0}'; ", headerId.ToString());
                context.Database.ExecuteSqlCommand(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region Validation Functions
        private bool ValidateShipLogic(TBL_SHIPLOGIC_CONFIG lkupSetting,
            TBL_WAFER_RESUME_ITEM waferResumeItem, string rw_Wafer_Id, string waferId)
        {
            bool isValid = false;

            //Get Parameter Type
            //string checkParaType = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(lkupSetting.CmpParamType))
                    lkupSetting.CmpParamType = typeof(System.String).FullName;
                

                if (lkupSetting.CmpParamType == typeof(System.Decimal).FullName && string.IsNullOrEmpty(waferResumeItem.Value))
                    return false;

                var resumeItemVal = Convert.ChangeType(waferResumeItem.Value, Type.GetType(lkupSetting.CmpParamType)); 
                object lookupVal = null;

                if (lkupSetting.CmpType != "NA" && lkupSetting.CmpType != "GET_DATE")
                {                    
                    lookupVal = (from head in _DB.TBL_WAFER_RESUME
                              join item in _DB.TBL_WAFER_RESUME_ITEM on head.Id equals item.Header_Id
                              where head.RW_Wafer_Id == rw_Wafer_Id & head.Type == lkupSetting.CmpType
                              && item.Key == lkupSetting.CmpKey
                              select item.Value).FirstOrDefault();
                    if (lookupVal != null)
                    {
                        // 2021/04/19 Jacky added: If CmpCondtion equals to "=?" that means we can allow it equals to and empty value
                        if (lkupSetting.CmpCondtion == "=?" && lookupVal.GetType() == typeof(string))
                        {
                            //if empty, will allow directly by pass
                            if (lookupVal.ToString() == "")
                                return true;
                        }
                        lookupVal = Convert.ChangeType(lookupVal, Type.GetType(lkupSetting.CmpParamType));
                    }
                    else
                    {
                        //waferResumeItem.Value = string.Format("Missing {0}_{1}", lkupSetting.SLCType, lkupSetting.SLCKey);
                        //waferResumeItem.Value = string.Format("Missing {0} info", lkupSetting.SLCType);
                        if (lkupSetting.Attribute2 == "?") // Special case for old data without final qty
                            return true;
                        return false;
                    }
                }
                else
                {
                    lookupVal = Convert.ChangeType(lkupSetting.CmpKey, Type.GetType(lkupSetting.CmpParamType));
                }                    

                isValid = this.CompareFunction(resumeItemVal, lookupVal, lkupSetting);

                //Check MiniQty
                if (isValid && string.IsNullOrEmpty(lkupSetting.Attribute1) == false && lkupSetting.Attribute1 == "Validate")
                    isValid = this.ValidateRWMinQty(lkupSetting, waferId, rw_Wafer_Id, resumeItemVal);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("RW Wafer Id: {0}, ex: {1}", rw_Wafer_Id, GetAllFootprints(e)));
            }            
            return isValid;
        }
        private bool CompareFunction(object srcVal, object refVal, TBL_SHIPLOGIC_CONFIG lkupSetting)
        {
            var genericListType = typeof(Operation<>);
            try
            {
                var specificListType = genericListType.MakeGenericType(Type.GetType(lkupSetting.CmpParamType));
                object instance = Activator.CreateInstance(specificListType);
                var method = instance.GetType().GetMethod("Compare"); //this function return bool
                var parameters = new object[] { lkupSetting.CmpCondtion, refVal, srcVal };
                return (bool)method.Invoke(instance, parameters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool ValidateRWMinQty(TBL_SHIPLOGIC_CONFIG lkupSetting, string waferId, string rw_Wafer_Id, object beCheckVal)
        {
            try
            {
                string maskPrefix = waferId.Substring(0, 5);
                string rwPrefix = rw_Wafer_Id.Substring(0, 1);
                decimal minQty = 0;
                decimal checkQty = 0;
                var minQtyObj = _DB.TBL_WAFER_RESUME_LOOKUP.Where(
                    r => r.Level == lkupSetting.Attribute1 && r.Type == "RW_MinQty"
                    && r.Key == maskPrefix && r.Attribute1 == rwPrefix && r.Status == 1).FirstOrDefault();
                if (minQtyObj != null)
                {
                    decimal.TryParse(minQtyObj.Value, out minQty);
                    decimal.TryParse(beCheckVal.ToString(), out checkQty);

                    if (checkQty < minQty) return false;
                }
                else
                {
                    //If MP product did not setup the minimum Qty, get default value for the MP checking
                    minQtyObj = _DB.TBL_WAFER_RESUME_LOOKUP.Where(
                    r => r.Level == lkupSetting.Attribute1 && r.Type == "RW_MinQty"
                    && r.Key == "DEFAULT" && r.Attribute1 == rwPrefix && r.Status == 1).FirstOrDefault();
                    if (minQtyObj != null)
                    {
                        decimal.TryParse(minQtyObj.Value, out minQty);
                        decimal.TryParse(beCheckVal.ToString(), out checkQty);
                        if (checkQty < minQty) return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetWaferIdByRwWaferIdFromExistList(string rwWaferId, List<TBL_WAFER_RESUME> list)
        {
            try
            {
                var res = list.Where(r => r.RW_Wafer_Id == rwWaferId).FirstOrDefault();
                if (res != null)
                    return res.Wafer_Id;
                else return string.Empty;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetAllFootprints(Exception x)
        {
            var st = new StackTrace(x, true);
            var frames = st.GetFrames();
            var traceString = new StringBuilder();

            foreach (var frame in frames)
            {
                if (frame.GetFileLineNumber() < 1)
                    continue;

                //traceString.Append("File: " + frame.GetFileName());
                traceString.Append("Method:" + frame.GetMethod().Name);
                traceString.Append(", LineNumber: " + frame.GetFileLineNumber());
                traceString.Append("  -->  ");
                traceString.AppendLine();
            }
            traceString.Append("Message: " + x.Message);
            traceString.AppendLine();
            traceString.Append("StackTrace: " + x.StackTrace);
            traceString.AppendLine();
            traceString.Append("InnerException: " + x.InnerException);
            traceString.AppendLine();

            return traceString.ToString();
        }
        #endregion
    }
}