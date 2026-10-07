using System.Data;
using CommerceApi.Models;
using Dapper;
using Microsoft.Data.SqlClient;

namespace CommerceApi.Data;

public interface ICommerceRepository
{
    Task<int> InsertAsync(string fileName, IReadOnlyCollection<CommerceRow> rows);
    Task<int> ProcessAsync(DateTime processDate);
    Task<IEnumerable<CommerceQuarantineRow>> GetQuarantineAsync(DateTime? processDate);
}

public class CommerceRepository : ICommerceRepository
{
    private readonly string _connectionString;

    public CommerceRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("CommerceDb")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'CommerceDb'.");
    }

    public async Task<int> InsertAsync(string fileName, IReadOnlyCollection<CommerceRow> rows)
    {
        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        var count = 0;
        try
        {
            // Registrar el archivo. Si ya existe, la restricción UNIQUE lo rechaza.
            try
            {
                await conn.ExecuteAsync(
                    "INSERT INTO dbo.commerce_file_log (file_name, total_rows) VALUES (@fileName, @total)",
                    new { fileName, total = rows.Count },
                    tx);
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                throw new DuplicateFileException(fileName);
            }

            foreach (var r in rows)
            {
                await conn.ExecuteAsync(
                    "dbo.sp_create_commerce",
                    new
                    {
                        pc_codcomercio = r.PcCodComercio,
                        pc_nomcomred = r.PcNomComRed,
                        pc_razonsocial = r.PcRazonSocial,
                        pc_tipdoc = r.PcTipDoc,
                        pc_numdoc = r.PcNumDoc,
                        pc_direccion = r.PcDireccion,
                        pc_telefono = r.PcTelefono,
                        pc_email = r.PcEmail,
                        pc_processdate = r.PcProcessDate
                    },
                    tx,
                    commandType: CommandType.StoredProcedure);
                count++;
            }

            await tx.CommitAsync();
            return count;
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<int> ProcessAsync(DateTime processDate)
    {
        await using var conn = new SqlConnection(_connectionString);
        return await conn.QuerySingleAsync<int>(
            "dbo.sp_process_commerce",
            new { processdate = processDate },
            commandType: CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<CommerceQuarantineRow>> GetQuarantineAsync(DateTime? processDate)
    {
        const string sql = @"
            SELECT id             AS Id,
                   pc_codcomercio AS PcCodComercio,
                   pc_nomcomred   AS PcNomComRed,
                   pc_razonsocial AS PcRazonSocial,
                   pc_tipdoc      AS PcTipDoc,
                   pc_numdoc      AS PcNumDoc,
                   pc_direccion   AS PcDireccion,
                   pc_telefono    AS PcTelefono,
                   pc_email       AS PcEmail,
                   pc_processdate AS PcProcessDate,
                   motivo         AS Motivo
            FROM dbo.commerce_quarantine
            WHERE (@processDate IS NULL OR pc_processdate = @processDate)
            ORDER BY id DESC;";

        await using var conn = new SqlConnection(_connectionString);
        return await conn.QueryAsync<CommerceQuarantineRow>(sql, new { processDate });
    }
}