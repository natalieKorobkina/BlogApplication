﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Blog.Models.Domain
{
    public class Post
    {
        public int Id { get; set; }

        public virtual ApplicationUser User { get; set; }
        public string UserId { get; set; }

        public virtual List<Comment> Comments { get; set; }

        public string Title { get; set; }
        public string Slug { get; set; }
        public string SubTitle { get; set; }
        public string Body { get; set; }
        public bool Published { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string MediaUrl { get; set; }

        public Post()
        {
            Published = false;
            DateCreated = DateTime.Now;
            DateUpdated = DateTime.Now;
            Comments = new List<Comment>();
        }

    }
}