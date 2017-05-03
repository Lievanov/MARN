using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LaboratorioMarn.Controllers
{
    public class ReactivoController : Controller
    {
        private MARNEntities db = new MARNEntities();

        // GET: REACTIVOes
        public ActionResult Index()
        {
            return View(db.REACTIVOes.ToList());
        }

        // GET: REACTIVOes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REACTIVO rEACTIVO = db.REACTIVOes.Find(id);
            if (rEACTIVO == null)
            {
                return HttpNotFound();
            }
            return View(rEACTIVO);
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
        public ActionResult Create([Bind(Include = "id_reactivo,nombre")] REACTIVO rEACTIVO)
        {
            if (ModelState.IsValid)
            {
                db.REACTIVOes.Add(rEACTIVO);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rEACTIVO);
        }

        // GET: REACTIVOes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REACTIVO rEACTIVO = db.REACTIVOes.Find(id);
            if (rEACTIVO == null)
            {
                return HttpNotFound();
            }
            return View(rEACTIVO);
        }

        // POST: REACTIVOes/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_reactivo,nombre")] REACTIVO rEACTIVO)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rEACTIVO).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rEACTIVO);
        }

        // GET: REACTIVOes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            REACTIVO rEACTIVO = db.REACTIVOes.Find(id);
            if (rEACTIVO == null)
            {
                return HttpNotFound();
            }
            return View(rEACTIVO);
        }

        // POST: REACTIVOes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                REACTIVO rEACTIVO = db.REACTIVOes.Find(id);
                db.REACTIVOes.Remove(rEACTIVO);
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