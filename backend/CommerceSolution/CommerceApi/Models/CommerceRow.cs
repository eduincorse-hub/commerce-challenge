using CsvHelper.Configuration.Attributes;

namespace CommerceApi.Models;

public class CommerceRow
{
    [Name("pc_codcomercio")] public string? PcCodComercio { get; set; }
    [Name("pc_nomcomred")] public string? PcNomComRed { get; set; }
    [Name("pc_razonsocial")] public string? PcRazonSocial { get; set; }
    [Name("pc_tipdoc")] public string? PcTipDoc { get; set; }
    [Name("pc_numdoc")] public string? PcNumDoc { get; set; }
    [Name("pc_direccion")] public string? PcDireccion { get; set; }
    [Name("pc_telefono")] public string? PcTelefono { get; set; }
    [Name("pc_email")] public string? PcEmail { get; set; }

    [Name("pc_processdate")]
    [Format("yyyy-MM-dd")]
    public DateTime PcProcessDate { get; set; }
}