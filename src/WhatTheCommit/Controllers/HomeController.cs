using Microsoft.AspNet.Mvc;

namespace WhatTheCommit.Controllers
{
    public class HomeController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}
