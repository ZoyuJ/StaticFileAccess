namespace StaticFileAccess.Controllers {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using StaticFileAccess.Models;

    public class HomeController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly IOptions<StaticFileServCfgs> FilePathes;
        public HomeController(ILogger<HomeController> logger, IOptions<StaticFileServCfgs> FilePathes) {
            _logger = logger;
            this.FilePathes = FilePathes;
        }

        public IActionResult Index() {
            
            return View(FilePathes.Value.Pathes.Select(E => E.RootUrl).Distinct().ToArray());
        }
        [HttpGet]
        public IActionResult Upload() {
            return View();
        }
        [HttpPost]
        public IActionResult UploadFile(IFormFile Upload) {
            if (Upload != null) {
                using (var FS = System.IO.File.Create(System.IO.Path.Combine(FilePathes.Value.UploadTo, Upload.FileName))) {
                    using (var UploadStm = Upload.OpenReadStream()) {
                        UploadStm.CopyTo(FS);
                    }
                }
            }
            return RedirectToAction("Upload");
        }

    }
}
