using Microsoft.SqlServer.Server;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NomenclatureDemo.Models
{
    public class ArticlePropertys
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Articles.Code")]
        public string? Code { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.##%}")]
        [RegularExpression(@"^\d+.?\d{0,2}$", ErrorMessage = "Invalid value. Please ensure that you have entered a positive amount with a maximum of 2 decimal places.")]
        [ValidTVA]
        [Required]
        public double TVA { get; set; }

        [RegularExpression(@"^\d+.?\d{0,2}$", ErrorMessage = "Invalid value. Please ensure that you have entered a positive amount with a maximum of 2 decimal places.")]
        public double? Acquisition_Price { get; set; }
        public double? Full_Price { get; set; }
        //Valability period
        [DataType(DataType.Date)]
        [ValidStartDate]
        public DateTime? Start_Date { get; set; }
        [DataType(DataType.Date)]
        [ValidEndDate]
        public DateTime? End_Date { get; set; }
    }
}
