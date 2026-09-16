using CSMMYM.Models;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class ProductosController : Controller
    {
        private readonly CSMMYMEntities modeloBD = new CSMMYMEntities();

        private bool PuedeAdministrarProductos()
        {
            if (!(Session["id_usuario"] is int idUsuario)) return false;
            return PermisosPorUsuario.Lista.ContainsKey(idUsuario) &&
                   PermisosPorUsuario.Lista[idUsuario].AccesoProductos;
        }

        private ActionResult SinAcceso()
        {
            return Session["id_usuario"] == null
                ? (ActionResult)RedirectToAction("SignIn", "Administrador")
                : RedirectToAction("SinPermiso", "Usuario");
        }

        [Authorize]
        public ActionResult ListaProductos()
        {
            if (!PuedeAdministrarProductos()) return SinAcceso();
            return View(modeloBD.Select_Producto().ToList());
        }

        [HttpGet, Authorize]
        public ActionResult CrearProducto()
        {
            if (!PuedeAdministrarProductos()) return SinAcceso();
            return View();
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult CrearProducto(Select_Producto_Result modeloVista, HttpPostedFileBase imagen)
        {
            if (!PuedeAdministrarProductos()) return SinAcceso();
            if (!ModelState.IsValid) return View(modeloVista);

            try
            {
                var nombreArchivo = GuardarImagen(imagen, null);
                var resultado = modeloBD.Insert_Producto(modeloVista.nombre, modeloVista.tipo,
                    modeloVista.precio, modeloVista.descripcion, modeloVista.estado, nombreArchivo);

                if (resultado > 0)
                {
                    TempData["MensajeExito"] = "Producto registrado correctamente.";
                    return RedirectToAction("ListaProductos");
                }

                TempData["MensajeError"] = "No se pudo registrar el producto.";
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("imagen", ex.Message);
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error al registrar el producto.";
            }

            return View(modeloVista);
        }

        [HttpGet, Authorize]
        public ActionResult EditarProducto(int id_producto)
        {
            if (!PuedeAdministrarProductos()) return SinAcceso();
            var producto = modeloBD.RetornaProducto_ID(id_producto).FirstOrDefault();
            if (producto == null) return HttpNotFound();
            return View(producto);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult EditarProducto(RetornaProducto_ID_Result modelo, HttpPostedFileBase imagen)
        {
            if (!PuedeAdministrarProductos()) return SinAcceso();
            if (!ModelState.IsValid) return View(modelo);

            try
            {
                var nombreArchivo = GuardarImagen(imagen, modelo.imagen);
                var resultado = modeloBD.ProductoUpdate(modelo.id_producto, modelo.nombre, modelo.tipo,
                    modelo.precio, modelo.descripcion, nombreArchivo);

                if (resultado > 0)
                {
                    TempData["MensajeExito"] = "Producto actualizado correctamente.";
                    return RedirectToAction("ListaProductos");
                }

                ViewBag.MensajeError = "No se realizaron cambios.";
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("imagen", ex.Message);
            }
            catch (Exception)
            {
                ViewBag.MensajeError = "Ocurrió un error al actualizar el producto.";
            }

            return View(modelo);
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult DeshabilitarProducto(int id)
        {
            if (!PuedeAdministrarProductos()) return SinAcceso();
            modeloBD.sp_DeshabilitarProducto(id);
            TempData["MensajeExito"] = "Producto deshabilitado correctamente.";
            return RedirectToAction("ListaProductos");
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult HabilitarProducto(int id)
        {
            if (!PuedeAdministrarProductos()) return SinAcceso();
            modeloBD.sp_HabilitarProducto(id);
            TempData["MensajeExito"] = "Producto habilitado correctamente.";
            return RedirectToAction("ListaProductos");
        }

        [AllowAnonymous]
        public ActionResult Electrico() => View(modeloBD.Select_Producto().Where(p => p.estado == "activo" && p.tipo == "Eléctrico").ToList());

        [AllowAnonymous]
        public ActionResult Diesel() => View(modeloBD.Select_Producto().Where(p => p.estado == "activo" && p.tipo == "Diésel").ToList());

        [AllowAnonymous]
        public ActionResult Detalle(int id)
        {
            var producto = modeloBD.Producto.FirstOrDefault(p => p.id_producto == id && p.estado == "activo");
            return producto == null ? (ActionResult)HttpNotFound() : View(producto);
        }

        private string GuardarImagen(HttpPostedFileBase imagen, string actual)
        {
            if (imagen == null || imagen.ContentLength <= 0) return actual;
            if (imagen.ContentLength > 5 * 1024 * 1024) throw new InvalidOperationException("La imagen no puede superar 5 MB.");

            var extension = Path.GetExtension(imagen.FileName)?.ToLowerInvariant();
            var permitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (string.IsNullOrEmpty(extension) || !permitidas.Contains(extension))
                throw new InvalidOperationException("Formato de imagen no permitido. Use JPG, PNG o WEBP.");

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
