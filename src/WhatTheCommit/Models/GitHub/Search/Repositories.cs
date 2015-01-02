using System.Collections.Generic;

namespace WhatTheCommit.Models.GitHub.Search
{
    public class Repositories
    {
        public int TotalCount { get; set; }
        public IEnumerable<Repository> Items { get; set; }
    }
}