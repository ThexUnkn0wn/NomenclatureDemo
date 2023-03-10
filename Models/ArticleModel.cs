using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NomenclatureDemo.Model
{
    public class Article
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [RegularExpression(@"^[A-Z]+[a-zA-Z0-9\s]*$", 
            ErrorMessage = "{0} must start with a capital letter and can only include alphanumerical characters and spaces.")]
        [StringLength(255,MinimumLength = 5, 
            ErrorMessage = "{0} length must be between {2} and {1}.")]
        [Required]
        public string Name { get; set; }

        [RegularExpression(@"^[a-zA-Z]+[a-zA-Z0-9\s-_]*$",
            ErrorMessage = "{0} can only include alphanumerical characters, -, _ and spaces.")]
        [StringLength(50, MinimumLength = 3, 
            ErrorMessage = "{0} length must be between {2} and {1}.")]
        [Required]
        public string Code { get; set; }


        [StringLength(50)]
        [Required]
        public string State { get; set; } = "INACTIVE";

    }
}
