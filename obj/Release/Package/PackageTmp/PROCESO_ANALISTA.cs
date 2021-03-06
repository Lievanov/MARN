//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LaboratorioMarn
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class PROCESO_ANALISTA
    {
        public int id_proceso_analista { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> fecha_asignacion { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> fecha_inicio { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> fecha_fin { get; set; }
        public int solicitud_id { get; set; }
        public byte empleado_id { get; set; }
        public byte cantidad_analisis { get; set; }
        public string id_analisis { get; set; }
        public Nullable<byte> id_estado_proceso_analista { get; set; }
    
        public virtual EMPLEADO EMPLEADO { get; set; }
        public virtual SOLICITUD SOLICITUD { get; set; }
        public virtual ESTADO_PROCESO_ANALISTA ESTADO_PROCESO_ANALISTA { get; set; }
    }
}
