using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaboratorioMarn.Models
{
    public class reporteSolicitudxsolicitantes
    {
        public string id_solicitante { get; set; }
        public string nombreSolicitante { get; set; }
        public int año { get; set; }
        public int mes { get; set; }
        public int nosolicitudes { get; set; }
        public string fecha_inicio { get; set; }
        public string fecha_fin { get; set; }
        public string nombreSolicitantegraph { get; set; }
        //
        public byte id_area { get; set; }
        public string nombreArea { get; set; }
        public int noareas { get; set; }
        public int id_empleado { get; set; }
        public int id_analisis { get; set; }
        public string nombreAnalisis { get; set; }
        public int noAnalisis { get; set; }
        public BasicInfoModel BasicInfo { get; set; }
        public class BasicInfoModel
        {
            public string MesInicio { get; set; }
            public string MesFin { get; set; }

        }

    }
    
}