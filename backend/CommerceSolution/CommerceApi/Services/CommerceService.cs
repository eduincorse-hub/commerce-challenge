using System.Globalization;
using System.Text.RegularExpressions;
using CommerceApi.Data;
using CommerceApi.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace CommerceApi.Services;

/// <inheritdoc />
public class CommerceService : ICommerceService
{
    // Formato esperado del nombre: commerce_DDMMYYYY.csv
    private static readonly Regex FileNameRegex =
        new(@"^commerce_\d{8}\.csv$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly ICommerceRepository _repository;

    public CommerceService(ICommerceRepository repository) => _repository = repository;

    /// <inheritdoc />
    public async Task<int> UploadAsync(IFormFile file)
    {
        if (file is null || file.Length == 0)
            throw new InvalidFileException("El archivo está vacío.");

        if (!FileNameRegex.IsMatch(file.FileName))
            throw new InvalidFileException("El nombre del archivo debe ser commerce_DDMMYYYY.csv.");

        var rows = ReadCsv(file);
        if (rows.Count == 0)
            throw new InvalidFileException("El archivo no contiene registros.");

        return await _repository.InsertAsync(file.FileName, rows);
    }

    /// <inheritdoc />
    public Task<int> ProcessAsync(DateTime processDate) =>
        _repository.ProcessAsync(processDate);

    /// <inheritdoc />
    public Task<IEnumerable<CommerceQuarantineRow>> GetQuarantineAsync(DateTime? processDate) =>
        _repository.GetQuarantineAsync(processDate);

    /// <summary>Lee el CSV completo y lo convierte en una lista de filas.</summary>
    private static List<CommerceRow> ReadCsv(IFormFile file)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            HeaderValidated = null
        };

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, config);
            return csv.GetRecords<CommerceRow>().ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidFileException("No se pudo leer el CSV: " + ex.Message);
        }
    }
}