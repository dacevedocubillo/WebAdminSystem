using CSMMYM.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace CSMMYM.Controllers
{
    public class CotizacionController : Controller
    {

        CSMMYMEntities modeloBD = new CSMMYMEntities();

        // LISTA
        public ActionResult Lista()
        {
            var lista = modeloBD.Cotizacion.ToList();
            return View(lista);
        }

        // CREAR NORMAL (GET)
        public ActionResult Crear()
        {
            return View("CrearCotizacion");
        }

        // CREAR NORMAL (POST)
        [HttpPost]
        public ActionResult Crear(Cotizacion cotizacion, int? idContacto)
        {
            if (ModelState.IsValid)
            {
                cotizacion.fecha_creacion = DateTime.Now;
                cotizacion.estado = "Pendiente";

                modeloBD.Cotizacion.Add(cotizacion);
                modeloBD.SaveChanges();

                if (idContacto != null)
                {
                    var contacto = modeloBD.Contacto.Find(idContacto);
                    contacto.id_cotizacion = cotizacion.id_cotizacion;
                    modeloBD.SaveChanges();
                }

                return RedirectToAction("Detalle", new { id = cotizacion.id_cotizacion });
            }

            return View("CrearCotizacion", cotizacion);
        }

        // CREAR DESDE CONTACTO (GET)
        public ActionResult CrearDesdeContacto(int id)
        {
            var contacto = modeloBD.Contacto.Find(id);

            if (contacto == null)
                return HttpNotFound();

            Cotizacion model = new Cotizacion
            {
                nombre = contacto.nombre,
                correo = contacto.correo,
                detalle = contacto.mensaje
            };

            ViewBag.IdContacto = id;

            return View("CrearCotizacion", model);
        }

        // DETALLE
        public ActionResult Detalle(int id)
        {
            var cot = modeloBD.Cotizacion.Find(id);

            if (cot == null)
                return HttpNotFound();

            return View(cot);
        }

        // AGREGAR DETALLE (GET)
        public ActionResult AgregarDetalle(int id)
        {
            ViewBag.IdCotizacion = id;

            ViewBag.Productos = new SelectList(modeloBD.Producto.ToList(), "id_producto", "nombre");
            ViewBag.Servicios = new SelectList(modeloBD.Servicio.ToList(), "id_servicio", "nombre");

            Detalle_Cotizacion model = new Detalle_Cotizacion
            {
                id_cotizacion = id
            };

            return View(model);
        }



        [HttpPost]
        public ActionResult AgregarDetalle(Detalle_Cotizacion model)
        {
            // VALIDACIÓN POR REGLA DE NEGOCIO (OBLIGATORIA)
            if (model.id_producto == null && model.id_servicio == null)
            {
                ModelState.AddModelError("", "Debe seleccionar un producto o un servicio.");
            }

            if (model.id_producto != null && model.id_servicio != null)
            {
                ModelState.AddModelError("", "Seleccione solo un producto o un servicio, no ambos.");
            }

            // SI HAY ERRORES, RECARGAR LISTAS Y DEVOLVER LA VISTA
            if (!ModelState.IsValid)
            {
                ViewBag.Productos = new SelectList(modeloBD.Producto.ToList(), "id_producto", "nombre");
                ViewBag.Servicios = new SelectList(modeloBD.Servicio.ToList(), "id_servicio", "nombre");
                return View(model);
            }

            // SI TODO ESTÁ BIEN, GUARDAR
            modeloBD.Detalle_Cotizacion.Add(model);
            modeloBD.SaveChanges();

            return RedirectToAction("Detalle", new { id = model.id_cotizacion });
        }

        // ELIMINAR DETALLE
        public ActionResult EliminarDetalle(int id)
        {
            var det = modeloBD.Detalle_Cotizacion.Find(id);

            if (det == null)
                return HttpNotFound();

            int idCot = det.id_cotizacion;

            modeloBD.Detalle_Cotizacion.Remove(det);
            modeloBD.SaveChanges();

            return RedirectToAction("Detalle", new { id = idCot });
        }

        // CAMBIAR ESTADO
        [HttpPost]
        public ActionResult CambiarEstado(int id, string estado)
        {
            var cot = modeloBD.Cotizacion.Find(id);

            if (cot == null)
                return HttpNotFound();

            cot.estado = estado;
            modeloBD.SaveChanges();

            return RedirectToAction("Detalle", new { id = id });
        }

        public ActionResult Descargar(int id)
        {
            var cot = modeloBD.Cotizacion.Find(id);

            if (cot == null)
                return HttpNotFound();

            // Crear documento QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header()
                        .Text($"Cotización #{cot.id_cotizacion}")
                        .FontSize(20)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Generado el: {DateTime.Now:dd/MM/yyyy}");
                        col.Item().Text("\n");

                        col.Item().Text("Datos del Cliente").FontSize(14).Bold();
                        col.Item().Text($"Nombre: {cot.nombre}");
                        col.Item().Text($"Correo: {cot.correo}");
                        col.Item().Text($"Descripción: {cot.detalle}");
                        col.Item().Text($"Fecha: {cot.fecha_creacion:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Estado: {cot.estado}");

                        col.Item().Text("\n");

                        col.Item().Text("Ítems de la Cotización").FontSize(14).Bold();

                        // Tabla
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3); // Producto/Servicio
                                columns.ConstantColumn(60); // Cantidad
                                columns.ConstantColumn(60); // Duración
                                columns.ConstantColumn(80); // Precio
                                columns.ConstantColumn(80); // Total
                            });

                            // Encabezados
                            table.Header(header =>
                            {
                                header.Cell().Text("Producto / Servicio").Bold();
                                header.Cell().Text("Cant.").Bold();
                                header.Cell().Text("Durac.").Bold();
                                header.Cell().Text("Precio").Bold();
                                header.Cell().Text("Total").Bold();
                            });

                            // Filas
                            foreach (var item in cot.Detalle_Cotizacion)
                            {
                                var nombre = item.Producto?.nombre ?? item.Servicio?.nombre ?? "Sin asignar";
                                var total = item.cantidad * item.precio_unitario;

                                table.Cell().Text(nombre);
                                table.Cell().Text(item.cantidad.ToString());
                                table.Cell().Text(item.duracion?.ToString() ?? "-");
                                table.Cell().Text(item.precio_unitario.ToString("C"));
                                table.Cell().Text(total.ToString("C"));
                            }
                        });

                        col.Item().Text("\n");

                        col.Item().Text(
                            $"Total General: {cot.Detalle_Cotizacion.Sum(x => x.cantidad * x.precio_unitario).ToString("C")}"
                        )
                        .FontSize(14)
                        .Bold();
                    });

                    page.Footer()
                        .AlignRight()
                        .Text(text =>
                        {
                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                });
            });

            // Exportar PDF
            using (var ms = new MemoryStream())
            {
                document.GeneratePdf(ms);
                return File(ms.ToArray(), "application/pdf", $"Cotizacion_{id}.pdf");
            }
        }


    }
}