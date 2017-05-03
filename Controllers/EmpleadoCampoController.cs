using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LaboratorioMarn.Controllers
{
    public class EmpleadoCampoController : Controller
    {
        private MARNEntities db = new MARNEntities();

        // GET: REACTIVOes
        public ActionResult Index()
        {
            return View(db.EMPLEADOes.Where(e => e.id_tipo_empleado == 3).ToList());
        }

        // GET: REACTIVOes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: REACTIVOes/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(EMPLEADO empleado)
        {
            if (ModelState.IsValid)
            {
                empleado.id_aspnet_user = (db.EMPLEADOes.Count() + 1).ToString();
                empleado.id_tipo_empleado = 3;
                db.EMPLEADOes.Add(empleado);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(empleado);
        }

        // GET: REACTIVOes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EMPLEADO empleado = db.EMPLEADOes.Find(id);
            if (empleado == null)
            {
                return HttpNotFound();
            }
            return View(empleado);
        }

        // POST: REACTIVOes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EMPLEADO empleado)
        {
            if (ModelState.IsValid)
            {
                EMPLEADO empleadoUpdate = db.EMPLEADOes.Find(empleado.id_empleado);
                empleadoUpdate.nombre_empleado = empleado.nombre_empleado;
                empleadoUpdate.telefono_empleado = empleado.telefono_empleado;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(empleado);
        }

        // GET: REACTIVOes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EMPLEADO empleado = db.EMPLEADOes.Find(id);
            if (empleado == null)
            {
                return HttpNotFound();
            }
            return View(empleado);
        }

        // POST: REACTIVOes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                EMPLEADO empleado = db.EMPLEADOes.Find(id);
                db.EMPLEADOes.Remove(empleado);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                TempData["msjError"] = "No se pudo eliminar el Reactivo";
                return RedirectToAction("Delete");
            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}