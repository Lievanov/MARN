using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LaboratorioMarn.Infrastructure;
using LaboratorioMarn.Models;

namespace LaboratorioMarn.Controllers
{
    public class GrupoSolicitudController : Controller
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

            return db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);
        }

        public ActionResult Index()
        {
            IEnumerable<GRUPO_SOLICITUD> grupoSolicitudes = db.GRUPO_SOLICITUD.OrderByDescending(s => s.fecha_creacion).ToList();
            return View(grupoSolicitudes);
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            return View();
        }

        // GET: Solicitud
        public ActionResult Create()
        {
            return View(db.SOLICITANTEs.ToList());
        }

        [HttpPost]
        public ActionResult FormSolicitud(string idSolicitante)
        {
            SOLICITANTE solicitante = db.SOLICITANTEs.Find(Int32.Parse(idSolicitante));

            List<string> selectedItems = new List<string>();
            selectedItems.Add("0");
            if (solicitante.preset_analisis != null)
            {
                // List<string> selectedItems = solicitante.preset_analisis.Split(',').ToList();
                selectedItems = solicitante.preset_analisis.Split(',').ToList();
            }



            List<AREA> areas = db.AREAs.ToList();

            ViewBag.Solicitante = solicitante;
            ViewBag.SelectedItems = selectedItems;
            return PartialView(areas);
        }

        [HttpPost]
        public ActionResult Guardar(string idSolicitante)
        {
            SOLICITANTE solicitante = db.SOLICITANTEs.Find(Int32.Parse(idSolicitante));

            GRUPO_SOLICITUD grupoSolicitud = new GRUPO_SOLICITUD();

            var fechaSolicitud = Request.Form["fecha_solicitud"];

            grupoSolicitud.preset = Request.Form["checkAnalisis[]"];
            grupoSolicitud.id_solicitante = solicitante.id_solicitante;
            grupoSolicitud.fecha_creacion = DateTime.Now;
            grupoSolicitud.fecha_solicitud = new DateTime(Int32.Parse(fechaSolicitud.Substring(0, 4)),Int32.Parse(fechaSolicitud.Substring(5, 2)), Int32.Parse(fechaSolicitud.Substring(8, 2)));
            grupoSolicitud.identificador = Request.Form["identificador"];

            db.GRUPO_SOLICITUD.Add(grupoSolicitud);

            db.SaveChanges();

            return Json(new { isValid = true, id = grupoSolicitud.id_grupo_solicitud });
        }

        
    }
}