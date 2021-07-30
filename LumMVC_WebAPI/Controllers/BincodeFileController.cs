using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using LumMVC_WebAPI.Models;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Controllers
{
    public class BincodeFileController : ApiController
    {
        private MadridTestEntities db = new MadridTestEntities();

        // GET: api/BincodeFile
        public IQueryable<tbl_bin_code_file_status> Gettbl_bin_code_file_status()
        {
            return db.tbl_bin_code_file_status;
        }

        // GET: api/BincodeFile/5
        [ResponseType(typeof(tbl_bin_code_file_status))]
        public async Task<IHttpActionResult> Gettbl_bin_code_file_status(int id)
        {
            tbl_bin_code_file_status tbl_bin_code_file_status = await db.tbl_bin_code_file_status.FindAsync(id);
            if (tbl_bin_code_file_status == null)
            {
                return NotFound();
            }

            return Ok(tbl_bin_code_file_status);
        }

        // PUT: api/BincodeFile/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Puttbl_bin_code_file_status(int id, tbl_bin_code_file_status tbl_bin_code_file_status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tbl_bin_code_file_status.Id)
            {
                return BadRequest();
            }

            db.Entry(tbl_bin_code_file_status).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!tbl_bin_code_file_statusExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/BincodeFile
        [ResponseType(typeof(tbl_bin_code_file_status))]
        public async Task<IHttpActionResult> Posttbl_bin_code_file_status(tbl_bin_code_file_status tbl_bin_code_file_status)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.tbl_bin_code_file_status.Add(tbl_bin_code_file_status);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = tbl_bin_code_file_status.Id }, tbl_bin_code_file_status);
        }

        // DELETE: api/BincodeFile/5
        [ResponseType(typeof(tbl_bin_code_file_status))]
        public async Task<IHttpActionResult> Deletetbl_bin_code_file_status(int id)
        {
            tbl_bin_code_file_status tbl_bin_code_file_status = await db.tbl_bin_code_file_status.FindAsync(id);
            if (tbl_bin_code_file_status == null)
            {
                return NotFound();
            }

            db.tbl_bin_code_file_status.Remove(tbl_bin_code_file_status);
            await db.SaveChangesAsync();

            return Ok(tbl_bin_code_file_status);
        }

        #region Customize API
        [ResponseType(typeof(List<string>))]
        [Route("api/GetTodayChopListByDate/{date}")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTodayChopListByDate(string date)
        {
            //List<string> list = new List<string>();
            DateTime fromDt = DateTime.Parse(date);
            DateTime toDt = fromDt.AddDays(1);
            var res = from item in db.tbl_bin_code_file_status
                      where item.loaded_date > fromDt && item.loaded_date < toDt
                      && item.is_Completed == true && item.is_Valid == false
                      select item.rw_wafer_id;

            if (res == null) return NotFound();

            return Ok(res.ToList());
        }

        //[ResponseType(typeof(HttpResponseMessage))]
        [Route("api/GetNoChopList")]
        [HttpGet]
        public HttpResponseMessage GetNoChopList()
        {
            var outputPath = ConfigurationManager.AppSettings["BincodeFileOutputPath"].ToString();

            try
            {                
                var notInRes = (from item in db.tbl_bin_code_file_status
                                where item.is_Valid == false
                                && item.App_Mode == "Validate" && item.File_Type == "T_Map"
                                && item.updated_date == (from item2 in db.tbl_bin_code_file_status
                                                         where item2.rw_wafer_id == item.rw_wafer_id
                                                         select item2.updated_date).Max().Value
                                select item.wafer_id).Distinct();

                var resList = (from item in db.tbl_bin_code_file_status
                               where item.App_Mode == "Validate" && item.File_Type == "T_Map"
                               && (notInRes.ToList()).Contains(item.wafer_id)
                               && item.updated_date == (from item2 in db.tbl_bin_code_file_status
                                                        where item2.rw_wafer_id == item.rw_wafer_id
                                                        select item2.updated_date).Max().Value
                               select new NonChopClass
                               {
                                   Wafer_Id = item.wafer_id,
                                   RW_Wafer_Id = item.rw_wafer_id,
                                   Is_Complete = item.is_Completed.Value ? "Y" : "N",
                                   Is_Valid = item.is_Valid.Value ? "Y" : "N",
                                   EPI_Type = item.epi_type,
                                   Updated_Date = item.updated_date.Value
                               }).Distinct().ToList();

                if (resList == null) return null;
                this.SetWATData(resList);
                string tempFile = string.Format(@"{0}\NonChopList_{1}.csv", outputPath, DateTime.Now.ToString("yyyyMMddHHmmss"));
                this.GenNoChopListStream(tempFile, resList);
                return DownloadHd(tempFile);
            }
            catch (Exception e)
            {
                LogHelper.WriteLine(e.StackTrace);
                return null;
            }            
        }

        public HttpResponseMessage DownloadHd(string path)
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

        private void GenNoChopListStream(string tempFile, List<NonChopClass> list)
        {
            StreamWriter Swr = new StreamWriter(tempFile);
            Swr.Write("Total Count: " + list.ToList().Count());
            Swr.WriteLine();
            StringBuilder sb = new StringBuilder();
            sb = new StringBuilder();

            foreach (PropertyInfo pi in list.First().GetType().GetProperties())
            {
                sb.AppendFormat("{0},", pi.Name);
            }
            sb.Length--;
            Swr.Write(sb.ToString());
            Swr.WriteLine();
            sb = new StringBuilder();

            for (int i = 0; i <= list.Count() - 1; i++)
            {
                foreach (PropertyInfo pi in list[i].GetType().GetProperties())
                {
                    sb.AppendFormat("{0},", pi.GetValue(list[i], null));
                }
                sb.Length--;
                Swr.Write(sb.ToString());
                Swr.WriteLine();
                sb = new StringBuilder();
            }
            Swr.Close();
            Swr.Dispose();
        }
        private void SetWATReactor(List<NonChopClass> list)
        {
            var waferList = (from wafer in list
                            select wafer.Wafer_Id).Distinct();

            var watList = (from wat in db.tbl_serial_wat
                          where waferList.Contains(wat.wafer_id)
                          select wat).ToList();
            var watHisListTemp = (from watHis in db.tbl_serial_wat_history
                             where waferList.Contains(watHis.wafer_id)
                             select watHis).ToList();
            var watHisList = (from tt in watHisListTemp
                              select new tbl_serial_wat()
                              {
                                  part_id = tt.part_id,
                                  wafer_id = tt.wafer_id,
                                  loaded_date = tt.loaded_date,
                                  remap_required = tt.remap_required
                              }).ToList();

            var view = watList.Union(watHisList).Distinct().OrderByDescending(r => r.loaded_date).ToList();

            var lastView = (from a in view
                            from b in (
                                (from tbl_serial_wat in view
                                 group tbl_serial_wat by new
                                 {
                                     tbl_serial_wat.part_id
                                 } into g
                                 select new
                                 {
                                     g.Key.part_id,
                                     loaded_date = (DateTime?)g.Max(p => p.loaded_date)
                                 }))
                            where
                              a.part_id == b.part_id &&
                              a.loaded_date == b.loaded_date
                            select a).ToList();
            list.ForEach(x => x.Remap_Required = lastView.Find(y => y.wafer_id == x.Wafer_Id).remap_required);
        }

        private void SetWATData(List<NonChopClass> list)
        {
            var waferList = (from wafer in list
                             select wafer.Wafer_Id).Distinct();

            var view = (from tbl_serial_wat in db.tbl_serial_wat
                        where waferList.Contains(tbl_serial_wat.wafer_id)
                        select new
                        {
                            Id = (tbl_serial_wat.Id.ToString() + "_tbl_serial_wat"),
                            tbl_serial_wat.wafer_id,
                            tbl_serial_wat.epi_reactor,
                            tbl_serial_wat.epi_type,
                            loaded_date = (DateTime?)tbl_serial_wat.loaded_date,
                            tbl_serial_wat.remap_required,
                        }
                        ).Union
                        (
                            from tbl_serial_wat_history in db.tbl_serial_wat_history
                            where waferList.Contains(tbl_serial_wat_history.wafer_id)
                            select new
                            {
                                Id = (tbl_serial_wat_history.Id.ToString() + "_tbl_serial_wat_history"),
                                wafer_id = tbl_serial_wat_history.wafer_id,
                                epi_reactor = tbl_serial_wat_history.epi_reactor,
                                epi_type = tbl_serial_wat_history.epi_type,
                                loaded_date = (DateTime?)tbl_serial_wat_history.loaded_date,
                                remap_required = tbl_serial_wat_history.remap_required,
                            }
                        ).ToList();

            var lastView = (from a in view
                            from b in (
                                (from tbl_serial_wat in view
                                 group tbl_serial_wat by new
                                 {
                                     tbl_serial_wat.wafer_id
                                 } into g
                                 select new
                                 {
                                     g.Key.wafer_id,
                                     loaded_date = (DateTime?)g.Max(p => p.loaded_date)
                                 }))
                            where
                              a.wafer_id == b.wafer_id &&
                              a.loaded_date == b.loaded_date
                            select a).ToList();

          list.ForEach(x => x.Remap_Required = lastView.Find(y => y.wafer_id == x.Wafer_Id).remap_required);
        }
        #endregion



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool tbl_bin_code_file_statusExists(int id)
        {
            return db.tbl_bin_code_file_status.Count(e => e.Id == id) > 0;
        }
    }
}