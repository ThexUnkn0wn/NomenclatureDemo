using Microsoft.EntityFrameworkCore;
using NomenclatureDemo.Model;
using NomenclatureDemo.Models;

namespace NomenclatureDemo.Data
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticlePropertys> ArticlePropertys { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
    }
}
