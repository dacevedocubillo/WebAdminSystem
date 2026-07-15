using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class ProductosController : Controller
    {

        CSMMYMEntities modeloBD = new CSMMYMEntities();


        public ActionResult ListaProductos()
        {
            if (Session["id_usuario"] == null)
            {
                return RedirectToAction("SignIn", "Administrador");
            }

            int idUsuario = (int)Session["id_usuario"];

            if (!PermisosPorUsuario.Lista.ContainsKey(idUsuario) ||
                !PermisosPorUsuario.Lista[idUsuario].AccesoProductos)
            {
                return RedirectToAction("SinPermiso", "Usuario");
            }

            var modeloVista = modeloBD.Select_Producto().ToList();
            return View(modeloVista);
        }



        // GET: Productos


        [HttpGet]
        public ActionResult CrearProducto()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearProducto(Select_Producto_Result modeloVista, HttpPostedFileBase imagen)
        {
            if (!ModelState.IsValid)
            {
                TempData["MensajeError"] = "⚠️ Datos inválidos, revise el formulario.";
                return View(modeloVista);
            }

            try
            {
                string nombreArchivo = null;

                if (imagen != null && imagen.ContentLength > 0)
                {
                    string carpeta = Server.MapPath("~/Content/Imagenes");
                    nombreArchivo = Path.GetFileName(imagen.FileName);
                    string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    // Guardar archivo en el servidor
                    imagen.SaveAs(rutaCompleta);
                }

                int cantidadRegistrosAfectados = modeloBD.Insert_Producto(
                    modeloVista.nombre,
                    modeloVista.tipo,
                    modeloVista.precio,
                    modeloVista.descripcion,
                    modeloVista.estado,
                    nombreArchivo
                );

                if (cantidadRegistrosAfectados > 0)
                {
                    TempData["MensajeExito"] = "✅ Producto registrado correctamente.";
                    return RedirectToAction("ListaProductos"); // asegúrate de que esta acción exista
                }

                TempData["MensajeError"] = "⚠️ No se pudo insertar el registro.";
                return View(modeloVista);
            }
            catch (Exception ex)
            {
                TempData["MensajeError"] = "❌ Ocurrió un error inesperado: " + ex.Message;
                return View(modeloVista);
            }
        }



        [HttpGet]
        public ActionResult EditarProducto(int id_producto)
        {
            var producto = modeloBD.RetornaProducto_ID(id_producto).FirstOrDefault();

            if (producto == null)
            {
                TempData["MensajeError"] = "❌ Producto no encontrado.";
                return RedirectToAction("ListaProductos");
            }

            return View(producto);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarProducto(RetornaProducto_ID_Result modelo, HttpPostedFileBase imagen)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MensajeError = "⚠️ Datos inválidos, revise el formulario.";
                return View(modelo);
            }

            try
            {
                string nombreArchivo = modelo.imagen; // mantener la imagen actual por defecto

                if (imagen != null && imagen.ContentLength > 0)
                {
                    string carpeta = Server.MapPath("~/Content/Imagenes");
                    nombreArchivo = Path.GetFileName(imagen.FileName);
                    string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    // Guardar archivo en el servidor
                    imagen.SaveAs(rutaCompleta);
                }

                var resultado = modeloBD.ProductoUpdate(
                    modelo.id_producto,
                    modelo.nombre,
                    modelo.tipo,
                    modelo.precio,
                    modelo.descripcion,
                    nombreArchivo // ahora sí guardamos la nueva imagen o mantenemos la anterior
                );

                if (resultado > 0)
                {
                    TempData["MensajeExito"] = "✅ Producto actualizado correctamente.";
                    return RedirectToAction("ListaProductos");
                }

                ViewBag.MensajeError = "⚠️ No se realizaron cambios.";
                return View(modelo);
            }
            catch (Exception ex)
            {
                ViewBag.MensajeError = "❌ Error al actualizar: " + ex.Message;
                return View(modelo);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeshabilitarProducto(int id)
        {
            modeloBD.sp_DeshabilitarProducto(id);
            TempData["MensajeExito"] = "Producto deshabilitado correctamente.";
            return RedirectToAction("ListaProductos");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HabilitarProducto(int id)
        {
            modeloBD.sp_HabilitarProducto(id);
            TempData["MensajeExito"] = "Producto habilitado correctamente.";
            return RedirectToAction("ListaProductos");
        }


        public ActionResult Electrico()
        {
            var productos = modeloBD.Select_Producto()
                                    .Where(p => p.estado == "activo" && p.tipo == "Eléctrico")
                                    .ToList();
            return View(productos); // Vista Catalogo.cshtml
        }

        public ActionResult Diesel()
        {
            var productos = modeloBD.Select_Producto()
                                    .Where(p => p.estado == "activo" && p.tipo == "Diésel")
                                    .ToList();
            return View(productos); // Vista Diesel.cshtml
        }

        // GET: Productos/Detalle/5
        public ActionResult Detalle(int id)
        {
            var producto = modeloBD.Producto.FirstOrDefault(p => p.id_producto == id);
            if (producto == null)
            {
                return HttpNotFound();
            }
            return View(producto);
        }



    }
}