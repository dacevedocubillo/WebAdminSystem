namespace CSMMYM.Models
{
    /// <summary>
    /// Application-level permissions for an authenticated administrative user.
    /// Permissions default to false so a missing configuration never grants access.
    /// </summary>
    public class PermisosUsuario
    {
        public bool AccesoUsuarios { get; set; }
        public bool AccesoReportes { get; set; }
        public bool AccesoSeguridad { get; set; }
        public bool AccesoProductos { get; set; }
    }
}
