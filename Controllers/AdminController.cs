using Blogifier.Core.Common;
using Blogifier.Core.Data.Domain;
using Blogifier.Core.Data.Interfaces;
using Blogifier.Core.Extensions;
using Blogifier.Core.Middleware;
using Blogifier.Models;
using Blogifier.Models.AccountViewModels;
using Blogifier.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger _logger;
        private readonly string _theme;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger,
            IUnitOfWork db
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
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

            var model = new UsersViewModel
            {
                Profile = profile,
                RegisterModel = new RegisterViewModel(),
                Blogs = blogs,
                Pager = pager
            };

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

                    await _signInManager.SignInAsync(user, isPersistent: false);

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

                    return RedirectToLocal(returnUrl);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            var pager = new Pager(1);
            var blogs = _db.Profiles.ProfileList(p => p.Id > 0, pager);

            var regModel = new UsersViewModel
            {
                Profile = GetProfile(),
                RegisterModel = new RegisterViewModel(),
                Blogs = blogs,
                Pager = pager
            };
            return View(_theme + "Settings/Users.cshtml", regModel);
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

        #endregion
    }
}
