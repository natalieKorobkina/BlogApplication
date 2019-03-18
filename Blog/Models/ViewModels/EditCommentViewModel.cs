using Blog.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Blog.Models.ViewModels
{
    public class EditCommentViewModel
    {
        [Required(ErrorMessage = "Please, enter your comment.")]
        public string Body { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }

        [Required(ErrorMessage = "Please, provide the reason.")]
        public string EditReason { get; set; }
    }
}