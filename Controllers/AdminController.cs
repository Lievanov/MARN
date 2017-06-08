using LaboratorioMarn.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using LaboratorioMarn.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.IO;

namespace LaboratorioMarn.Controllers
{

    public class AdminController : Controller
    {

        MARNEntities db = new MARNEntities();

        // GET: Admin
        public ActionResult Index()
        {
            List<EMPLEADO> empleados = db.EMPLEADOes.Where(e => e.id_tipo_empleado == 1 || e.id_tipo_empleado == 2).ToList();

            ViewBag.Empleados = empleados;

            return View(UserManager.Users);
        }

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }

        private AppRoleManager RoleManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppRoleManager>();
            }
        }

        private EMPLEADO empleado()
        {
            var username = User.Identity.GetUserName();

            AppUser user = UserManager.FindByName(username);

            return db.EMPLEADOes.First(e => e.id_aspnet_user == user.Id);
        }

        public ActionResult Create()
        {

            this.setViewBagParameters();

            return View();
        }

        private void setViewBagParameters()
        {
            var areas = db.AREAs.ToList();

            IEnumerable<SelectListItem> areasList =

            from value in areas

            select new SelectListItem
            {
                Text = value.nombre_area,
                Value = value.id_area.ToString()
            };

            var tipos = db.TIPO_EMPLEADO.Where(te => te.id_tipo_empleado == 1 || te.id_tipo_empleado == 2).ToList();

            IEnumerable<SelectListItem> tiposList =

            from value in tipos

            select new SelectListItem
            {
                Text = value.nombre,
                Value = value.id_tipo_empleado.ToString()

            };

            ViewBag.AreaSelection = areasList;

            ViewBag.TipoSelection = tiposList;

            return;
        }


        [HttpPost]
        public async Task<ActionResult> Create(CreateModel model)
        {


            if (ModelState.IsValid)
            {

                AppUser user = new AppUser { UserName = model.Name, Email = model.Email };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var name = "Admin";

                    if (model.tipo == 2)
                    {
                        name = "Analista";
                    }
                    else if (model.tipo == 3)
                    {
                        name = "Campo";
                    }

                    AppRole role = await RoleManager.FindByNameAsync(name);
                    if (role == null)
                    {
                        IdentityResult resultRole = await RoleManager.CreateAsync(new AppRole(name));
                        if (resultRole.Succeeded)
                        {
                            role = await RoleManager.FindByNameAsync(name);
                        }
                        else
                        {
                            AddErrorsFromResult(resultRole);
                        }
                    }

                    await UserManager.AddToRoleAsync(user.Id, name);

                    EMPLEADO empleado = new EMPLEADO()
                    {
                        nombre_empleado = model.nombre,
                        telefono_empleado = model.telefono,
                        id_tipo_empleado = model.tipo,
                        id_aspnet_user = user.Id
                    };

                    if (model.area > 0)
                    {
                        empleado.id_area = model.area;
                    }

                    db.EMPLEADOes.Add(empleado);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }

            this.setViewBagParameters();

            return View(model);

        }

        [HttpPost]
        public async Task<ActionResult> Delete(string id)
        {
            try
            {
                AppUser user = await UserManager.FindByIdAsync(id);
                if (user != null)
                {
                    var userid = user.Id;
                    IdentityResult result = await UserManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        EMPLEADO empleado = db.EMPLEADOes.Single(e => e.id_aspnet_user == userid);

                        empleado.id_aspnet_user = null;

                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View("Error", result.Errors);
                    }
                }
                else
                {
                    return View("Error", new string[] { "User Not Found" });
                }

            }
            catch (System.Data.SqlClient.SqlException)
            {
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> Edit(string id)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {

                EMPLEADO empleado = db.EMPLEADOes.Single(e => e.id_aspnet_user == user.Id);
                ViewBag.Empleado = empleado;

                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public async Task<ActionResult> Edit(string id, string email, string password)
        {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Email = email;
                IdentityResult validEmail = await UserManager.UserValidator.ValidateAsync(user);

                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }

                IdentityResult validPass = null;
                if (password != string.Empty)
                {
                    validPass = await UserManager.PasswordValidator.ValidateAsync(password);
                    if (validPass.Succeeded)
                    {
                        user.PasswordHash =
                        UserManager.PasswordHasher.HashPassword(password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPass);
                    }
                }

                if ((validEmail.Succeeded && validPass == null) || (validEmail.Succeeded
                && password != string.Empty && validPass.Succeeded))
                {
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        EMPLEADO empleado = db.EMPLEADOes.Single(e => e.id_aspnet_user == user.Id);

                        empleado.nombre_empleado = Request.Form["empleado.nombre_empleado"];
                        empleado.telefono_empleado = Request.Form["empleado.telefono_empleado"];

                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];

                            if (file != null && file.ContentLength > 0)
                            {
                                var fileName = "empleado_" + empleado.id_empleado + ".jpg";
                                var path = Path.Combine(Server.MapPath("~/Avatar/"), fileName);
                                file.SaveAs(path);

                                empleado.avatar = "/Avatar/" + fileName;
                            }
                        }

                        db.SaveChanges();

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }

            return View(user);
        }

        public async Task<ActionResult> EditProfile()
        {
            EMPLEADO empleado = this.empleado();

            AppUser user = await UserManager.FindByIdAsync(empleado.id_aspnet_user);
            ViewBag.Empleado = empleado;

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> EditProfile(string email, string password)
        {
            EMPLEADO empleado = this.empleado();

            AppUser user = await UserManager.FindByIdAsync(empleado.id_aspnet_user);
            if (user != null)
            {
                user.Email = email;
                IdentityResult validEmail = await UserManager.UserValidator.ValidateAsync(user);

                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }

                IdentityResult validPass = null;
                if (password != string.Empty)
                {
                    validPass = await UserManager.PasswordValidator.ValidateAsync(password);
                    if (validPass.Succeeded)
                    {
                        user.PasswordHash =
                        UserManager.PasswordHasher.HashPassword(password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPass);
                    }
                }

                if ((validEmail.Succeeded && validPass == null) || (validEmail.Succeeded
                && password != string.Empty && validPass.Succeeded))
                {
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        empleado.nombre_empleado = Request.Form["empleado.nombre_empleado"];
                        empleado.telefono_empleado = Request.Form["empleado.telefono_empleado"];

                        if (Request.Files.Count > 0)
                        {
                            var file = Request.Files[0];

                            if (file != null && file.ContentLength > 0)
                            {
                                var fileName = "empleado_" + empleado.id_empleado + ".jpg";
                                var path = Path.Combine(Server.MapPath("~/Avatar/"), fileName);
                                file.SaveAs(path);

                                empleado.avatar = "/Avatar/" + fileName;
                            }
                        }

                        db.SaveChanges();

                        return RedirectToAction("EditProfile");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }

            return View(user);
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
    }
}
