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
    
    public partial class PRODUCTO
    {
        public PRODUCTO()
        {
            this.EXISTENCIA_PRODUCTO = new HashSet<EXISTENCIA_PRODUCTO>();
        }
    
        public int id_producto { get; set; }
        public byte id_almacen { get; set; }
        public byte id_marca { get; set; }
        public byte id_catalogo { get; set; }
        public int id_lote { get; set; }
        public int id_reactivo { get; set; }
        public System.DateTime fecha_ingreso { get; set; }
        public Nullable<System.DateTime> fecha_vencimiento { get; set; }
        public decimal presentacion_cantidad { get; set; }
        public byte id_unidad { get; set; }
        public byte minimo_porcentaje { get; set; }
        public byte estado { get; set; }
    
        public virtual ALMACEN ALMACEN { get; set; }
        public virtual CATALOGO CATALOGO { get; set; }
        public virtual ICollection<EXISTENCIA_PRODUCTO> EXISTENCIA_PRODUCTO { get; set; }
        public virtual LOTE LOTE { get; set; }
        public virtual MARCA MARCA { get; set; }
        public virtual REACTIVO REACTIVO { get; set; }
        public virtual UNIDAD UNIDAD { get; set; }
    }
}
