using System;

namespace WhatTheCommit.Models.GitHub
{
    public class Commit
    {
        public string Message { get; set; }
        public Author Author { get; set; }
        public Author Committer { get; set; }
    }
}