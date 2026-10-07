namespace CommerceApi.Models;

public class CommerceQuarantineRow : CommerceRow
{
    public int Id { get; set; }
    public string Motivo { get; set; } = string.Empty;
}