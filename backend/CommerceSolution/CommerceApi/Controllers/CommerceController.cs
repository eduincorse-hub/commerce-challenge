using CommerceApi.Models;
using CommerceApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CommerceApi.Controllers;

/// <summary>Endpoints para cargar, procesar y consultar comercios.</summary>
[ApiController]
[Route("api/[controller]")]
public class CommerceController : ControllerBase
{
    private readonly ICommerceService _service;

    public CommerceController(ICommerceService service) => _service = service;

    /// <summary>Carga un archivo commerce_DDMMYYYY.csv en la tabla commerce.</summary>
    /// <param name="file">Archivo CSV con los comercios.</param>
    /// <response code="200">Archivo cargado. Devuelve la cantidad de registros insertados.</response>
    /// <response code="400">El archivo está vacío, el nombre es incorrecto o no se puede leer.</response>
    /// <response code="409">El archivo ya fue cargado anteriormente.</response>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            var inserted = await _service.UploadAsync(file);
            return Ok(new { registrosInsertados = inserted });
        }
        catch (InvalidFileException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (DuplicateFileException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Valida los registros de una fecha y envía los inválidos a cuarentena.</summary>
    /// <param name="processDate">Fecha de proceso (columna pc_processdate).</param>
    /// <response code="200">Devuelve la cantidad de registros enviados a cuarentena.</response>
    [HttpPost("process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Process([FromQuery] DateTime processDate)
    {
        var count = await _service.ProcessAsync(processDate);
        return Ok(new { registrosCuarentena = count });
    }

    /// <summary>Lista los registros en cuarentena con el motivo del rechazo.</summary>
    /// <param name="processDate">Opcional. Filtra por fecha de proceso.</param>
    /// <response code="200">Lista de registros en cuarentena.</response>
    [HttpGet("quarantine")]
    [ProducesResponseType(typeof(IEnumerable<CommerceQuarantineRow>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuarantine([FromQuery] DateTime? processDate)
    {
        var data = await _service.GetQuarantineAsync(processDate);
        return Ok(data);
    }
}