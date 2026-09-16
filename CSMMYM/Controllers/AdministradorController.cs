using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using CSMMYM.Models;

namespace CSMMYM.Controllers
{
    public class AdministradorController : Controller
    {
        private readonly CSMMYMEntities modeloBD = new CSMMYMEntities();

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("PanelAdmin");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
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

            var esValida = false;

            if (user != null && !string.IsNullOrEmpty(user.contrasena))
            {
                if (user.contrasena.StartsWith("$2", StringComparison.Ordinal))
                {
                    esValida = BCrypt.Net.BCrypt.Verify(password, user.contrasena);
                }
                else if (user.contrasena == password)
                {
                    // Compatibility migration for legacy records. Remove this branch
                    // once every existing password has been converted to BCrypt.
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

            // Least privilege: only administrator roles receive administrative
            // permissions. Other authenticated roles start with no privileges.
            var nombreRol = user.Rol != null ? user.Rol.nombre : string.Empty;
            var esAdministrador = string.Equals(nombreRol, "Administrador", StringComparison.OrdinalIgnoreCase)
                                  || string.Equals(nombreRol, "Admin", StringComparison.OrdinalIgnoreCase);

            PermisosPorUsuario.Lista[user.id_usuario] = new PermisosUsuario
            {
                AccesoUsuarios = esAdministrador,
                AccesoReportes = esAdministrador,
                AccesoSeguridad = esAdministrador,
                AccesoProductos = esAdministrador
            };

            return RedirectToAction("PanelAdmin");
        }

        [Authorize]
        public ActionResult PanelAdmin()
        {
            if (Session["id_usuario"] == null)
            {
                FormsAuthentication.SignOut();
                return RedirectToAction("SignIn");
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            if (Session["id_usuario"] is int idUsuario)
            {
                PermisosPorUsuario.Lista.Remove(idUsuario);
            }

            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("SignIn");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                modeloBD.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
