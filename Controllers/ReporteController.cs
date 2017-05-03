using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaboratorioMarn.Models;
using System.IO;
using Newtonsoft.Json;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LaboratorioMarn.Infrastructure;
using LaboratorioMarn.Classes;

namespace LaboratorioMarn.Controllers
{
    public class ReporteController : Controller
    {
        MARNEntities db = new MARNEntities();
        // GET: Reporte

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private EMPLEADO empleado()
        {
            var username = User.Identity.GetUserName();
            AppUser user = UserManager.FindByName(username);
            return db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);
        }

        [HttpGet]
        public ActionResult Create(int idAnalisisEmpleado)
        {
            ANALISIS_EMPLEADO analisisEmpleado = db.ANALISIS_EMPLEADO.Find(idAnalisisEmpleado);
            ViewBag.AnalisisEmpleado = analisisEmpleado;

            var unidades = new SelectList(
                db.UNIDADs.Select(u => u.nombre).Distinct().ToList());

            ViewBag.Unidades = unidades;

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(int idAnalisisEmpleado, REPORTE reporte)
        {
            object result;
            try
            {
                if (!true) { 
                    throw new Exception();
                }
                
                ANALISIS_EMPLEADO analisisEmpleado = db.ANALISIS_EMPLEADO.Find(idAnalisisEmpleado);

                reporte.id_analisis_empleado = analisisEmpleado.id_analisis_empleado;
                db.REPORTEs.Add(reporte);

                db.SaveChanges();
                
                result = new { valid = true };
            }
            catch(Exception e) {
                result = new { valid = false, msg = "Favor verifique los datos"};
            }
            return Json(result);
        }

        [HttpPost]
        public ActionResult Get(int procesoId)
        {
            PROCESO_ANALISTA procesoAnalista = db.PROCESO_ANALISTA.Find(procesoId);
            EMPLEADO empleado = procesoAnalista.EMPLEADO;

            IEnumerable<REPORTE> reportes = db.REPORTEs.Where(r =>
                r.ANALISIS_EMPLEADO.DETALLE_SOLICITUD.SOLICITUD.id_solicitud == procesoAnalista.SOLICITUD.id_solicitud &&
                r.ANALISIS_EMPLEADO.EMPLEADO.id_empleado == empleado.id_empleado
                ).ToList();

            ViewBag.empleado = empleado;
            ViewBag.procesoAnalista = procesoAnalista;

            return PartialView(reportes);
        }

        [HttpGet]
        public ActionResult Editar(int idReporte)
        {

            //REPORTE reporte = detalle.ANALISIS_EMPLEADO.First().REPORTEs.First();
            REPORTE reporte = db.REPORTEs.Find(idReporte);
            ViewBag.Reporte = reporte;
            DETALLE_SOLICITUD detalle = db.DETALLE_SOLICITUD.Find(reporte.ANALISIS_EMPLEADO.id_detalle_solicitud);
            ViewBag.DetalleSol = detalle;
            return PartialView(reporte);
        }


        [HttpPost]
        public ActionResult Editar(int idReporte, REPORTE postReporte)
        {
            object result;
            REPORTE reporteF = db.REPORTEs.Find(idReporte);

            db.Entry(reporteF).CurrentValues.SetValues(postReporte);

            db.SaveChanges();
            result = new { valid = true };

            return Json(result);

        }

        [HttpPost]
        [Authorize(Roles = "Analista")]
        public ActionResult EnviarResultado(int idSolicitud)
        {
            object result;

            EMPLEADO empleado = this.empleado();

            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            PROCESO_ANALISTA procesoAnalista = db.PROCESO_ANALISTA.Single(pa => pa.empleado_id == empleado.id_empleado && pa.solicitud_id == solicitud.id_solicitud);

            procesoAnalista.fecha_fin = DateTime.Now;
            procesoAnalista.id_estado_proceso_analista = 3;
            db.SaveChanges();

            var reportesCompletos = true;
            foreach (PROCESO_ANALISTA procesosAnalista in solicitud.PROCESO_ANALISTA)
            {
                if (procesosAnalista.id_estado_proceso_analista != 3)
                {
                    reportesCompletos = false;
                }
            }

            // enviar notificacion
            Notificacion notificacion = new Notificacion(empleado.id_empleado, "Analisis de solicitud " + solicitud.no_referencia + " finalizados.", "Se finalizaron los analisis y ingresaron los resultados.", Url.Action("Detalle", "Solicitud", new { id = solicitud.id_solicitud }));
            notificacion.setReceptor();
            notificacion.send();

            if (reportesCompletos)
            {
                solicitud.id_estado_proceso = 4;
            }

            db.SaveChanges();

            if (reportesCompletos)
            {
                // enviar notificacion
                Notificacion notificacionCompleto = new Notificacion(empleado.id_empleado, "Reportes de solicitud " + solicitud.no_referencia + " finalizados.", "Se finalizaron todos los reportes y fueron enviados.", Url.Action("Detalle", "Solicitud", new { id = solicitud.id_solicitud }));
                notificacionCompleto.setReceptor();
                notificacionCompleto.send();
            }

            result = new { valid = true };
            return Json(result);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Revisar(int procesoId, string accion)
        {
            Object result;

            PROCESO_ANALISTA procesoAnalista = db.PROCESO_ANALISTA.Find(procesoId);
            SOLICITUD solicitud = procesoAnalista.SOLICITUD;

            try
            {
                if (accion == "aprobar")
                {
                    // si es diferente de reportado
                    if (procesoAnalista.id_estado_proceso_analista != 3)
                    {
                        throw new Exception("No es posible aprobar los resultado por el estado del proceso");
                    }

                    procesoAnalista.id_estado_proceso_analista = 4; // finalizado
                    db.SaveChanges();

                    bool todosFinalizados = true;
                    foreach (PROCESO_ANALISTA procesoAnalistaSolicitud in solicitud.PROCESO_ANALISTA.ToList())
                    { 
                        // si es diferente de finalizado y cerrado
                        if (procesoAnalistaSolicitud.id_estado_proceso_analista != 4 && procesoAnalistaSolicitud.id_estado_proceso_analista != 6)
                        {
                            todosFinalizados = false;
                        }
                    }

                    // si todos los procesos estan finalizados
                    if (todosFinalizados)
                    {
                        solicitud.id_estado_proceso = 4; // reportes completos
                        solicitud.id_estado = 3; // finalizado
                        solicitud.fecha_finalizacion = DateTime.Now;
                    }
        
                    db.SaveChanges();

                    EMPLEADO empleadoAnalista = db.EMPLEADOes.Find(procesoAnalista.EMPLEADO.id_empleado);
                    EMPLEADO empleado = this.empleado();

                    // enviar notificacion
                    Notificacion notificacion = new Notificacion(empleado.id_empleado, "Los resultados para solicitud " + solicitud.no_referencia + " fueron aprobados.", "Aprobados", Url.Action("Administrar", "Analisis", new { idSolicitud = solicitud.id_solicitud }));
                    notificacion.setReceptor(empleadoAnalista.id_empleado);
                    notificacion.send();

                    var msg = "El reporte de " + procesoAnalista.EMPLEADO.nombre_empleado + " se aprobo con exito.";
                    if (todosFinalizados)
                    {
                        msg += " El proceso ha finalizado, ahora puede generar un informe con todos los resultados";
                    }

                    result = new { valid = true, msg = msg };
                }
                else 
                { 
                    // finalizado y generado
                    if (procesoAnalista.id_estado_proceso_analista == 4 && procesoAnalista.SOLICITUD.informe_generado == 1)
                    {
                        throw new Exception ("El informe ya fue generado, para modificar los resultados debe de estar en un estado de 'Correccion'");
                    }

                    if (solicitud.id_estado_proceso == 6)
                    {
                        procesoAnalista.id_estado_proceso_analista = 5; // estado correccion
                    }
                    else
                    {
                        procesoAnalista.id_estado_proceso_analista = 2; // estado iniciado
                    }
                    procesoAnalista.fecha_fin = null;

                    // si el estado del proceso es reportes completos entonces set a iniciado
                    if (solicitud.id_estado_proceso == 4)
                    {
                        solicitud.id_estado_proceso = 3;
                    }

                    // si es estado de la solicitud es finalizada entonces set a proceso
                    if (solicitud.id_estado == 3)
                    {
                        solicitud.id_estado = 2;
                    }

                    db.SaveChanges();

                    EMPLEADO empleadoAnalista = db.EMPLEADOes.Find(procesoAnalista.EMPLEADO.id_empleado);
                    EMPLEADO empleado = this.empleado();

                    // enviar notificacion
                    Notificacion notificacion = new Notificacion(empleado.id_empleado, "Los resultados para solicitud " + solicitud.no_referencia + " fueron rechazados.", "Rechazados", Url.Action("Administrar", "Analisis", new { idSolicitud = solicitud.id_solicitud }));
                    notificacion.setReceptor(empleadoAnalista.id_empleado);
                    notificacion.send();

                    result = new { valid = true, msg = "El reporte de " + procesoAnalista.EMPLEADO.nombre_empleado + " se ha denegado. Se le ha notificado y el proceso abierto para hacer las correcciones necesarias." };
                }
            }
            catch (Exception e)
            {
                result = new { valid = false, msg = e };
            }

            return Json(result);
        }
    }
}