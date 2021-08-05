using IndicoRazorFileDemo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IndicoRazorFileDemo.Integrations
{
    public class ResultsStorage
    {
        private string _directory;
        public ResultsStorage(string directory)
        {
            _directory = directory;
        }

        public IEnumerable<string> ListResultsFiles()
        {
            string[] allfiles = Directory.GetFiles(_directory, "*.*", SearchOption.TopDirectoryOnly);
           
            var names = allfiles.Select(x => Path.GetFileNameWithoutExtension(x));
        
            return names;
        }

        public void StoreResultsAsFile(SubmissionResult results)
        {
            var filename = results.SubmissionId + ".txt";
            var path = _directory + "/" + filename;
            File.WriteAllText(path, results.Results);
        }

        public SubmissionResult GetSubmissionResultFromFile(string submissionFileName)
        {
            var path = _directory + "/" + submissionFileName +".txt";
            string text;
            using (var streamReader = new StreamReader(path))
            {
                text = streamReader.ReadToEnd();
            }
            return new SubmissionResult
            {
                Results = text,
                SubmissionId = int.Parse(Path.GetFileNameWithoutExtension(path))
            };
        }
    }
}
