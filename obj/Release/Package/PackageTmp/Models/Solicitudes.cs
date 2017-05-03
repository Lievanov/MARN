using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaboratorioMarn.Models
{
    public class Solicitudes
    {
        public int id_solicitud { get; set; }
        public string no_referencia { get; set; }
        public int no_analisis { get; set; }
        public string nombre_area { get; set; }
        public Nullable<System.DateTime> fecha_solicitud { get; set; }
        public Nullable<System.DateTime> fecha_visita { get; set; }
        public Nullable<System.DateTime> fecha_inicio { get; set; }
        public Nullable<System.DateTime> fecha_finalizacion { get; set; }
        public string nombre_solicitante { get; set; }
        public int noareas { get; set; }
        public string nombre_estado  { get; set; }
        public string nombre_estado_proceso { get; set; } 
    }
}