using System;
using Newtonsoft.Json;



namespace dangercord
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Invalid arguments parsed.\n\n1. dangercord reports <userid>\n2. dangercord cblacklist <userid>\nMore features coming soon\n\n");
                return;
            }

            string toc = args[0];
            ulong userId = ulong.Parse(args[1]);

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


                if (toc == "cblacklist")
                {
                    if (data.badges.blacklisted != null)
                    {
                        Console.WriteLine($"\n{userTag} - {userId} is blacklisted from dangercord");
                    }
                    else
                    {
                        Console.WriteLine($"\n{userTag} - {userId} is not blacklisted from dangercord");
                    }
                }



                else if (toc == "reports")
                {
                    int reportss = data.reports;
                    Console.WriteLine($"\n{userTag} - {userId} has {reportss} reports on dangercord");
                }
            }
        }

        static string ReadApiKeyFromJsonFile(string filePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                dynamic json = JsonConvert.DeserializeObject(jsonContent);
                string apikeythi = $"Bearer {json.apiKey}";
                return apikeythi;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error has occoured reading from your json file the exception is below\n\n{ex.Message}\n\n");
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
