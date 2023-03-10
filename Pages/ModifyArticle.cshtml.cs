using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NomenclatureDemo.Data;
using NomenclatureDemo.Model;
using NomenclatureDemo.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace NomenclatureDemo.Pages
{
    public class ModifyArticleModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? SearchValue { get; set; }

        [BindProperty(SupportsGet =true)]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public ArticleViewModel ArticleViewModel { get; set; }

        [TempData]
        public string CurentArticleName { get; set; }
        [TempData]
        public string CurentArticleCode { get; set; }

        private string? _curentArticleName;
        private string? _curentArticleCode;


        private readonly ApplicationDbContext _context;
        private readonly ILogger<ModifyArticleModel> _logger;

        public ModifyArticleModel(ApplicationDbContext context, ILogger<ModifyArticleModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void OnGet()
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
            else
            {
                return;
            }

            try
            {
                ArticleViewModel = query.First();
            }
            catch (Exception e)
            {
                ModelState.AddModelError("SearchValue", "Article not found. Exception: " + e.Message);
                _logger.LogError(e, "Exception in post(GetArticleForUpdate)");
                return;
            }

            ArticleViewModel.ArticlePropertysVm.TVA *= 100;
            ArticleViewModel.ArticlePropertysVm.TVA = Math.Round((double)ArticleViewModel.ArticlePropertysVm.TVA, 2);
            TempData["ArticleId"] = ArticleViewModel?.ArticleVm?.Id;
            TempData["CurentArticleName"] = ArticleViewModel?.ArticleVm?.Name;
            TempData["CurentArticleCode"] = ArticleViewModel?.ArticleVm?.Code;
        }

        public IActionResult OnPostUpdate() 
        {
            

            _curentArticleName = TempData["CurentArticleName"]?.ToString();
            _curentArticleCode = TempData["CurentArticleCode"]?.ToString();

            if(_curentArticleName==null || _curentArticleCode == null)
            {
                ModelState.AddModelError("SearchValue", "Please enter an article name, code or id.");
                return Page();
            }


            if (ModelState.IsValid)
            {
                if (!isDataValid())
                    return Page();

                prepareData();

                

                int result = updateArticleIntoDB(ArticleViewModel);

                if (result > 0)
                {
                    StatusMessage = "The article was successfully updated.";
                    return RedirectToPage(new { StatusMessage });

                }
                else if (result < 0)
                {
                    
                    return Page();
                }

            }

            return Page();
        }

        public IActionResult OnPostDelete()
        {
            _curentArticleName = TempData["CurentArticleName"]?.ToString();
            _curentArticleCode = TempData["CurentArticleCode"]?.ToString();

            if (ModelState.IsValid)
            {
                if (_curentArticleName == null || _curentArticleCode == null)
                {
                    ModelState.AddModelError("SearchValue", "Please enter an article name, code or id.");
                    return Page();
                }


                int result = deleteArticleFromDB(_curentArticleCode);

                if (result > 0)
                {
                    StatusMessage = "The article was successfully deleted.";
                    return RedirectToPage(new { StatusMessage });

                }
                else if (result < 0)
                {
                    return Page();
                }
            }

            return Page();

        }

        public IActionResult OnPostSearch()
        {
            if(SearchValue.IsNullOrEmpty())
            {
                ModelState.AddModelError("SearchValue", "Please enter an article name, code or id.");
                return Page();
            }

            return RedirectToPage(new { SearchValue });
        }

        private bool isDataValid()
        {
            //DB validation checks
            if (!_curentArticleName.Equals(ArticleViewModel.ArticleVm.Name))
            {
                bool nameAlreadyExists = _context.Articles.Where(row => row.Name.Equals(ArticleViewModel.ArticleVm.Name)).Count() > 0;
                if (nameAlreadyExists)
                {
                    ModelState.AddModelError("", "Article name already exists.");
                    return false;
                }
            }

            if (!_curentArticleCode.Equals(ArticleViewModel.ArticleVm.Code))
            {
                bool codeAlreadyExists = _context.Articles.Where(row => row.Code.Equals(ArticleViewModel.ArticleVm.Code.ToUpper())).Count() > 0;
                if (codeAlreadyExists)
                {
                    ModelState.AddModelError("", "Article code already exists.");
                    return false;
                }
            }

            return true;
        }

        private void prepareData()
        {
            ArticleViewModel.ArticleVm.Code = ArticleViewModel.ArticleVm.Code.ToUpper();

            ArticleViewModel.ArticlePropertysVm.Code = ArticleViewModel.ArticleVm.Code;

            ArticleViewModel.ArticlePropertysVm.TVA /= 100;
            ArticleViewModel.ArticlePropertysVm.TVA = Math.Round((double)ArticleViewModel.ArticlePropertysVm.TVA, 4);

            //Full_price= Acqu.Cost+Acqu.Cost*TVA
            ArticleViewModel.ArticlePropertysVm.Full_Price = (ArticleViewModel.ArticlePropertysVm.Acquisition_Price == null) ? 0 :
                                                            (ArticleViewModel.ArticlePropertysVm.Acquisition_Price +
                                                            ArticleViewModel.ArticlePropertysVm.Acquisition_Price * ArticleViewModel.ArticlePropertysVm.TVA);
        }

        private int updateArticleIntoDB(ArticleViewModel ArticleViewModel)
        {

            Article tempArticle;
            ArticlePropertys tempArticlePropertys;
            try
            {
                tempArticle = _context.Articles.Single(a => a.Code == _curentArticleCode);
                tempArticle.Name = ArticleViewModel.ArticleVm.Name;
                tempArticle.Code = ArticleViewModel.ArticleVm.Code;
                tempArticle.State = ArticleViewModel.ArticleVm.State;
                _context.Articles.Update(tempArticle);
                _context.SaveChanges();
                tempArticlePropertys = _context.ArticlePropertys.Single(ap => ap.Code == tempArticle.Code);
                tempArticlePropertys.TVA = ArticleViewModel.ArticlePropertysVm.TVA;
                tempArticlePropertys.Acquisition_Price = ArticleViewModel.ArticlePropertysVm.Acquisition_Price;
                tempArticlePropertys.Full_Price = ArticleViewModel.ArticlePropertysVm.Full_Price;
                tempArticlePropertys.Start_Date = ArticleViewModel.ArticlePropertysVm.Start_Date;
                tempArticlePropertys.End_Date = ArticleViewModel.ArticlePropertysVm.End_Date;
                _context.ArticlePropertys.Update(tempArticlePropertys);
                return _context.SaveChanges();

            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", "Article not found. Exception: " + ex.Message);
                _logger.LogError(ex, "Exception in post(UpdateArticle)");
                return -3;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Exception in post(UpdateArticle)");
                ModelState.AddModelError("", "Failed to update article! Exception: " + ex.Message);
                return -3;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Exception in post(UpdateArticle)");
                ModelState.AddModelError("", "Failed to update the article! Unexpected exception: "+ ex.Message);
                return -3;
            }
            

        }

        private int deleteArticleFromDB(string articleCode)
        {

            Article tempArticle;
            try
            {
                tempArticle = _context.Articles.Single(a => a.Code == articleCode);
                _context.Articles.Remove(tempArticle);
                return _context.SaveChanges();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", "Article not found. Exception: " + ex.Message);
                _logger.LogError(ex, "Exception in post(DeleteArticle)");
                return -2;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Exception in post(DeleteArticle)");
                ModelState.AddModelError("", "Failed to delete article! Exception: " + ex.Message);
                return -2;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in post(DeleteArticle)");
                ModelState.AddModelError("", "Failed to delete the article! Unexpected exception: " + ex.Message);
                return -2;
            }
        }

    }//class
}
