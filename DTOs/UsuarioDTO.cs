namespace Pasteleria.Models
{
    public class UsuarioDTO //Los DTO's funcionan como clases mensajeras para llevar los datoss crudos que arrojen 
    {
        public string Correo { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
    }
}
