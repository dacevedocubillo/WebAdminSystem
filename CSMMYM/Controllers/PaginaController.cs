using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class PaginaController : Controller
    {

        CSMMYMEntities modeloBD = new CSMMYMEntities();

        // GET: Pagina
        public ActionResult ListaPaginaPrincipal()
        {
            int idUsuario = (int)Session["id_usuario"];

            // Validar si el usuario tiene permiso
            if (!PermisosPorUsuario.Lista.ContainsKey(idUsuario) ||
                !PermisosPorUsuario.Lista[idUsuario].AccesoSeguridad) // o el permiso que definas
            {
                return RedirectToAction("SinPermiso", "Usuario");
            }

            var lista = modeloBD
                .PaginaContenido_ListByTipo("home")
                .ToList();

            return View(lista);
        }



        [HttpGet]
        public ActionResult EditarPagina(int id_pagina)
        {
            var pagina = modeloBD
                .RetornaPaginaContenido_ID(id_pagina)
                .FirstOrDefault();

            if (pagina == null)
            {
                TempData["MensajeError"] = "❌ Sección no encontrada.";
                return RedirectToAction("ListaPaginaPrincipal");
            }

            return View(pagina);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPagina(RetornaPaginaContenido_ID_Result modelo)
        {
            try
            {
                var resultado = modeloBD.PaginaContenido_Update(
                    modelo.id_pagina,
                    modelo.titulo,
                    modelo.contenido,
                    modelo.tipo,
                    modelo.seccion,
                    modelo.estado,
                    modelo.id_usuario_editor ?? 1 // evitar NULL
                );

                TempData["MensajeExito"] = "✅ Contenido actualizado correctamente.";
                return RedirectToAction("ListaPaginaPrincipal");
            }
            catch (Exception ex)
            {
                ViewBag.MensajeError = "❌ Error al actualizar: " + ex.Message;
                return View(modelo);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeshabilitarSeccion(int id)
        {
            modeloBD.sp_DeshabilitarSeccion(id);
            TempData["MensajeExito"] = "Sección deshabilitada correctamente.";
            return RedirectToAction("ListaPaginaPrincipal");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HabilitarSeccion (int id)
        {
            modeloBD.sp_HabilitarSeccion(id);
            TempData["MensajeExito"] = "Seccion habilitada correctamente.";
            return RedirectToAction("ListaPaginaPrincipal");
        }



    }
}