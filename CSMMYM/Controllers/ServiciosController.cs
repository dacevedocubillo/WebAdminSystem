using CSMMYM.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class ServiciosController : Controller
    {
        private readonly CSMMYMEntities modeloBD = new CSMMYMEntities();

        private bool PuedeAdministrar()
        {
            if (!(Session["id_usuario"] is int idUsuario)) return false;
            return PermisosPorUsuario.Lista.ContainsKey(idUsuario) && PermisosPorUsuario.Lista[idUsuario].AccesoProductos;
        }

        private ActionResult SinAcceso() => Session["id_usuario"] == null
            ? (ActionResult)RedirectToAction("SignIn", "Administrador")
            : RedirectToAction("SinPermiso", "Usuario");

        [Authorize]
        public ActionResult ListaServicios()
        {
            if (!PuedeAdministrar()) return SinAcceso();
            return View(modeloBD.Select_Servicio().ToList());
        }

        [HttpGet, Authorize]
        public ActionResult CrearServicios()
        {
            if (!PuedeAdministrar()) return SinAcceso();
            return View();
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult CrearServicios(Select_Servicio_Result modeloVista, HttpPostedFileBase imagen)
        {
            if (!PuedeAdministrar()) return SinAcceso();
            if (!ModelState.IsValid) return View(modeloVista);

            try
            {
                var nombreArchivo = GuardarImagen(imagen, null);
                var resultado = modeloBD.Insert_Servicio(modeloVista.nombre, modeloVista.tipo, modeloVista.precio_base, modeloVista.estado, nombreArchivo);
                if (resultado > 0)
                {
                    TempData["MensajeExito"] = "Servicio registrado correctamente.";
                    return RedirectToAction("ListaServicios");
                }
                TempData["MensajeError"] = "No se pudo registrar el servicio.";
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("imagen", ex.Message); }
            catch (Exception) { TempData["MensajeError"] = "Ocurrió un error al registrar el servicio."; }
            return View(modeloVista);
        }

        [HttpGet, Authorize]
        public ActionResult EditarServicio(int id_servicio)
        {
            if (!PuedeAdministrar()) return SinAcceso();
            var servicio = modeloBD.RetornaServicio_ID(id_servicio).FirstOrDefault();
            return servicio == null ? (ActionResult)HttpNotFound() : View(servicio);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult EditarServicio(RetornaServicio_ID_Result modelo, HttpPostedFileBase archivoImagen)
        {
            if (!PuedeAdministrar()) return SinAcceso();
            if (!ModelState.IsValid) return View(modelo);
            try
            {
                var nombreArchivo = GuardarImagen(archivoImagen, modelo.imagen);
                var resultado = modeloBD.ServicioUpdate(modelo.id_servicio, modelo.nombre, modelo.tipo, modelo.precio_base, nombreArchivo);
                if (resultado > 0)
                {
                    TempData["MensajeExito"] = "Servicio actualizado correctamente.";
                    return RedirectToAction("ListaServicios");
                }
                ViewBag.MensajeError = "No se realizaron cambios.";
            }
            catch (InvalidOperationException ex) { ModelState.AddModelError("archivoImagen", ex.Message); }
            catch (Exception) { ViewBag.MensajeError = "Ocurrió un error al actualizar el servicio."; }
            return View(modelo);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult DeshabilitarServicio(int id)
        {
            if (!PuedeAdministrar()) return SinAcceso();
            modeloBD.sp_DeshabilitarServicio(id);
            TempData["MensajeExito"] = "Servicio deshabilitado correctamente.";
            return RedirectToAction("ListaServicios");
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult HabilitarServicio(int id)
        {
            if (!PuedeAdministrar()) return SinAcceso();
            modeloBD.sp_HabilitarServicio(id);
            TempData["MensajeExito"] = "Servicio habilitado correctamente.";
            return RedirectToAction("ListaServicios");
        }

        [AllowAnonymous]
        public ActionResult Alquiler() => View(modeloBD.Select_Servicio().Where(s => s.estado == "activo" && s.tipo == "Alquiler").ToList());

        [AllowAnonymous]
        public ActionResult Mantenimiento() => View(modeloBD.Select_Servicio().Where(s => s.estado == "activo" && s.tipo == "Mantenimiento").ToList());

        [AllowAnonymous]
        public ActionResult Detalle(int id)
        {
            var servicio = modeloBD.Servicio.FirstOrDefault(s => s.id_servicio == id && s.estado == "activo");
            return servicio == null ? (ActionResult)HttpNotFound() : View(servicio);
        }

        private string GuardarImagen(HttpPostedFileBase imagen, string actual)
        {
            if (imagen == null || imagen.ContentLength <= 0) return actual;
            if (imagen.ContentLength > 5 * 1024 * 1024) throw new InvalidOperationException("La imagen no puede superar 5 MB.");
            var extension = Path.GetExtension(imagen.FileName)?.ToLowerInvariant();
            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (string.IsNullOrEmpty(extension) || !permitidas.Contains(extension)) throw new InvalidOperationException("Formato de imagen no permitido. Use JPG, PNG o WEBP.");
            var carpeta = Server.MapPath("~/Content/Imagenes");
            Directory.CreateDirectory(carpeta);
            var nombre = Guid.NewGuid().ToString("N") + extension;
            imagen.SaveAs(Path.Combine(carpeta, nombre));
            return nombre;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) modeloBD.Dispose();
            base.Dispose(disposing);
        }
    }
}
