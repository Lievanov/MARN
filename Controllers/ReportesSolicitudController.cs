using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LaboratorioMarn.Models;
using System.Net;
using System.Web.Script.Serialization;
using System.Web.Helpers;



namespace LaboratorioMarn.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReportesSolicitudController : Controller
    {
        MARNEntities db = new MARNEntities();

        [HttpGet]
        public ActionResult TrabajoCosto()
        {
            var today = DateTime.Now;
            var inicio = new DateTime(today.Year, today.Month, 1);
            var fin =  new DateTime(inicio.Year, inicio.Month, DateTime.DaysInMonth(inicio.Year, inicio.Month));

            List<SOLICITANTE> solicitantesReport = db.SOLICITANTEs.ToList();
            List<SITIO_MUESTREO> sitiosMuestreo = db.SITIO_MUESTREO.ToList();

            ViewBag.inicio_datetime = inicio;
            ViewBag.fin_datetime = fin;

            ViewBag.inicio = inicio.Month + "-" + inicio.Year;
            ViewBag.fin = fin.Month + "-" + fin.Year;

            ViewBag.SolicitantesReport = solicitantesReport;
            ViewBag.SitiosMuestreo = sitiosMuestreo;

            List<AREA> areas = db.AREAs.ToList();

            IEnumerable<SelectListItem> areaList =
            from area in areas
            select new SelectListItem
            {
                Text = area.nombre_area,
                Value = area.id_area.ToString()
            };

            ViewBag.area = "";
            ViewBag.AreasSelection = areaList;

            return View();
        }

        [HttpPost]
        public ActionResult TrabajoCosto(String inicio, String fin, string area)
        {
            Int32 anioInicio = Int32.Parse(inicio.Substring(2, 4));
            Int32 mesInicio = Int32.Parse(inicio.Substring(0, 1));

            Int32 anioFin = Int32.Parse(fin.Substring(2, 4));
            Int32 mesFin = Int32.Parse(fin.Substring(0, 1));

            if (inicio.Length == 7)
            {
                anioInicio = Int32.Parse(inicio.Substring(3, 4));
                mesInicio =Int32.Parse(inicio.Substring(0, 2));
            }

            if (fin.Length == 7)
            {
                anioFin = Int32.Parse(fin.Substring(3, 4));
                mesFin =Int32.Parse(fin.Substring(0, 2));
            }

            var inicioFecha = new DateTime(anioInicio, mesInicio, 1);
            var finFecha = new DateTime(anioFin, mesFin, 1);

            List<SOLICITANTE> solicitantesReport = db.SOLICITANTEs.ToList();
            List<SITIO_MUESTREO> sitiosMuestreo = db.SITIO_MUESTREO.ToList();

            ViewBag.inicio_datetime = inicioFecha;
            ViewBag.fin_datetime = finFecha;

            ViewBag.inicio = inicioFecha.Month + "-" + inicioFecha.Year;
            ViewBag.fin = finFecha.Month + "-" + finFecha.Year;

            ViewBag.SolicitantesReport = solicitantesReport;
            ViewBag.SitiosMuestreo = sitiosMuestreo;

            List<AREA> areas = db.AREAs.ToList();

            IEnumerable<SelectListItem> areaList =
            from areaO in areas
            select new SelectListItem
            {
                Text = areaO.nombre_area,
                Value = areaO.id_area.ToString()
            };

            foreach (SelectListItem areaItem in areaList)
            {
                if (areaItem.Value == area)
                {
                    areaItem.Selected = true;
                }
            }

            ViewBag.area = area;

            ViewBag.AreasSelection = areaList;

            return View();
        }

        [HttpGet]
        public ActionResult Reporte()
        {
            // valores quemados
            var fecha_inicio = DateTime.Now.AddMonths(-3).Date;
            var fecha_fin = DateTime.Now.Date;
           
            //DateTime inicio = Convert.ToDateTime("2016/02/01").Date;
           
            // /valores quemados
            this.fillReport(fecha_inicio, fecha_fin);

            ViewBag.fecha_inicio = fecha_inicio.ToString("yyyy/MM/dd");
            ViewBag.fecha_fin = fecha_fin.ToString("yyyy/MM/dd");
            return View();

        }

        [HttpPost]
        public ActionResult Reporte(string inicio, string fin, reporteSolicitudxsolicitantes model )
        {
            DateTime fecha_inicio = Convert.ToDateTime(inicio).Date;
            DateTime fecha_fin = Convert.ToDateTime(fin).Date;

            if (fecha_inicio > fecha_fin)
            {
                fecha_inicio = DateTime.Now.AddYears(-1).Date;
                fecha_fin = DateTime.Now.Date;
                ViewBag.Fecha = 0;
                
                this.fillReport(fecha_inicio, fecha_fin);
            }
            else { this.fillReport(fecha_inicio, fecha_fin); }




            ViewBag.fecha_inicio = fecha_inicio.ToString("yyyy/MM/dd");
            ViewBag.fecha_fin = fecha_fin.ToString("yyyy/MM/dd");
           

            return View();

        }


        private void fillReport(DateTime inicio, DateTime fin)
        {
            List<List<string>> solicitudxsolicitante = new List<List<string>>();
            List<List<string>> solicitudxsolicitantetotal = new List<List<string>>();
            List<List<string>> analisisxsolicitante = new List<List<string>>();
            List<List<string>> analisisxsolicitantetotal = new List<List<string>>();
            List<List<string>> analisisxarea = new List<List<string>>();
            List<List<string>> analisisxareatotal = new List<List<string>>();
            List<string> anios = new List<string>();

            List<string> filaEncabezado = new List<string>();
            filaEncabezado.Add("Mes");

            //for (var i = inicio.Year; i <= fin.Year; i++)
            //{
            //    anios.Add(i.ToString());
            //    filaEncabezado.Add(i.ToString());

            //}
            while (inicio < fin)
            {
                anios.Add(inicio.Month.ToString());
                filaEncabezado.Add(inicio.ToString("MMMM"));
                inicio = inicio.AddMonths(1);
            }

            solicitudxsolicitante.Add(filaEncabezado); // Fila de encabezados
            analisisxsolicitante.Add(filaEncabezado);
            analisisxarea.Add(filaEncabezado);
            List<SOLICITANTE> solicitantes = db.SOLICITANTEs.ToList();
            List<SOLICITUD> solicitud = db.SOLICITUDs.ToList();
            List<AREA> areas = db.AREAs.ToList();
            List<EMPLEADO> empleados = db.EMPLEADOes.Where(s => s.id_tipo_empleado!=1 && s.id_tipo_empleado != 3).ToList();
            
            var querysolicitudxsolicitante = (from s in db.SOLICITUDs
                                              join sol in db.SOLICITANTEs on s.id_solicitante equals sol.id_solicitante
                                              
                                              group new { s, sol } by new { s.fecha_creacion.Value.Month, sol.nombre_solicitante, sol.id_solicitante,s.fecha_creacion.Value.Year } into ns

                                              select new reporteSolicitudxsolicitantes
                                              {
                                                  id_solicitante = ns.Key.id_solicitante.ToString(),
                                                  nombreSolicitante = ns.Key.nombre_solicitante,
                                                  mes = ns.Key.Month,
                                                  año=ns.Key.Year,
                                                  nosolicitudes = ns.Count()

                                              }
                        );
            var querysolicitudxsolicitantetotal = (from s in db.SOLICITUDs
                                                   join sol in db.SOLICITANTEs on s.id_solicitante equals sol.id_solicitante
                                                   
                                                   
                                                   group new { s, sol } by new { s.fecha_creacion.Value.Month,s.fecha_creacion.Value.Year } into ns

                                                   select new reporteSolicitudxsolicitantes
                                                   {
                                                       mes = ns.Key.Month,
                                                       año = ns.Key.Year,
                                                       nosolicitudes = ns.Count()

                                                   }
                        );


            var queryanalisisxsolicitante = (from s in db.SOLICITUDs
                                             join sol in db.SOLICITANTEs on s.id_solicitante equals sol.id_solicitante
                                             join ds in db.DETALLE_SOLICITUD on s.id_solicitud equals ds.id_solicitud
                                             //where (s.fecha_creacion.Value >= inicio && s.fecha_creacion.Value <= fin)
                                             group new { s, sol } by new { s.fecha_creacion.Value.Month, sol.nombre_solicitante, s.id_solicitante, s.fecha_creacion.Value.Year } into ns

                                             select new reporteSolicitudxsolicitantes
                                             {
                                                 id_solicitante = ns.Key.id_solicitante.ToString(),
                                                 nombreSolicitante = ns.Key.nombre_solicitante,
                                                 mes = ns.Key.Month,
                                                 año = ns.Key.Year,
                                                 nosolicitudes = ns.Count()

                                             }
                         );

            var queryanalisisxsolicitantetotal = (from s in db.SOLICITUDs
                                                  join sol in db.SOLICITANTEs on s.id_solicitante equals sol.id_solicitante
                                                  join ds in db.DETALLE_SOLICITUD on s.id_solicitud equals ds.id_solicitud
                                                  //where (s.fecha_creacion.Value >= inicio && s.fecha_creacion.Value <= fin)
                                                  group new { s } by new {s.fecha_creacion.Value.Month, s.fecha_creacion.Value.Year} into ns

                                                  select new reporteSolicitudxsolicitantes
                                                  {

                                                      mes = ns.Key.Month,
                                                      año = ns.Key.Year,
                                                      nosolicitudes = ns.Count()

                                                  }
                        );


            var queryanalisisxarea = (from ar in db.AREAs
                                      join an in db.ANALISIS on ar.id_area equals an.id_area
                                      join ds in db.DETALLE_SOLICITUD on an.id_analisis equals ds.id_analisis
                                      join sol in db.SOLICITUDs on ds.id_solicitud equals sol.id_solicitud
                                      //where (sol.fecha_creacion.Value >= inicio && sol.fecha_creacion.Value <= fin)
                                      group new { ar, sol } by new {sol.fecha_creacion.Value.Month, sol.fecha_creacion.Value.Year, ar.nombre_area, ar.id_area } into na

                                      select new reporteAnalisixAreas
                                      {
                                          id_area = na.Key.id_area,
                                          nombreArea = na.Key.nombre_area,
                                          mes = na.Key.Month,
                                          año = na.Key.Year,
                                          noareas = na.Count()
                                      }
                     );

            var queryanalisisxareatotal = (from ar in db.AREAs
                                           join an in db.ANALISIS on ar.id_area equals an.id_area
                                           join ds in db.DETALLE_SOLICITUD on an.id_analisis equals ds.id_analisis
                                           join sol in db.SOLICITUDs on ds.id_solicitud equals sol.id_solicitud
                                           //where (sol.fecha_creacion.Value>= inicio && sol.fecha_creacion.Value <= fin)
                                           group new { ar, sol } by new {sol.fecha_creacion.Value.Month, sol.fecha_creacion.Value.Year } into na

                                           select new reporteAnalisixAreas
                                           {

                                               mes = na.Key.Month,
                                               año = na.Key.Year,
                                               noareas = na.Count()
                                           }
                    );

            
            foreach (SOLICITANTE sol in solicitantes)
            {

                List<string> filaTemporal = new List<string>();
                filaTemporal.Add(sol.nombre_solicitante);
                foreach (string anio in anios)
                {
                    for (var b = inicio.Year; b <= fin.Year; b++)
                    {

                        var valor = "0";
                        // buscar en query el area y el anio 
                        // es row

                        if (querysolicitudxsolicitante.FirstOrDefault(i => i.id_solicitante == sol.id_solicitante.ToString() && i.mes.ToString() == anio && i.año == b) != null)
                        // aca se implementa
                        // buscar el anio del area en el query
                        {

                            valor = querysolicitudxsolicitante.FirstOrDefault(a => a.id_solicitante == sol.id_solicitante.ToString() && a.mes.ToString() == anio && a.año == b).nosolicitudes.ToString(); // el valor que encontraste

                        }

                        filaTemporal.Add(valor);


                    }
                }

                solicitudxsolicitante.Add(filaTemporal);

            }
            //Grafica Solicitante x Solicitud
            List<Dictionary<string, object>> dataSolicitanteSolicitud = new List<Dictionary<string, object>>();
            List<string> nombresSolicitantes = db.SOLICITANTEs.OrderBy(s => s.nombre_solicitante).Select(s => s.nombre_solicitante).ToList();

            Dictionary<string, object> objetoDataSolicitanteSolicitud;

            for (var b = inicio.Year; b <= fin.Year; b++)
            {
                foreach (string anio in anios)
                {
                    objetoDataSolicitanteSolicitud = new Dictionary<string, object>();                    
                    objetoDataSolicitanteSolicitud.Add("year",anio);

                    foreach (SOLICITANTE sol in solicitantes)
                    {

                        var valor = "0";

                        if (querysolicitudxsolicitante.FirstOrDefault(i => i.id_solicitante == sol.id_solicitante.ToString() && i.mes.ToString() == anio && i.año==b) != null)
                        // aca se implementa
                        // buscar el anio del area en el query
                        {

                            valor = querysolicitudxsolicitante.FirstOrDefault(a => a.id_solicitante == sol.id_solicitante.ToString() && a.mes.ToString() == anio && a.año == b).nosolicitudes.ToString(); // el valor que encontraste
                        }

                        objetoDataSolicitanteSolicitud.Add(sol.nombre_solicitante, valor);
                    }

                    dataSolicitanteSolicitud.Add(objetoDataSolicitanteSolicitud);
                }
            }
            var jsonDataSolicitanteSolicitud = Json(dataSolicitanteSolicitud);

            ViewBag.jsonDataSolicitanteSolicitud = jsonDataSolicitanteSolicitud;
            ViewBag.nombresSolicitantes = nombresSolicitantes;
            //Grafica Solicitante x Solicitud


            List<string> filasolicitantexsolicitudes = new List<string>();
            filasolicitantexsolicitudes.Add("TOTAL");
            foreach (string anio in anios)
            {
                for (var b = inicio.Year; b <= fin.Year; b++)
                {

                    var valor = "0";
                    // buscar en query el area y el anio 
                    // es row

                    if (querysolicitudxsolicitantetotal.FirstOrDefault(i => i.mes.ToString() == anio && i.año==b) != null)
                    // aca se implementa
                    // buscar el anio del area en el query
                    {

                        valor = querysolicitudxsolicitantetotal.FirstOrDefault(a => a.mes.ToString() == anio && a.año == b).nosolicitudes.ToString(); // el valor que encontraste

                    }
                    filasolicitantexsolicitudes.Add(valor);
                }
            }
            solicitudxsolicitantetotal.Add(filasolicitantexsolicitudes);



            var valor2 = "0";
            foreach (SOLICITANTE sol in solicitantes)
            {
                List<string> filaTemporal2 = new List<string>();
                filaTemporal2.Add(sol.nombre_solicitante);
                foreach (string anio in anios)
                {
                    for (var b = inicio.Year; b <= fin.Year; b++)
                    {
                        valor2 = "0";
                        // buscar en query el area y el anio 
                        // es row

                        if (queryanalisisxsolicitante.FirstOrDefault(i => i.id_solicitante == sol.id_solicitante.ToString() && i.mes.ToString() == anio && i.año==b) != null)
                        {
                            // aca se implementa
                            // buscar el anio del area en el query


                            valor2 = queryanalisisxsolicitante.First(s => s.id_solicitante == sol.id_solicitante.ToString() && s.mes.ToString() == anio && s.año == b).nosolicitudes.ToString(); // el valor que encontraste

                        }

                        filaTemporal2.Add(valor2);
                    }
                }
                analisisxsolicitante.Add(filaTemporal2);
            }

            //Grafica Solicitante x analisis
            List<Dictionary<string, object>> dataAnalisisSolicitante = new List<Dictionary<string, object>>();
            List<string> nombresSolicitantesxAnalisis = db.SOLICITANTEs.OrderBy(s => s.nombre_solicitante).Select(s => s.nombre_solicitante).ToList();

            Dictionary<string, object> objetoDataAnalisisSolicitante;
            for (var b = inicio.Year; b <= fin.Year; b++)
            {
                foreach (string anio in anios)
                {
                    objetoDataAnalisisSolicitante = new Dictionary<string, object>();
                    objetoDataAnalisisSolicitante.Add("year", anio);

                    foreach (SOLICITANTE sol in solicitantes)
                    {
                        var valor = "0";

                        if (queryanalisisxsolicitante.FirstOrDefault(i => i.id_solicitante == sol.id_solicitante.ToString() && i.mes.ToString() == anio && i.año==b) != null)
                        {

                            valor = queryanalisisxsolicitante.FirstOrDefault(a => a.id_solicitante == sol.id_solicitante.ToString() && a.mes.ToString() == anio && a.año ==b).nosolicitudes.ToString(); // el valor que encontraste
                        }

                        objetoDataAnalisisSolicitante.Add(sol.nombre_solicitante, valor);
                    }

                    dataAnalisisSolicitante.Add(objetoDataAnalisisSolicitante);
                }
            }
            var jsonDataAnalisisSolicitante = Json(dataAnalisisSolicitante);

            ViewBag.jsonDataAnalisisSolicitante = jsonDataAnalisisSolicitante;
            ViewBag.nombresSolicitantes = nombresSolicitantes;
            //Grafica Solicitante x analisis

            List<string> filasolicitantexanalisis = new List<string>();
            filasolicitantexanalisis.Add("TOTAL");
            foreach (string anio in anios)
            {
                for (var b = inicio.Year; b <= fin.Year; b++)
                {

                    var valor = "0";
                    // buscar en query el area y el anio 
                    // es row

                    if (queryanalisisxsolicitantetotal.FirstOrDefault(i => i.mes.ToString() == anio && i.año==b) != null)
                    // aca se implementa
                    // buscar el anio del area en el query
                    {

                        valor = queryanalisisxsolicitantetotal.FirstOrDefault(a => a.mes.ToString() == anio && a.año==b).nosolicitudes.ToString(); // el valor que encontraste

                    }
                    filasolicitantexanalisis.Add(valor);
                }
            }
            analisisxsolicitantetotal.Add(filasolicitantexanalisis);








            foreach (AREA ar in areas)
            {

                List<string> filaTemporal = new List<string>();
                filaTemporal.Add(ar.nombre_area);
                foreach (string anio in anios)
                {
                    for (var b = inicio.Year; b <= fin.Year; b++)
                    {
                        var valor3 = "0";
                        // buscar en query el area y el anio 
                        // es row

                        if (queryanalisisxarea.FirstOrDefault(i => i.id_area == ar.id_area && i.mes.ToString() == anio && i.año == b) != null)
                        // aca se implementa
                        // buscar el anio del area en el query
                        {

                            valor3 = queryanalisisxarea.FirstOrDefault(a => a.id_area == ar.id_area && a.mes.ToString() == anio && a.año ==b).noareas.ToString(); // el valor que encontraste
                        }

                        filaTemporal.Add(valor3);
                    }
                }
                analisisxarea.Add(filaTemporal);
            }



            //Grafica analisis x Areas
            List<Dictionary<string, object>> dataAnalisisAreas = new List<Dictionary<string, object>>();
            List<string> nombresAreas = db.AREAs.OrderBy(s => s.nombre_area).Select(s => s.nombre_area).ToList();

            Dictionary<string, object> objetoDataAnalisisAreas;
            for (var b = inicio.Year; b <= fin.Year; b++)
            {
                foreach (string anio in anios)
                {
                    objetoDataAnalisisAreas = new Dictionary<string, object>();
                    objetoDataAnalisisAreas.Add("year", anio);

                    foreach (AREA ar in areas)
                    {
                        var valor = "0";

                        if (queryanalisisxarea.FirstOrDefault(i => i.id_area == ar.id_area && i.mes.ToString() == anio && i.año ==b) != null)
                        {

                            valor = queryanalisisxarea.FirstOrDefault(a => a.id_area == ar.id_area && a.mes.ToString() == anio && a.año==b).noareas.ToString(); // el valor que encontraste
                        }

                        objetoDataAnalisisAreas.Add(ar.nombre_area, valor);
                    }

                    dataAnalisisAreas.Add(objetoDataAnalisisAreas);
                }
            }
            var jsonDataAnalisisAreas = Json(dataAnalisisAreas);

            ViewBag.jsonDataAnalisisAreas = jsonDataAnalisisAreas;
            ViewBag.nombresAreas = nombresAreas;
            //Grafica analisis x Areas



            List<string> filaanalisisxareatotal = new List<string>();
            filaanalisisxareatotal.Add("TOTAL");
            foreach (string anio in anios)
            {
                for (var b = inicio.Year; b <= fin.Year; b++)
                {

                    var valor = "0";
                    // buscar en query el area y el anio 
                    // es row

                    if (queryanalisisxareatotal.FirstOrDefault(i => i.mes.ToString() == anio && i.año==b) != null)
                    // aca se implementa
                    // buscar el anio del area en el query
                    {

                        valor = queryanalisisxareatotal.FirstOrDefault(a => a.mes.ToString() == anio && a.año==b).noareas.ToString(); // el valor que encontraste

                    }
                    filaanalisisxareatotal.Add(valor);
                }
            }
            analisisxareatotal.Add(filaanalisisxareatotal);

            //dashboard
            var solfinalizadas = (from sol in db.SOLICITUDs
                                  where (sol.id_estado == 4)
                                  && (sol.fecha_creacion.Value.Month == DateTime.Now.Month)


                                  select sol.id_solicitud
                    );

            var analisisFinalizados = (from s in db.SOLICITUDs
                                       join ds in db.DETALLE_SOLICITUD on s.id_solicitud equals ds.id_solicitud
                                       where s.id_estado == 4
                                         && (s.fecha_creacion.Value.Month == DateTime.Now.Month)
                                       select s.id_solicitud

                       );

            var SolicitudesEnProceso = (from sol in db.SOLICITUDs
                                        where (sol.id_estado_proceso < 4)
                                        select sol.id_solicitud
                   );

            var analisisPendientes = (from s in db.SOLICITUDs
                                      join ds in db.DETALLE_SOLICITUD on s.id_solicitud equals ds.id_solicitud
                                      where s.id_estado < 4
                                      
                                      select s.id_solicitud

                       );



            var queryAnalisisxEmpleado = (from ae in db.ANALISIS_EMPLEADO
                                          join an in db.EMPLEADOes on ae.id_empleado equals an.id_empleado
                                          join ds in db.DETALLE_SOLICITUD on ae.id_detalle_solicitud equals ds.id_detalle_solicitud
                                          join s in db.SOLICITUDs on ds.id_solicitud equals s.id_solicitud
                                          where 
                                          (s.fecha_creacion.Value.Year >= inicio.Year && s.fecha_creacion.Value.Year <= fin.Year) &&
                                          (an.id_tipo_empleado != 1 && an.id_tipo_empleado != 3)
                                          group new { ae, an, ds } by new { an.nombre_empleado, an.id_empleado} into na
                                          select new reporteSolicitudxsolicitantes

                                          {
                                              id_empleado = na.Key.id_empleado,
                                            
                                              nombreSolicitante = na.Key.nombre_empleado,
                                              nosolicitudes = na.Count(),
                                          }
                                          );


            //Grafica analisis x Areas
            List<Dictionary<string, object>> dataAnalisisEmpleado = new List<Dictionary<string, object>>();
            Dictionary<string, object> objetoDataAnalisisEmpleado;


           
                   
                    foreach (EMPLEADO ar in empleados)
                    {
                        objetoDataAnalisisEmpleado = new Dictionary<string, object>();
                        var valor = "0";

                        if (queryAnalisisxEmpleado.FirstOrDefault(i => i.id_empleado == ar.id_empleado ) != null)
                        {

                            valor = queryAnalisisxEmpleado.FirstOrDefault(i => i.id_empleado == ar.id_empleado ).nosolicitudes.ToString(); // el valor que encontraste
                        }

                        objetoDataAnalisisEmpleado.Add("label",ar.nombre_empleado);
                        objetoDataAnalisisEmpleado.Add("value", valor);
                        dataAnalisisEmpleado.Add(objetoDataAnalisisEmpleado);
                    }

                   
                   
                
                var jsonDataAnalisisEmpleado = Json(dataAnalisisEmpleado);
                ViewBag.jsonDataAnalisisEmpleado = jsonDataAnalisisEmpleado;
                
            //Grafica analisis x Areas



                var queryAnalisis = (from a in db.ANALISIS
                                              join ds in db.DETALLE_SOLICITUD on a.id_analisis equals ds.id_analisis
                                              join s in db.SOLICITUDs on ds.id_solicitud equals s.id_solicitud
                                             where 
                                              (s.fecha_creacion.Value.Year >= inicio.Year && s.fecha_creacion.Value.Year <= fin.Year) 
                                              group new { a,ds,s } by new { a.nombre, ds.id_analisis } into na
                                              orderby na.Count() descending
                                              select new reporteSolicitudxsolicitantes

                                              {
                                                  id_analisis = na.Key.id_analisis,

                                                  nombreAnalisis = na.Key.nombre,
                                                  noAnalisis = na.Count(),
                                              }
                                             );
                List<string> nombresAnalisis = queryAnalisis.Select(s => s.nombreAnalisis).Take(15).ToList();

                //Grafica analisis mas solicitados
                List<Dictionary<string, object>> dataAnalisis = new List<Dictionary<string, object>>();
                Dictionary<string, object> objetoDataAnalisis;
                List<reporteSolicitudxsolicitantes> analisis = queryAnalisis.Take(15).ToList();



                foreach (reporteSolicitudxsolicitantes ar in analisis )
                {
                    objetoDataAnalisis = new Dictionary<string, object>();
                    var valor = "0";

                    if (queryAnalisis.FirstOrDefault(i => i.id_analisis == ar.id_analisis) != null)
                    {

                        valor = queryAnalisis.FirstOrDefault(i => i.id_analisis == ar.id_analisis).noAnalisis.ToString(); // el valor que encontraste
                        objetoDataAnalisis.Add("label", ar.nombreAnalisis);
                        objetoDataAnalisis.Add(ar.nombreAnalisis, valor);
                   
                        dataAnalisis.Add(objetoDataAnalisis);
                        
                    }
                }




                var jsonDataAnalisis = Json(dataAnalisis);
                ViewBag.jsonDataAnalisis = jsonDataAnalisis;
                ViewBag.nombresAnalisis = nombresAnalisis;
                //Grafica analisis mas solicitados
            
            
           


            //dashboard
            ViewBag.queryAnalisisxEmpleado = queryAnalisisxEmpleado;            
            ViewBag.solfinalizadas = solfinalizadas.Count();
            ViewBag.analisisFinalizados = analisisFinalizados.Count();
            ViewBag.solicitudesEnProceso = SolicitudesEnProceso.Count();
            ViewBag.analisisPendientes = analisisPendientes.Count();
            //dashboard
            ViewBag.solicitudxsolicitante = solicitudxsolicitante;
            ViewBag.solicitudxsolicitantetotal = solicitudxsolicitantetotal;
            ViewBag.analisisxsolicitante = analisisxsolicitante;
            ViewBag.analisisxsolicitantetotal = analisisxsolicitantetotal;
            ViewBag.analisisxarea = analisisxarea;
            ViewBag.analisisxareatotal = analisisxareatotal;

           
        }
    }
}

