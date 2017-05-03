using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LaboratorioMarn.Controllers
{
    public class EmpleadoController : Controller
    {
        MARNEntities db = new MARNEntities();

        public ActionResult Index()
        {
            return View(db.EMPLEADOes.ToList());
        }

        [HttpGet]
        public ActionResult Create()
        {

            var areas = db.AREAs.ToList();

            IEnumerable<SelectListItem> areasList =

            from value in areas

            select new SelectListItem
            {

                Text = value.nombre_area,
                Value = value.id_area.ToString()

            };

            var tipos = db.TIPO_EMPLEADO.ToList();

            IEnumerable<SelectListItem> tiposList =

            from value in tipos

            select new SelectListItem
            {

                Text = value.nombre,
                Value = value.id_tipo_empleado.ToString()

            };

            ViewBag.AreaSelection = areasList;

            ViewBag.TipoSelection = tiposList;

            return View();
        }

        [HttpPost]
        public ActionResult Create(EMPLEADO empleado)
        {
            if (ModelState.IsValid)
            {
                db.EMPLEADOes.Add(empleado);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(empleado);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var empleado = db.EMPLEADOes.Find(id);

            return View(empleado);
        }

        [HttpPost]
        public ActionResult Edit(int id, EMPLEADO postEmpleado)
        {
            if (ModelState.IsValid)
            {
                var empleado = db.EMPLEADOes.Find(id);

                empleado = postEmpleado;

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(postEmpleado);
        }
    }
}