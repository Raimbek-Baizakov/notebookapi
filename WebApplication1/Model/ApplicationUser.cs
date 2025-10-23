using Microsoft.AspNetCore.Identity;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Notebook>? Notebooks { get; set; }
    }
}
