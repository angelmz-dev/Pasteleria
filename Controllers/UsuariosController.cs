using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pasteleria.Data;
using Pasteleria.Models;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pasteleria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly PasteleriaContext _context;

        public UsuariosController(PasteleriaContext context) //Colocamos el contexto y la conexión en nuestro controller
        {
            _context = context;
        }

        [HttpPost("registrar")]
        public async Task<ActionResult> Registrar(UsuarioDTO peticion)
        {
            bool usuarioExiste = await _context.Usuarios.AnyAsync(u => u.Correo == peticion.Correo);
            if (usuarioExiste) return BadRequest("El Correo ya está en uso");

            string hashGenerado = BCrypt.Net.BCrypt.HashPassword(peticion.Contrasena);

            var nuevoUsuario = new Usuarios
            {
                Correo = peticion.Correo,
                ContrasenaHash = hashGenerado,
                // 🚨 SEGURIDAD EXTREMA: Ignoramos al frontend y lo forzamos a ser Cliente
                Rol = "Cliente"
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();
            return Ok("¡Usuario registrado con éxito!");
        }

        [HttpPost("login")]
        public async Task<ActionResult> Iniciar(UsuarioDTO peticion)
        {
            var usuarioEnBD = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == peticion.Correo);

            if(usuarioEnBD == null)
            {
                return BadRequest("Correo o contraseña incorrectos");
            }

            bool contrasenaCorrecta = BCrypt.Net.BCrypt.Verify(peticion.Contrasena, usuarioEnBD.ContrasenaHash);

            if(!contrasenaCorrecta)
            {
                return BadRequest("¡EL correo o la contraseña es incorrectos!");
            }

            string tokenJWT = CrearToken(usuarioEnBD);
            return Ok(new {token=tokenJWT, mensaje = "¡Login exitoso!"});
        }

        private string CrearToken(Usuarios usuario) //Revisar este código 
        {
            // 1. Los datos que irán impresos en el gafete (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol ?? "Cliente") //Agregamos cliente por defecto siempre para evitar "hackeos"
            };

            // 2. Sacar la llave secreta de nuestro archivo appsettings.json
            // Nota: Debemos pedir la configuración desde el entorno, o instanciarla (Te mostraré la forma más rápida)
            var llaveSecreta = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("EstaEsUnaLlaveSuperSecretaYMuyLargaParaDeliciasAny2024!"));

            // 3. Crear el sello criptográfico
            var credenciales = new SigningCredentials(llaveSecreta, SecurityAlgorithms.HmacSha256Signature);

            // 4. Fabricar el gafete (Validez de 1 día)
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: credenciales
            );

            // 5. Entregar el gafete ya escrito en texto
            var jwtGenerado = new JwtSecurityTokenHandler().WriteToken(token);
            return jwtGenerado;
        }
    }
}
