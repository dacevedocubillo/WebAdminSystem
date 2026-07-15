using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Xml.Linq;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace CSMMYM.Controllers
{
    public class ReportesController : Controller
    {

        CSMMYMEntities modeloBD = new CSMMYMEntities();

        // GET: Reportes
        public ActionResult ReporteProductos()
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

            // Si tiene permiso, mostrar la vista
            var lista = modeloBD.Reporte_Productos().ToList();
            return View(lista);
        }


        public ActionResult ReporteServicios()
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

            var lista = modeloBD.Reporte_Servicio()?.ToList() ?? new List<Reporte_Servicio_Result>();
            return View(lista);
        }

       



public ActionResult DescargarReporteServiciosPDF()
    {
        var listaServicios = modeloBD.Reporte_Servicio().ToList();

        if (!listaServicios.Any())
        {
            return new HttpStatusCodeResult(HttpStatusCode.NoContent, "No hay registros de servicios disponibles.");
        }

        // Generar el documento con QuestPDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header()
                    .Text("Reporte de Servicios")
                    .FontSize(20)
                    .Bold();

                page.Content().Column(col =>
                {
                    col.Item().Text($"Generado el: {DateTime.Now:dd/MM/yyyy}");
                    col.Item().Text("\n");

                    // Crear tabla
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);   // ID
                            columns.RelativeColumn(3);    // Servicio
                            columns.RelativeColumn(2);    // Precio
                            columns.RelativeColumn(2);    // Estado
                        });

                        // Encabezados
                        table.Header(header =>
                        {
                            header.Cell().Text("ID").Bold();
                            header.Cell().Text("Servicio").Bold();
                            header.Cell().Text("Precio").Bold();
                            header.Cell().Text("Estado").Bold();
                        });

                        // Filas
                        foreach (var s in listaServicios)
                        {
                            table.Cell().Text(s.id_servicio.ToString());
                            table.Cell().Text(s.nombre);
                            table.Cell().Text(Convert.ToDecimal(s.precio_base).ToString("C"));
                            table.Cell().Text(s.estado);
                        }
                    });
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

        // Exportar a memoria y devolver como archivo
        using (var ms = new MemoryStream())
        {
            document.GeneratePdf(ms);
            return File(ms.ToArray(), "application/pdf", "ReporteServicios.pdf");
        }
    }


public ActionResult DescargarReporteProductosPDF()
    {
        var listaProductos = modeloBD.Reporte_Productos().ToList();

        if (!listaProductos.Any())
        {
            return new HttpStatusCodeResult(HttpStatusCode.NoContent, "No hay registros de productos disponibles.");
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header()
                    .Text("Reporte de Productos")
                    .FontSize(20)
                    .Bold();

                page.Content().Column(col =>
                {
                    col.Item().Text($"Generado el: {DateTime.Now:dd/MM/yyyy}");
                    col.Item().Text("\n");

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);   // ID
                            columns.RelativeColumn(3);    // Nombre
                            columns.RelativeColumn(2);    // Precio
                            columns.RelativeColumn(2);    // Tipo
                            columns.RelativeColumn(2);    // Estado
                        });

                        // Encabezados
                        table.Header(header =>
                        {
                            header.Cell().Text("ID").Bold();
                            header.Cell().Text("Nombre").Bold();
                            header.Cell().Text("Precio").Bold();
                            header.Cell().Text("Tipo").Bold();
                            header.Cell().Text("Estado").Bold();
                        });

                        // Filas
                        foreach (var p in listaProductos)
                        {
                            table.Cell().Text(p.id_producto.ToString());
                            table.Cell().Text(p.nombre);
                            table.Cell().Text(p.precio.ToString("C"));
                            table.Cell().Text(p.tipo);
                            table.Cell().Text(p.estado);
                        }
                    });
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

        using (var ms = new MemoryStream())
        {
            document.GeneratePdf(ms);
            return File(ms.ToArray(), "application/pdf", "ReporteProductos.pdf");
        }
    }






}
}