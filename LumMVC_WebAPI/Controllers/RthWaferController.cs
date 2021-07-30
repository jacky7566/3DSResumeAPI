using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OracleClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/RthWafer")]
    public class RthWaferController : ApiController
    {

        [Route("GetRthListByShipmentID/{shipmentId}")]
        [HttpGet]
        public HttpResponseMessage GetRthListByShipmentID(string shipmentId)
        {
            var outputPath = ConfigurationManager.AppSettings["RthSIDOutputPath"].ToString();
            try
            {
                string queryCond = string.Format(@"s.shipmentid = '{0}' ", shipmentId);
                DataTable shipmentDt = GetShipmentList(queryCond);
                var rwList = (from row in shipmentDt.AsEnumerable()
                              select row["rw_wafer_id"].ToString()).Distinct().ToList();
                DataTable rthDt = GetRthResult(rwList);

                string optFile = string.Format(@"{0}\RthAssessment_{1}.csv", outputPath, DateTime.Now.ToString("yyyyMMddHHmmss"));
                if (rthDt.Rows.Count > 0)
                {
                    this.GetRthResultStream(optFile, rthDt);
                    return this.DownloadFile(optFile);
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine(ex.StackTrace);
                throw ex;
            }
        }

        [Route("GetRthListByShipmentDate/{date}")]
        [HttpGet]
        public HttpResponseMessage GetRthListByShipmentDate(string date)
        {
            LogHelper.WriteLine("Start GetRthListByShipmentDate!");            
            try
            {
                var outputPath = ConfigurationManager.AppSettings["RthSDateOutputPath"].ToString();
                DateTime dt = DateTime.Parse(date);
                string dtNewStr = dt.ToString("dd-MMM-yy");
                string queryCond = string.Format(@"s.shipmentdate = TO_DATE('{0}','dd-MON-yy') ", dtNewStr);
                DataTable shipmentDt = GetShipmentList(queryCond);
                var rwList = (from row in shipmentDt.AsEnumerable()
                              select row["rw_wafer_id"].ToString()).Distinct().ToList();
                DataTable rthDt = GetRthResult(rwList);

                string optFile = string.Format(@"{0}\RthAssessment_{1}.csv", outputPath, DateTime.Now.ToString("yyyyMMddHHmmss"));
                if (rthDt.Rows.Count > 0)
                {
                    this.GetRthResultStream(optFile, rthDt);
                    return this.DownloadFile(optFile);
                }
                else return null;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLine(ex.Message);
                LogHelper.WriteLine(ex.StackTrace);
                throw ex;
            }            
        }

        private DataTable GetShipmentList(string condition)
        {
            DBHelper dB = new DBHelper();
            DbConnection eTDataConn = dB.GetOraConn("Etdata_Connection");
            string sql = string.Format(@"select sd.lotnumber rw_wafer_id, s.shipmentid from shipment s
                                        inner join shipmentdetail sd on s.shipmentid = sd.shipmentid
                                        where {0} and sd.lotnumber is not null and s.organizationname = '495'
                                        and s.productname = '22201214' ", condition);
            var dt = dB.GetDataTable(eTDataConn, sql);
            return dt;
        }

        private DataTable GetRthResult(List<string> rwList)
        {
            DBHelper db = new DBHelper();
            DbConnection hiveConn = db.GetDBConn("Hive");
            string sql = string.Format(@"select vwy.rw_wafer_id, rth_category from win.vi2_wafer_yield vwy 
                                         inner join elaser.view_tbl_rth_grading_summary rth on rth.wafer_id = vwy.in_wafer_id
                                        where vwy.rw_wafer_id in ({0}) and rth_category = 'High Rth fallout likely' ", db.List2InCond(rwList));
            var dt = db.GetDataTable(hiveConn, sql);
            return dt;
        }



        // GET: api/RthWafer
        public IEnumerable<string> Get()
        {
            DBHelper dB = new DBHelper();
            DbConnection eTDataConn = dB.GetOraConn("Etdata_Connection");
            var dt = dB.GetDataTable(eTDataConn, "select * from SHIPMENT  Where shipmentid = '13623669' ");

            return new string[] { "value1", "value2" };
        }

        private void GetRthResultStream(string tempFile, DataTable rthDt)
        {
            StreamWriter Swr = new StreamWriter(tempFile);
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();

            foreach (DataColumn dc in rthDt.Columns)
            {
                sb.AppendFormat("{0},", dc.ColumnName);
            }
            sb.Length--;
            Swr.Write(sb.ToString());
            Swr.WriteLine();
            sb = new StringBuilder();

            foreach (DataRow dtr in rthDt.Rows)
            {
                foreach (DataColumn dc in rthDt.Columns)
                {
                    sb.AppendFormat("{0},", dtr[dc].ToString());
                }
                sb.Length--;
                Swr.Write(sb.ToString());
                Swr.WriteLine();
                sb = new StringBuilder();
            }

            Swr.Close();
            Swr.Dispose();
        }

        public HttpResponseMessage DownloadFile(string path)
        {
            //var path = @"C:\Users\anson\Downloads\2017-05-20.txt";
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            response.Content = new StreamContent(stream);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition.FileName = HttpUtility.UrlPathEncode(Path.GetFileName(path));
            response.Content.Headers.ContentLength = stream.Length; //告知瀏覽器下載長度
            return response;
        }
    }
}
