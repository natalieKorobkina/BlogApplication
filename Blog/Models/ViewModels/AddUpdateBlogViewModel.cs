using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Models.ViewModels
{
    public class AddUpdateBlogViewModel
    {
        [Required(ErrorMessage = "Please, enter title.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please, enter subtitle.")]
        public string SubTitle { get; set; }

        [Required(ErrorMessage = "Please, enter content.")]
        [AllowHtml]
        public string Body { get; set; }

        public bool Published { get; set; }
        public HttpPostedFileBase Media { get; set; }
        public string MediaUrl { get; set; }
    }
}