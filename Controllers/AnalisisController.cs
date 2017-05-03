using LaboratorioMarn.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Web.Mvc;
using LaboratorioMarn.Models;

namespace LaboratorioMarn.Controllers
{
    [Authorize(Roles = "Analista")]
    public class AnalisisController : Controller
    {
        MARNEntities db = new MARNEntities();

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

            EMPLEADO empleado = db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);

            return empleado;
        }

        [Route("Analista/Solicitud/{idSolicitud}")]
        public ActionResult Administrar (int idSolicitud)
        {
            EMPLEADO empleado = this.empleado();
            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            PROCESO_ANALISTA procesoAnalista = solicitud.PROCESO_ANALISTA.Single(pa => pa.empleado_id == empleado.id_empleado);

            List<ANALISIS_EMPLEADO> analisis = db.ANALISIS_EMPLEADO.Where(ae => ae.DETALLE_SOLICITUD.id_solicitud == idSolicitud && ae.id_empleado == empleado.id_empleado).ToList();

            var reportesFinalizados = true;

            foreach (ANALISIS_EMPLEADO analisi in analisis)
            {
                if (analisi.REPORTEs.Count() == 0)
                {
                    reportesFinalizados = false;
                }
            }

            ViewBag.Analisis = analisis;
            ViewBag.ProcesoAnalista = procesoAnalista;
            ViewBag.reportesFinalizados = reportesFinalizados;

            return View(solicitud);
        }
    }
}