using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSMMYM.Models
{
    public class FormularioContacto
    {
        public int Id_Contacto { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Mensaje { get; set; }

        // columnas que faltaban
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }

        // opcionales según tu tabla
        public int? Id_Usuario_Contacto { get; set; }
        public int? Id_Cotizacion { get; set; }



    }
}