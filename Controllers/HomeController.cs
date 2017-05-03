using LaboratorioMarn.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LaboratorioMarn.Models;
using Microsoft.AspNet.Identity.EntityFramework;

namespace LaboratorioMarn.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        MARNEntities db = new MARNEntities();

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        public ActionResult PreIndex()
        {
            var username = User.Identity.GetUserName();

            AppUser user = UserManager.FindByName(username);

            UserManager.GetRoles(user.Id);

            AppIdentityDbContext dbIdentity = new AppIdentityDbContext();


            IEnumerable<IdentityUserRole> roles = user.Roles.ToList();

            foreach (IdentityUserRole role in roles)
            { 
                var roleObj = dbIdentity.Roles.Find(role.RoleId);
                if (roleObj.Name == "Admin")
                {
                    return RedirectToAction("Index");
                }
                else if (roleObj.Name == "Analista")
                {
                    return RedirectToAction("Index", "Analista");
                }
            }

            return View("Error", new string[] { "Access Denied" });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var today = DateTime.Now;
            var inicio = new DateTime(today.Year, today.Month, 1);
            var fin = new DateTime(inicio.Year, inicio.Month, DateTime.DaysInMonth(inicio.Year, inicio.Month));

            ViewBag.inicio_datetime = inicio;
            ViewBag.fin_datetime = fin;

            IEnumerable<SOLICITUD> solicitudes = db.SOLICITUDs.Where(s => s.id_estado < 3).OrderByDescending(s => s.no_referencia).ToList();
            IEnumerable<EMPLEADO> empleados = db.EMPLEADOes.Where(e => e.id_tipo_empleado == 2).ToList();

            IEnumerable<PRODUCTO> productos = db.PRODUCTOes.Where(p => ((p.EXISTENCIA_PRODUCTO.Sum(ep => ep.cantidad) / (p.presentacion_cantidad * p.EXISTENCIA_PRODUCTO.Count())) * 100) < p.minimo_porcentaje).ToList();

            ViewBag.Solicitudes = solicitudes;
            ViewBag.Productos = productos;

            return View(empleados);
        }
    }
}