using Blogifier.Core.Common;
using Blogifier.Core.Data.Domain;
using Blogifier.Core.Data.Interfaces;
using Blogifier.Core.Data.Models;
using Blogifier.Core.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blogifier.Controllers
{
    [Authorize]
    [Route("admin")]
    public class AdminController : Controller
    {
        IUnitOfWork _db;
        string _theme;

        public AdminController(IUnitOfWork db)
        {
            _db = db;
            _theme = "~/Views/Blogifier/Admin/" + ApplicationSettings.AdminTheme + "/";
        }

        [VerifyProfile]
        [Route("settings/users")]
        public IActionResult Users(int page = 1)
        {
            var pager = new Pager(page);
            var blogs = _db.Profiles.ProfileList(p => p.Id > 0, pager);

            var model = new AdminApplicationModel
            {
                Profile = GetProfile(),
                Blogs = blogs,
                Pager = pager
            };

            return View(_theme + "Settings/Users.cshtml", model);
        }

        private Profile GetProfile()
        {
            return _db.Profiles.Single(b => b.IdentityName == User.Identity.Name);
        }
    }
}
