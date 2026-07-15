using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class RespuestasController : Controller
    {
        CSMMYMEntities modeloBD = new CSMMYMEntities();

        // GET: Respuestas
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Responder(int id)
        {
            var contacto = modeloBD.Contacto
                .Include("Respuesta")
                .FirstOrDefault(c => c.id_contacto == id);

            if (contacto == null)
                return HttpNotFound();

            return View(contacto);
        }

        [HttpPost]
        public ActionResult Responder(int id, string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                TempData["Error"] = "Debe escribir una respuesta.";
                return RedirectToAction("Responder", new { id });
            }

            var respuesta = new Respuesta
            {
                IdContacto = id,
                id_usuario_autor = 1, // luego lo cambiamos por el usuario logueado
                mensaje = mensaje,
                fecha = DateTime.Now
            };

            modeloBD.Respuesta.Add(respuesta);

            // Cambiar estado del contacto
            var contacto = modeloBD.Contacto.Find(id);
            contacto.estado = "Respondido";

            modeloBD.SaveChanges();

            return RedirectToAction("Responder", new { id });
        }


        public ActionResult ListaParaRespuestas()
        {
            var contactos = modeloBD.Contacto
                .OrderByDescending(c => c.fecha)
                .ToList();

            return View(contactos);
        }

        public ActionResult MisRespuestas(int idUsuario)
        {
            var respuestas = modeloBD.Respuesta
                .Where(r => r.id_usuario_autor == idUsuario)
                .OrderByDescending(r => r.fecha)
                .ToList();

            return View(respuestas);
        }


    }
}