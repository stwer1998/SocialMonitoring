using Microsoft.AspNetCore.Mvc;
using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Social_Monitoring.Controllers
{
    public class AdminController : Controller
    {
        private readonly ParserManager parserManager;
        public AdminController(ParserManager parserManager)
        {
            this.parserManager = parserManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(parserManager.GetParserSettings());
        }

        [HttpGet]
        public IActionResult Setting(string name)
        {
            return View(parserManager.GetParserSetting(name));
        }
    }
}
