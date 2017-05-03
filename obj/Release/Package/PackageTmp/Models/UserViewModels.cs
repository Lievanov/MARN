using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LaboratorioMarn.Models
{
    public class CreateModel {

        [Display(Name = "Area")]
        public byte area { get; set; }

        [Display(Name = "Tipo")]
        public byte tipo { get; set; }

        [Display(Name = "Telefono")]
        public string telefono { get; set; }

        [Display(Name="Nombre")]
        [Required]
        public string nombre { get; set; }

        [Display(Name = "Username")]
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
    }
}