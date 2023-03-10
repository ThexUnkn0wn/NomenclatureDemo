using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using NomenclatureDemo.Data;
using NomenclatureDemo.Model;
using NomenclatureDemo.Models;
using System.Net.NetworkInformation;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Text.RegularExpressions;

namespace NomenclatureDemo.Pages
{
    

    public class BrowseModel : PageModel
    {
        public List<ArticleViewModel> articleViewModel;

        private readonly ApplicationDbContext _context;

        //Filters
        [BindProperty(SupportsGet = true)]
        public string? SearchValue { get; set; }

        [BindProperty(SupportsGet =true)]
        public string StateFilter { get; set; } = "ALL";


        //PageNav  <<Prev Next>>
        [BindProperty(SupportsGet = true)]
        public int lastId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Direction { get; set; }

        [TempData]
        public int tempNextId { get; set; }
        [TempData]
        public int tempPrevId { get; set; }





        public BrowseModel(ApplicationDbContext context)
        {
            _context = context;
            
        }


        public IActionResult OnGet()
        {
            var query = (from a in _context.Articles
                     join ap in _context.ArticlePropertys on a.Code equals ap.Code
                         select new ArticleViewModel { ArticleVm = a, ArticlePropertysVm = ap }
                      ).AsQueryable();
            string pattern = @"^[0-9]+$";
            if (!SearchValue.IsNullOrEmpty())
            {
                if (Regex.IsMatch(SearchValue, pattern, RegexOptions.IgnoreCase))
                {
                    query = query.Where(a => a.ArticleVm.Id.Equals(int.Parse(SearchValue)));
                }
                else
                {
                    query = query.Where(a => a.ArticleVm.Name.Contains(SearchValue) || a.ArticleVm.Code.ToUpper().Equals(SearchValue.ToUpper()));
                }
            }
            if (!StateFilter.ToUpper().Equals("ALL"))
                query = query.Where(a => a.ArticleVm.State.ToUpper().Equals(StateFilter.ToUpper()));

            try
            {
                if(query.First()==null);
            }
            catch {
                articleViewModel = new List<ArticleViewModel>() { };
                return Page();
            }

            if(Direction.IsNullOrEmpty() || Direction == "next"||lastId< query.First().ArticleVm.Id)
            {
                articleViewModel = (query.Where(a => a.ArticleVm.Id > lastId).Take(10)).ToList();
                if (articleViewModel.IsNullOrEmpty())
                    TempData["tempPrevId"] = TempData["tempNextId"];

            }
            else if(Direction=="prev")
            {
                query = query.OrderByDescending(a => a.ArticleVm.Id);
                articleViewModel = (query.Where(a => a.ArticleVm.Id < lastId+1).Take(10)).OrderBy(a =>a.ArticleVm.Id).ToList();
            }
            
            if (!articleViewModel.IsNullOrEmpty()&& !(articleViewModel.Count() < 10) )
            {
                
                lastId = articleViewModel.Last().ArticleVm.Id;
            }
            if(!articleViewModel.IsNullOrEmpty()&& articleViewModel.First()!=null)
                TempData["tempPrevId"] = Convert.ToInt32(articleViewModel.First().ArticleVm.Id) - 1;
            TempData["tempNextId"] = lastId;

            TempData.Keep("tempPrevId");
            TempData.Keep("tempNextId");
            return Page();
        }


        public IActionResult OnPostNext()
        {
            Direction = "next";
            lastId = TempData["tempNextId"] == null ? 0 : (int)TempData["tempNextId"];
            if (ModelState.IsValid == false)
            {
                return Page();
            }
            return RedirectToPage(new { StateFilter, SearchValue, lastId, Direction });
        }

        public IActionResult OnPostPrev()
        {
            Direction = "prev";
            lastId = TempData["tempPrevId"] == null ? 0 : (int)TempData["tempPrevId"];
            if (ModelState.IsValid == false)
            {
                return Page();
            }
            return RedirectToPage(new { StateFilter, SearchValue, lastId, Direction });
        }
        public IActionResult OnPost()
        {
            if (ModelState.IsValid == false)
            {
                return Page();
            }
            return RedirectToPage(new { StateFilter, SearchValue });
        }

    }
}
