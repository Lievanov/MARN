using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using LaboratorioMarn.Infrastructure;
using LaboratorioMarn.Models;

namespace LaboratorioMarn.Controllers
{
    public class InventarioAnalistaController : Controller
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

        [HttpPost]
        public ActionResult Lista(int idAnalisisEmpleado)
        {
            EMPLEADO empleado = this.empleado();

            ANALISIS_EMPLEADO analisisEmpleado = db.ANALISIS_EMPLEADO.Find(idAnalisisEmpleado);

            PROCESO_ANALISTA procesoAnalista = db.PROCESO_ANALISTA.Where(pa => pa.solicitud_id == analisisEmpleado.DETALLE_SOLICITUD.SOLICITUD.id_solicitud && pa.empleado_id == empleado.id_empleado).First();

            List<ANALISIS_REACTIVO> analsisReactivoList = analisisEmpleado.ANALISIS_REACTIVO.ToList();

            ViewBag.AnalisisEmpleado = analisisEmpleado;
            ViewBag.ProcesoAnalista = procesoAnalista;

            return PartialView(analsisReactivoList);
        }

        [HttpGet]
        public ActionResult Salida()
        {
            EMPLEADO empleado = this.empleado();

            var inventario =
                    db.SALIDA_EXISTENCIA
                        .Where(se => se.id_empleado == empleado.id_empleado && se.cantidad_existencia > 0)
                        .OrderByDescending(se => se.EXISTENCIA_PRODUCTO.id_producto)
                        .GroupBy(s => new { s.EXISTENCIA_PRODUCTO.PRODUCTO.REACTIVO })
                        .Select(a => new { reactivo = a.Key.REACTIVO, sum = a.Sum(s => s.cantidad_salida) }).ToList();

            IEnumerable<SelectListItem> inventarioList =
            from element in inventario
            select new SelectListItem
            {
                Text = element.reactivo.nombre,
                Value = element.reactivo.id_reactivo.ToString()
            };            

            ViewBag.InventarioList = inventarioList;

            return PartialView();
        }

        [HttpPost]
        public ActionResult ExistenciaReactivo(int idReactivo)
        {
            object returnObj;

            EMPLEADO empleado = this.empleado();

            var existenciaReactivo = db.SALIDA_EXISTENCIA
                        .Where(se => se.id_empleado == empleado.id_empleado && se.cantidad_existencia > 0 && se.REACTIVO.id_reactivo == idReactivo)
                        .OrderByDescending(se => se.EXISTENCIA_PRODUCTO.id_producto)
                        .GroupBy(s => new { s.EXISTENCIA_PRODUCTO.PRODUCTO.REACTIVO })
                        .Select(a => new { reactivo = a.Key.REACTIVO, sum = a.Sum(s => s.cantidad_salida) });
                        
           var existencia = existenciaReactivo.First();

           returnObj = new { cantidad = existencia.sum };

            return Json(returnObj);
        }

        [HttpPost]
        public ActionResult Salida(int idAnalisisEmpleado, int idReactivo, decimal cantidadSalida)
        {
            EMPLEADO empleado = this.empleado();

            var existenciaReactivo = db.SALIDA_EXISTENCIA
                        .Where(se => se.id_empleado == empleado.id_empleado && se.cantidad_existencia > 0 && se.REACTIVO.id_reactivo == idReactivo)
                        .OrderByDescending(se => se.EXISTENCIA_PRODUCTO.id_producto)
                        .GroupBy(s => new { s.EXISTENCIA_PRODUCTO.PRODUCTO.REACTIVO })
                        .Select(a => new { reactivo = a.Key.REACTIVO, sum = a.Sum(s => s.cantidad_salida) }).First();
            if (existenciaReactivo.sum < cantidadSalida)
            {
                return Json(new { valid = false, msg = "La cantidad solicitada es mayor que la existente" });
            }

            List<SALIDA_EXISTENCIA> salidasExistencia = db.SALIDA_EXISTENCIA.Where(se => se.id_empleado == empleado.id_empleado && se.cantidad_existencia > 0 && se.REACTIVO.id_reactivo == idReactivo).ToList();

            byte unidad = 0;
            decimal cantidadSalidaTotal = cantidadSalida;

            foreach (SALIDA_EXISTENCIA salidaExistencia in salidasExistencia)
            {
                unidad = salidaExistencia.UNIDAD.id_unidad;

                decimal salida;

                decimal cantidadExistencia = salidaExistencia.cantidad_existencia;

                if (cantidadSalida >= cantidadExistencia)
                {
                    salida = cantidadExistencia;
                    cantidadSalida -= cantidadExistencia;
                    salidaExistencia.cantidad_existencia = 0;
                }
                else
                {
                    salida = cantidadSalida;
                    cantidadSalida = 0;

                    salidaExistencia.cantidad_existencia -= salida;
                }

                if (cantidadSalida == 0)
                {
                    break;
                }
            }

            ANALISIS_REACTIVO analisisReactivo = new ANALISIS_REACTIVO()
            {
                cantidad = cantidadSalidaTotal,
                id_analisis_empleado = idAnalisisEmpleado,
                id_reactivo = idReactivo,
                id_unidad = unidad,
                fecha = DateTime.Now
            };

            db.ANALISIS_REACTIVO.Add(analisisReactivo);

            db.SaveChanges();

            return Json(new { valid = true });
        }
    }
}