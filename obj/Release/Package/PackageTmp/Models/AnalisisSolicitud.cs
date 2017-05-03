using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaboratorioMarn.Models
{
    public class AnalisisSolicitud
    {
        public ANALISI analisis { set; get;}
        public SOLICITUD solicitud { set; get; }
        public bool asignado { set; get; }
    }
}