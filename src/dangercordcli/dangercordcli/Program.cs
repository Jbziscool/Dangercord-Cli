using System;
using System.ComponentModel.Design;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dangercord
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid arguments parsed.\n\n1. dangercord creports <userid>\n2. dangercord cblacklist <userid>\n3. dangercord report <userid> <reason>\n\nMore features coming soon\n\n");
                return;
            }

            string toc = args[0];
            ulong userId = ulong.Parse(args[1]);
            string reasoncliarg = string.Join(" ", args[2..]);

            string apiKey = ReadApiKeyFromJsonFile("config.json");

            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("API key not found or invalid. Please check the configuration.");
                return;
            }

            string userTag = await GetUserTagViaUserIdAsync(userId, apiKey);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", apiKey);
                var url = $"https://dangercord.com/api/v1/user/{userId}";
                var response = await client.GetAsync(url);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(jsonResponse);

                switch (toc)
                {



                    case "cblacklist":
                        if (data.badges.blacklisted != null)
                        {
                            Console.WriteLine($"\n{userTag} - {userId} is blacklisted from dangercord");
                        }
                        else
                        {
                            Console.WriteLine($"\n{userTag} - {userId} is not blacklisted from dangercord");
                        }
                        break;



                    case "creports":
                        int reportss = data.reports;
                        Console.WriteLine($"\n{userTag} - {userId} has {reportss} reports on dangercord");
                        break;



                    case "report":
                        var createUrl = $"https://dangercord.com/api/v1/user/{userId}/report";
                        var payload = new { reason = reasoncliarg };
                        var jsonPayload = JsonConvert.SerializeObject(payload);
                        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                        var createResponse = await client.PostAsync(createUrl, content);
                        var createJsonResponse = await createResponse.Content.ReadAsStringAsync();
                        dynamic jsonresponse = JsonConvert.DeserializeObject(createJsonResponse);

                        if (jsonresponse == null)
                        {
                            Console.WriteLine("No response.");
                        }
                        else if (jsonresponse.error != null)
                        {
                            string errorMessage = jsonresponse.message;
                            Console.WriteLine($"ERROR\nError message: {errorMessage} ");
                        }
                        else
                        {
                            Console.WriteLine($"\n{userTag} - {userId} has been reported to the API for: {reasoncliarg}");
                        }
                        break;



                }
            }
        }



        static string ReadApiKeyFromJsonFile(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                dynamic json = JsonConvert.DeserializeObject(jsonContent);
                string apiKey = $"Bearer {json.apiKey}";
                return apiKey;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error has occurred while reading from your JSON file. The exception is below:\n\n{ex.Message}\n\n");
                return null;
            }
        }



        static async Task<string> GetUserTagViaUserIdAsync(ulong userId, string apiKey)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", apiKey);
                string url = $"https://dangercord.com/api/v1/user/{userId}";
                HttpResponseMessage response = await client.GetAsync(url);
                string jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(jsonResponse);
                
                string username = data.username;
                string discriminator = data.discriminator;
                string userTag = $"{username}#{discriminator}";

                return userTag;
            }
        }
    }
}
//END
