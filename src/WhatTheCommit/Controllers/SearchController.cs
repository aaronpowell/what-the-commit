using Microsoft.AspNet.Mvc;
using Microsoft.Framework.ConfigurationModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using WhatTheCommit.Models;
using WhatTheCommit.Models.GitHub;
using WhatTheCommit.Models.GitHub.Search;

namespace WhatTheCommit.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly IConfiguration config;
        public SearchController(IConfiguration config)
        {
            this.config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] SearchQuery query)
        {
            var term = query.Term ?? "language:csharp+sort:updated";

            var repos = await GetRepositories(term.Replace(" ", "+"));

            var tasks = repos.Select(r => GetBlobs(r));

            await Task.WhenAll(tasks);

            var commits = tasks.SelectMany(t => t.Result);

            return Json(new
            {
                term = term,
                commits = commits.OrderByDescending(c => c.Commit.Author.Date)
            });
        }

        private async Task<IEnumerable<Blob>> GetBlobs(Repository repo)
        {
            using (var client = new HttpClient())
            {
                SetupHeaders(client);
                var uri = repo.CommitsUrl.ToString().Replace("{/sha}", "");
                var response = await client.GetAsync(uri);

                var obj = JsonConvert.DeserializeObject<IEnumerable<Blob>>(await response.Content.ReadAsStringAsync());

                if (obj == null)
                    return Enumerable.Empty<Blob>();

                return obj.Select(b =>
                {
                    b.Repository = repo;
                    return b;
                }).Take(5);
            }
        }

        private void SetupHeaders(HttpClient client)
        {
            var identity = (ClaimsIdentity)User.Identity;

            client.DefaultRequestHeaders.Add("User-Agent", config.Get("userAgent"));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + identity.FindFirst(ClaimTypes.Authentication).Value);
        }

        private async Task<IEnumerable<Repository>> GetRepositories(string term)
        {
            using (var handler = new WebRequestHandler())
            {
                handler.ServerCertificateValidationCallback += (sender, cert, chain, ssl) => true;
                using (var client = new HttpClient(handler))
                {
                    SetupHeaders(client);
                    var uri = new Uri(config.Get("serachUrl"));
                    var response = await client.GetAsync(uri + "?q=" + term);

                    var obj = JsonConvert.DeserializeObject<Repositories>(await response.Content.ReadAsStringAsync());

                    return obj.Items.Take(4);
                }
            }
        }
    }
}