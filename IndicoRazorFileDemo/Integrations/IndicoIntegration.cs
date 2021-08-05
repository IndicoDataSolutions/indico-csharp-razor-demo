using IndicoV2.Extensions.SubmissionResult;
using IndicoV2.Storage;
using IndicoV2.Submissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndicoV2;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using IndicoSDKDemo.Models;
using IndicoV2.Submissions.Models;
using System.Threading;
using System.IO;
using Newtonsoft.Json.Linq;

//This is the bulk of the indico-specific integrations. Simplified and straightforward.
//Some setup is needed in Index.cshtml/Index.cs, Program.cs, appsettings.json, and Startup.cs to fully set this up
namespace IndicoSDKDemo.Integrations
{
    public class IndicoIntegrations
    {

        private ISubmissionsClient _submissionsClient;
        private ISubmissionResultAwaiter _submissionResultAwaiter;
        private IStorageClient _storageClient;
        private int _workflowId;

        //Create this integration class to handle calling and returning various indico functions
        //These values are done with dependency injection + read from appsettings.json
        public IndicoIntegrations(IConfiguration config)
        {
            var token = config["IndicoApiKey"];
            var uri = config["IndicoEnv"];
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Token and url are required.");
            }

            if (!uri.StartsWith("http"))
            {
                throw new ArgumentException("Url must start with http or https");
            }

            var client = new IndicoV2.IndicoClient(token, new Uri(uri));
            _submissionsClient = client.Submissions();
            _submissionResultAwaiter = client.GetSubmissionResultAwaiter();
            _storageClient = client.Storage();
            _workflowId = int.Parse(config["IndicoWorkflow"]);

        }



        public IEnumerable<int> SubmitToWorkflow(List<string> filepaths)
        {
            try
            {
                var ids = Task.Run(async () => await _submissionsClient.CreateAsync(_workflowId, filepaths)).GetAwaiter().GetResult().ToArray();
                return ids;
            }
            catch (Exception ex)
            {
                //add extra logging or handling here as fit
                throw new Exception(ex.Message);
            }

        }

        public SubmissionResult GetSubmissionResult(int submissionId)
        {

            //there are two ways to do this, one is looping while stauts == pending, and then manually
            //fetch the file yourself... the other
            //is to use the _submissionAwaiter which will wait until the submission is out of PENDING or not FAILED
            
            var status = Task.Run(async () => await _submissionsClient.GetAsync(submissionId)).Result;

            while(status.Status == SubmissionStatus.PROCESSING)
            {
                //wait for result
                status = Task.Run(async () => await _submissionsClient.GetAsync(submissionId)).Result;
            }
            if(status.Status == SubmissionStatus.FAILED)
            {
                throw new Exception("Submission failed! " + status.Errors);
            }
           var resultUri = new Uri(new Uri("indico-file://"), status.ResultFile);
            CancellationToken cancellationToken = default;

            var submissionResultsFile = Task.Run(async () => await _storageClient.GetAsync(resultUri, cancellationToken)).GetAwaiter().GetResult();

            using var reader = new JsonTextReader(new StreamReader(submissionResultsFile));
            var result = JsonSerializer.Create().Deserialize<JObject>(reader);
            var errors = ((JArray)result["errors"]).Select(x => x.ToString());
            //Note: what json is returned here depends heavily on the type of worklfow (extraction vs clasification) and
            //The labels being extracted and if review is on/off
            //So for the demo: we will save and display the raw json result
            var raw_results = result["results"]["document"]["results"];

            var workflow_sub = new SubmissionResult()
            {
                Results = raw_results.ToString(Formatting.Indented),
                SubmissionId = submissionId,
                 
            };
            return workflow_sub;
        }
    }
}
