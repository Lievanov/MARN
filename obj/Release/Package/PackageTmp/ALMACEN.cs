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
    
    public partial class ALMACEN
    {
        public ALMACEN()
        {
            this.PRODUCTOes = new HashSet<PRODUCTO>();
        }
    
        public byte id_almacen { get; set; }
        public Nullable<byte> estante { get; set; }
        public Nullable<byte> nivel { get; set; }
    
        public virtual ICollection<PRODUCTO> PRODUCTOes { get; set; }
    }
}
