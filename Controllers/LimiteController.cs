using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LaboratorioMarn;

namespace LaboratorioMarn.Controllers
{
    public class LimiteController : Controller
    {
        private MARNEntities db = new MARNEntities();

        // GET: Limite
        public async Task<ActionResult> Index()
        {
            return View(await db.LIMITES.ToListAsync());
        }

        // GET: Limite/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LIMITE lIMITE = await db.LIMITES.FindAsync(id);
            if (lIMITE == null)
            {
                return HttpNotFound();
            }
            return View(lIMITE);
        }

        // GET: Limite/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Limite/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "analisis,id_analisis,unidades,incerteza,limite_deteccion")] LIMITE lIMITE)
        {
            if (ModelState.IsValid)
            {
                db.LIMITES.Add(lIMITE);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(lIMITE);
        }

        // GET: Limite/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LIMITE lIMITE = await db.LIMITES.FindAsync(id);
            if (lIMITE == null)
            {
                return HttpNotFound();
            }
            return View(lIMITE);
        }

        // POST: Limite/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "analisis,id_analisis,unidades,incerteza,limite_deteccion")] LIMITE lIMITE)
        {
            if (ModelState.IsValid)
            {
                db.Entry(lIMITE).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(lIMITE);
        }

        // GET: Limite/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            LIMITE lIMITE = await db.LIMITES.FindAsync(id);
            if (lIMITE == null)
            {
                return HttpNotFound();
            }
            return View(lIMITE);
        }

        // POST: Limite/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            LIMITE lIMITE = await db.LIMITES.FindAsync(id);
            db.LIMITES.Remove(lIMITE);
            await db.SaveChangesAsync();
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
