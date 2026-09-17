using System.ComponentModel.DataAnnotations;

namespace Pasteleria.Models
{
    public class Usuarios
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string ContrasenaHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "cliente"; //por defecto cliente 

    }
}
