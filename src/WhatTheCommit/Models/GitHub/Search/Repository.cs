using Newtonsoft.Json;
using System;

namespace WhatTheCommit.Models.GitHub.Search
{
    public class Repository
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [JsonProperty(PropertyName = "full_name")]
        public string FullName { get; set; }

        public string Description { get; set; }

        [JsonProperty(PropertyName = "html_url")]
        public string HtmlUrl { get; set; }


        [JsonProperty(PropertyName = "commits_url")]
        public Uri CommitsUrl { get; set; }
    }
}