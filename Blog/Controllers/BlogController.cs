using Blog.Models;
using Blog.Models.Domain;
using Blog.Models.ViewModels;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        public ActionResult Index(string searchString)
        {
            ViewBag.Message = "All Posts";

            var model = DbContext.Posts.Select(p => new IndexBlogViewModel
            {
                Id = p.Id,
                Title = p.Title,
                SubTitle = p.SubTitle,
                Body = p.Body,
                DateCreated = p.DateCreated,
                DateUpdated = p.DateUpdated,
                UserName = p.User.UserName,
                Published = p.Published,
                Slug = p.Slug,
                MediaUrl = p.MediaUrl
            }).ToList();

            // filter for non-admins
            var filteredModel = model.FindAll(p => p.Published == true);
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(DbContext));
            var currentUser = User.Identity.GetUserId();
  
            if (currentUser != null && userManager.IsInRole(currentUser, "admin"))
                return Search(searchString, model);

            return Search(searchString, filteredModel);
        }

        private ActionResult Search(string searchString, List<IndexBlogViewModel> listForSearch)
        {
            if (!String.IsNullOrEmpty(searchString))
            {
                var lowerCaseSearch = searchString.ToLower();
                var searchedList = listForSearch.FindAll(p => p.Title.ToLower().Contains(lowerCaseSearch) ||
                p.Slug.ToLower().Contains(lowerCaseSearch) ||
                p.Body.ToLower().Contains(lowerCaseSearch));

                return View(searchedList);
            }

            return View(listForSearch);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        [Route("Blog/Create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("Blog/Create")]
        public ActionResult Create(AddUpdateBlogViewModel formData)
        {
            ViewBag.Message = "Add New Post";

            return AddPostToDatabase(null, formData);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Update(int? id)
        {
            ViewBag.Message = "Update Post";

            if (id.HasValue)
            {
                var post = DbContext.Posts.FirstOrDefault(p => p.Id == id.Value);

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
                post.Title = formData.Title;
                post.Slug = post.SlugCalculation(post);
            }
            else
            {
                post = DbContext.Posts.FirstOrDefault(p => p.Id == id);

                if (post == null)
                {
                    return RedirectToAction(nameof(BlogController.Index));
                }

                post.UserId = userId;
                post.Title = formData.Title;
                post.DateUpdated = DateTime.Now;
            }

            post.SubTitle = formData.SubTitle;
            post.Body = formData.Body;
            post.Published = formData.Published;

            return FileUpload(post, formData);
        }

        private ActionResult FileUpload(Post post, AddUpdateBlogViewModel formData)
        {
            //Handling file upload and check if file extension is from list of constants - allowed
            string fileExtension;

            if (formData.Media != null)
            {
                fileExtension = Path.GetExtension(formData.Media.FileName).ToLower();

                if (!ConstantsBlog.AllowedFileExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("", "File extension is not allowed.");
                    return View();
                }

                //Create directory if it doesn't exists
                if (!Directory.Exists(ConstantsBlog.MappedUploadFolder))
                {
                    Directory.CreateDirectory(ConstantsBlog.MappedUploadFolder);
                }

                //Get file name with special method and calculate full path with upload folder which is in constants
                var fileName = Guid.NewGuid().ToString() + fileExtension;
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
                var post = DbContext.Posts.FirstOrDefault(p => p.Id == id.Value);

                if (post != null)
                {
                    DbContext.Posts.Remove(post);
                    DbContext.SaveChanges();
                }
            }

            return RedirectToAction(nameof(BlogController.Index));
        }

        [HttpGet]
        [Route("blog/{slug}")]
        public ActionResult PostDetails(string slug)
        {
            ViewBag.Message = "Post Details";

            if (!ModelState.IsValid)
            {
                return View();
            }

            if (!string.IsNullOrWhiteSpace(slug))
            {
                var post = DbContext.Posts.FirstOrDefault(p => p.Slug == slug);

                if (post != null)
                {
                    return GetPost(post);
                }
            }
            return RedirectToAction(nameof(BlogController.Index));
        }

        [HttpPost]
        [Authorize]
        [Route("blog/{slug}")]
        public ActionResult PostDetails(string slug, PostDetailsViewModel formData)
        {
            if (!String.IsNullOrEmpty(slug))
            {
                var post = DbContext.Posts.FirstOrDefault(p => p.Slug == slug);

                if (post != null)
                {
                    if (!ModelState.IsValid)
                    {
                        return GetPost(post);
                    }

                    Comment comment;
                    var userId = User.Identity.GetUserId();

                    comment = new Comment();
                    comment.UserId = userId;
                    comment.Body = formData.CommentBody;
                    DbContext.Comments.Add(comment);
                    post.Comments.Add(comment);

                    DbContext.SaveChanges();

                    return RedirectToAction(nameof(BlogController.PostDetails));
                }
            }

            return RedirectToAction(nameof(BlogController.Index));
        }

        private ActionResult GetPost(Post post)
        {
            var model = new PostDetailsViewModel();

            model.Id = post.Id;
            model.Title = post.Title;
            model.SubTitle = post.SubTitle;
            model.Body = post.Body;
            model.UserName = post.User.UserName;
            model.DateCreated = post.DateCreated;
            model.DateUpdated = post.DateUpdated;
            model.MediaUrl = post.MediaUrl;
            model.Comments = post.Comments;
            model.Slug = post.Slug;

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult UpdateComment(int? id, int? postId)
        {
            ViewBag.Message = "Edit Comment";

            if (id.HasValue && postId.HasValue)
            {
                var comment = DbContext.Comments.FirstOrDefault(p => p.Id == id.Value);

                if (comment != null)
                {
                    var model = new EditCommentViewModel();

                    model.Body = comment.Body;
                    model.EditReason = comment.UpdatedReason;

                    if (model.EditReason != null)
                    {
                        model.EditReason = comment.UpdatedReason;
                    }

                    return View(model);
                }
            }
            return RedirectToAction(nameof(BlogController.Index));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult UpdateComment(int? id, int? postId, EditCommentViewModel formData)
        {
            if (!ModelState.IsValid)
                return View();

            Comment comment;
            var userId = User.Identity.GetUserId();
            var post = DbContext.Posts.FirstOrDefault(p => p.Id == postId);
            comment = DbContext.Comments.FirstOrDefault(p => p.Id == id);

            if (comment == null)
                return RedirectToAction(nameof(BlogController.Index));

            comment.UserId = userId;
            comment.Body = formData.Body;
            comment.DateUpdated = DateTime.Now;
            comment.UpdatedReason = formData.EditReason;

            DbContext.SaveChanges();

            return RedirectToAction(nameof(BlogController.PostDetails), routeValues: new { slug = post.Slug });
        }

        //[HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult DeleteComment(int? id, int? postId)
        {
            if (id.HasValue && postId.HasValue)
            {
                var comment = DbContext.Comments.FirstOrDefault(p => p.Id == id.Value);
                var post = DbContext.Posts.FirstOrDefault(p => p.Id == postId.Value);

                if (comment != null)
                {
                    DbContext.Comments.Remove(comment);
                    DbContext.SaveChanges();
                }
                return RedirectToAction(nameof(BlogController.PostDetails), routeValues: new { slug = post.Slug });
            }

            return RedirectToAction(nameof(BlogController.Index));
        }
    }
}