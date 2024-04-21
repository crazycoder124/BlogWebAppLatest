﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing.Constraints;
using System.IO;
using BlogWebApp.ViewModel;

namespace BlogWebApp.Controllers
{
    //[Route("api/[controller]/[Action]")]
    //[ApiController]
    public class FileController : Controller
    {

        private static IWebHostEnvironment _webHostEnvironment;

        public FileController(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] FileModel filemodel)
        {
            try
            {
                if(filemodel.File.Length > 0)
                {
                    var path = _webHostEnvironment.WebRootPath + "\\uploads\\"; 
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);    
                    }
                    var filename = Guid.NewGuid().ToString() + filemodel.File.FileName;
                    using (FileStream filestream = System.IO.File.Create(path + filename))
                    {
                        filemodel.File.CopyTo(filestream);
                        filestream.Flush();
                        return Ok(filename);
                    }
                }
                else
                {
                    return BadRequest("Failed");
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status417ExpectationFailed, ex.Message);

            }

        }

        [HttpGet("{filename}")]
        public async Task<IActionResult> GetFile([FromRoute] string filename)
        {
            string path = _webHostEnvironment.WebRootPath + "\\uploads\\";
            string filepath = path + filename;
            string ext = Path.GetExtension(filepath);
            if (System.IO.File.Exists(filepath))
            {
                //byte[] b= System.IO.File.ReadAllBytes(filepath);
                return Ok(filepath);    
            }
            return null;
        }

        [HttpDelete("{filename}")]
        public async Task<IActionResult> DeleteFile([FromRoute] string filename) 
        {
            try
            {
                string path = _webHostEnvironment.WebRootPath + "\\uploads\\";
                string filepath = path + filename;
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                    return Ok(filepath);

                }
                return null;
            }
            catch (Exception exp)
            {
                return BadRequest(exp.Message);
            }
        }
   
    }
}
