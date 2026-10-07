using CommerceApi.Models;

namespace CommerceApi.Services;

/// <summary>Casos de uso de la carga y el procesamiento de comercios.</summary>
public interface ICommerceService
{
    /// <summary>Valida el archivo CSV y registra sus filas en la tabla commerce.</summary>
    /// <returns>Cantidad de registros insertados.</returns>
    /// <exception cref="InvalidFileException">El archivo está vacío, el nombre es incorrecto o no se puede leer.</exception>
    /// <exception cref="DuplicateFileException">El archivo ya fue cargado antes.</exception>
    Task<int> UploadAsync(IFormFile file);

    /// <summary>Valida los registros de la fecha indicada y mueve los inválidos a cuarentena.</summary>
    /// <returns>Cantidad de registros enviados a commerce_quarantine.</returns>
    Task<int> ProcessAsync(DateTime processDate);

    /// <summary>Lista los registros en cuarentena, opcionalmente filtrados por fecha de proceso.</summary>
    Task<IEnumerable<CommerceQuarantineRow>> GetQuarantineAsync(DateTime? processDate);
}