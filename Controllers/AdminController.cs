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
        public IActionResult Import()
        {
            return View(_theme + "Settings/Users.cshtml", new AdminBaseModel { Profile = GetProfile() });
        }

        private Profile GetProfile()
        {
            return _db.Profiles.Single(b => b.IdentityName == User.Identity.Name);
        }
    }
}
