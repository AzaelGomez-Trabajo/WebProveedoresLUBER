using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
	public class LoadFile
	{
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayName("Archivo PDF")]
        //[FileExtensions(Extensions = ".pdf", ErrorMessage = "Por favor, suba un archivo PDF.")]
        [FileSize(2 * 1024 * 1024)]
        public IFormFile FilePDF { get; set; } = null!;
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [DisplayName("Archivo XML")]
        //[FileExtensions(Extensions = ".xml", ErrorMessage = "Por favor, suba un archivo XML.")]
        public IFormFile? FileXML { get; set; } = null!;

        [Required]
        [DisplayName("Número de Orden de Compra")]
        public int OrderNumber { get; set; }

        public List<int> SelectedDocuments { get; set; } = [];
    }
    public class FileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;
        public FileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    return new ValidationResult($"El tamaño del archivo no debe exceder {_maxFileSize / (1024 * 1024)} MB.");
                }
            }
            return ValidationResult.Success;
        }
    }
}