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
    
    public partial class EMPLEADO
    {
        public EMPLEADO()
        {
            this.ANALISIS_EMPLEADO = new HashSet<ANALISIS_EMPLEADO>();
            this.EXISTENCIA_SOLUCION = new HashSet<EXISTENCIA_SOLUCION>();
            this.PROCESO_ANALISTA = new HashSet<PROCESO_ANALISTA>();
            this.SALIDA_EXISTENCIA = new HashSet<SALIDA_EXISTENCIA>();
            this.SOLICITUDs = new HashSet<SOLICITUD>();
            this.VISITAs = new HashSet<VISITA>();
            this.NOTIFICACIONs = new HashSet<NOTIFICACION>();
            this.NOTIFICACIONs1 = new HashSet<NOTIFICACION>();
        }
    
        public byte id_empleado { get; set; }
        public Nullable<byte> id_area { get; set; }
        public byte id_tipo_empleado { get; set; }
        public string nombre_empleado { get; set; }
        public string telefono_empleado { get; set; }
        public string id_aspnet_user { get; set; }
        public string avatar { get; set; }
    
        public virtual ICollection<ANALISIS_EMPLEADO> ANALISIS_EMPLEADO { get; set; }
        public virtual AREA AREA { get; set; }
        public virtual TIPO_EMPLEADO TIPO_EMPLEADO { get; set; }
        public virtual ICollection<EXISTENCIA_SOLUCION> EXISTENCIA_SOLUCION { get; set; }
        public virtual ICollection<PROCESO_ANALISTA> PROCESO_ANALISTA { get; set; }
        public virtual ICollection<SALIDA_EXISTENCIA> SALIDA_EXISTENCIA { get; set; }
        public virtual ICollection<SOLICITUD> SOLICITUDs { get; set; }
        public virtual ICollection<VISITA> VISITAs { get; set; }
        public virtual ICollection<NOTIFICACION> NOTIFICACIONs { get; set; }
        public virtual ICollection<NOTIFICACION> NOTIFICACIONs1 { get; set; }
    }
}
