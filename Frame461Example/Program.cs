using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace Frame461Example
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var token = await GetEligibilityToken();
            var token = GetEligibilityTokenRestSharp();


            if (!string.IsNullOrWhiteSpace(token?.access_token))
            {
                Console.WriteLine($"Token: {token.access_token}");
                Console.WriteLine($"");
                Console.WriteLine($"Shipments: ");

                //var shipments = await GetShipments(token.access_token);

                //foreach (var shipment in shipments)
                //{
                //    Console.WriteLine(shipment.ToString());
                //}

                var shipments = GetShipmentsRestSharp(token.access_token);

                foreach (var shipment in shipments)
                {
                    Console.WriteLine(shipment.ToString());
                }

            }

            Console.ReadLine();
        }

        public static async Task<Token> GetEligibilityToken()
        {
            var tokenUrl = "https://ws.integration.banyantechnology.com/auth/connect/token";

            var form = new Dictionary<string, string>()
            {
                {"grant_type", "client_credentials"},
                {"client_id", "your client id"},
                {"client_secret", "your secret"}
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(tokenUrl, new FormUrlEncodedContent(form));

                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonContent = await response.Content.ReadAsStringAsync();
                var token = JsonConvert.DeserializeObject<Token>(jsonContent);

                return token;
            }
        }

        public static async Task<IEnumerable<BanyanShipment>> GetShipments(string token)
        {
            var shipmentUrl = "https://ws.integration.banyantechnology.com/api/v3/shipments";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                var response = await client.GetAsync(shipmentUrl);

                if (!response.IsSuccessStatusCode)
                    return null;

                var jsonContent = await response.Content.ReadAsStringAsync();
                var shipments = JsonConvert.DeserializeObject<IEnumerable<BanyanShipment>>(jsonContent);

                return shipments;
            }
        }

        public static IEnumerable<BanyanShipment> GetShipmentsRestSharp(string token)
        {
            var shipmentUrl = "https://ws.integration.banyantechnology.com/api/v3/shipments";
             

            var client = new RestClient();
            client.Authenticator = new JwtAuthenticator(token);

            var request = new RestRequest(shipmentUrl);
            var response = client.Get(request);

            return response.IsSuccessful
                ? JsonConvert.DeserializeObject<IEnumerable<BanyanShipment>>(response.Content)
                : null;


        }


        public static Token GetEligibilityTokenRestSharp()
        {
            var tokenUrl = "https://ws.integration.banyantechnology.com/auth/connect/token";

            var client = new RestClient();

            var request = new RestRequest(tokenUrl)
                .AddParameter("grant_type", "client_credentials")
                .AddParameter("client_id", "your client id")
                .AddParameter("client_secret", "your secret");

            var response = client.Post(request);

            return response.IsSuccessful
                ? JsonConvert.DeserializeObject<Token>(response.Content)
                : null;

        }

    }




    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }
    }

    public class BanyanShipment
    {
        public int LoadId { get; set; }
        public string Status { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Load Id: {LoadId}, Status: {Status}";
        }
    }

}
