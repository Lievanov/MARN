using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LaboratorioMarn.Controllers
{
    public class SitioMuestreoController : Controller
    {
        private MARNEntities db = new MARNEntities();

        public ActionResult Index()
        {
            return View(db.SITIO_MUESTREO.ToList());
        }
        
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SITIO_MUESTREO sitioMuestreo)
        {
            if (ModelState.IsValid)
            {
                db.SITIO_MUESTREO.Add(sitioMuestreo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sitioMuestreo);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SITIO_MUESTREO sitioMuestreo = db.SITIO_MUESTREO.Find(id);
            if (sitioMuestreo == null)
            {
                return HttpNotFound();
            }
            return View(sitioMuestreo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SITIO_MUESTREO sitioMuestreo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sitioMuestreo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sitioMuestreo);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SITIO_MUESTREO sitioMuestreo = db.SITIO_MUESTREO.Find(id);
            if (sitioMuestreo == null)
            {
                return HttpNotFound();
            }
            return View(sitioMuestreo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                SITIO_MUESTREO sitioMuestreo = db.SITIO_MUESTREO.Find(id);
                db.SITIO_MUESTREO.Remove(sitioMuestreo);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["msjError"] = "No se pudo eliminar el sitio de muestreo";
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