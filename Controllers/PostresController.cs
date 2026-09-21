using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pasteleria.Data;
using Pasteleria.Models;
namespace Pasteleria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostresController : ControllerBase
    {
        private readonly PasteleriaContext _context;

        public PostresController(PasteleriaContext context)
        {
            _context = context;
        }

        [HttpGet] //Controlador decorado 
        public async Task<ActionResult<IEnumerable<Postres>>> GetPostres()
        {
            var postres = await _context.Postres.Include( p => p.Categorias).ToListAsync();
            return Ok(postres);
        }

        [Authorize] //Protección para evitar que el usuario cree un postre falso
        [HttpPost] //Más decoraciones para el PostresController
        public async Task<ActionResult<Postres>> PostPostre(Postres nuevoPostre)
        {
            // Verificamos si la categoría que nos enviaron realmente existe en SQL Server
            var categoriaExiste = await _context.Categorias.FindAsync(nuevoPostre.IdCategoria);

            if (categoriaExiste == null)
            {
                return BadRequest("Error: La categoría ingresada no existe.");
            }

            _context.Postres.Add(nuevoPostre);
            await _context.SaveChangesAsync();

            return Ok(nuevoPostre);
        }
    }
}
