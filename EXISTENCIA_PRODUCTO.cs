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
    
    public partial class EXISTENCIA_PRODUCTO
    {
        public EXISTENCIA_PRODUCTO()
        {
            this.SALIDA_EXISTENCIA = new HashSet<SALIDA_EXISTENCIA>();
        }
    
        public int id_existencia_producto { get; set; }
        public int id_producto { get; set; }
        public decimal cantidad { get; set; }
        public string estado { get; set; }
    
        public virtual PRODUCTO PRODUCTO { get; set; }
        public virtual ICollection<SALIDA_EXISTENCIA> SALIDA_EXISTENCIA { get; set; }
    }
}
