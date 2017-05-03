using LaboratorioMarn.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LaboratorioMarn.Infrastructure;
using LaboratorioMarn.Models.Informe;
using LaboratorioMarn.Classes;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Text;

namespace LaboratorioMarn.Controllers
{
    public class EmpleadoSeleccion 
    {
        public byte idEmpleado { get; set; }
        public List<Int32> analisis { get; set; }
    }

    [Authorize(Roles = "Admin")]
    public class SolicitudController : Controller
    {
        protected const byte estadoAprobado = 2, estadoEsperaVisita = 2, estadoIniciado = 3;
        protected const byte estadoProcesoAnalistaAsignado = 1;

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

        public ActionResult Index()
        {
            IEnumerable<SOLICITUD> solicitudes = db.SOLICITUDs.OrderByDescending(s => s.id_solicitud).ToList();
            return View(solicitudes);
        }

        [HttpPost]
        public ActionResult Lista(string visita)
        {
            IEnumerable<SOLICITUD> solicitudes = db.SOLICITUDs.Where(s => s.VISITAs.FirstOrDefault().id == visita).OrderByDescending(s => s.id_solicitud).ToList();
            return PartialView(solicitudes);
        }

        [HttpGet]
        public ActionResult FormDetalleSolicitud(int idSolicitud) {

            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            List<int> selectedItems = new List<int>();
            foreach (DETALLE_SOLICITUD detalle in solicitud.DETALLE_SOLICITUD)
            {
                selectedItems.Add(detalle.id_analisis);
            }

            List<AREA> areas = db.AREAs.ToList();

            ViewBag.Solicitud = solicitud;
            ViewBag.SelectedItems = selectedItems;
            ViewBag.Areas = areas;
            return View();
        }


        [HttpPost]
        public ActionResult AsignarAnalisis(int idSolicitud, List<string> checkAnalisis)
        {
            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            List<AREA> areas = db.AREAs.ToList();
            List<ANALISI> analisis = new List<ANALISI>(); 
            for (var i = 0; i < checkAnalisis.Count; i++)
            {
                ANALISI analisi = db.ANALISIS.Find(Int32.Parse(checkAnalisis[i]));
                analisis.Add(analisi);
            }
            
            ViewBag.Analisis = analisis;
            ViewBag.Solicitud = solicitud;
            ViewBag.Areas = areas;
            return PartialView();
        }


        [HttpPost]
        public ActionResult Asignacion(string idArea, List<string> analisis)
        {
            AREA area = db.AREAs.Find(Int32.Parse(idArea));

            List<EMPLEADO> empleadosArea = db.EMPLEADOes.Where(e => e.id_area == area.id_area).ToList();

            List<ANALISI> analisisList = new List<ANALISI>();

            foreach (var analisi in analisis)
            {
                analisisList.Add(db.ANALISIS.Find(Int32.Parse(analisi)));
            }
            
            ViewBag.Analisis = analisisList;
            ViewBag.Empleados = empleadosArea;

            return PartialView();
        }

        [HttpPost]
        public ActionResult GuardarAsignacion()
        {

            Stream req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string json = new StreamReader(req).ReadToEnd();

            var dbTransaction = db.Database.BeginTransaction();
            Object result;
            try
            {
                EMPLEADO empleado = this.empleado();

                JObject input = (JObject)JsonConvert.DeserializeObject(json);

                int idSolicitud = input.GetValue("idSolicitud").ToObject<int>();
                var empleados = input.GetValue("empleados");

                SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

                List<EmpleadoSeleccion> empleadosSeleccion = getEmpleadosSeleccion(empleados);

                List<int> analisisSolicitud = new List<int>();

                foreach (EmpleadoSeleccion empleadoSeleccion in empleadosSeleccion)
                {
                    List<Int32> analisisList = empleadoSeleccion.analisis;

                    for (var i = 0; i < analisisList.Count(); i++)
                    {
                        analisisSolicitud.Add(analisisList[i]);

                        ANALISI analisis = db.ANALISIS.Find(analisisList[i]);

                        IEnumerable<DETALLE_SOLICITUD> detalles = solicitud.DETALLE_SOLICITUD;

                        DETALLE_SOLICITUD detalleSolicitud;
                        if (detalles.Where(d => d.id_analisis == analisis.id_analisis).Count() == 1)
                        {
                            detalleSolicitud = detalles.Where(d => d.id_analisis == analisis.id_analisis).First();
                        }
                        else 
                        {
                            detalleSolicitud = new DETALLE_SOLICITUD()
                            {
                                id_solicitud = solicitud.id_solicitud,
                                id_analisis = analisis.id_analisis
                            };

                            db.DETALLE_SOLICITUD.Add(detalleSolicitud);

                            db.SaveChanges();
                        }

                        ANALISIS_EMPLEADO analisisEmpleado;

                        if (detalleSolicitud.ANALISIS_EMPLEADO.Count() == 1)
                        {
                            analisisEmpleado = detalleSolicitud.ANALISIS_EMPLEADO.First();
                            analisisEmpleado.id_empleado = empleadoSeleccion.idEmpleado;
                        }
                        else 
                        {
                            analisisEmpleado = new ANALISIS_EMPLEADO()
                            {
                                id_detalle_solicitud = detalleSolicitud.id_detalle_solicitud,
                                id_empleado = empleadoSeleccion.idEmpleado
                            };

                            db.ANALISIS_EMPLEADO.Add(analisisEmpleado);

                            db.SaveChanges();
                        } 
                    }

                    if (analisisList.Count() > 0)
                    {
                        PROCESO_ANALISTA procesoAnalista;
                        if (db.PROCESO_ANALISTA.Where(pa => pa.empleado_id == empleadoSeleccion.idEmpleado).Count() > 1)
                        {
                            procesoAnalista = db.PROCESO_ANALISTA.Where(pa => pa.empleado_id == empleadoSeleccion.idEmpleado).First();

                            procesoAnalista.cantidad_analisis = (byte)analisisList.Count();
                        }
                        else 
                        { 
                            procesoAnalista = new PROCESO_ANALISTA
                            {
                                solicitud_id = solicitud.id_solicitud,
                                empleado_id = empleadoSeleccion.idEmpleado,
                                cantidad_analisis = (byte)analisisList.Count()
                            };

                            db.PROCESO_ANALISTA.Add(procesoAnalista);
                        }

                        db.SaveChanges();
                    }

                    List<DETALLE_SOLICITUD> detallesToRemove = db.DETALLE_SOLICITUD.Where(ds =>  ds.id_solicitud == solicitud.id_solicitud && ! analisisSolicitud.Contains(ds.id_analisis)).ToList();

                    foreach (DETALLE_SOLICITUD detalleToRemove in detallesToRemove)
                    {
                        db.DETALLE_SOLICITUD.Remove(detalleToRemove);
                    }                                        
                    
                    db.SaveChanges();
                }

                dbTransaction.Commit();

                result = new { isValid = true, id = solicitud.id_solicitud };
            }
            catch (Exception e)
            {
                dbTransaction.Rollback();

                var msg = e.Message;

                result = new { isValid = false, msg = msg };
            }

            return Json(result);
        }

        [HttpPost]
        public ActionResult IniciarAnalisis(int idSolicitud)
        {
            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);
            EMPLEADO empleado = this.empleado();

            if (solicitud.DETALLE_SOLICITUD.First().ANALISIS_EMPLEADO.FirstOrDefault() == null)
            {
                return Json(new { valid = false, msg = "No se han asignado trabajos a los analistas" });
            }

            if (solicitud.VISITAs.Count() == 0)
            {
                return Json(new { valid = false, msg = "La solicitud no tiene informacion de visita" });
            }
            

            solicitud.id_estado_proceso = estadoIniciado;
            solicitud.fecha_inicio = DateTime.Now;

            List<PROCESO_ANALISTA> procesosAnalistas = db.PROCESO_ANALISTA.Where(pa => pa.solicitud_id == idSolicitud).ToList();

            foreach (PROCESO_ANALISTA procesoAnalista in procesosAnalistas)
            {
                procesoAnalista.fecha_asignacion = DateTime.Now;
                procesoAnalista.id_estado_proceso_analista = estadoProcesoAnalistaAsignado;

                EMPLEADO empleadoReceptor = procesoAnalista.EMPLEADO;

                // enviar notificacion
                Notificacion notificacion = new Notificacion(empleado.id_empleado, "Se ha asignado trabajo para la solicitud " + solicitud.no_referencia + ".", "-", Url.Action("Administrar", "Analisis", new { idSolicitud = solicitud.id_solicitud }));
                notificacion.setReceptor(empleadoReceptor.id_empleado);
                notificacion.send();
            }

            db.SaveChanges();

            return Json(new { valid = true });
        }

        [HttpGet]
        public ActionResult Detalle(int id)
        {
            SOLICITUD solicitud = db.SOLICITUDs.Find(id);

            Dictionary<string, List<string>> analisisPorEmpleado = new Dictionary<string, List<string>>();

            foreach (DETALLE_SOLICITUD detalle in  solicitud.DETALLE_SOLICITUD)
            {
                try
                { 
                    ANALISIS_EMPLEADO analisisEmpleado = detalle.ANALISIS_EMPLEADO.First();

                    String nombreEmpleado = analisisEmpleado.EMPLEADO.nombre_empleado;

                    if (analisisPorEmpleado.Where(ape => ape.Key.Equals(nombreEmpleado)).Count() > 0)
                    {
                        analisisPorEmpleado.Where(ape => ape.Key.Equals(nombreEmpleado)).First().Value.Add(detalle.ANALISI.nombre);
                    }
                    else {
                        List<string> list = new List<string>();
                        list.Add(detalle.ANALISI.nombre);

                        analisisPorEmpleado.Add(nombreEmpleado, list);
                    }
                }
                catch (Exception e)
                {
                
                }
                
            }

            try
            {
                VISITA visita = db.VISITAs.First(v => v.id_solicitud == id);
                ViewBag.Visita = visita;
            }
            catch (Exception e)
            {
                ViewBag.Visita = null;
            }

            ViewBag.AnalisisPorEmpleado = analisisPorEmpleado;

            return View(solicitud);
        }

        [HttpPost]
        public ActionResult CrearInforme(int idSolicitud)
        {
            Object result;

            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);
            VISITA visita = solicitud.VISITAs.First();

            try
            {
                if (solicitud.id_estado != 3 || (solicitud.id_estado_proceso != 4 && solicitud.id_estado_proceso != 6))
                {
                    throw new Exception ("No puede generar reportes con los estados actuales de la solicitud");
                }

                if (solicitud.id_estado_proceso == 6 && solicitud.fecha_finalizacion == null)
                {
                    solicitud.fecha_finalizacion = DateTime.Now;
                }

                var tipo = "O";
                if (solicitud.INFORMES.Count() > 0)
                {
                    tipo = "R";
                }

                var areas = solicitud.DETALLE_SOLICITUD.GroupBy(ds => ds.ANALISI.AREA).ToList();

                List<string> areasInforme = new List<string>();
                List<INFORME> informes = new List<INFORME>();

                foreach (var area in areas)
                {
                    if (area.Key.id_area < 4)
                    {
                        if (!areasInforme.Contains("FQAA"))
                        { 
                            areasInforme.Add("FQAA");
                        }
                    }
                    else {
                        if (!areasInforme.Contains("Bact"))
                        {
                            areasInforme.Add("Bact");
                        }
                    }
                }

                foreach (string areaInforme in areasInforme)
                { 
                
                    List<Parametro> parametros = new List<Parametro>();
                    List<Resultado> resultados = new List<Resultado>();

                    foreach (DETALLE_VISITA detalleVisita in visita.DETALLE_VISITA)
                    {
                        Parametro parametro = new Parametro()
                        {
                            nombre = detalleVisita.MUESTRA.nombre_muestra,
                            valor = detalleVisita.value.ToString(),
                            expresado = detalleVisita.MUESTRA.expresado,
                        };

                        parametros.Add(parametro);
                    }

                    IEnumerable<LaboratorioMarn.DETALLE_SOLICITUD> detallesSolicitud = solicitud.DETALLE_SOLICITUD.ToList();

                    foreach (DETALLE_SOLICITUD detalle in detallesSolicitud)
                    {
                        REPORTE reporte = detalle.ANALISIS_EMPLEADO.First().REPORTEs.First();

                        Resultado resultado = new Resultado()
                        {
                            analisis = detalle.ANALISI.nombre,
                            resultado = reporte.resultado.ToString(),
                            incerteza = reporte.incerteza.ToString(),
                            unidades = reporte.unidades,
                            limiteDeteccion = reporte.limite_deteccion.ToString(),
                            metodo = detalle.ANALISI.metodo
                        };

                        resultados.Add(resultado);
                    }

                    InformeSolicitud informeSolicitud = new InformeSolicitud()
                    {
                        tipo = tipo,
                        numeroReferencia = solicitud.no_referencia,
                        solicitante = solicitud.SOLICITANTE.nombre_solicitante,
                        fechaInicio = solicitud.fecha_inicio.Value.ToShortDateString(),
                        fechaFinalizacion = solicitud.fecha_finalizacion.Value.ToShortDateString(),
                        direccion = visita.SITIO_MUESTREO.direccion_sitio,
                        fechaTomaMuestra = visita.fecha_visita.Value.ToString(),
                        fechaRecepcion = visita.fecha_entrega.Value.ToShortDateString(),
                        responsableMuestra = visita.EMPLEADO.nombre_empleado,
                        tipoMuestra = visita.TIPO_MUESTRA.nombre_tipo_muestra,
                        procedencia = visita.PROCEDENCIA.nombre_procedencia,
                        areas = areaInforme,
                        fechaGeneracion = DateTime.Now.ToShortDateString(),
                        Parametros = parametros,
                        Resultados = resultados
                    };

                    string informeJson = JsonConvert.SerializeObject(informeSolicitud);

                    INFORME informe = new INFORME()
                    {
                        data = informeJson,
                        fecha_creacion = DateTime.Now,
                        tipo = tipo,
                        id_solicitud = solicitud.id_solicitud,
                        areas = areaInforme
                    };

                    solicitud.INFORMES.Add(informe);

                    informes.Add(informe);
                }

                solicitud.informe_generado = 1;
                solicitud.id_estado_proceso = 5;

                foreach (PROCESO_ANALISTA procesoAnalista in solicitud.PROCESO_ANALISTA)
                {
                    procesoAnalista.id_estado_proceso_analista = 6;
                }

                db.SaveChanges();

                List<int> informesIds = new List<int>();

                foreach (INFORME informe in informes)
                {
                    informesIds.Add(informe.id_informe);
                }

                result = new { valid = true, informes = informesIds };
            }
            catch (Exception e)
            {
                result = new { valid = false, msg = e.Message };
            }

            return Json(result);
        }

        public ActionResult Informe(byte idInforme)
        {
            INFORME informe = db.INFORMES.Find(idInforme);

            InformeSolicitud informeSolicitud = JsonConvert.DeserializeObject<InformeSolicitud>(informe.data);

            ViewBag.Informe = informe;
            return View(informeSolicitud);
        }

        [HttpPost]
        public ActionResult Cerrar(int idSolicitud)
        { 
            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            solicitud.id_estado = 4; // cerrado
            solicitud.id_estado_proceso = 7; // cerrado

            db.SaveChanges();

            return Json(new { valid = true });
        }

        [HttpPost]
        public ActionResult AbrirCorreccion(int idSolicitud)
        {
            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            solicitud.id_estado_proceso = 6; // en correccion
            solicitud.informe_generado = 0;
            solicitud.fecha_finalizacion = null;

            //TODO: Guardar informacion de correcion

            db.SaveChanges();

            return Json(new { valid = true });
        }

        [HttpPost]
        public ActionResult AbrirProcesoAnalistaCorreccion(int idProcesoAnalista, string comentario)
        {
            try { 
                PROCESO_ANALISTA procesoAnalista = db.PROCESO_ANALISTA.Find(idProcesoAnalista);

                procesoAnalista.SOLICITUD.id_estado = 2; // en proceso

                procesoAnalista.id_estado_proceso_analista = 5; // Correccion
                procesoAnalista.fecha_fin = null;

                EMPLEADO empleado = this.empleado();

                // TODO Agregar informacion de correccion
                // TODO Notify
                // enviar notificacion
                Notificacion notificacion = new Notificacion(empleado.id_empleado, "Los resultados de solicitud " + procesoAnalista.SOLICITUD.no_referencia + " fueron abiertos para correccion.", comentario, Url.Action("Administrar", "Analisis", new { idSolicitud = procesoAnalista.SOLICITUD.id_solicitud }));
                notificacion.setReceptor(procesoAnalista.EMPLEADO.id_empleado);
                notificacion.send();

                db.SaveChanges();
                return Json(new { valid = true });
            
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}",
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }

            return Json(new { valid = false });
        }

        [HttpPost]
        public ActionResult CancelarCorreccion(int idSolicitud)
        {
            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            solicitud.id_estado = 3;
            solicitud.id_estado_proceso = 5;
            solicitud.informe_generado = 0;

            foreach (PROCESO_ANALISTA procesoAnalista in solicitud.PROCESO_ANALISTA)
            {
                procesoAnalista.id_estado_proceso_analista = 6;
                // TODO notify
            }

            db.SaveChanges();

            return Json(new { valid = true });
        }

        [HttpPost]
        public ActionResult ExportarCSV(int idSolicitud)
        {
            SOLICITUD solicitud = db.SOLICITUDs.Find(idSolicitud);

            string attachment = "attachment; filename=solicitud_" + solicitud.no_referencia + ".csv";

            Response.Clear();
            Response.ClearHeaders();
            Response.ClearContent();
            Response.AddHeader("content-disposition", attachment);
            Response.ContentType = "text/csv";
            Response.AddHeader("Pragma", "public");

            var sb = new StringBuilder();
            List<string> line = new List<string>();

            foreach (DETALLE_SOLICITUD detalle in solicitud.DETALLE_SOLICITUD)
            {
                line.Add(solicitud.no_referencia);
                line.Add(solicitud.fecha_inicio.Value.ToShortDateString());
                line.Add(solicitud.fecha_finalizacion.Value.ToShortDateString());
                line.Add(solicitud.VISITAs.First().SITIO_MUESTREO.nombre_sitio);
                line.Add(solicitud.VISITAs.First().SITIO_MUESTREO.lat.ToString());
                line.Add(solicitud.VISITAs.First().SITIO_MUESTREO.lng.ToString());

                List<DETALLE_VISITA> detalleVisita = solicitud.VISITAs.First().DETALLE_VISITA.ToList();

                foreach (DETALLE_VISITA visita in detalleVisita)
                {
                    line.Add(visita.MUESTRA.nombre_muestra + "(" +  visita.MUESTRA.expresado + ")");
                    line.Add(visita.value.ToString());
                }

                line.Add(detalle.ANALISI.AREA.nombre_area);
                line.Add(detalle.ANALISIS_EMPLEADO.First().EMPLEADO.nombre_empleado);
                line.Add(detalle.ANALISI.nombre);

                REPORTE reporte = detalle.ANALISIS_EMPLEADO.First().REPORTEs.First();

                line.Add(reporte.resultado.ToString());
                line.Add(reporte.incerteza.ToString());
                line.Add(reporte.limite_deteccion.ToString());
                line.Add(reporte.metodo);
                line.Add(reporte.unidades);                

                string lineString = string.Join(";", line.ToArray());
                line.Clear();

                sb.AppendLine(lineString);
            }

            Response.Write(sb.ToString());

            return null;
        }

        // ****** Metodos privados

        private List<EmpleadoSeleccion> getEmpleadosSeleccion(JToken empleados)
        {
            List<EmpleadoSeleccion> empleadosSeleccion = new List<EmpleadoSeleccion>();

            foreach (JObject element in empleados)
            {
                var empleado = element.GetValue("empleado");
                byte idEmpleado = empleado.ToObject<JObject>().GetValue("id").ToObject<byte>();

                List<Int32> analisis = element.GetValue("analisis").ToObject<List<Int32>>();

                EmpleadoSeleccion empleadoSeleccion =
                        new EmpleadoSeleccion
                        {
                            idEmpleado = idEmpleado,
                            analisis = analisis
                        };
                empleadosSeleccion.Add(empleadoSeleccion);
            }

            return empleadosSeleccion;
        }

        public SOLICITUD crearSolicitud(int idSolicitante, byte idEmpleado)
        {
            var referencia = DateTime.Now.Year.ToString().Substring(2) + "-0000";
            
            try
            {
                SOLICITUD lastSolicitud = db.SOLICITUDs.OrderByDescending(s => s.id_solicitud).First();
                referencia = lastSolicitud.no_referencia;
            }
            catch (Exception e){ }

            var correlativo = Int32.Parse(referencia.Substring(3)) + 1;
            var year = Int32.Parse(referencia.Substring(0, 2));

            var nowYear = Int32.Parse(DateTime.Now.Year.ToString().Substring(2));

            if (year != nowYear)
            {
                correlativo = 1;
                year = nowYear;
            }
            
            SOLICITUD solicitud = new SOLICITUD();

            solicitud.id_solicitante = idSolicitante;
            solicitud.id_empleado = idEmpleado;
            solicitud.id_estado = estadoAprobado;
            solicitud.id_estado_proceso = estadoEsperaVisita;
            solicitud.fecha_creacion = DateTime.Now;
            solicitud.no_referencia = year.ToString() + "-" + correlativo.ToString().PadLeft(4, '0');
            solicitud.informe_generado = 0;

            db.SOLICITUDs.Add(solicitud);
            db.SaveChanges();

            return solicitud;
        }

        public void crearDetalleSolicitud(SOLICITUD Solicitud, List<EmpleadoSeleccion> empleadosSeleccion)
        {
            List<DETALLE_SOLICITUD> detallesList = new List<DETALLE_SOLICITUD>();

            foreach (EmpleadoSeleccion empleadoSeleccion in empleadosSeleccion)
            {
                List<Int32> analisisList = empleadoSeleccion.analisis;

                for (var i = 0; i < analisisList.Count(); i++)
                {
                    ANALISI analisis = db.ANALISIS.Find(analisisList[i]);

                    DETALLE_SOLICITUD detalleSolicitud = new DETALLE_SOLICITUD()
                    {
                        id_solicitud = Solicitud.id_solicitud,
                        id_analisis = analisis.id_analisis

                    };

                    db.DETALLE_SOLICITUD.Add(detalleSolicitud);

                    db.SaveChanges();

                    ANALISIS_EMPLEADO analisisEmpleado = new ANALISIS_EMPLEADO()
                    {
                        id_detalle_solicitud = detalleSolicitud.id_detalle_solicitud,
                        id_empleado = empleadoSeleccion.idEmpleado
                    };

                    db.ANALISIS_EMPLEADO.Add(analisisEmpleado);

                    db.SaveChanges();
                }

                if (analisisList.Count() > 0)
                { 
                    PROCESO_ANALISTA procesoAnalista = new PROCESO_ANALISTA
                    {
                        solicitud_id = Solicitud.id_solicitud,
                        empleado_id = empleadoSeleccion.idEmpleado,
                        cantidad_analisis = (byte) analisisList.Count()
                    };

                    db.PROCESO_ANALISTA.Add(procesoAnalista);
                }


                db.SaveChanges();
            }
        }
    }
}