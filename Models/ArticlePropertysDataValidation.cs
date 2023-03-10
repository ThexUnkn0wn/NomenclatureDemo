using System.ComponentModel.DataAnnotations;

namespace NomenclatureDemo.Models
{
    public class ValidTVA : ValidationAttribute
    {
        protected override ValidationResult
                IsValid(object? value, ValidationContext validationContext)
        {
            double _tva = Convert.ToDouble(value);

            if (_tva< 0f || _tva > 100f)
            {
                return new ValidationResult("The field TVA must must be between 0 to 100");
            }
            else
            {
                return ValidationResult.Success;

            }
        }
    }
    public class ValidEndDate : ValidationAttribute
    {
        protected override ValidationResult
                IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;
            DateTime _endDate = Convert.ToDateTime(value).Date;
            DateTime _today = DateTime.Now.Date;
            if (_endDate < _today)
            {
                return new ValidationResult("The valability period - end date must be equal or greater than today(" + _today.ToString("dd/MM/yyyy") +").");
            }
            else
            {
                return ValidationResult.Success;
                
            }
        }
    }
    public class ValidStartDate : ValidationAttribute
    {
        protected override ValidationResult
                IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;
            var model = (Models.ArticlePropertys)validationContext.ObjectInstance;
            DateTime _startDate = Convert.ToDateTime(value).Date;
            DateTime? _endDate = model.End_Date!=null?Convert.ToDateTime(model.End_Date).Date:null;
            DateTime _today = DateTime.Now.Date;
            if (_endDate!=null && _endDate < _startDate)
            {
                return new ValidationResult
                    ("The valability period - start date must be set prior to the end date.");
            }
            else if (_startDate < _today)
            {
                return new ValidationResult
                    ("The valability period - start date must be equal or greater than today(" + _today.ToString("dd/MM/yyyy") + ").");
            }
            else
            {
                return ValidationResult.Success;
            }
        }
    }


}
