using System;

namespace WhatTheCommit.Models.GitHub
{
    public class Author
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTimeOffset Date { get; set; }
    }
}