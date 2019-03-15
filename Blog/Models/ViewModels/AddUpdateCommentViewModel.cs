using Blog.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models.ViewModels
{
    public class AddUpdateCommentViewModel
    {
        [Required(ErrorMessage = "Please, enter your comment.")]
        public string Body { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string EditReason { get; set; }
    }
}