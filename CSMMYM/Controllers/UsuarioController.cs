using CSMMYM.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly CSMMYMEntities modeloBD = new CSMMYMEntities();

        [AllowAnonymous]
        public ActionResult SinPermiso() => View();

        private bool TienePermisoUsuarios()
        {
            if (!(Session["id_usuario"] is int idUsuario)) return false;
            return PermisosPorUsuario.Lista.ContainsKey(idUsuario) && PermisosPorUsuario.Lista[idUsuario].AccesoUsuarios;
        }

        private ActionResult SinAcceso() => Session["id_usuario"] == null
            ? (ActionResult)RedirectToAction("SignIn", "Administrador")
            : RedirectToAction("SinPermiso");

        public ActionResult ListaUsuarios()
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            return View(modeloBD.Usuario_Select().ToList());
        }

        [HttpGet]
        public ActionResult CrearUsuario()
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CrearUsuario(string nombre, string usuario, string contrasena, int id_rol)
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Debe completar todos los campos.";
                ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre");
                return View();
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(contrasena);
            modeloBD.Usuario_Insert(nombre.Trim(), usuario.Trim(), passwordHash, "", "Activo", id_rol);
            modeloBD.SaveChanges();
            return RedirectToAction("ListaUsuarios");
        }

        [HttpGet]
        public ActionResult EditarUsuario(int id_usuario)
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            var usuario = modeloBD.Usuario_Select_Id(id_usuario).FirstOrDefault();
            if (usuario == null) return HttpNotFound();
            ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre", usuario.id_rol);
            return View(usuario);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditarUsuario(Usuario_Select_Id_Result modelo)
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre", modelo.id_rol);
                return View(modelo);
            }

            try
            {
                var resultado = modeloBD.Usuario_Update(modelo.id_usuario, modelo.nombre, modelo.usuario, modelo.id_rol, modelo.estado);
                if (resultado > 0)
                {
                    TempData["MensajeExito"] = "Usuario actualizado correctamente.";
                    return RedirectToAction("ListaUsuarios");
                }
                ViewBag.MensajeError = "No se realizaron cambios.";
            }
            catch (Exception)
            {
                ViewBag.MensajeError = "Ocurrió un error al actualizar el usuario.";
            }

            ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre", modelo.id_rol);
            return View(modelo);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Desactivar(int id)
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            modeloBD.sp_DeshabilitarUsuario(id);
            modeloBD.SaveChanges();
            return RedirectToAction("ListaUsuarios");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Activar(int id)
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            modeloBD.sp_HabilitarUsuario(id);
            modeloBD.SaveChanges();
            return RedirectToAction("ListaUsuarios");
        }

        [HttpGet]
        public ActionResult EditarPermisos(int id_usuario)
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            var usuario = modeloBD.Usuarios.Find(id_usuario);
            if (usuario == null) return HttpNotFound();
            if (!PermisosPorUsuario.Lista.ContainsKey(id_usuario)) PermisosPorUsuario.Lista[id_usuario] = new PermisosUsuario();
            ViewBag.IdUsuario = id_usuario;
            return View(PermisosPorUsuario.Lista[id_usuario]);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult GuardarPermisos(int id_usuario, PermisosUsuario permisos)
        {
            if (!TienePermisoUsuarios()) return SinAcceso();
            if (modeloBD.Usuarios.Find(id_usuario) == null) return HttpNotFound();
            PermisosPorUsuario.Lista[id_usuario] = new PermisosUsuario
            {
                AccesoUsuarios = permisos.AccesoUsuarios,
                AccesoReportes = permisos.AccesoReportes,
                AccesoSeguridad = permisos.AccesoSeguridad,
                AccesoProductos = permisos.AccesoProductos
            };
            TempData["Mensaje"] = "Permisos actualizados para esta sesión de la aplicación.";
            return RedirectToAction("ListaUsuarios");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) modeloBD.Dispose();
            base.Dispose(disposing);
        }
    }
}
