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
    
    public partial class ANALISIS_EMPLEADO
    {
        public ANALISIS_EMPLEADO()
        {
            this.CALCULOes = new HashSet<CALCULO>();
            this.REPORTEs = new HashSet<REPORTE>();
            this.ANALISIS_REACTIVO = new HashSet<ANALISIS_REACTIVO>();
        }
    
        public int id_analisis_empleado { get; set; }
        public byte id_empleado { get; set; }
        public int id_detalle_solicitud { get; set; }
    
        public virtual DETALLE_SOLICITUD DETALLE_SOLICITUD { get; set; }
        public virtual EMPLEADO EMPLEADO { get; set; }
        public virtual ICollection<CALCULO> CALCULOes { get; set; }
        public virtual ICollection<REPORTE> REPORTEs { get; set; }
        public virtual ICollection<ANALISIS_REACTIVO> ANALISIS_REACTIVO { get; set; }
    }
}