using Microsoft.EntityFrameworkCore;
using Pasteleria.Data;
using Pasteleria.DTOs;
using Pasteleria.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Pasteleria.Services
{
    public class PostreService: IPostreService
    {
        private readonly PasteleriaContext _context;
        public PostreService(PasteleriaContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostreResponseDTO>> ObtenerTodosAsync()
        {
            return await _context.Postres
                .AsNoTracking()// Optimización nivel Mid-level para lecturas. le dice a Entity Framework que no necesita rastrear estos objetos en memoria para posibles actualizaciones, ahorrando recursos valiosos en consultas de solo lectura.
                .Select(p => new PostreResponseDTO
                {
                    // Mapea las propiedades de tu modelo Postres a tu DTO
                    Id = p.IdPostre,
                    Nombre = p.Postre,
                    Precio = p.PrecioActual
                }).ToListAsync();

        }
    }
}
