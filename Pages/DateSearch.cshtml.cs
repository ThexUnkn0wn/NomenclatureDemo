using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using NomenclatureDemo.Data;
using NomenclatureDemo.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace NomenclatureDemo.Pages
{
    public class SearchModel : PageModel
    {

        public List<ArticleViewModel> articleViewModel;

        //Filters
        [BindProperty]
        [Required(ErrorMessage ="Please enter a date.")]
        public DateTime DateFilter { get; set; }= DateTime.Now.Date;

        [BindProperty]
        public string OrderBy { get; set; }




        private readonly ApplicationDbContext _context;
        public SearchModel(ApplicationDbContext context)
        {
            _context = context;

        }

        
        public void OnGet()
        {
            if (articleViewModel == null)
            {
                articleViewModel = new List<ArticleViewModel>() { };
                return;
            }

        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) { return Page(); }
            var query = (from a in _context.Articles
                         join ap in _context.ArticlePropertys on a.Code equals ap.Code
                         select new ArticleViewModel { ArticleVm = a, ArticlePropertysVm = ap }
                      ).AsQueryable();
            query = query.Where(a => a.ArticleVm.State.Equals("ACTIVE") && (a.ArticlePropertysVm.End_Date >= DateFilter || a.ArticlePropertysVm.Start_Date <= DateFilter) );

            switch(OrderBy)
            {
                case "Name":
                    query = query.OrderBy(a => a.ArticleVm.Name);
                    break;
                case "Code":
                    query = query.OrderBy(a => a.ArticleVm.Code);
                    break;
                case "Acqu.Price":
                    query = query.OrderBy(a => a.ArticlePropertysVm.Acquisition_Price);
                    break;
                case "Price":
                    query = query.OrderBy(a => a.ArticlePropertysVm.Full_Price);
                    break;
                default:
                break;
            }
            articleViewModel = query.ToList();
            if(articleViewModel==null)
                articleViewModel=new List<ArticleViewModel>() { };  

            return Page();
                
            
            
        }
    }
}
