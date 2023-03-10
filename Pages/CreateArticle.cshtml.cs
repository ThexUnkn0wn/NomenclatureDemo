using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NomenclatureDemo.Data;
using NomenclatureDemo.Model;
using NomenclatureDemo.Models;

namespace NomenclatureDemo.Pages
{
    public class CreateArticleModel : PageModel
    {
        [BindProperty]
        public ArticleViewModel ArticleViewModel { get; set; }

        [BindProperty(SupportsGet =true)]
        public string? StatusMessage { get; set; }


        private readonly ApplicationDbContext _context;
        private readonly ILogger<CreateArticleModel> _logger;

        public CreateArticleModel(ApplicationDbContext context, ILogger<CreateArticleModel> logger) 
        { 
            _context = context;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost() 
        {
            if (ModelState.IsValid)
            {
                if(!isDataValid())
                    return Page();

                prepareData();
                int result = insertArticleIntoDB();
                
                if (result>0) 
                {
                    StatusMessage = "The new article was created";
                    return RedirectToPage(new { StatusMessage });

                }
                else if(result!=-4)
                {
                    ModelState.AddModelError("", "Failed to create the new article! Unexpected exception!");
                    return Page();
                }
            }

            return Page();
        }


        private bool isDataValid() 
        {
            //DB validation checks
            bool nameAlreadyExists = _context.Articles.Where(row => row.Name.Equals(ArticleViewModel.ArticleVm.Name)).Count() > 0;
            if (nameAlreadyExists)
            {
                ModelState.AddModelError("", "Article name already exists.");
                return false;
            }

            bool codeAlreadyExists = _context.Articles.Where(row => row.Code.Equals(ArticleViewModel.ArticleVm.Code.ToUpper())).Count() > 0;
            if (codeAlreadyExists)
            {
                ModelState.AddModelError("", "Article code already exists.");
                return false;
            }

            return true;
        }


        private void prepareData()
        {
            ArticleViewModel.ArticleVm.Code = ArticleViewModel.ArticleVm.Code.ToUpper();

            ArticleViewModel.ArticlePropertysVm.Code = ArticleViewModel.ArticleVm.Code;

            ArticleViewModel.ArticlePropertysVm.TVA /= 100f;
            ArticleViewModel.ArticlePropertysVm.TVA=Math.Round(ArticleViewModel.ArticlePropertysVm.TVA, 4);
            //Full_price= Acqu.Cost+Acqu.Cost*TVA
            ArticleViewModel.ArticlePropertysVm.Full_Price = (ArticleViewModel.ArticlePropertysVm.Acquisition_Price == null) ? 0 :
                                                            (ArticleViewModel.ArticlePropertysVm.Acquisition_Price +
                                                            ArticleViewModel.ArticlePropertysVm.Acquisition_Price * ArticleViewModel.ArticlePropertysVm.TVA);
        }

        private int insertArticleIntoDB()
        {
            try
            {
                _context.Articles.Add(new Article
            {
                Name = ArticleViewModel.ArticleVm.Name,
                Code = ArticleViewModel.ArticleVm.Code,
                State = ArticleViewModel.ArticleVm.State
            });
                _context.SaveChanges();
            _context.ArticlePropertys.Add(new ArticlePropertys
            {
                Code = ArticleViewModel.ArticlePropertysVm.Code,
                TVA = ArticleViewModel.ArticlePropertysVm.TVA,
                Acquisition_Price = ArticleViewModel.ArticlePropertysVm.Acquisition_Price,
                Full_Price = ArticleViewModel.ArticlePropertysVm.Full_Price,
                Start_Date = ArticleViewModel.ArticlePropertysVm.Start_Date,
                End_Date = ArticleViewModel?.ArticlePropertysVm.End_Date
            });
                return _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in post(CreateArticle)");
                ModelState.AddModelError("", "Failed to create the new article! Exception: "+ex.Message);
                return -4;
            }
        }



    }//class CreateArticleModel
}
