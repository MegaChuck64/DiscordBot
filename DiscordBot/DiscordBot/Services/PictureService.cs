using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class PictureService
    {
        private readonly HttpClient _http;

        public PictureService(HttpClient http)
            => _http = http;

        public async Task<Stream> GetCatPictureAsync()
        {
            var resp = await _http.GetAsync("http://aws.random.cat/meow");
            var json = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
            var filePath = json["file"].Value<string>();
            var px = await _http.GetAsync(filePath);
            return await px.Content.ReadAsStreamAsync();
        }
    }
}
