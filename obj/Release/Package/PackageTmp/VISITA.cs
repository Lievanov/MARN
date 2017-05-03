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
    
    public partial class VISITA
    {
        public VISITA()
        {
            this.DETALLE_VISITA = new HashSet<DETALLE_VISITA>();
        }
    
        public int id_visita { get; set; }
        public int id_solicitud { get; set; }
        public byte id_empleado { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> fecha_visita { get; set; }

        [DataType(DataType.DateTime), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public Nullable<System.DateTime> fecha_entrega { get; set; }
        public byte id_procedencia { get; set; }
        public byte id_tipo_muestra { get; set; }
        public Nullable<System.DateTime> fecha_creacion { get; set; }
        public Nullable<int> id_sitio_muestreo { get; set; }
        public Nullable<byte> id_empleado_entrega { get; set; }
        public string identificador { get; set; }
        public Nullable<byte> no_frascos { get; set; }
        public Nullable<decimal> lat { get; set; }
        public Nullable<decimal> lng { get; set; }
        public string id { get; set; }
        public Nullable<int> id_grupo_solicitud { get; set; }
    
        public virtual ICollection<DETALLE_VISITA> DETALLE_VISITA { get; set; }
        public virtual EMPLEADO EMPLEADO { get; set; }
        public virtual PROCEDENCIA PROCEDENCIA { get; set; }
        public virtual SOLICITUD SOLICITUD { get; set; }
        public virtual TIPO_MUESTRA TIPO_MUESTRA { get; set; }
        public virtual SITIO_MUESTREO SITIO_MUESTREO { get; set; }
        public virtual GRUPO_SOLICITUD GRUPO_SOLICITUD { get; set; }
    }
}
