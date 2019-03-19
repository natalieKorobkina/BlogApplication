using Blog.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Models.ViewModels
{
    public class PostDetailsViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Slug { get; set; }

        [AllowHtml]
        public string Body { get; set; }
        public string UserName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string MediaUrl { get; set; }

        public List<Comment> Comments { get; set; }

        [Required(ErrorMessage = "Please, enter your comment before posting.")]
        public string CommentBody { get; set; }
        public DateTime CommentDateCreated { get; set; }
        public DateTime CommentDateUpdated { get; set; }
        public string CommentEditReason { get; set; }
    }
}