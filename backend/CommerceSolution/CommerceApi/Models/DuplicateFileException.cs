namespace CommerceApi.Models;

/// <summary>Se lanza cuando el archivo ya fue cargado anteriormente.</summary>
public class DuplicateFileException : Exception
{
    public DuplicateFileException(string fileName)
        : base($"El archivo '{fileName}' ya fue cargado anteriormente.") { }
}