﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Frame461Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var token = await GetEligibilityToken();

            if (!string.IsNullOrWhiteSpace(token?.access_token))
            {
                Console.WriteLine($"Token: {token.access_token}");
                Console.WriteLine($"");
                Console.WriteLine($"Shipments: ");

                var shipments = await GetShipments(token.access_token);

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
                {"grant_type","client_credentials"},
                {"client_id","your client id"},
                {"client_secret","your client secret"}
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
