using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.Exceptions;
using MinioDockerBucketTest.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MinioDockerBucketTest.Controllers
{
    public class UploadsController : Controller
    {
        private readonly IWebHostEnvironment environment;
        private readonly string minioURL;
        private readonly string minioUser;
        private readonly string minioPwd;
        private readonly string minioBucketName = "test";
        private readonly MinioClient minioClient;

        public UploadsController(IWebHostEnvironment environment, IConfiguration configuration)
        {
            this.environment = environment;
            var minioSettings = configuration.GetSection("MinioSettings");
            minioURL = minioSettings["url"];
            minioUser = minioSettings["user"];
            minioPwd = minioSettings["password"];

            minioClient = new MinioClient(minioURL, minioUser, minioPwd);
        }

        public IActionResult Index()
        {

            var list = minioClient.ListObjectsAsync(minioBucketName, null, true);

            var objectName = new List<string>();
            list.Subscribe(x => objectName.Add(x.Key));

            var userFiles = new List<UserFiles>();

            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                userFiles = context.UserFiles.ToList();
            }

            return View(userFiles);
        }

        public IActionResult Create()
        {
            ViewBag.message = TempData["message"];
            return View();
        }

        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return BadRequest();

            Stream s = null;

            try
            {
                // Check whether the object exists using statObject().
                // If the object is not found, statObject() throws an exception,
                // else it means that the object exists.
                // Execution is successful.
                await minioClient.StatObjectAsync("test", fileName);

                // Get input stream to have content of 'my-objectname' from 'my-bucketname'
                await minioClient.GetObjectAsync("test", fileName,
                                                 (stream) =>
                                                 {

                                                     //stream.CopyTo(s);
                                                     s = stream;
                                                 });
            }
            catch (MinioException e)
            {
                Debug.WriteLine("Error occurred: " + e);
            }

            if (s == null) return BadRequest();

            return new FileStreamResult(s, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }

        public async Task<IActionResult> Upload(UploadModel uploadModel)
        {

            var filePath = Path.Combine(environment.ContentRootPath, "uploads", uploadModel.File.FileName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await uploadModel.File.CopyToAsync(fs);
            }



            //byte[] bs = await System.IO.File.ReadAllBytesAsync(filePath);
            //var ms = new MemoryStream(bs);

            //await minioClient.PutObjectAsync(minioBucketName, uploadModel.File.FileName, ms, ms.Length, "application/octet-stream", null, null);

            string extension = Path.GetExtension(filePath);

            var userFile = new UserFiles
            {
                FileName = uploadModel.File.FileName,
                Url = $"{minioURL}/{minioBucketName}",
                InternalName = Guid.NewGuid() + extension,
            };

            await minioClient.PutObjectAsync(minioBucketName, userFile.InternalName, filePath, uploadModel.File.ContentType);



            System.IO.File.Delete(filePath);


            using (ApplicationDbContext context = new ApplicationDbContext())
            {
                context.UserFiles.Add(userFile);
                context.SaveChanges();
            }

            TempData["message"] = "Archivo subido";
            return RedirectToAction("Index");
        }
    }
}
