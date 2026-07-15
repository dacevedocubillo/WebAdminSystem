using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSMMYM.Models
{
    public class PermisosUsuario
    {

        public bool AccesoUsuarios { get; set; } = true;
        public bool AccesoReportes { get; set; } = true;
        public bool AccesoSeguridad { get; set; } = true;
        public bool AccesoProductos { get; set; } = true;


    }
}