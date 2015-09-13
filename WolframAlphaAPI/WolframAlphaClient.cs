using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WolframAlpha.Api.v2;
using WolframAlpha.Api.v2.Requests;

namespace WolframAlphaAPI
{
    public class WolframAlphaClient
    {
        public int CurrentAnswerIndex = 0;
        
        public bool HasAnswers
        {
            get
            {
                return Answers.Any();
            }
        }

        public string CurrentAnswer
        {
            get
            {
                return Answers[CurrentAnswerIndex];
            }
        }

        public string CurrentImageUrl
        {
            get
            {
                return AnswerImageUrls[CurrentAnswerIndex];
            }
        }


        public List<string> Answers = new List<string>();

        public List<string> AnswerImageUrls = new List<string>();

        public bool QueryInProgress;

        private async Task<string> SendQuery(string query)
        {
            CurrentAnswerIndex = 0;
            Answers.Clear();
            AnswerImageUrls.Clear();
            QueryInProgress = true;
            QueryBuilder builder = new QueryBuilder(new Uri(ApiConstants.QueryBaseUrl));
            builder.Input = query;
            builder.AppId = WolframAlphaConfig.AppId;

            QueryRequest request = new QueryRequest();
            var uri = builder.QueryUri;
            var response = await request.ExecuteAsync(uri);

            if (response == null || response.Pods == null)
            {
                throw new InvalidOperationException("No Results Found.");
            }
                
            foreach (var pod in response.Pods)
            {
                if (pod.SubPods.Length == 0)
                {
                    throw new InvalidOperationException("No Results Found.");
                }
                Answers.Add(pod.SubPods[0].PlainText);
                AnswerImageUrls.Add(pod.SubPods[0].Img.Src);
            }
        
            QueryInProgress = false;

            return CurrentAnswer;
        }

        public async Task<string> SendQueryAsync(string query)
        {
            try
            {
                await SendQuery(query);
            }
            catch
            {
                QueryInProgress = false;
                throw;
            }
            return CurrentAnswer;
        }

    }
}
