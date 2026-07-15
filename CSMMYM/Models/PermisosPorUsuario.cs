using CSMMYM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CSMMYM.Models
{
    public static class PermisosPorUsuario
    {
        public static Dictionary<int, PermisosUsuario> Lista { get; set; } = new Dictionary<int, PermisosUsuario>();
    }

}