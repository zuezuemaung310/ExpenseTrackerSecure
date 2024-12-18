﻿using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/NotFound")]
        public IActionResult NotFound()
        {
            return View(); 
        }
    }
}
