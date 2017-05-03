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
    public class NotificacionController : Controller
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

        // GET: Notificacion
        public ActionResult Index()
        {
            EMPLEADO empleado = this.empleado();

            List<NOTIFICACION> notificaciones = db.NOTIFICACIONs.Where(n => n.empleado_receptor == empleado.id_empleado && n.eliminado == 0).OrderByDescending(n => n.fecha_notificacion).ToList();

            return View(notificaciones);
        }

        [HttpPost]
        public ActionResult GetUnreadCount()
        {
            EMPLEADO empleado = this.empleado();

            int count = db.NOTIFICACIONs.Where(n => n.empleado_receptor == empleado.id_empleado && n.eliminado == 0 && n.leido == 0).Count();

            return Json(new { count = count });
        }

        [Route("Notificacion/GetLast/{number}")]
        public ActionResult GetLast(int number)
        {
            EMPLEADO empleado = this.empleado();

            List<NOTIFICACION> notificaciones = db.NOTIFICACIONs.Where(n => n.empleado_receptor == empleado.id_empleado && n.eliminado == 0).OrderByDescending(n => n.fecha_notificacion).Take(number).ToList();

            return PartialView(notificaciones);
        }

        public ActionResult View(int id)
        {
            EMPLEADO empleado = this.empleado();

            NOTIFICACION notificacion = db.NOTIFICACIONs.Single(n => n.empleado_receptor == empleado.id_empleado && n.id_notificacion == id);

            if (notificacion.leido == 0)
            {
                notificacion.leido = 1;
                notificacion.fecha_lectura = DateTime.Now;

                db.SaveChanges();
            }

            return Redirect(notificacion.redireccion);
        }

        [HttpPost]
        public ActionResult Remove(int id)
        {
            EMPLEADO empleado = this.empleado();

            NOTIFICACION notificacion = db.NOTIFICACIONs.Single(n => n.empleado_receptor == empleado.id_empleado && n.id_notificacion == id); ;

            notificacion.eliminado = 1;

            db.SaveChanges();

            return Json(new { valid = true });
        }

        [HttpPost]
        public ActionResult MarkAsRead(int id)
        {
            EMPLEADO empleado = this.empleado();

            NOTIFICACION notificacion = db.NOTIFICACIONs.Single(n => n.empleado_receptor == empleado.id_empleado && n.id_notificacion == id); ;

            notificacion.leido = 1;
            notificacion.fecha_lectura = DateTime.Now;

            db.SaveChanges();

            return Json(new { valid = true });
        }
    }
}