using System.Collections.Generic;
using System.Threading.Tasks;
using Pasteleria.DTOs;
namespace Pasteleria.Interfaces
{
    public interface IPostreService
    {
        Task<IEnumerable<PostreResponseDTO>> ObtenerTodosAsync();
    }
}
