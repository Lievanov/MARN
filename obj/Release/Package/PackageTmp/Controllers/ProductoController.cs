using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaboratorioMarn.Models;
using System.IO;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LaboratorioMarn.Infrastructure;
using Newtonsoft.Json.Linq;
using LaboratorioMarn.Classes;

namespace LaboratorioMarn.Controllers
{
    public class ProductoController : Controller
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

            EMPLEADO empleado = db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);

            return empleado;
        }

        [Authorize(Roles = "Admin, Analista")]
        public ActionResult Index()
        {
            ViewBag.Empleado = this.empleado();

            List<REACTIVO> reactivos = db.REACTIVOes.OrderBy(r => r.nombre).ToList();

            reactivos.Insert(0, new REACTIVO() { id_reactivo = 0, nombre = "Todos" });

            IEnumerable<SelectListItem> reactivoList =
            from reactivo in reactivos
            select new SelectListItem
            {
                Text = reactivo.nombre,
                Value = reactivo.id_reactivo.ToString()
            };

            ViewBag.ReactivoSelection = reactivoList;

            return View(db.PRODUCTOes.ToList());
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            setViewBagParameters();

            return PartialView();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(PRODUCTO producto)
        {

            var dbTransaction = db.Database.BeginTransaction();
            var msg = "";
            Object result;

            try
            {
                EMPLEADO empleado = this.empleado();

                this.setCatalogos(producto, this.Request);

                db.PRODUCTOes.Add(producto);
                db.SaveChanges();

                var cantidad = Request.Form["existencia"];

                for (var i = 0; i < Int32.Parse(cantidad); i++)
                {
                    var existencia = new EXISTENCIA_PRODUCTO();

                    existencia.cantidad = producto.presentacion_cantidad;
                    existencia.id_producto = producto.id_producto;
                    existencia.estado = "cerrado";

                    db.EXISTENCIA_PRODUCTO.Add(existencia);

                    db.SaveChanges();
                }

                foreach (EMPLEADO empleadoAnalista in db.EMPLEADOes.Where(e => e.id_tipo_empleado == 2).ToList())
                {
                    REACTIVO reactivo = db.REACTIVOes.Find(producto.id_reactivo);
                    // enviar notificacion
                    Notificacion notificacion = new Notificacion(empleado.id_empleado, "Ha ingresado producto a inventario.", "Se ingresó " + reactivo.nombre + ". Para mas detalles vea la pagina de inventario", Url.Action("Index", "Producto"));
                    notificacion.setReceptor(empleadoAnalista.id_empleado);
                    notificacion.send();
                }

                dbTransaction.Commit();

                result = new { valid = true };
                
            }
            catch (Exception e)
            {
                Console.Write(e);
                msg = e.Message;
                dbTransaction.Rollback();

                result = new { valid = false, msg = msg };
            }
            
            return Json(result);
        }

        [HttpGet]
        [Authorize(Roles = "Analista")]
        public ActionResult SeleccionSalida()
        {
            return View(db.PRODUCTOes.ToList());
        }

        [Authorize(Roles = "Analista")]
        public ActionResult FormularioSalida(List<int> products)
        {  
            List<PRODUCTO> productosParaSalida = new List<PRODUCTO>();

            foreach (int id in products)
            {
                productosParaSalida.Add(db.PRODUCTOes.Find(id));
            }

            return PartialView("~/Views/Producto/Salida/Formulario.cshtml", productosParaSalida);
        }

        [Authorize(Roles = "Analista")]
        [HttpPost]
        public ActionResult Salida()
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

                var productos = input.GetValue("productos");

                foreach (JObject element in productos)
                {
                    int productId = element.GetValue("id").ToObject<int>();
                    var salida = element.GetValue("salida").ToObject<string>();

                    PRODUCTO producto = db.PRODUCTOes.Find(productId);

                    if (salida == "")
                    {
                        throw new Exception("Por favor llene todos los campos de salida");
                    }

                    decimal cantidad = db.EXISTENCIA_PRODUCTO.Where(ep => ep.id_producto == productId && ep.estado != "terminado").Sum(ep => ep.cantidad);

                    decimal salidaDecimal = decimal.Parse(salida);

                    if (salidaDecimal > cantidad)
                    {
                        throw new Exception("El cantidad de salida para el reactivo " + producto.REACTIVO.nombre + " es mayor que la existencia");
                    }

                    Boolean notificacionAlerta = false;

                    if (((cantidad - salidaDecimal) / cantidad) * 100 < producto.minimo_porcentaje) {
                        notificacionAlerta = true;
                    }

                    DateTime fechaSalida = DateTime.Now;

                    foreach (EXISTENCIA_PRODUCTO existencia in producto.EXISTENCIA_PRODUCTO.Where(ep => ep.estado == "abierto"))
                    {
                        decimal cantidadSalida;

                        var cantidadExistencia = existencia.cantidad;

                        if (salidaDecimal >= cantidadExistencia)
                        {
                            cantidadSalida = cantidadExistencia;
                            salidaDecimal -= cantidadExistencia;
                            existencia.cantidad = 0;
                            existencia.estado = "terminado";
                        }
                        else
                        {
                            cantidadSalida = salidaDecimal;
                            salidaDecimal = 0;

                            existencia.cantidad -= cantidadSalida;
                            existencia.estado = "abierto";
                        }

                        SALIDA_EXISTENCIA salidaExistencia = new SALIDA_EXISTENCIA() { 
                            cantidad_existencia = cantidadSalida,
                            cantidad_salida = cantidadSalida,
                            fecha_salida = fechaSalida,
                            id_empleado = empleado.id_empleado,
                            id_existencia_producto = existencia.id_existencia_producto,
                            id_unidad = existencia.PRODUCTO.UNIDAD.id_unidad,
                            id_reactivo = existencia.PRODUCTO.id_reactivo
                        };

                        db.SALIDA_EXISTENCIA.Add(salidaExistencia);

                        if (salidaDecimal == 0)
                        {
                            break;
                        }
                    }

                    foreach (EXISTENCIA_PRODUCTO existencia in producto.EXISTENCIA_PRODUCTO.Where(ep => ep.estado == "cerrado"))
                    {
                        decimal cantidadSalida;

                        var cantidadExistencia = existencia.cantidad;

                        if (salidaDecimal >= cantidadExistencia)
                        {
                            cantidadSalida = cantidadExistencia;
                            salidaDecimal -= cantidadExistencia;
                            existencia.cantidad = 0;
                            existencia.estado = "terminado";
                        }
                        else
                        {
                            cantidadSalida = salidaDecimal;
                            salidaDecimal = 0;

                            existencia.cantidad -= cantidadSalida;
                            existencia.estado = "abierto";
                        }

                        SALIDA_EXISTENCIA salidaExistencia = new SALIDA_EXISTENCIA()
                        {
                            cantidad_existencia = cantidadSalida,
                            cantidad_salida = cantidadSalida,
                            fecha_salida = fechaSalida,
                            id_empleado = empleado.id_empleado,
                            id_existencia_producto = existencia.id_existencia_producto,
                            id_unidad = existencia.PRODUCTO.UNIDAD.id_unidad,
                            id_reactivo = existencia.PRODUCTO.id_reactivo
                        };

                        db.SALIDA_EXISTENCIA.Add(salidaExistencia);

                        if (salidaDecimal == 0)
                        {
                            break;
                        }
                    }

                    if (notificacionAlerta)
                    {
                        // enviar notificacion
                        Notificacion notificacionA = new Notificacion(empleado.id_empleado, "Ha dejado en alerta un producto del reactivo: " + producto.REACTIVO.nombre + ".", "Para mas detalles vea las salidas de inventario", Url.Action("reporteSalida", "Producto"));
                        notificacionA.setReceptor();
                        notificacionA.send();
                    }   
                }

                db.SaveChanges();

                // enviar notificacion
                Notificacion notificacion = new Notificacion(empleado.id_empleado, "Ha efectuado una salida de inventario.", "Para mas detalles vea las salidas de inventario", Url.Action("reporteSalida", "Producto"));
                notificacion.setReceptor();
                notificacion.send();

                
                
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

        [Authorize(Roles = "Admin, Analista")]
        public ActionResult reporteSalida()
        {
            EMPLEADO empleado = this.empleado();

            List<SALIDA_EXISTENCIA> salidaExistencia;
            if (empleado.id_tipo_empleado == 2)
            {
                salidaExistencia = db.SALIDA_EXISTENCIA.Where(se => se.id_empleado == empleado.id_empleado).OrderByDescending(se => se.fecha_salida).ToList();
            }
            else
            {
                salidaExistencia = db.SALIDA_EXISTENCIA.OrderByDescending(se => se.fecha_salida).ToList();
            }

            List<REACTIVO> reactivos = db.REACTIVOes.OrderBy(r => r.nombre).ToList();

            reactivos.Insert(0, new REACTIVO() { id_reactivo = 0, nombre = "Todos" });

            IEnumerable<SelectListItem> reactivoList = 
            from reactivo in reactivos
            select new SelectListItem
            {
                Text = reactivo.nombre,
                Value = reactivo.id_reactivo.ToString()
            };

            ViewBag.ReactivoSelection = reactivoList;
            ViewBag.Empleado = this.empleado();

            return View(salidaExistencia);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Analista")]
        public ActionResult reporteSalida(string fecha_inicio, string fecha_fin, int reactivo)
        {
            EMPLEADO empleado = this.empleado();

           

            IEnumerable<SALIDA_EXISTENCIA> salidaExistencia;
            if (empleado.id_tipo_empleado == 2)
            {
                if (fecha_inicio != "" && reactivo == 0)
                {
                    DateTime inicio = DateTime.Parse(fecha_inicio);
                    DateTime fin = DateTime.Parse(fecha_fin);

                    salidaExistencia = 
                        db.SALIDA_EXISTENCIA
                            .Where(se =>
                                se.id_empleado == empleado.id_empleado &&
                                se.fecha_salida >= inicio && se.fecha_salida <= fin
                                );
                }
                else if (fecha_inicio != "" && reactivo != 0)
                {
                    DateTime inicio = DateTime.Parse(fecha_inicio);
                    DateTime fin = DateTime.Parse(fecha_fin);

                    salidaExistencia =
                        db.SALIDA_EXISTENCIA
                            .Where(se =>
                                se.id_empleado == empleado.id_empleado &&
                                se.fecha_salida >= inicio && se.fecha_salida <= fin &&
                                se.REACTIVO.id_reactivo == reactivo
                                );
                }
                else if (fecha_inicio == "" && reactivo != 0)
                {
                    salidaExistencia =
                        db.SALIDA_EXISTENCIA
                            .Where(se =>
                                se.id_empleado == empleado.id_empleado &&
                                se.REACTIVO.id_reactivo == reactivo
                                );
                }
                else
                {
                    salidaExistencia =
                        db.SALIDA_EXISTENCIA
                            .Where(se => se.id_empleado == empleado.id_empleado);

                }
                    
            }
            else
            {
                if (fecha_inicio != "" && reactivo == 0)
                {
                    DateTime inicio = DateTime.Parse(fecha_inicio);
                    DateTime fin = DateTime.Parse(fecha_fin);

                    salidaExistencia =
                        db.SALIDA_EXISTENCIA
                            .Where(se =>
                                se.fecha_salida >= inicio && se.fecha_salida <= fin
                                );
                }
                else if (fecha_inicio != "" && reactivo != 0)
                {
                    DateTime inicio = DateTime.Parse(fecha_inicio);
                    DateTime fin = DateTime.Parse(fecha_fin);

                    salidaExistencia =
                        db.SALIDA_EXISTENCIA
                            .Where(se =>
                                se.fecha_salida >= inicio && se.fecha_salida <= fin &&
                                se.REACTIVO.id_reactivo == reactivo
                                );
                }
                else if (fecha_inicio == "" && reactivo != 0)
                {
                    salidaExistencia =
                        db.SALIDA_EXISTENCIA
                            .Where(se =>
                                se.REACTIVO.id_reactivo == reactivo
                                );
                }
                else
                {
                    salidaExistencia = db.SALIDA_EXISTENCIA;

                }
            }
            
            salidaExistencia = salidaExistencia.OrderByDescending(se => se.fecha_salida).ToList();

            List<REACTIVO> reactivos = db.REACTIVOes.OrderBy(r => r.nombre).ToList();

            reactivos.Insert(0, new REACTIVO() { id_reactivo = 0, nombre = "Todos" });

            IEnumerable<SelectListItem> reactivoList =
            from value in reactivos
            select new SelectListItem
            {
                Text = value.nombre,
                Value = value.id_reactivo.ToString()
            };

            ViewBag.ReactivoSelection = reactivoList;
            ViewBag.Empleado = this.empleado();

            return View(salidaExistencia);
        }

        [Authorize(Roles = "Analista")]
        public ActionResult inventarioLocal()
        {
            EMPLEADO empleado = this.empleado();

            var inventario = 
                    db.SALIDA_EXISTENCIA
                        .Where(se => se.id_empleado == empleado.id_empleado && se.cantidad_existencia > 0 )
                        .OrderByDescending(se => se.EXISTENCIA_PRODUCTO.id_producto)
                        .GroupBy(s => new { s.EXISTENCIA_PRODUCTO.PRODUCTO.REACTIVO })
                        .Select(a => new { reactivo = a.Key.REACTIVO.nombre, sum = a.Sum(s => s.cantidad_salida)}).ToList();

            Dictionary<string, decimal> list = new Dictionary<string, decimal>();

            foreach (var item in inventario)
            {
                list.Add(item.reactivo, item.sum);
            }

            ViewBag.inventario = list;

            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ReporteUso()
        {

            DateTime inicio = DateTime.Now;
            inicio = inicio.AddMonths(-1);

            DateTime fin = DateTime.Now;


            List<ANALISIS_REACTIVO> analisisReactivos = db.ANALISIS_REACTIVO.Where(ar => ar.fecha >= inicio && ar.fecha <= fin).OrderByDescending(ar => ar.ANALISIS_EMPLEADO.DETALLE_SOLICITUD.SOLICITUD.no_referencia).ToList();

            List<REACTIVO> reactivos = db.REACTIVOes.OrderBy(r => r.nombre).ToList();

            reactivos.Insert(0, new REACTIVO() { id_reactivo = 0, nombre = "Reactivos" });

            IEnumerable<SelectListItem> reactivoList =
            from reactivo in reactivos
            select new SelectListItem
            {
                Text = reactivo.nombre,
                Value = reactivo.nombre
            };

            ViewBag.ReactivoSelection = reactivoList;

            List<SOLICITUD> solicitudes = db.SOLICITUDs.OrderByDescending(s => s.no_referencia).ToList();

            solicitudes.Insert(0, new SOLICITUD() { id_solicitud = 0, no_referencia = "Solicitudes" });

            IEnumerable<SelectListItem> solicitudList =
            from solicitud in solicitudes
            select new SelectListItem
            {
                Text = solicitud.no_referencia,
                Value = solicitud.no_referencia
            };

            ViewBag.SolicitudSelection = solicitudList;

            ViewBag.fechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.fechaFin = fin.ToString("yyyy-MM-dd");

            return View(analisisReactivos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult ReporteUso(string fecha_inicio, string fecha_fin, string solicitudP, string reactivoP)
        {
            DateTime inicio;
            DateTime fin;

            DateTime.TryParse(fecha_inicio, out inicio);
            DateTime.TryParse(fecha_fin, out fin);

            List<ANALISIS_REACTIVO> analisisReactivos;

            if (solicitudP != "Solicitudes" && reactivoP == "Reactivos")
            {
                analisisReactivos = db.ANALISIS_REACTIVO.Where(ar => ar.fecha >= inicio && ar.fecha <= fin && ar.ANALISIS_EMPLEADO.DETALLE_SOLICITUD.SOLICITUD.no_referencia == solicitudP)
                    .OrderByDescending(ar => ar.ANALISIS_EMPLEADO.DETALLE_SOLICITUD.SOLICITUD.no_referencia).ToList();
            }
            else if (solicitudP == "Solicitudes" && reactivoP != "Reactivos")
            {
                analisisReactivos = db.ANALISIS_REACTIVO.Where(ar => ar.fecha >= inicio && ar.fecha <= fin && ar.REACTIVO.nombre == reactivoP)
                    .OrderByDescending(ar => ar.ANALISIS_EMPLEADO.DETALLE_SOLICITUD.SOLICITUD.no_referencia).ToList();
            }
            else if (solicitudP != "Solicitudes" && reactivoP != "Reactivos")
            {
                analisisReactivos = db.ANALISIS_REACTIVO.Where(ar => ar.fecha >= inicio && ar.fecha <= fin && ar.REACTIVO.nombre == reactivoP && ar.ANALISIS_EMPLEADO.DETALLE_SOLICITUD.SOLICITUD.no_referencia == solicitudP)
                    .OrderByDescending(ar => ar.ANALISIS_EMPLEADO.DETALLE_SOLICITUD.SOLICITUD.no_referencia).ToList();
            }
            else 
            {
                analisisReactivos = db.ANALISIS_REACTIVO.Where(ar => ar.fecha >= inicio && ar.fecha <= fin)
                .OrderByDescending(ar => ar.ANALISIS_EMPLEADO.DETALLE_SOLICITUD.SOLICITUD.no_referencia).ToList();
            }

            List<REACTIVO> reactivos = db.REACTIVOes.OrderBy(r => r.nombre).ToList();

            reactivos.Insert(0, new REACTIVO() { id_reactivo = 0, nombre = "Reactivos" });

            IEnumerable<SelectListItem> reactivoList =
            from reactivo in reactivos
            select new SelectListItem
            {
                Text = reactivo.nombre,
                Value = reactivo.nombre
            };

            ViewBag.ReactivoSelection = reactivoList;

            List<SOLICITUD> solicitudes = db.SOLICITUDs.OrderByDescending(s => s.no_referencia).ToList();

            solicitudes.Insert(0, new SOLICITUD() { id_solicitud = 0, no_referencia = "Solicitudes" });

            IEnumerable<SelectListItem> solicitudList =
            from solicitud in solicitudes
            select new SelectListItem
            {
                Text = solicitud.no_referencia,
                Value = solicitud.no_referencia
            };

            ViewBag.SolicitudSelection = solicitudList;

            ViewBag.fechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.fechaFin = fin.ToString("yyyy-MM-dd");

            return View(analisisReactivos);
        }
       
        private void setViewBagParameters()
        {
            var almacenes = db.ALMACENs.ToList();
            var unidades = db.UNIDADs.ToList();
            var reactivos = db.REACTIVOes.OrderByDescending(r => r.nombre).ToList();

            IEnumerable<SelectListItem> almacenesList =

            from almacen in almacenes

            select new SelectListItem
            {

                Text = "Estante: " + almacen.estante + " Nivel: " + almacen.nivel,
                Value = almacen.id_almacen.ToString()

            };

            IEnumerable<SelectListItem> unidadesList =

            from unidad in unidades

            select new SelectListItem
            {

                Text = unidad.nombre,
                Value = unidad.id_unidad.ToString()

            };

            IEnumerable<SelectListItem> reactivosList =

            from reactivo in reactivos

            select new SelectListItem
            {
               Text = reactivo.nombre,
               Value = reactivo.id_reactivo.ToString()
            };

            ViewBag.ReactivoSelection = reactivosList;
            ViewBag.AlmacenSelection = almacenesList;
            ViewBag.UnidadSelection = unidadesList;
        }

        private ProductoController setCatalogos(PRODUCTO producto, HttpRequestBase request)
        {
            var marca = request.Form["marca"];
            var lote = request.Form["lote"];
            var catalogo = request.Form["catalogo"];

            var marcaObj = db.MARCAs.Where(m => m.nombre == marca).FirstOrDefault();

            if (marcaObj == null)
            {
                marcaObj = new MARCA();

                marcaObj.nombre = marca;

                db.MARCAs.Add(marcaObj);
                db.SaveChanges();
            }

            var loteObj = db.LOTEs.Where(l => l.nombre == lote).FirstOrDefault();

            if (loteObj == null)
            {
                loteObj = new LOTE();

                loteObj.nombre = lote;

                db.LOTEs.Add(loteObj);

                db.SaveChanges();
            }

            var catalogoObj = db.CATALOGOes.Where(c => c.nombre_catalogo == catalogo).FirstOrDefault();

            if (catalogoObj == null)
            {
                catalogoObj = new CATALOGO();

                catalogoObj.nombre_catalogo = catalogo;

                db.CATALOGOes.Add(catalogoObj);

                db.SaveChanges();
            }

            producto.MARCA = db.MARCAs.First(m => m.nombre == marcaObj.nombre);
            producto.LOTE = loteObj;
            producto.CATALOGO = catalogoObj;

            return this;
        }
    }
}