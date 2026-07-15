using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class ServiciosController : Controller
    {

        CSMMYMEntities modeloBD = new CSMMYMEntities();



        public ActionResult ListaServicios()
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

            var modeloVista = modeloBD.Select_Servicio().ToList();
            return View(modeloVista);
        }


        // GET: Servicios

        [HttpGet]
        public ActionResult CrearServicios()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearServicios(Select_Servicio_Result modeloVista, HttpPostedFileBase imagen)
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

                int cantidadRegistrosAfectados = modeloBD.Insert_Servicio(
                    modeloVista.nombre,
                    modeloVista.tipo,
                    modeloVista.precio_base,
                    modeloVista.estado,
                    nombreArchivo // CORREGIDO: antes se usaba modeloVista.imagen
                );

                if (cantidadRegistrosAfectados > 0)
                {
                    TempData["MensajeExito"] = "✅ Servicio registrado correctamente.";
                    return RedirectToAction("ListaServicios"); // asegúrate de que esta acción exista
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
        public ActionResult EditarServicio(int id_servicio)
        {
            var servicio = modeloBD.RetornaServicio_ID(id_servicio).FirstOrDefault();

            if (servicio == null)
            {
                TempData["MensajeError"] = "❌ Servicio no encontrado.";
                return RedirectToAction("ListaServicios");
            }

            return View(servicio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarServicio(RetornaServicio_ID_Result modelo, HttpPostedFileBase archivoImagen)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MensajeError = "⚠️ Datos inválidos, revise el formulario.";
                return View(modelo);
            }

            try
            {
                // Mantener la imagen actual por defecto
                string nombreArchivo = modelo.imagen;

                // Si se sube una nueva imagen, reemplazar
                if (archivoImagen != null && archivoImagen.ContentLength > 0)
                {
                    string carpeta = Server.MapPath("~/Content/Imagenes");
                    nombreArchivo = Path.GetFileName(archivoImagen.FileName);
                    string rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                    archivoImagen.SaveAs(rutaCompleta);
                }

                var resultado = modeloBD.ServicioUpdate(
                    modelo.id_servicio,
                    modelo.nombre,
                    modelo.tipo,
                    modelo.precio_base,
                    nombreArchivo // ✅ se guarda la nueva o se mantiene la anterior
                );

                if (resultado > 0)
                {
                    TempData["MensajeExito"] = "✅ Servicio actualizado correctamente.";
                    return RedirectToAction("ListaServicios");
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
        public ActionResult DeshabilitarServicio(int id)
        {
            modeloBD.sp_DeshabilitarServicio(id);
            TempData["MensajeExito"] = "Servicio deshabilitado correctamente.";
            return RedirectToAction("ListaServicios");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HabilitarServicio(int id)
        {
            modeloBD.sp_HabilitarServicio(id);
            TempData["MensajeExito"] = "Servicio deshabilitado correctamente.";
            return RedirectToAction("ListaServicios");
        }


        public ActionResult Alquiler()
        {
            var servicios = modeloBD.Select_Servicio()
                                    .Where(s => s.estado == "activo" && s.tipo == "Alquiler")
                                    .ToList();
            return View(servicios); // Vista Alquiler.cshtml
        }

        public ActionResult Mantenimiento()
        {
            var servicios = modeloBD.Select_Servicio()
                                    .Where(s => s.estado == "activo" && s.tipo == "Mantenimiento")
                                    .ToList();
            return View(servicios); // Vista Mantenimiento.cshtml
        }


        // GET: Servicios/Detalle/5
        public ActionResult Detalle(int id)
        {
            var servicio = modeloBD.Servicio.FirstOrDefault(s => s.id_servicio == id);
            if (servicio == null)
            {
                return HttpNotFound();
            }
            return View(servicio);
        }



    }
}