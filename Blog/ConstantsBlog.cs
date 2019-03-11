using Blog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog
{
    public static class ConstantsBlog
    {
        public static readonly List<string> AllowedFileExtensions =
            new List<string> { ".jpg", ".jpeg", ".png" };

        public static readonly string UploadFolder = "~/Upload/";

        public static readonly string MappedUploadFolder = HttpContext.Current.Server.MapPath(UploadFolder);
    }
}