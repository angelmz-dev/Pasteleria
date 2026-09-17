using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pasteleria.Data;
using Pasteleria.Models;
using System.Security.Claims;

namespace Pasteleria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // 🔒 LA BARRERA: Si no hay token JWT válido, el código se detiene aquí.
    public class VentasController : ControllerBase
    {
        private readonly PasteleriaContext _context;

        public VentasController(PasteleriaContext context)
        {
            _context = context;
        }

        [HttpPost("pagar")]
        public async Task<IActionResult> ProcesarPago([FromBody] List<ItemCarritoDTO> carritoCliente)
        {
            // 1. CLÁUSULA DE GUARDA: Validamos que no nos manden un carrito vacío
            if (carritoCliente == null || carritoCliente.Count == 0)
            {
                return BadRequest("El carrito está vacío.");
            }

            // 2. IDENTIFICAR AL USUARIO: Extraemos el correo del token JWT del usuario que está comprando
            var correoUsuario = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoUsuario);
            if (usuario == null) return Unauthorized("Usuario no encontrado.");

            // 3. LA REGLA DE ORO: Recalcular el total confiando SOLO en la Base de Datos
            decimal totalReal = 0;
            var detallesNuevos = new List<Detalles>();

            foreach (var item in carritoCliente)
            {
                // Buscamos el postre real en la BD
                var postreDb = await _context.Postres.FindAsync(item.IdPostre);
                if (postreDb == null) return NotFound($"El postre con ID {item.IdPostre} ya no existe.");

                // Sumamos al total usando el precio de la BD, NO el que podría venir alterado de React
                totalReal += postreDb.PrecioActual * item.Cantidad;

                // Preparamos la línea del detalle (aún no tiene IdVenta porque no la hemos creado)
                detallesNuevos.Add(new Detalles
                {
                    IdPostre = postreDb.IdPostre,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = postreDb.PrecioActual
                });
            }

            // 4. CREAR LA CABECERA DE LA VENTA (El Ticket)
            var nuevaVenta = new Ventas
            {
                IdUsuario = usuario.IdUsuario,
                FechaVenta = DateTime.Now,
                Total = totalReal
            };

            // Guardamos la Venta en la BD para que SQL Server le asigne un ID automáticamente
            _context.Ventas.Add(nuevaVenta);
            await _context.SaveChangesAsync();

            // 5. ASIGNAR EL ID DE LA VENTA A LOS DETALLES Y GUARDARLOS
            foreach (var detalle in detallesNuevos)
            {
                detalle.IdVenta = nuevaVenta.IdVenta; // Ahora sí sabemos de qué ticket son
            }

            _context.DetallesVentas.AddRange(detallesNuevos);
            await _context.SaveChangesAsync(); // Guardamos todos los detalles de golpe

            // 6. ÉXITO
            return Ok(new { mensaje = "Pago procesado con éxito", totalCobrado = totalReal, idTicket = nuevaVenta.IdVenta });
        }
    }
}