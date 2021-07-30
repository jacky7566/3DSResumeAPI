using LumMVC_WebAPI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LumMVC_WebAPI.Controllers
{
    public class WaferItemsController : ApiController
    {
        grading_devEntities db = new grading_devEntities();
        // GET: api/WaferItems
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/WaferItems/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/WaferItems
        public void Post([FromBody]JObject data)
        {
            //TBL_WAFER_HEADER header = data["wafer_header"].ToObject<TBL_WAFER_HEADER>();
            //List<TBL_WAFER_ITEM> inputList = data["wafer_item_list"].ToObject<List<TBL_WAFER_ITEM>>();

            //if (header.Type.Equals("eDoc"))
            //{
            //    var existItem = db.TBL_WAFER_HEADER.Where(r => r.Wafer_Id == header.Wafer_Id & r.Type == "Chop");
            //    if (existItem != null && string.IsNullOrEmpty(header.Attribute1) == false)
            //    {
            //        List<TBL_WAFER_ITEM> list = JObject.Parse(header.Attribute1).ToObject<List<TBL_WAFER_ITEM>>();
            //        var existChopRec = list.Find(r => r.Type == "CBPNP2");
            //        if (existChopRec != null) //如果找的到chopping記錄，就往下展延
            //        {

            //        }
            //        else
            //        {

            //        }
            //    }
            //}
            //var existItem = db.TBL_WAFER_HEADER.Where(r => r.Wafer_Id == header.Wafer_Id).ToList();

            //if (existItem != null)
            //{
            //    //判斷是否為RW Base
            //    if (string.IsNullOrEmpty(existItem.First().RW_Wafer_Id) == false)
            //    {

            //    }
            //    else
            //    {

            //    }
            //}
            ////List<TBL_WAFER_ITEM> list = data["wafer_item_list"].ToObject<List<TBL_WAFER_ITEM>>();
            //db.TBL_WAFER_HEADER.Add(header);
            //try
            //{
            //    header.Creation_Date = DateTime.Now;
            //    header.Last_Upadted_Date = DateTime.Now;
            //    header.Attribute1 = data["wafer_item_list"].ToString();
            //    header.Status = 1;
            //    db.SaveChanges();
            //    //db.Entry<TBL_WAFER_HEADER>(header).Reload();
            //}
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            //foreach (TBL_WAFER_ITEM item in list)
            //{
            //    item.Header_Id = header.Id;
            //    item.Creation_Date = DateTime.Now;
            //    item.Last_Updated_Date = DateTime.Now;
            //    db.TBL_WAFER_ITEM.Add(item);
            //    db.SaveChanges();
            //}
        }

        // PUT: api/WaferItems/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/WaferItems/5
        public void Delete(int id)
        {
        }
    }
}
