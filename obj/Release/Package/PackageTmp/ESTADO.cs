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
    
    public partial class ESTADO
    {
        public ESTADO()
        {
            this.SOLICITUDs = new HashSet<SOLICITUD>();
        }
    
        public byte id_estado { get; set; }
        public string nombre_estado { get; set; }
    
        public virtual ICollection<SOLICITUD> SOLICITUDs { get; set; }
    }
}
