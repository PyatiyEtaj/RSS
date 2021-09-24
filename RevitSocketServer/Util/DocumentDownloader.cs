using ExtensionLib.Util;
using SocketServerEntities.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
namespace RevitSocketServer.Util
{
    public class DocumentDownloader
    {
        private class DocumentInfo
        {
            public string Urn { get; set; }
            public DateTime DateTime { get; set; }
            public string FilePath { get; set; }
        }

        private readonly HttpClient _client = new HttpClient();
        private readonly WebClient _webClient = new WebClient();

        private readonly List<DocumentInfo> _docs = new List<DocumentInfo>();
        public bool IsDocChanged { get; private set; } = default;

        private (string key, string name) GetKeyNameFromUrn(string urn)
        {
            var s1 = urn.Split(':');
            if (s1.Length >= 2)
            {
                var s2 = s1[s1.Length - 1].Split('/');
                if (s2.Length >= 2)
                {
                    return (s2[0], s2[1]);
                }
            }
            return (default, default);
        }

        private async Task<HttpResponseMessage> GetResponseFrom(DocumentInfo doc, string url, string authorization)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("Authorization", authorization);
            if (doc != default)
                req.Headers.Add("If-Modified-Since", doc.DateTime.ToUniversalTime().ToString("r"));

            return await _client.SendAsync(req);
        }
        private void DownloadByWebClient(string url, string authorization, string path)
        {
            _webClient.Headers.Clear();
            _webClient.Headers.Add("Authorization", authorization);
            _webClient.DownloadFile(new System.Uri(url), path);
        }

        private async Task<bool> IsFileModified(DocumentInfo doc, string url, string authorization)
        {
            var response = await GetResponseFrom(doc, $"{url}/details", authorization);
            Logger.WriteInfo($"status={response.StatusCode} info={response.Content}");
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NotModified)
                throw new GetBim360FileException($"status={response.StatusCode} info={response.Content}");
            return response.StatusCode != HttpStatusCode.NotModified;
        }

        public async Task<string> DownloadAsync(string urn, string authorization, string path)
        {
            (var key, var name) = GetKeyNameFromUrn(urn);
            var doc = _docs.FirstOrDefault(x => x.Urn == urn);
            var url = $"https://developer.api.autodesk.com/oss/v2/buckets/{key}/objects/{name}";

            var isModified = await IsFileModified(doc, url, authorization);
            string file = isModified
                ? Path.Combine(path, $"{key}_{name}")
                : doc.FilePath;

            if (isModified)
            {
                DownloadByWebClient(url, authorization, file);
                if (doc == default)
                {
                    doc = new DocumentInfo();
                    _docs.Add(doc);
                }
                doc.FilePath = file;
                doc.DateTime = DateTime.Now;
                doc.Urn = urn;
            }

            return file;
        }
    }
}
