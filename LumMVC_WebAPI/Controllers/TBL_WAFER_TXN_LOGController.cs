using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using LumMVC_WebAPI.Models;
using LumMVC_WebAPI.Utils;
using SystemLibrary.Utility;

namespace LumMVC_WebAPI.Controllers
{
    public class TBL_WAFER_TXN_LOGController : ApiController
    {
        private grading_devEntities db = new grading_devEntities();

        // GET: api/TBL_WAFER_TXN_LOG
        public IQueryable<TBL_WAFER_TXN_LOG> GetTBL_WAFER_TXN_LOG()
        {
            return db.TBL_WAFER_TXN_LOG;
        }

        // GET: api/TBL_WAFER_TXN_LOG/5
        [ResponseType(typeof(TBL_WAFER_TXN_LOG))]
        public async Task<IHttpActionResult> GetTBL_WAFER_TXN_LOG(Guid id)
        {
            TBL_WAFER_TXN_LOG tBL_WAFER_TXN_LOG = await db.TBL_WAFER_TXN_LOG.FindAsync(id);
            if (tBL_WAFER_TXN_LOG == null)
            {
                return NotFound();
            }

            return Ok(tBL_WAFER_TXN_LOG);
        }

        // PUT: api/TBL_WAFER_TXN_LOG/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTBL_WAFER_TXN_LOG(Guid id, TBL_WAFER_TXN_LOG tBL_WAFER_TXN_LOG)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tBL_WAFER_TXN_LOG.Id)
            {
                return BadRequest();
            }

            db.Entry(tBL_WAFER_TXN_LOG).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TBL_WAFER_TXN_LOGExists(id))
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

        // POST: api/TBL_WAFER_TXN_LOG
        [ResponseType(typeof(TBL_WAFER_TXN_LOG))]
        public async Task<IHttpActionResult> PostTBL_WAFER_TXN_LOG(TBL_WAFER_TXN_LOG tBL_WAFER_TXN_LOG)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.TBL_WAFER_TXN_LOG.Add(tBL_WAFER_TXN_LOG);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (TBL_WAFER_TXN_LOGExists(tBL_WAFER_TXN_LOG.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw ex;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                if (TBL_WAFER_TXN_LOGExists(tBL_WAFER_TXN_LOG.Id))
                {
                    return Conflict();
                }
                else
                {
                    var entityError = dbEx.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);
                    var getFullMessage = string.Join("; ", entityError);
                    var exceptionMessage = string.Concat(dbEx.Message, "errors are: ", getFullMessage);
                    //NLog
                    LogHelper.WriteLine(exceptionMessage);
                    throw dbEx;
                }
            }
            catch (Exception e)
            {
                if (TBL_WAFER_TXN_LOGExists(tBL_WAFER_TXN_LOG.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw e;
                }
            }

            //MailHelper.SendMail(string.Empty, null, string.Empty, string.Format("Error message: {0}", tBL_WAFER_TXN_LOG.Log), true, null, false);
            return CreatedAtRoute("DefaultApi", new { id = tBL_WAFER_TXN_LOG.Id }, tBL_WAFER_TXN_LOG);
        }

        // DELETE: api/TBL_WAFER_TXN_LOG/5
        [ResponseType(typeof(TBL_WAFER_TXN_LOG))]
        public async Task<IHttpActionResult> DeleteTBL_WAFER_TXN_LOG(Guid id)
        {
            TBL_WAFER_TXN_LOG tBL_WAFER_TXN_LOG = await db.TBL_WAFER_TXN_LOG.FindAsync(id);
            if (tBL_WAFER_TXN_LOG == null)
            {
                return NotFound();
            }

            db.TBL_WAFER_TXN_LOG.Remove(tBL_WAFER_TXN_LOG);
            await db.SaveChangesAsync();

            return Ok(tBL_WAFER_TXN_LOG);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TBL_WAFER_TXN_LOGExists(Guid id)
        {
            return db.TBL_WAFER_TXN_LOG.Count(e => e.Id == id) > 0;
        }
    }
}