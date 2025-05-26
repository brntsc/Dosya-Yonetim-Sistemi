using Microsoft.AspNetCore.Identity;

namespace Uyg.API.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? PhotoUrl { get; set; }
        public virtual ICollection<FileModel> Files { get; set; }
    }
}
