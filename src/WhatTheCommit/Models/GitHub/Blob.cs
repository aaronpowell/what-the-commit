using Newtonsoft.Json;
using System;
using WhatTheCommit.Models.GitHub.Search;

namespace WhatTheCommit.Models.GitHub
{
    public class Blob
    {
        public string Sha { get; set; }
        public Uri Url { get; set; }
        public Commit Commit { get; set; }
        public Repository Repository { get; set; }

        [JsonProperty(PropertyName = "html_url")]
        public string HtmlUrl { get; set; }
    }
}