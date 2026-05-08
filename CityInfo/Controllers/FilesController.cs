using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace CityInfo.Controllers
{
    [Route("api/v{version:apiVersion}/files")] // this route template includes the API version as a route parameter, which allows you to specify the version of the API in the URL when making requests.
                                               // For example, you can make a request to /api/v1/files to access version 1 of the API, or /api/v2/files to access version 2 of the API.
    [Authorize]
    [ApiController]
    public class FilesController : ControllerBase
    {
        // contructor for the FileExtensionContentTypeProvider class. 
        private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider; // This is a service that provides content type information based on file extensions.
                                                                                             // It is used to determine the content type of a file when serving it to clients.
        private readonly IWebHostEnvironment _env; // This is a service that provides information about the hosting environment of the application,
                                                   // such as the content root path and the web root path. It is used to access files and other resources in the application.

        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider, IWebHostEnvironment env) // This is the constructor for the FilesController class.
                                                                                                                           // It takes two parameters: a FileExtensionContentTypeProvider and an IWebHostEnvironment.
        {
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider
                ?? throw new System.ArgumentNullException(
                    nameof(fileExtensionContentTypeProvider));

            _env = env;
        }


        [HttpGet("fileId")]
        public ActionResult GetFile(string fileId)
        {
            var pathToFile = Path.Combine(_env.ContentRootPath, "STAR-Documentation.pdf");
            if (!System.IO.File.Exists(pathToFile))
            {
                return NotFound();
            }

            // content must be set. Use the fileExtensionContentTypeProvider to get the content type based on the file extension. If it fails, use application/octet-stream as the default content type.
            // see this line in the prograbuilder.Services.AddSingleton<FileExtensionContentTypeProvider>();
            // without the DI container, you would need to create an instance of the FileExtensionContentTypeProvider class in the controller constructor and use it to get the content type. With the DI container,
            // you can simply inject it into the controller constructor and use it directly in the action method.
            if (!_fileExtensionContentTypeProvider.TryGetContentType(
                pathToFile, out var contentType)) // set the output type. if it fails, use application/octet-stream as the default content type.
            {
                contentType = "application/octet-stream";
            }

            var bytes = System.IO.File.ReadAllBytes(pathToFile);
            return File(bytes, contentType, Path.GetFileName(pathToFile));
        }


        [HttpPost]
        public async Task<ActionResult> CreateFile(IFormFile file)
        {
            if (file.Length == 0 || file.Length > 20971520 || file.ContentType != "application/pdf")
            {
                return BadRequest("Invalid file. Please upload a non-empty PDF file that is less than 20MB in size.");
            }

            var uploadsFolder = Path.Combine(_env.ContentRootPath, $"CityInfo_{DateTime.Now:yyyyMMdd}");
            //var pathToSave = Path.Combine(_env.ContentRootPath, $"CityFile_{Guid.NewGuid()}_DateTimeNow:yyyyMMDD.pdf");
            // we can also use Directory.GetCurrentDirectory() instead of _env.ContentRootPath, but using the env variable is more robust and works better in different hosting environments.
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            var filePath = Path.Combine(uploadsFolder, $"{file.FileName}_{Guid.NewGuid()}");
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return Ok(new { filePath } + " has been successfully uploaded");
        }
    }
}
