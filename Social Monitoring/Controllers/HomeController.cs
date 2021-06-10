using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models;
using Parser;
using Social_Monitoring.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Social_Monitoring.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ParserManager parserManager; 

        public HomeController(ILogger<HomeController> logger, ParserManager parserManager)
        {
            _logger = logger;
            this.parserManager = parserManager;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string inn)
        {
            if (inn.Length == 10)
            {
                return RedirectToAction("Legal", new { inn  });
            }
            else if (inn.Length == 12)
            {
                return RedirectToAction($"Phsical", new { inn });
            }
            else
            {
                return Index();
            }
        }

        [HttpGet]       
        public IActionResult Legal(string inn)
        {
               
            return View(parserManager.GetLegalOrParse(inn));
        }

        [HttpGet]
        public IActionResult Phsical(string inn)
        {            
            return View(parserManager.GetPhysicalOrParse(inn));
        }

        [HttpPost]
        public IActionResult Phsical(PhysicalPerson physicalPerson)
        {
            
            return View(parserManager.GetPhysicalOrParse(physicalPerson));
        }

        [HttpPost]
        public IActionResult Subscribe(string inn)
        {
            parserManager.Subscribe(inn, User.Identity.Name);
            return RedirectToAction("Monitoring");
        }

        [HttpPost]
        public IActionResult Unsubscribe(string inn)
        {
            parserManager.UnSubscribe(inn, User.Identity.Name);

            return RedirectToAction("Monitoring");
        }

        [HttpGet]
        public IActionResult Monitoring() 
        {
            return View(parserManager.Monitoring(User.Identity.Name));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
