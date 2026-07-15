using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;




namespace CSMMYM.Controllers
{
    public class ContactoController : Controller
    {
        CSMMYMEntities modeloBD = new CSMMYMEntities();

        // GET: Contacto
        public ActionResult Index()
        {
            return View();
        }

        // GET: Contacto/Create
        public ActionResult Contacto()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Enviar(FormularioContacto model)
        {
            if (ModelState.IsValid)
            {
                using (CSMMYMEntities db = new CSMMYMEntities())
                {
                    Contacto contacto = new Contacto();

                    contacto.nombre = model.Nombre;
                    contacto.correo = model.Correo;
                    contacto.mensaje = model.Mensaje;

                    // Campos que NO vienen del usuario
                    contacto.fecha = DateTime.Now;
                    contacto.estado = "Pendiente";
                    contacto.id_usuario_contacto = null;
                    contacto.id_cotizacion = null;

                    db.Contacto.Add(contacto);
                    db.SaveChanges();
                }

                return RedirectToAction("Gracias");
            }

            return View(model);
        }



        [HttpGet]
        public ActionResult ListaContacto()
        {
            if (Session["id_usuario"] == null)
                return RedirectToAction("Login");

            var contactos = modeloBD.Contacto.ToList();
            return View(contactos);
        }


        public ActionResult Gracias()
        {
            return View();
        }



    }





}
