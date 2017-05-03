using LaboratorioMarn.Infrastructure;
using LaboratorioMarn.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace LaboratorioMarn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class VisitaController : Controller
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

            return db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);
        }

        [HttpGet]
        public ActionResult Index()
        {
            EMPLEADO empleado = this.empleado();

            List<VISITA> visitas = db.VISITAs.Where(v => v.SOLICITUD.id_empleado == empleado.id_empleado).OrderByDescending(v => v.fecha_visita).ToList();

            return View(visitas);
        }

        [HttpGet]
        public ActionResult Create(int idSolicitud)
        {
            ViewBag.idSolicitud = idSolicitud;

            this.fillViewBag();
            return PartialView();
        }

        [HttpPost]
        public ActionResult Create(VISITA visita)
        {
            if (!ModelState.IsValid)
            {
                string messages = string.Join("; ",
                                ModelState.Values
                                    .SelectMany(x => x.Errors)
                                    .Select(x => x.ErrorMessage));


                string[] messagesArray = messages.Split(';');

                return Json(new { valid = false, msgs = messagesArray });
            }

            db.VISITAs.Add(visita);
            db.SaveChanges();

            return Json(new { valid = true });
        }


        [HttpGet]
        public ActionResult Edit(int id, int idSolicitud)
        {
            ViewBag.idSolicitud = idSolicitud;

            var visita = db.VISITAs.Find(id);

            this.fillViewBag();

            var empleadoVisita = visita.id_empleado;

            IEnumerable<SelectListItem> empleadosList = ViewBag.EmpleadoSelection;

            foreach (SelectListItem empleadoItem in empleadosList)
            {
                if (empleadoItem.Value.Equals(empleadoVisita.ToString()))
                {
                    empleadoItem.Selected = true;
                }
            }

            var procedenciaVisita = visita.id_procedencia;
            IEnumerable<SelectListItem> procedenciaList = ViewBag.ProcedenciaSelection;
            foreach (SelectListItem procedenciaItem in procedenciaList)
            {
                if (procedenciaItem.Value.Equals(procedenciaVisita.ToString()))
                {
                    procedenciaItem.Selected = true;
                }
            }

            var muestraVisita = visita.id_tipo_muestra;
            IEnumerable<SelectListItem> muestraList = ViewBag.MuestraSelection;
            foreach (SelectListItem muestraItem in muestraList)
            {
                if (muestraItem.Value.Equals(muestraVisita.ToString()))
                {
                    muestraItem.Selected = true;
                }
            }

            var sitio = visita.id_sitio_muestreo;
            IEnumerable<SelectListItem> sitioList = ViewBag.MuestraSelection;
            foreach (SelectListItem sitioItem in sitioList)
            {
                if (sitioItem.Value.Equals(sitio.ToString()))
                {
                    sitioItem.Selected = true;
                }
            }

            ViewBag.SitioMuestreoSelection = empleadosList;
            ViewBag.EmpleadoSelection = empleadosList;
            ViewBag.procedenciaSelection = procedenciaList;
            ViewBag.MuestraSelection = muestraList;

            return PartialView(visita);
        }

        [HttpPost]
        public ActionResult Edit(int id, VISITA postVisita)
        {
            if (!ModelState.IsValid)
            {
                string messages = string.Join("; ",
                                ModelState.Values
                                    .SelectMany(x => x.Errors)
                                    .Select(x => x.ErrorMessage));


                string[] messagesArray = messages.Split(';');

                return Json(new { valid = false, msgs = messagesArray });
            }

            VISITA visita = db.VISITAs.Find(id);
            db.Entry(visita).CurrentValues.SetValues(postVisita);
            db.SaveChanges();

            return Json(new { valid = true });
        }

        [HttpGet]
        public ActionResult CompleteVisit(int id)
        {
            var visita = db.VISITAs.Find(id);

            this.fillViewBag();

            var muestras = db.MUESTRAs.ToList();
            muestras.Insert(0, new MUESTRA { id_muestra = 0, nombre_muestra = "Seleccionar"  });

            IEnumerable<SelectListItem> muestrasList =

            from value in muestras

            select new SelectListItem

            {
                Text = value.nombre_muestra + "(" + value.expresado + ")",
                Value = value.id_muestra.ToString()
            };

            ViewBag.MuestraSelection = muestrasList;

            if (visita.id_empleado_entrega != null)
            {
                IEnumerable<SelectListItem> empleadosList = ViewBag.EmpleadoSelection;

                foreach (SelectListItem empleado in empleadosList)
                {
                    if (empleado.Value == visita.id_empleado_entrega.ToString())
                    {
                        empleado.Selected = true;
                    }
                }

                ViewBag.EmpleadoSelection = empleadosList;
            }

            return PartialView(visita);
        }

        [HttpPost]
        public ActionResult CompleteVisit()
        {
            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            var dbTransaction = db.Database.BeginTransaction();
            Object result;
            try
            {
                // assuming JSON.net/Newtonsoft library from http://json.codeplex.com/
                JObject input = (JObject)JsonConvert.DeserializeObject(json);

                int idVisita = input.GetValue("visita").ToObject<int>();
                string horaVisita = input.GetValue("hora").ToObject<string>();

                var latObject = input.GetValue("lat");
                var lngObject = input.GetValue("lng");

                Nullable<decimal> lat = null;
                Nullable<decimal> lng = null;

                if (latObject != null)
                {
                    lat = latObject.ToObject<decimal>();
                }

                if (lngObject != null)
                {
                    lng = lngObject.ToObject<decimal>();
                }

                string identificador = input.GetValue("identificador").ToObject<string>();
                string frascos = input.GetValue("frascos").ToObject<string>();

                var detalles = input.GetValue("detalles");

                
                VISITA visita = db.VISITAs.Find(idVisita);

                visita.identificador = identificador;
                visita.no_frascos = byte.Parse(frascos);
                visita.lat = lat;
                visita.lng = lng;

                var horas = Int32.Parse(horaVisita.Substring(0, 2));
                var minutos = Int32.Parse(horaVisita.Substring(3, 2));
                
                visita.fecha_visita = new DateTime(visita.fecha_visita.Value.Year, visita.fecha_visita.Value.Month, visita.fecha_visita.Value.Day, horas, minutos, 0);

                List<DETALLE_VISITA> detallesVisita = db.DETALLE_VISITA.Where(dv => dv.id_visita == visita.id_visita).ToList();

                List<int> detallesVisitaInput = new List<int>();
                foreach (JObject detalle in detalles)
                {
                    byte muestra = detalle.GetValue("muestra").ToObject<byte>();
                    string value = detalle.GetValue("value").ToObject<string>();
                    
                    DETALLE_VISITA detalleVisita = db.DETALLE_VISITA.SingleOrDefault(dv => dv.id_muestra == muestra && dv.id_visita == idVisita );

                    if (detalleVisita == null)
                    {
                        detalleVisita = new DETALLE_VISITA(){
                            id_visita = idVisita,
                            id_muestra = muestra,
                            value = Decimal.Parse(value)
                        };

                        visita.DETALLE_VISITA.Add(detalleVisita);
                    }
                    else
                    {
                        detalleVisita.value = Decimal.Parse(value);
                    }

                    db.SaveChanges();
                    detallesVisitaInput.Add(detalleVisita.id_detalle_visita);
                    
                }

                foreach (DETALLE_VISITA detalleVisita in detallesVisita)
                {
                    if (!detallesVisitaInput.Exists(d => d == detalleVisita.id_detalle_visita))
                    { 
                        db.DETALLE_VISITA.Remove(detalleVisita);
                    }
                }
                db.SaveChanges();

                dbTransaction.Commit();

                result = new { isValid = true };
            }
            catch (Exception e)
            {
                dbTransaction.Rollback();

                var msg = e.Message;

                result = new { isValid = false, msg = msg };
            }

            return Json(result);
        }

        public ActionResult Administrar(int id)
        {
            GRUPO_SOLICITUD solicitud = db.GRUPO_SOLICITUD.Find(id);

            ViewBag.solicitud = solicitud;

            return View();
        }

        [HttpGet]
        public ActionResult EditarVisita(string id)
        {
            VISITA visita = db.VISITAs.Where(v => v.id == id).First();

            this.fillViewBag();

            var empleadoVisita = visita.id_empleado;
            var empleadoEntrega = visita.id_empleado_entrega;

            IEnumerable<SelectListItem> empleadosList = ViewBag.EmpleadoSelection;

            foreach (SelectListItem empleadoItem in empleadosList)
            {
                if (empleadoItem.Value.Equals(empleadoVisita.ToString()))
                {
                    empleadoItem.Selected = true;
                }
            }

            IEnumerable<SelectListItem> empleados2List = ViewBag.EmpleadoSelection;

            foreach (SelectListItem empleadoItem in empleadosList)
            {
                if (empleadoItem.Value.Equals(empleadoEntrega.ToString()))
                {
                    empleadoItem.Selected = true;
                }
            }

            var procedenciaVisita = visita.id_procedencia;
            IEnumerable<SelectListItem> procedenciaList = ViewBag.ProcedenciaSelection;
            foreach (SelectListItem procedenciaItem in procedenciaList)
            {
                if (procedenciaItem.Value.Equals(procedenciaVisita.ToString()))
                {
                    procedenciaItem.Selected = true;
                }
            }

            var muestraVisita = visita.id_tipo_muestra;
            IEnumerable<SelectListItem> muestraList = ViewBag.MuestraSelection;
            foreach (SelectListItem muestraItem in muestraList)
            {
                if (muestraItem.Value.Equals(muestraVisita.ToString()))
                {
                    muestraItem.Selected = true;
                }
            }

            var sitio = visita.id_sitio_muestreo;
            IEnumerable<SelectListItem> sitioList = ViewBag.MuestraSelection;
            foreach (SelectListItem sitioItem in sitioList)
            {
                if (sitioItem.Value.Equals(sitio.ToString()))
                {
                    sitioItem.Selected = true;
                }
            }

            ViewBag.EmpleadoSelection = empleadosList;
            ViewBag.Empleado2Selection = empleados2List;
            ViewBag.procedenciaSelection = procedenciaList;
            ViewBag.MuestraSelection = muestraList;
            ViewBag.SitioMuestreoSelection = sitioList;

            return PartialView();
        }

        [HttpPost]
        public ActionResult EditarVisitaPost(string visitaId)
        {
            IEnumerable<VISITA> visita = db.VISITAs.Where(v => v.id == visitaId);

            Object result = new { isValid = true };

            return Json(result);
        }

        [HttpPost]
        public ActionResult ElimnarVisita(string id)
        {
            Object result;
            
            IEnumerable<SOLICITUD> solicitudes = db.SOLICITUDs.Where(s => s.VISITAs.FirstOrDefault().id == id);

            var todelete = true;

            try
            {
                foreach (SOLICITUD solicitud in solicitudes)
                {
                    if (solicitud.id_estado != 1)
                    {
                        todelete = false;
                    }
                }

                if (todelete)
                {
                    IEnumerable<SOLICITUD> solicitudesToRemove = db.SOLICITUDs.Where(s => s.VISITAs.FirstOrDefault().id == id);

                    foreach (SOLICITUD solicitudToRemove in solicitudesToRemove)
                    {
                        if (solicitudToRemove.DETALLE_SOLICITUD.Count() > 0)
                        {
                            foreach (DETALLE_SOLICITUD detalle in solicitudToRemove.DETALLE_SOLICITUD.ToList())
                            {
                                if (detalle.ANALISIS_EMPLEADO.FirstOrDefault() != null)
                                {
                                    db.ANALISIS_EMPLEADO.Remove(detalle.ANALISIS_EMPLEADO.FirstOrDefault());
                                }

                                db.DETALLE_SOLICITUD.Remove(detalle);
                            }
                        }

                        db.SOLICITUDs.Remove(solicitudToRemove);
                    }

                    db.SaveChanges();
                }
        
                result = new { isValid = true };
            }
            catch (Exception e)
            {
                result = new { isValid = false, msg = e.Message };
            }
                        

            return Json(result);
        }

        [HttpGet]
        public ActionResult CreateVisitas(int solicitud)
        {
            ViewBag.solicitud = solicitud;

            this.fillViewBag();
            return PartialView();
        }

        [HttpPost]
        public ActionResult CreateVisitas(VISITA visita, int solicitud, int cantidad_muestras)
        {
            GRUPO_SOLICITUD grupoSolicitud = db.GRUPO_SOLICITUD.Find(solicitud);

            var referencia = DateTime.Now.Year.ToString().Substring(2) + "-0000";

            try
            {
                SOLICITUD lastSolicitud = db.SOLICITUDs.OrderByDescending(s => s.id_solicitud).First();
                referencia = lastSolicitud.no_referencia;
            }
            catch (Exception e) { }

            var correlativo = Int32.Parse(referencia.Substring(3)) + 1;
            var year = Int32.Parse(referencia.Substring(0, 2));

            var nowYear = Int32.Parse(DateTime.Now.Year.ToString().Substring(2));

            if (year != nowYear)
            {
                correlativo = 1;
                year = nowYear;
            }

            var dbTransaction = db.Database.BeginTransaction();
            Object result;
            try
            {
                for (var i = 0; i < cantidad_muestras; i++)
                {

                    SOLICITUD newSolicitud = new SOLICITUD();

                    newSolicitud.fecha_creacion = DateTime.Now;
                    newSolicitud.id_empleado = this.empleado().id_empleado;
                    newSolicitud.id_estado = 1;
                    newSolicitud.id_estado_proceso = 1;
                    newSolicitud.no_referencia = year.ToString() + "-" + correlativo.ToString().PadLeft(4, '0');
                    newSolicitud.id_solicitante = grupoSolicitud.SOLICITANTE.id_solicitante;

                    db.SOLICITUDs.Add(newSolicitud);
                    db.SaveChanges();

                    visita.id_solicitud = newSolicitud.id_solicitud;
                    visita.id_grupo_solicitud = solicitud;

                    var horaEntrega = Request.Form["hora_entrega"];

                    var horasEntrega = Int32.Parse(horaEntrega.Substring(0, 2));
                    var minutosEntrega = Int32.Parse(horaEntrega.Substring(3, 2));

                    var fechaEntrega = visita.fecha_entrega.Value.ToShortDateString();

                    visita.fecha_entrega = new DateTime(Int32.Parse(fechaEntrega.Substring(6, 4)), Int32.Parse(fechaEntrega.Substring(3, 2)), Int32.Parse(fechaEntrega.Substring(0, 2)), horasEntrega, minutosEntrega, 0);

                    if (visita.lat == null)
                    { 
                        SITIO_MUESTREO sitio = db.SITIO_MUESTREO.Find(visita.id_sitio_muestreo);
                        visita.lat = sitio.lat;
                        visita.lng = sitio.lng;
                    }

                    db.VISITAs.Add(visita);
                    db.SaveChanges();


                    foreach (var id in grupoSolicitud.preset.Split(','))
                    {
                        int analisis = Int32.Parse(id);

                        DETALLE_SOLICITUD detalle = new DETALLE_SOLICITUD();

                        detalle.id_analisis = analisis;
                        detalle.id_solicitud = newSolicitud.id_solicitud;

                        db.DETALLE_SOLICITUD.Add(detalle);
                    }

                    db.SaveChanges();

                    correlativo++;
                }

                dbTransaction.Commit();

                result = new { isValid = true };
            }
            catch (Exception e)
            {
                
                dbTransaction.Rollback();

                var msg = e.Message;

                result = new { isValid = false, msg = msg };
            }
               

            return Json(result);
        }
        

        private void fillViewBag()
        {
            List<EMPLEADO> empleadosCampo = db.EMPLEADOes.Where(e => e.id_tipo_empleado == 3).ToList();

            IEnumerable<SelectListItem> empleadoList =
            from empleado in empleadosCampo
            select new SelectListItem
            {
                Text = empleado.nombre_empleado,
                Value = empleado.id_empleado.ToString()
            };

            var procedencias = db.PROCEDENCIAs.ToList();
            IEnumerable<SelectListItem> procedenciasList =

            from value in procedencias

            select new SelectListItem

            {
                Text = value.nombre_procedencia,
                Value = value.id_procedencia.ToString()
            };

            var muestras = db.TIPO_MUESTRA.ToList();
            IEnumerable<SelectListItem> muestrasList =

            from value in muestras

            select new SelectListItem

            {
                Text = value.nombre_tipo_muestra,
                Value = value.id_tipo_muestra.ToString()
            };

            var sitios = db.SITIO_MUESTREO.ToList();
            IEnumerable<SelectListItem> sitiosList =

            from value in sitios

            select new SelectListItem

            {
                Text = value.nombre_sitio,
                Value = value.id_sitio_muestreo.ToString()
            };

            ViewBag.SitioMuestreoSelection = sitiosList;
            ViewBag.ProcedenciaSelection = procedenciasList;
            ViewBag.MuestraSelection = muestrasList;
            ViewBag.EmpleadoSelection = empleadoList;
        }
    }
}