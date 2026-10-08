namespace CommerceApi.Exceptions;

/// <summary>Se lanza cuando el archivo recibido no cumple las reglas de validación.</summary>
public class InvalidFileException : Exception
{
    public InvalidFileException(string message) : base(message) { }
}