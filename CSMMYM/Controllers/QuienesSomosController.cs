using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class QuienesSomosController : Controller
    {
        CSMMYMEntities modeloBD = new CSMMYMEntities();

        public ActionResult QuienesSomos()
        {
            var seccion = modeloBD.Pagina
                                  .FirstOrDefault(s => s.seccion == "quienes_somos" && s.estado == "activo");

            if (seccion == null)
            {
                // Puedes devolver un modelo vacío o un mensaje
                seccion = new Pagina { titulo = "Sección desactivada", contenido = "" };
            }

            return View(seccion);
        }


    }
}