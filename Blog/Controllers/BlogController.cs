using Blog.Models;
using Blog.Models.Domain;
using Blog.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Blog.Controllers
{
    public class BlogController : Controller
    {
        private ApplicationDbContext DbContext;

        public object GUID { get; private set; }

        public BlogController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            var model = DbContext.Posts.Select(p => new IndexBlogViewModel
            {
                Id = p.Id,
                Title = p.Title,
                SubTitle = p.SubTitle,
                DateCreated = p.DateCreated,
                DateUpdated = p.DateUpdated,
                UserName = p.User.UserName
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public ActionResult PostDetails(int? id)
        {
            if (id.HasValue)
            {
                var post = DbContext.Posts.FirstOrDefault(p => p.Id == id.Value);

                if (post != null)
                {
                    var model = new PostDetailsViewModel();
                    model.Title = post.Title;
                    model.SubTitle = post.SubTitle;
                    model.Body = post.Body;
                    model.UserName = post.User.UserName;
                    model.DateCreated = post.DateCreated;
                    model.DateUpdated = post.DateUpdated;
                    model.MediaUrl = post.MediaUrl;

                    return View(model);
                }
            }
            return RedirectToAction(nameof(BlogController.Index));
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(AddUpdateBlogViewModel formData)
        {
            return AddPostToDatabase(null, formData);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Update(int? id)
        {
            if (id.HasValue)
            {
                var post = DbContext.Posts.FirstOrDefault(p => p.Id == id);

                if (post != null)
                {
                    var model = new AddUpdateBlogViewModel();

                    model.Title = post.Title;
                    model.SubTitle = post.SubTitle;
                    model.Body = post.Body;
                    model.Published = post.Published;
                    model.MediaUrl = post.MediaUrl;

                    return View(model);
                }
            }
            return RedirectToAction(nameof(BlogController.Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Update(int? id, AddUpdateBlogViewModel formData)
        {
            return AddPostToDatabase(id, formData);
        }

        private ActionResult AddPostToDatabase(int? id, AddUpdateBlogViewModel formData)
        {
            //any error with any property
            if (!ModelState.IsValid)
            {
                return View();
            }

            Post post;
            var userId = User.Identity.GetUserId();

            if (!id.HasValue)
            {
                post = new Post();
                post.UserId = userId;
                DbContext.Posts.Add(post);
            }
            else
            {
                post = DbContext.Posts.FirstOrDefault(p => p.Id == id);
                post.DateUpdated = DateTime.Now;

                if (post == null)
                {
                    return RedirectToAction(nameof(BlogController.Index));
                }
            }

            post.Title = formData.Title;
            post.SubTitle = formData.SubTitle;
            post.Body = formData.Body;
            post.Published = formData.Published;

            //Handling file upload and check if file extension is from list of constants - allowed
            string fileExtension;

            if (formData.Media != null)
            {
                fileExtension = Path.GetExtension(formData.Media.FileName);

                if (!ConstantsBlog.AllowedFileExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "File extension is not allowed.");
                    return View();
                }

                //Create directory if it doen't exists
                if (!Directory.Exists(ConstantsBlog.MappedUploadFolder))
                {
                    Directory.CreateDirectory(ConstantsBlog.MappedUploadFolder);
                }
                //Get file name with special method and calculate full path with upload folder which is in constants
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(formData.Media.FileName).ToString();
                //var fileName = formData.Media.FileName;
                var fullPathWithName = ConstantsBlog.MappedUploadFolder + fileName;
                //Actual save on hard disk
                formData.Media.SaveAs(fullPathWithName);
                //Set property with relative path for image
                post.MediaUrl = ConstantsBlog.UploadFolder + fileName;
            }

            DbContext.SaveChanges();

            return RedirectToAction(nameof(BlogController.Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id.HasValue)
            {
                var post = DbContext.Posts.FirstOrDefault(p => p.Id == id);

                if (post != null)
                {
                    DbContext.Posts.Remove(post);
                    DbContext.SaveChanges();
                }
            }

            return RedirectToAction(nameof(BlogController.Index));
        }
    }
}