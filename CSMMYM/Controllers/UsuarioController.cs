using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class UsuarioController : Controller
    {

        CSMMYMEntities modeloBD = new CSMMYMEntities();


        public ActionResult SinPermiso()
        {
            return View();
        }

        // LISTA DE USUARIOS
        public ActionResult ListaUsuarios()
        {
            var lista = modeloBD.Usuario_Select().ToList();
            return View(lista);
        }



        [HttpGet]
        public ActionResult CrearUsuario()
        {
            ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearUsuario(string nombre, string usuario, string contrasena, int id_rol)
        {
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(usuario) ||
                string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.Error = "Debe completar todos los campos.";
                ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre");
                return View();
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(contrasena);

            modeloBD.Usuario_Insert(
                nombre,
                usuario,
                passwordHash,
                "",
                "Activo",
                id_rol
            );

            modeloBD.SaveChanges();

            return RedirectToAction("ListaUsuarios", "Usuario");

        }


        [HttpGet]
        public ActionResult EditarUsuario(int id_usuario)
        {
            var usuario = modeloBD.Usuario_Select_Id(id_usuario).FirstOrDefault();

            if (usuario == null)
            {
                TempData["MensajeError"] = "❌ Usuario no encontrado.";
                return RedirectToAction("ListaUsuarios");
            }

            ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre", usuario.id_rol);

            return View(usuario);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarUsuario(Usuario_Select_Id_Result modelo)
        {
            try
            {
                var resultado = modeloBD.Usuario_Update(
                    modelo.id_usuario,
                    modelo.nombre,
                    modelo.usuario,
                    modelo.id_rol,
                    modelo.estado
                );

                if (resultado > 0)
                {
                    TempData["MensajeExito"] = "✅ Usuario actualizado correctamente.";
                    return RedirectToAction("ListaUsuarios");
                }

                ViewBag.MensajeError = "⚠️ No se realizaron cambios.";

                ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre", modelo.id_rol);

                return View(modelo);
            }
            catch (Exception ex)
            {
                ViewBag.MensajeError = "❌ Error al actualizar: " + ex.Message;

                ViewBag.Roles = new SelectList(modeloBD.Rol_Select(), "id_rol", "nombre", modelo.id_rol);

                return View(modelo);
            }
        }



        public ActionResult Desactivar(int id)
        {
            modeloBD.sp_DeshabilitarUsuario(id);
            modeloBD.SaveChanges();

            return RedirectToAction("ListaUsuarios");
        }

        public ActionResult Activar(int id)
        {
            modeloBD.sp_HabilitarUsuario(id);
            modeloBD.SaveChanges();

            return RedirectToAction("ListaUsuarios");
        }


        public ActionResult EditarPermisos(int id_usuario)
        {
            var usuario = modeloBD.Usuarios.Find(id_usuario);
            if (usuario == null) return HttpNotFound();

            // Si no existe en memoria, lo creamos
            if (!PermisosPorUsuario.Lista.ContainsKey(id_usuario))
            {
                PermisosPorUsuario.Lista[id_usuario] = new PermisosUsuario();
            }

            ViewBag.IdUsuario = id_usuario;
            return View(PermisosPorUsuario.Lista[id_usuario]);
        }

        [HttpPost]
        public ActionResult GuardarPermisos(int id_usuario, PermisosUsuario permisos)
        {
            if (!PermisosPorUsuario.Lista.ContainsKey(id_usuario))
            {
                PermisosPorUsuario.Lista[id_usuario] = new PermisosUsuario();
            }

            PermisosPorUsuario.Lista[id_usuario].AccesoUsuarios = permisos.AccesoUsuarios;
            PermisosPorUsuario.Lista[id_usuario].AccesoReportes = permisos.AccesoReportes;
            PermisosPorUsuario.Lista[id_usuario].AccesoSeguridad = permisos.AccesoSeguridad;
            PermisosPorUsuario.Lista[id_usuario].AccesoProductos = permisos.AccesoProductos;

            TempData["Mensaje"] = "Permisos del usuario actualizados correctamente.";
            return RedirectToAction("ListaUsuarios");
        }













    }
}