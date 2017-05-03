using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LaboratorioMarn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SolicitanteController : Controller
    {
        MARNEntities db = new MARNEntities();
        // GET: Solicitante
        public ActionResult Index()
        {
            List<ANALISI> analisisList = new List<ANALISI>();

            ViewBag.Analisis = analisisList;
            return View(db.SOLICITANTEs.ToList());
        }
        
        [HttpGet]
        public ActionResult Create(int bandera = 0)
        {
            if (bandera == 1)
            {
                return PartialView("~/Views/Solicitante/_Create.cshtml");
            }
   
            return View();

         }

        [HttpPost]
        public ActionResult Create(SOLICITANTE solicitante, int bandera)
        {
            if (!ModelState.IsValid)
            {
                if (bandera == 1)
                {
                    string messages = string.Join("; ",
                                    ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));


                    string[] messagesArray = messages.Split(';');

                    return Json(new { valid = false, msgs = messagesArray });
                }

                return View();

            }

            //var presetckbox = Request.Form["preset"];

            var presetckbox2 = Request.Form["preset[]"];
            solicitante.preset_analisis = presetckbox2;

            db.SOLICITANTEs.Add(solicitante);
            db.SaveChanges();

            if (bandera == 1)
            {
                return Json(new { valid = true });
            }
                
            return RedirectToAction("Index");
        
        }

        // var PresetEditar

        

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var solicitante = db.SOLICITANTEs.Find(id);

            return View(solicitante);
        }

        [HttpPost]
        public ActionResult Edit(int id, SOLICITANTE postSolicitante)
        {
            if (ModelState.IsValid)
            {
                var solicitante = db.SOLICITANTEs.Find(id);

                var presetEdit = Request.Form["checkAnalisis[]"];
                postSolicitante.preset_analisis = presetEdit;

                db.Entry(solicitante).CurrentValues.SetValues(postSolicitante);
                
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(postSolicitante);
        }
        
          

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var solicitante = db.SOLICITANTEs.Find(id);

            return View(solicitante);
        }

        [HttpPost]
        public ActionResult Delete(int id, SOLICITANTE postSolicitante)
        {
            if (ModelState.IsValid)
            {
                var solicitante = db.SOLICITANTEs.Find(id);

                //solicitante = postSolicitante;
                db.SOLICITANTEs.Remove(solicitante);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(postSolicitante);
        }
    
    }
}