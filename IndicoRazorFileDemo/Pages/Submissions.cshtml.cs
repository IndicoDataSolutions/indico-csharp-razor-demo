using IndicoSDKDemo.Integrations;
using IndicoSDKDemo.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IndicoSDKDemo.Pages
{
    public class SubmissionsModel : PageModel
    {

        private readonly ILogger<SubmissionsModel> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IndicoIntegrations _indico;
        private readonly ResultsStorage _storeage;

        public SubmissionResult selectedResult { get; set; }

        public List<SubmissionResult> allResults { get; set; }


        public SubmissionsModel(ILogger<SubmissionsModel> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
            _storeage = new ResultsStorage($"{ _environment.ContentRootPath }/Results");
            allResults = new List<SubmissionResult>();
        }

        public void OnGet()
        {
            allResults = new List<SubmissionResult>();
            var results = _storeage.ListResultsFiles();
            foreach (var file in results)
            {
                var result = _storeage.GetSubmissionResultFromFile(file);
                allResults.Add(result);
            }
        }


    }
}
