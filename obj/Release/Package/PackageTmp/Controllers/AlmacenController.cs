using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LaboratorioMarn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AlmacenController : Controller
    {
        MARNEntities db = new MARNEntities();

        // GET: Almacen
        public ActionResult Index()
        {
            return View(db.ALMACENs.ToList());
        }

        // GET: Almacen/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Almacen/Create
        [HttpPost]
        public ActionResult Create(ALMACEN almacen)
        {
            if (ModelState.IsValid)
            {
                db.ALMACENs.Add(almacen);
                db.SaveChanges();

                return RedirectToAction("Index");

            }
            return View(almacen);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var almacen = db.ALMACENs.Find(id);

            return View(almacen);
        }

        [HttpPost]
        public ActionResult Edit(int id, ALMACEN almacenPost)
        {

            if (ModelState.IsValid)
            {
                var almacen = db.ALMACENs.Find(id);
                almacen.estante = almacenPost.estante;
                almacen.nivel = almacenPost.nivel;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(almacenPost);
        }
    }
}