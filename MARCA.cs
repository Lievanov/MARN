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
    
    public partial class MARCA
    {
        public MARCA()
        {
            this.PRODUCTOes = new HashSet<PRODUCTO>();
        }
    
        public byte id_marca { get; set; }
        public string nombre { get; set; }
    
        public virtual ICollection<PRODUCTO> PRODUCTOes { get; set; }
    }
}
