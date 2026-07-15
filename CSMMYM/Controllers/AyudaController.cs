using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class AyudaController : Controller
    {

        CSMMYMEntities modeloBD = new CSMMYMEntities();


        // GET: Ayuda

        [HttpGet]
        public ActionResult CambiarContrasena()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarContrasena(string actual, string nueva, string confirmar)
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(actual) ||
                string.IsNullOrWhiteSpace(nueva) ||
                string.IsNullOrWhiteSpace(confirmar))
            {
                ViewBag.Error = "Todos los campos son obligatorios.";
                return View();
            }

            if (nueva.Length < 8)
            {
                ViewBag.Error = "La nueva contraseña debe tener al menos 8 caracteres.";
                return View();
            }

            int idUsuario = Convert.ToInt32(Session["IdUsuario"]);

            var usuario = modeloBD.Usuarios
                .FirstOrDefault(u => u.id_usuario == idUsuario);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario no encontrado.";
                return View();
            }

            // Validar contraseña actual
            if (!BCrypt.Net.BCrypt.Verify(actual, usuario.contrasena))
            {
                ViewBag.Error = "La contraseña actual es incorrecta.";
                return View();
            }

            // Validar coincidencia
            if (nueva != confirmar)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            // Generar hash
            string hash = BCrypt.Net.BCrypt.HashPassword(nueva);

            // Actualizar en BD
            modeloBD.Usuario_CambiarContrasena(idUsuario, hash);
            modeloBD.SaveChanges();

            ViewBag.Exito = "Contraseña actualizada correctamente.";

            return View();
        }

        public ActionResult Version()
        {
            return View();
        }


        // Vista para mostrar el botón de descarga
        public ActionResult ManualUsuario()
        {
            return View();
        }

        // Acción para descargar el manual en PDF
        public FileResult DescargarManualUsuarioPDF()
        {
            // Ruta del archivo PDF dentro del proyecto
            string ruta = Server.MapPath("~/Content/Manual/ManualUsuario.pdf");

            // Nombre que tendrá el archivo al descargarse
            string nombreArchivo = "ManualUsuario.pdf";

            // Retorna el archivo como descarga
            return File(ruta, "application/pdf", nombreArchivo);
        }



    }
}