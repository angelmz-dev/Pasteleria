using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pasteleria.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class PostresController : ControllerBase
{
    private readonly IPostreService _postreService;

    // Inyectamos la interfaz, NO la implementación ni el contexto de BD
    public PostresController(IPostreService postreService)
    {
        _postreService = postreService;
    }

    [HttpGet("{id}")] //importante especificar la ruta para evitar colisiones 
    public async Task<IActionResult> GetPostres()
    {
        var postres = await _postreService.ObtenerTodosAsync();

        if (!postres.Any())
            return NotFound(new { mensaje = "No se encontraron postres registrados." });

        return Ok(postres);
    }
}