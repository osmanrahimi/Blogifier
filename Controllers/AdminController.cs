﻿using Blogifier.Core.Common;
using Blogifier.Core.Data.Domain;
using Blogifier.Core.Data.Interfaces;
using Blogifier.Core.Extensions;
using Blogifier.Core.Middleware;
using Blogifier.Core.Services.FileSystem;
using Blogifier.Models;
using Blogifier.Models.AccountViewModels;
using Blogifier.Models.Admin;
using Blogifier.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blogifier.Controllers
{
    [Authorize]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly string _theme;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger,
            IUnitOfWork db
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = logger;
            _db = db;
            _theme = "~/Views/Blogifier/Admin/" + ApplicationSettings.AdminTheme + "/";
        }

        [VerifyProfile]
        [Route("settings/users")]
        public IActionResult Users(int page = 1)
        {
            var profile = GetProfile();
            if (!profile.IsAdmin)
            {
                return View("~/Views/Blogifier/Blog/" + ApplicationSettings.BlogTheme + "/Error.cshtml", 403);
            }
            var pager = new Pager(page);
            var blogs = _db.Profiles.ProfileList(p => p.Id > 0, pager);

            var model = GetUsersModel();
            model.Blogs = blogs;
            model.Pager = pager;

            ViewData["ShowSendEmailNotification"] = _emailSender.EmailServiceEnabled();

            return View(_theme + "Settings/Users.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("settings/users")]
        public async Task<IActionResult> Register(UsersViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["AdminPage"] = true;
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.RegisterModel.Email, Email = model.RegisterModel.Email };
                var result = await _userManager.CreateAsync(user, model.RegisterModel.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation(string.Format("Created a new account for {0}", user.UserName));

                    // create new profile
                    var profile = new Profile();

                    if (_db.Profiles.All().ToList().Count == 0 || model.RegisterModel.IsAdmin)
                    {
                        profile.IsAdmin = true;
                    }

                    profile.AuthorName = model.RegisterModel.AuthorName;
                    profile.AuthorEmail = model.RegisterModel.Email;
                    profile.Title = "New blog";
                    profile.Description = "New blog description";

                    profile.IdentityName = user.UserName;
                    profile.Slug = SlugFromTitle(profile.AuthorName);
                    profile.Avatar = ApplicationSettings.ProfileAvatar;
                    profile.BlogTheme = ApplicationSettings.BlogTheme;

                    profile.LastUpdated = SystemClock.Now();

                    _db.Profiles.Add(profile);
                    _db.Complete();

                    _logger.LogInformation(string.Format("Created a new profile at /{0}", profile.Slug));

                    var subject = string.Format("Welcome to {0}", ApplicationSettings.Title);
                    var userUrl = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, profile.Slug);
                    var body = string.Format("<p>Thanks for joining!</p><p>Here is URL to your new blog: <a href=\"{0}\">{0}</a></p><p>Welcome and happy blogging!</p>", userUrl);

                    if (model.RegisterModel.SendEmailNotification)
                    {
                        await _emailSender.SendEmailAsync(model.RegisterModel.Email, subject, body);
                    }

                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            var pager = new Pager(1);
            var blogs = _db.Profiles.ProfileList(p => p.Id > 0, pager);

            var regModel = GetUsersModel();
            regModel.Blogs = blogs;
            regModel.Pager = pager;

            return View(_theme + "Settings/Users.cshtml", regModel);
        }


        [HttpDelete("{id}")]
        [Route("settings/users/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var admin = GetProfile();

            if (!admin.IsAdmin || admin.Id == id)
                return NotFound();

            var profile = _db.Profiles.Single(p => p.Id == id);

            _logger.LogInformation(string.Format("Delete blog {0} by {1}", profile.Title, profile.AuthorName));

            var assets = _db.Assets.Find(a => a.ProfileId == id);
            _db.Assets.RemoveRange(assets);
            _db.Complete();
            _logger.LogInformation("Assets deleted");

            var categories = _db.Categories.Find(c => c.ProfileId == id);
            _db.Categories.RemoveRange(categories);
            _db.Complete();
            _logger.LogInformation("Categories deleted");

            var posts = _db.BlogPosts.Find(p => p.ProfileId == id);
            _db.BlogPosts.RemoveRange(posts);
            _db.Complete();
            _logger.LogInformation("Posts deleted");

            var fields = _db.CustomFields.Find(f => f.CustomType == CustomType.Profile && f.ParentId == id);
            _db.CustomFields.RemoveRange(fields);
            _db.Complete();
            _logger.LogInformation("Custom fields deleted");

            var profileToDelete = _db.Profiles.Single(b => b.Id == id);

            var storage = new BlogStorage(profileToDelete.Slug);
            storage.DeleteFolder("");
            _logger.LogInformation("Storage deleted");

            _db.Profiles.Remove(profileToDelete);
            _db.Complete();
            _logger.LogInformation("Profile deleted");

            // remove login

            var user = await _userManager.FindByNameAsync(profile.IdentityName);
            if (user == null)
            {
                throw new ApplicationException($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred removing login for user with ID '{user.Id}'.");
            }
            return new NoContentResult();
        }


        #region Helpers

        private Profile GetProfile()
        {
            return _db.Profiles.Single(b => b.IdentityName == User.Identity.Name);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        string SlugFromTitle(string title)
        {
            var slug = title.ToSlug();
            if (_db.Profiles.Single(b => b.Slug == slug) != null)
            {
                for (int i = 2; i < 100; i++)
                {
                    if (_db.Profiles.Single(b => b.Slug == slug + i.ToString()) == null)
                    {
                        return slug + i.ToString();
                    }
                }
            }
            return slug;
        }

        UsersViewModel GetUsersModel()
        {
            var model = new UsersViewModel
            {
                Profile = GetProfile(),
                RegisterModel = new RegisterViewModel()
            };

            var fields = _db.CustomFields.Find(f => f.CustomType == CustomType.Profile && f.ParentId == model.Profile.Id);
            if (fields != null && fields.Count() > 0)
            {
                foreach (var field in fields)
                {
                    if(field.CustomKey == "SendGridApiKey")
                    {
                        model.RegisterModel.SendGridApiKey = field.CustomValue;
                        break;
                    }
                }
            }
            return model;
        }

        #endregion
    }
}
