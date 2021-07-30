﻿using Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace LumMVC_WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AttachmentController : ApiController
    {        
        [HttpPost]
        [Route("api/attachment/upload")]
        public async Task<IHttpActionResult> Upload()
        {
            if (!this.Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            var exists = Directory.Exists(root);
            if (!exists)
            {
                Directory.CreateDirectory("App_Data");
            }

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await this.Request.Content.ReadAsMultipartAsync(provider);

                var uploadResponse = new UploadResponse();
                foreach (var content in provider.Contents)
                {
                    var fileName = content.Headers.ContentDisposition.FileName.Trim('\"');
                    var fileBytes = await content.ReadAsByteArrayAsync();

                    var outputPath = Path.Combine(root, fileName);
                    using (var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        await output.WriteAsync(fileBytes, 0, fileBytes.Length);
                    }

                    uploadResponse.Names.Add(fileName);
                    uploadResponse.FileNames.Add(outputPath);
                    uploadResponse.ContentTypes.Add(content.Headers.ContentType.MediaType);
                }

                return this.Ok(uploadResponse);
            }
            catch (Exception e)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.Message)
                });
            }
        }

        [HttpGet]
        [Route("api/attachment/download")]
        public async Task<IHttpActionResult> Download(string fileName)
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            var exists = Directory.Exists(root);
            if (!exists)
            {
                Directory.CreateDirectory("App_Data");
            }

            var filePath = Path.Combine(root, fileName);
            try
            {
                if (!File.Exists(filePath))
                {
                    return this.NotFound();
                }

                var fileStream = new FileStream(filePath, FileMode.Open);
                var content = new StreamContent(fileStream);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                //content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));
                content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName
                };
                return new ResponseMessageResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = content
                });
            }
            catch (Exception e)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.Message)
                });
            }
        }

        [HttpPost]
        [Route("api/attachment/UploadFormData")]
        public async Task<IHttpActionResult> UploadFormData()
        {
            if (!this.Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            var exists = Directory.Exists(root);
            if (!exists)
            {
                Directory.CreateDirectory("App_Data");
            }

            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                await this.Request.Content.ReadAsMultipartAsync(provider);
                var uploadResponse = new UploadResponse();
                uploadResponse.Description = provider.FormData["description"];

                foreach (var content in provider.FileData)
                {
                    var fileName = content.Headers.ContentDisposition.FileName.Trim('\"');
                    uploadResponse.Names.Add(fileName);
                    uploadResponse.FileNames.Add(content.LocalFileName);
                    uploadResponse.ContentTypes.Add(content.Headers.ContentType.MediaType);
                }

                return this.Ok(uploadResponse);
            }
            catch (Exception e)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(e.Message)
                });
            }
        }
    }
}
