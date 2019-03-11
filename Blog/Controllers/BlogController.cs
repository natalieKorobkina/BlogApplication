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

        public BlogController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();

            var model = DbContext.Posts
                .Select(p => new IndexBlogViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    DateCreated = p.DateCreated,
                    DateUpdated = p.DateUpdated,
                    UserName = p.User.UserName
                }).ToList();

            return View(model);
        }

        [HttpGet]
        public ActionResult PostDetails(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction(nameof(BlogController.Index));

            var post = DbContext.Posts.FirstOrDefault(p =>
            p.Id == id.Value);

            if (post == null)
                return RedirectToAction(nameof(BlogController.Index));

            var model = new PostDetailsViewModel();
            model.Title = post.Title;
            model.Body = post.Body;
            model.UserName = post.User.UserName;
            model.DateCreated = post.DateCreated;
            model.DateUpdated = post.DateUpdated;
            model.MediaUrl = post.MediaUrl;

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(AddUpdateBlogViewModel formData)
        {
            return SavePost(null, formData);
        }

        [HttpGet]
        public ActionResult Update(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(BlogController.Index));
            }

            var post = DbContext.Posts.FirstOrDefault(
                p => p.Id == id);

            if (post == null)
            {
                return RedirectToAction(nameof(BlogController.Index));
            }

            var model = new AddUpdateBlogViewModel();
            
            model.Title = post.Title;
            model.Body = post.Body;
            model.Published = post.Published;

            return View(model);
        }

        [HttpPost]
        public ActionResult Update(int id, AddUpdateBlogViewModel formData)
        {
            return SavePost(id, formData);
        }

        private ActionResult SavePost(int? id, AddUpdateBlogViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var userId = User.Identity.GetUserId();

            if (DbContext.Posts.Any(p => p.Title == formData.Title && (!id.HasValue || p.Id != id.Value)))
            {
                ModelState.AddModelError(nameof(AddUpdateBlogViewModel.Title),
                    "Title should be unique");

                return View();
            }

            string fileExtension;

            //Validating file upload
            if (formData.Media != null)
            {
                fileExtension = Path.GetExtension(formData.Media.FileName);

                if (!ConstantsBlog.AllowedFileExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "File extension is not allowed.");
                    return View();
                }
            }

            Post post;

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
            post.Body = formData.Body;
            post.Published = formData.Published;

            //Handling file upload
            if (formData.Media != null)
            {
                //Create directory if it doen't exists
                if (!Directory.Exists(ConstantsBlog.MappedUploadFolder))
                {
                    Directory.CreateDirectory(ConstantsBlog.MappedUploadFolder);
                }
                //Get file name with special method and calculate full path with upload folder which is in constants
                var fileName = formData.Media.FileName;
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
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(BlogController.Index));
            }

            var movie = DbContext.Posts.FirstOrDefault(p => p.Id == id);

            if (movie != null)
            {
                DbContext.Posts.Remove(movie);
                DbContext.SaveChanges();
            }

            return RedirectToAction(nameof(BlogController.Index));
        }
    }
}