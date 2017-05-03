using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LaboratorioMarn.Classes
{
    public class Notificacion
    {
        MARNEntities db = new MARNEntities();

        private NOTIFICACION objectNotification { set; get; }

        public Notificacion(byte remitente, string titulo, string mensaje, string redireccion)
        {
            objectNotification = new NOTIFICACION() 
            { 
                empleado_remitente = remitente,
                titulo = titulo,
                mensaje = mensaje,
                redireccion = redireccion
            };
        }

        public Notificacion setReceptor(byte receptor)
        {
            objectNotification.empleado_receptor = receptor;

            return this;
        }

        public Notificacion setReceptor()
        {
            EMPLEADO empleado = db.EMPLEADOes.Where(e => e.id_tipo_empleado == 1).First();

            objectNotification.empleado_receptor = empleado.id_empleado;

            return this;
        }

        public void send()
        {
            objectNotification.fecha_notificacion = DateTime.Now;

            db.NOTIFICACIONs.Add(objectNotification);

            db.SaveChanges();
        }
    }
}