using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Models.ViewModels
{
    public class PostDetailsViewModel
    {
        public string Title { get; set; }

        [AllowHtml]
        public string Body { get; set; }
        public string UserName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string MediaUrl { get; set; }
    }
}