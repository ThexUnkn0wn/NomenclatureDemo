using Microsoft.EntityFrameworkCore;
using NomenclatureDemo.Data;
using NomenclatureDemo.Model;
using System;

namespace NomenclatureDemo.Models
{
    public class ArticleViewModel
    {
        public Article ArticleVm { get; set; }
        public ArticlePropertys ArticlePropertysVm { get; set; }

    }
}
