using Blog.Models;
using Blog.Models.Domain;
using Blog.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Contact";

            return View();
        }

        [HttpPost]
        public ActionResult Contact(ContactViewModel formData)
        {
            var email = new EmailService();

            email.Send("natalie888@mail.ru", formData.Message, formData.Subject);
            ModelState.Clear();

            return View();
        }
    }
}