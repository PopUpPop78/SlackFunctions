using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Data;

namespace SlackFunctions
{
    public class SlackTimerTrigger
    {
        private const string SlackUrl = "https://hooks.slack.com/services/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";
        private const string StackOverviewUrl = "https://api.stackexchange.com/2.3/search?fromdate={0}&order=desc&sort=activity&intitle=rcs&site=stackoverflow";

        private readonly ILogger<SlackTimerTrigger> _log;

        public SlackTimerTrigger(ILogger<SlackTimerTrigger> log)
        {
            _log = log;
        }

        [FunctionName("SlackTimerTrigger")]
        public async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer)
        {
            var json = await MakeStackOverflowRequest();
            var jsonObj = JsonSerializer.Deserialize<Root>(json);

            await MakeSlackRequest($"Hello with stack-overflow call. Number of questions: {jsonObj.Items.Count}");
            _log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }

        public async Task<string> MakeSlackRequest(string message)
        {
            using(var client = new HttpClient())
            {
                var request = new StringContent("{'text':'" + message + "'}", Encoding.UTF8, "application/json");
                var response = await client.PostAsync(SlackUrl, request);

                var result = await response.Content.ReadAsStringAsync();
                _log.LogInformation(result);

                return result;
            }
        }

        public async Task<string> MakeStackOverflowRequest()
        {
            // Last 24 hours
            var epochTime = (int)DateTime.UtcNow.AddDays(-1).Subtract(new DateTime(1970,1,1)).TotalSeconds;

            var clientHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            };

            using (var client = new HttpClient(clientHandler))
            {
                var response = await client.GetAsync(string.Format(StackOverviewUrl, epochTime));

                var result = await response.Content.ReadAsStringAsync();
                _log.LogInformation(result);

                return result;
            }
        }
    }
    public class Item
    {
        [JsonPropertyName("tags")]
        public List<string> Tags { get; set; }

        [JsonPropertyName("owner")]
        public Owner Owner { get; set; }

        [JsonPropertyName("is_answered")]
        public bool IsAnswered { get; set; }

        [JsonPropertyName("view_count")]
        public int ViewCount { get; set; }

        [JsonPropertyName("answer_count")]
        public int AnswerCount { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("last_activity_date")]
        public int LastActivityDate { get; set; }

        [JsonPropertyName("creation_date")]
        public int CreationDate { get; set; }

        [JsonPropertyName("question_id")]
        public int QuestionId { get; set; }

        [JsonPropertyName("content_license")]
        public string ContentLicense { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }

    public class Owner
    {
        [JsonPropertyName("account_id")]
        public int AccountId { get; set; }

        [JsonPropertyName("reputation")]
        public int Reputation { get; set; }

        [JsonPropertyName("user_id")]
        public int UserId { get; set; }

        [JsonPropertyName("user_type")]
        public string UserType { get; set; }

        [JsonPropertyName("profile_image")]
        public string ProfileImage { get; set; }

        [JsonPropertyName("display_name")]
        public string DisplayName { get; set; }

        [JsonPropertyName("link")]
        public string Link { get; set; }
    }

    public class Root
    {
        [JsonPropertyName("items")]
        public List<Item> Items { get; set; }
    }
}
