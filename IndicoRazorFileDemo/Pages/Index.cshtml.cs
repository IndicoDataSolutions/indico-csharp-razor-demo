using IndicoRazorFileDemo.Integrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IndicoRazorFileDemo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IndicoIntegrations _indico;
        private readonly ResultsStorage _storeage;

        [BindProperty]
        public IFormFile UploadedFile { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IHostEnvironment environment, IndicoIntegrations indico)
        {
            _logger = logger;
            _environment = environment;
            _indico = indico;
            _storeage = new ResultsStorage($"{ _environment.ContentRootPath }/Results");
        }

        public void OnGet()
        {
        }

        //Must create the /Uploads folder as part of the setup process.
        public async Task OnPostAsync()
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                return;
            }

            _logger.LogInformation($"Uploading {UploadedFile.FileName}.");
            string targetFileName = $"{_environment.ContentRootPath}/Uploads/{UploadedFile.FileName}";

            using (var stream = new FileStream(targetFileName, FileMode.Create))
            {
                await UploadedFile.CopyToAsync(stream);
            }

        
            var ids = _indico.SubmitToWorkflow(new List<string> { targetFileName });

            var result = _indico.GetSubmissionResult(ids.First());

            _storeage.StoreResultsAsFile(result);

        }
    }
}
