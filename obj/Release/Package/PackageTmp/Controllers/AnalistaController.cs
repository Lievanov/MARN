using LaboratorioMarn.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LaboratorioMarn.Models;
using LaboratorioMarn.Classes;

namespace LaboratorioMarn.Controllers
{
    [Authorize(Roles = "Analista")]
    public class AnalistaController : Controller
    {
        MARNEntities db = new MARNEntities();

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        // GET: Analista
        public ActionResult Index()
        {
            var username = User.Identity.GetUserName();
            AppUser user = UserManager.FindByName(username);

            EMPLEADO empleado = db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);

            List<PROCESO_ANALISTA> procesosAnalista = db.PROCESO_ANALISTA.Where(pa => pa.empleado_id == empleado.id_empleado && pa.fecha_asignacion != null).OrderByDescending(pa => pa.SOLICITUD.no_referencia).ToList();

            List<AnalisisSolicitud> analisisSolicitud = new List<AnalisisSolicitud>();

            List<ANALISI> analisis = db.ANALISIS.Where(a => a.id_area == empleado.id_area).ToList();

            foreach (PROCESO_ANALISTA procesoAnalista in procesosAnalista)
            {
                SOLICITUD solicitud = procesoAnalista.SOLICITUD;

                List<DETALLE_SOLICITUD> detallesSolicitudEmpleado = solicitud.DETALLE_SOLICITUD.Where(ds => ds.ANALISIS_EMPLEADO.First().id_empleado == empleado.id_empleado).ToList();

                foreach (ANALISI analisisElem in analisis)
                {
                    AnalisisSolicitud analisisSolicitudElem = new AnalisisSolicitud();

                    analisisSolicitudElem.analisis = analisisElem;
                    analisisSolicitudElem.solicitud = solicitud;

                    var detalleSolicitud = detallesSolicitudEmpleado.SingleOrDefault(ds => ds.id_analisis == analisisElem.id_analisis);

                    if (detalleSolicitud != null)
                    {
                        analisisSolicitudElem.asignado = true;
                    }

                    analisisSolicitud.Add(analisisSolicitudElem);
                }
            }

            ViewBag.AnalisisSolicitud = analisisSolicitud;
            ViewBag.Analisis = analisis;

            return View(procesosAnalista);
        }

        [HttpPost]
        public ActionResult Index(string fechas, DateTime fecha_inicio, DateTime fecha_fin, string estado)
        {
            var username = User.Identity.GetUserName();
            AppUser user = UserManager.FindByName(username);

            EMPLEADO empleado = db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);

            List<PROCESO_ANALISTA> procesosAnalista = this.FiltrarProcesoAnalista(empleado, fechas, fecha_inicio, fecha_fin, estado);

            List<AnalisisSolicitud> analisisSolicitud = new List<AnalisisSolicitud>();

            List<ANALISI> analisis = db.ANALISIS.Where(a => a.id_area == empleado.id_area).ToList();

            foreach (PROCESO_ANALISTA procesoAnalista in procesosAnalista)
            {
                SOLICITUD solicitud = procesoAnalista.SOLICITUD;

                List<DETALLE_SOLICITUD> detallesSolicitudEmpleado = solicitud.DETALLE_SOLICITUD.Where(ds => ds.ANALISIS_EMPLEADO.First().id_empleado == empleado.id_empleado).ToList();

                foreach (ANALISI analisisElem in analisis)
                {
                    AnalisisSolicitud analisisSolicitudElem = new AnalisisSolicitud();

                    analisisSolicitudElem.analisis = analisisElem;
                    analisisSolicitudElem.solicitud = solicitud;

                    var detalleSolicitud = detallesSolicitudEmpleado.SingleOrDefault(ds => ds.id_analisis == analisisElem.id_analisis);

                    if (detalleSolicitud != null)
                    {
                        analisisSolicitudElem.asignado = true;
                    }

                    analisisSolicitud.Add(analisisSolicitudElem);
                }
            }

            ViewBag.AnalisisSolicitud = analisisSolicitud;
            ViewBag.Analisis = analisis;

            return View(procesosAnalista);
        }

        public ActionResult IniciarProceso(int idSolicitud)
        {
            var username = User.Identity.GetUserName();
            AppUser user = UserManager.FindByName(username);

            EMPLEADO empleado = db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);

            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            if (solicitud.PROCESO_ANALISTA.Count(pa => pa.empleado_id == empleado.id_empleado) != 1)
            {
                return View("Error", new string[] { "Permiso denegado" });
            }

            PROCESO_ANALISTA procesoAnalista = solicitud.PROCESO_ANALISTA.Single(pa => pa.empleado_id == empleado.id_empleado);

            procesoAnalista.fecha_inicio = DateTime.Now;
            procesoAnalista.id_estado_proceso_analista = 2;

            db.SaveChanges();

            // enviar notificacion
            Notificacion notificacion = new Notificacion(empleado.id_empleado, "Ha iniciado el analisis de la solicitud " + solicitud.no_referencia, "", Url.Action("Detalle", "Solicitud",new { id = idSolicitud}));
            notificacion.setReceptor();
            notificacion.send();

            return RedirectToAction("Administrar", "Analisis", new { idSolicitud = idSolicitud });
        }

        private List<PROCESO_ANALISTA> FiltrarProcesoAnalista(EMPLEADO empleado, string fechas, DateTime fecha_inicio, DateTime fecha_fin, string estado)
        {
            List<PROCESO_ANALISTA> procesosAnalista;

            procesosAnalista = db.PROCESO_ANALISTA.Where(pa => pa.empleado_id == empleado.id_empleado && pa.fecha_asignacion != null).OrderByDescending(pa => pa.SOLICITUD.no_referencia).ToList();

            return procesosAnalista;
        }
    }
}