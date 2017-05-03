using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LaboratorioMarn.Controllers
{
    public class MuestraController : Controller
    {
        private MARNEntities db = new MARNEntities();

        // GET: MUESTRA_s
        public ActionResult Index()
        {
            return View(db.MUESTRAs.ToList());
        }

        // GET: MUESTRA_s/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUESTRA mUESTRA = db.MUESTRAs.Find(id);
            if (mUESTRA == null)
            {
                return HttpNotFound();
            }
            return View(mUESTRA);
        }

        // GET: MUESTRA_s/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MUESTRA_s/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id_muestra,nombre_muestra,expresado")] MUESTRA muestra)
        {
            //db.MUESTRAs.Select(i => i.id_muestra).Max().ToString();
            if (ModelState.IsValid)
            {
                muestra.id_muestra = int.Parse(db.MUESTRAs.Select(i => i.id_muestra).Max().ToString()) + 1;
                db.MUESTRAs.Add(muestra);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(muestra);
        }

        // GET: MUESTRA_s/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUESTRA mUESTRA = db.MUESTRAs.Find(id);
            if (mUESTRA == null)
            {
                return HttpNotFound();
            }
            return View(mUESTRA);
        }

        // POST: MUESTRA_s/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id_muestra,nombre_muestra,expresado")] MUESTRA mUESTRA)
        {
            if (ModelState.IsValid)
            {
                db.Entry(mUESTRA).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(mUESTRA);
        }

        // GET: MUESTRA_s/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MUESTRA mUESTRA = db.MUESTRAs.Find(id);
            if (mUESTRA == null)
            {
                return HttpNotFound();
            }
            return View(mUESTRA);
        }

        // POST: MUESTRA_s/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MUESTRA mUESTRA = db.MUESTRAs.Find(id);
            db.MUESTRAs.Remove(mUESTRA);
            db.SaveChanges();
            return RedirectToAction("Index");
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