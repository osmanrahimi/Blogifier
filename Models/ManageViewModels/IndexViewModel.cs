using Blogifier.Core.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace Blogifier.Models.ManageViewModels
{
    public class IndexViewModel : AdminBaseModel
    {
        public string Username { get; set; }

        public bool IsEmailConfirmed { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        public string StatusMessage { get; set; }
    }
}
