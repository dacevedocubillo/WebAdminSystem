using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSMMYM.Controllers
{
    public class HomeController : Controller
    {
        CSMMYMEntities modeloBD = new CSMMYMEntities();

        // ============================
        // MÉTODO CENTRALIZADO
        // ============================
        private void CargarEstadoSecciones()
        {
            var secciones = modeloBD.Pagina.ToList();

            ViewBag.QuienesSomosActivo = secciones.Any(s => s.seccion == "quienes_somos" && s.estado == "activo");
            ViewBag.HistoriaActivo = secciones.Any(s => s.seccion == "historia" && s.estado == "activo");
            ViewBag.NovedadesActivo = secciones.Any(s => s.seccion == "novedades" && s.estado == "activo");
        }

        // ============================
        // PÁGINA PRINCIPAL
        // ============================
        public ActionResult Index()
        {
            CargarEstadoSecciones();

            // Página principal
            ViewBag.Principal = modeloBD.Pagina
                .FirstOrDefault(p => p.seccion == "principal" && p.estado == "activo");

            // Novedades
            ViewBag.Novedades = modeloBD.Pagina
                .FirstOrDefault(p => p.seccion == "novedades" && p.estado == "activo");

            // ⭐ HISTORIA (corrección clave)
            ViewBag.Historia = modeloBD.Pagina
                .FirstOrDefault(s => s.seccion == "historia" && s.estado == "activo");

            // Productos destacados
            ViewBag.ProductosDestacados = modeloBD.Select_Producto()
                .Where(p => p.estado.ToLower() == "activo")
                .Take(3)
                .ToList();

            return View();
        }

        // ============================
        // QUIÉNES SOMOS
        // ============================
        public ActionResult QuienesSomos()
        {
            CargarEstadoSecciones();

            var seccion = modeloBD.Pagina
                .FirstOrDefault(s => s.seccion == "quienes_somos" && s.estado == "activo");

            if (seccion == null)
                seccion = new Pagina { titulo = "Sección desactivada", contenido = "" };

            return View(seccion);
        }

        // ============================
        // HISTORIA
        // ============================
        public ActionResult Historia()
        {
            CargarEstadoSecciones();

            var seccion = modeloBD.Pagina
                .FirstOrDefault(s => s.seccion == "historia" && s.estado == "activo");

            if (seccion == null)
                seccion = new Pagina { titulo = "Sección desactivada", contenido = "" };

            return View(seccion);
        }

        // ============================
        // NOVEDADES
        // ============================
        public ActionResult Novedades()
        {
            CargarEstadoSecciones();

            var seccion = modeloBD.Pagina
                .FirstOrDefault(s => s.seccion == "novedades" && s.estado == "activo");

            if (seccion == null)
                seccion = new Pagina { titulo = "Sección desactivada", contenido = "" };

            return View(seccion);
        }
    }
}