using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaboratorioMarn.Models
{
    public class reporteAnalisixAreas
    {
        public byte id_area { get; set; }
        public string nombreArea { get; set; }
        public int año { get; set; }
        public int noareas { get; set; }
        public string fecha_inicio { get; set; }
        public string fecha_fin { get; set; }
        public int mes { get; set; }
    }
}