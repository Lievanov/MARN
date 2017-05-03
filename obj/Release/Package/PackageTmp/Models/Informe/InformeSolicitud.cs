using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaboratorioMarn.Models.Informe
{
    public class InformeSolicitud
    {
        public InformeSolicitud()
        {
            this.Parametros = new HashSet<Parametro>();
            this.Resultados = new HashSet<Resultado>();
        }

        public string tipo { get; set; }
        public string areas { get; set; }
        public string numeroReferencia { get; set; }
        public string solicitante { get; set; }
        public string direccion { get; set; }
        public string fechaTomaMuestra { get; set; }
        public string fechaRecepcion { get; set; }
        public string fechaInicio { get; set; }
        public string fechaFinalizacion { get; set; }
        public string fechaGeneracion { get; set; }
        public string responsableMuestra { get; set; }
        public string tipoMuestra { get; set; }
        public string procedencia { get; set; }

        public virtual ICollection<Parametro> Parametros { get; set; }
        public virtual ICollection<Resultado> Resultados { get; set; }

    }
}