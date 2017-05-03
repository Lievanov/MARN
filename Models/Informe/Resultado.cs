using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaboratorioMarn.Models.Informe
{
    public class Resultado
    {
        public string analisis { get; set; }
        public string resultado { get; set; }
        public string incerteza { get; set; }
        public string unidades { get; set; }
        public string limiteDeteccion { get; set; }
        public string metodo { get; set; }
    }
}