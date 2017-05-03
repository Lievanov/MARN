using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LaboratorioMarn.Controllers
{
    public class AnalisiController : Controller
    {
        private MARNEntities db = new MARNEntities();

        public ActionResult Index()
        {
            return View(db.ANALISIS.ToList());
        }

        public ActionResult Create()
        {
            List<AREA> areas = db.AREAs.ToList();

            IEnumerable<SelectListItem> areasList =
            from area in areas
            select new SelectListItem
            {
                Value = area.id_area.ToString(),
                Text = area.nombre_area
            };

            ViewBag.AreasSelection = areasList;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ANALISI analisis)
        {
            if (ModelState.IsValid)
            {
                analisis.formula = "formula";

                ANALISI lastAnalisis = db.ANALISIS.OrderByDescending(a => a.id_analisis).First();

                analisis.id_analisis = lastAnalisis.id_analisis + 1;

                db.ANALISIS.Add(analisis);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(analisis);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ANALISI analisis = db.ANALISIS.Find(id);
            if (analisis == null)
            {
                return HttpNotFound();
            }

            List<AREA> areas = db.AREAs.ToList();

            IEnumerable<SelectListItem> areasList =
            from area in areas
            select new SelectListItem
            {
                Value = area.id_area.ToString(),
                Text = area.nombre_area
            };

            foreach (SelectListItem item in areasList)
            {
                if (item.Text.Equals(analisis.AREA.nombre_area))
                {
                    item.Selected = true;
                }
            }

            ViewBag.AreasSelection = areasList;

            return View(analisis);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ANALISI analisis)
        {
            if (ModelState.IsValid)
            {
                ANALISI analisisOriginal = db.ANALISIS.Find(analisis.id_analisis);
                analisisOriginal.id_area = analisis.id_area;
                analisisOriginal.metodo = analisis.metodo;
                analisisOriginal.nombre = analisis.nombre;
                analisisOriginal.unidades = analisis.unidades;
                analisisOriginal.costo = analisis.costo;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(analisis);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ANALISI analisis = db.ANALISIS.Find(id);
            if (analisis == null)
            {
                return HttpNotFound();
            }
            return View(analisis);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                ANALISI analisis = db.ANALISIS.Find(id);
                db.ANALISIS.Remove(analisis);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                TempData["msjError"] = "No se pudo eliminar el análisis";
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