using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using CSMMYM.Models;


namespace CSMMYM.Controllers
{
    public class AdministradorController : Controller
    {

       CSMMYMEntities modeloBD = new CSMMYMEntities();


        [HttpGet]
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Debe ingresar usuario y contraseña.";
                return View();
            }

            username = username.Trim();

            var user = modeloBD.Usuarios
                .FirstOrDefault(u => u.usuario.ToLower() == username.ToLower()
                                  && u.estado == "Activo");

            bool esValida = false;

            if (user != null && !string.IsNullOrEmpty(user.contrasena))
            {
                if (user.contrasena.StartsWith("$2"))
                {
                    esValida = BCrypt.Net.BCrypt.Verify(password, user.contrasena);
                }
                else if (user.contrasena == password)
                {
                    esValida = true;

                    user.contrasena = BCrypt.Net.BCrypt.HashPassword(password);
                    modeloBD.SaveChanges();
                }
            }

            if (!esValida)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View();
            }

            FormsAuthentication.SetAuthCookie(user.usuario, false);


            Session["id_usuario"] = user.id_usuario;
            Session["Usuario"] = user.usuario;
            Session["RolUsuario"] = user.id_rol;
            Session.Timeout = 30;

            // Inicializar permisos para este usuario
            PermisosPorUsuario.Lista[user.id_usuario] = new PermisosUsuario
            {
                AccesoUsuarios = true,
                AccesoReportes = true,
                AccesoSeguridad = true,
                AccesoProductos = true
            };

            return RedirectToAction("PanelAdmin", "Administrador");
        }


        public ActionResult PanelAdmin()
        {
            return View();
        }



        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();

            return RedirectToAction("SignIn", "Administrador"); // ✅ Redirección corregida
        }



    }



}