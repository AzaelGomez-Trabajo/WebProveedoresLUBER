﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebProveedoresN.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        [Display(Name = "Código de Proveedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        public string SupplierCode { get; set; } = null!;

        [Display(Name = "Proveedor")]
        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        public string SupplierName { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [DisplayName("Nombre Completo")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        public string Correo { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El campo {0} no puede tener más de {1} caracteres")]
        [DisplayName("Contraseña")]
        public string Clave { get; set; } = null!;

        public bool Restablecer { get; set; }
        public bool Confirmado { get; set; }
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public int IdStatus { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio.")]
        public List<string> Roles { get; set; } = [];
    }
}