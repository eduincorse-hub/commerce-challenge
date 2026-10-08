using System.Text;
using CommerceApi.Data;
using CommerceApi.Exceptions;
using CommerceApi.Models;
using CommerceApi.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace CommerceApi.Tests;

/// <summary>Pruebas de las validaciones de carga en CommerceService.</summary>
public class CommerceServiceTests
{
    // Encabezado del CSV, igual al del archivo de ejemplo en samples/
    private const string Encabezado =
        "pc_codcomercio,pc_nomcomred,pc_razonsocial,pc_tipdoc,pc_numdoc," +
        "pc_direccion,pc_telefono,pc_email,pc_processdate";

    [Fact]
    public async Task UploadAsync_ArchivoVacio_LanzaInvalidFileException()
    {
        // Arrange: repositorio falso, servicio real y un archivo de 0 bytes
        var repoFalso = new Mock<ICommerceRepository>();
        var service = new CommerceService(repoFalso.Object);
        var archivo = CrearArchivo("commerce_07102026.csv", "");

        // Act + Assert: subir el archivo debe lanzar InvalidFileException
        await Assert.ThrowsAsync<InvalidFileException>(
            () => service.UploadAsync(archivo));

        // Assert: nunca debió intentar guardar en la base de datos
        repoFalso.Verify(
            r => r.InsertAsync(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<CommerceRow>>()),
            Times.Never);
    }

    [Theory]
    [InlineData("datos.csv")]               // no empieza con commerce_
    [InlineData("commerce_2026.csv")]       // fecha con menos de 8 dígitos
    [InlineData("commerce_07102026.txt")]   // extensión distinta de .csv
    public async Task UploadAsync_NombreIncorrecto_LanzaInvalidFileException(string nombre)
    {
        // Arrange: contenido válido, para que el único error sea el nombre
        var repoFalso = new Mock<ICommerceRepository>();
        var service = new CommerceService(repoFalso.Object);
        var archivo = CrearArchivo(nombre, Encabezado + "\nC101,Tienda,Tienda SA,RUC,123,Calle 1,022,a@b.com,2026-10-07");

        // Act + Assert
        await Assert.ThrowsAsync<InvalidFileException>(
            () => service.UploadAsync(archivo));
    }

    [Fact]
    public async Task UploadAsync_SoloEncabezado_LanzaInvalidFileException()
    {
        // Arrange: nombre válido y solo la línea de encabezado
        var repoFalso = new Mock<ICommerceRepository>();
        var service = new CommerceService(repoFalso.Object);
        var archivo = CrearArchivo("commerce_07102026.csv", Encabezado);

        // Act + Assert
        await Assert.ThrowsAsync<InvalidFileException>(
            () => service.UploadAsync(archivo));
    }

    [Fact]
    public async Task UploadAsync_CsvValido_InsertaFilasYDevuelveCantidad()
    {
        // Arrange: CSV con 2 filas; la segunda tiene el nombre vacío a propósito
        var contenido = Encabezado + "\n" +
            "C101,Tienda Sol,Tienda Sol SA,RUC,1790012345001,Av. Central 123,022345678,sol@mail.com,2026-10-07\n" +
            "C102,,Comercial Luna,RUC,1790099999001,Calle 2,022111111,luna@mail.com,2026-10-07";
        var archivo = CrearArchivo("commerce_07102026.csv", contenido);

        var repoFalso = new Mock<ICommerceRepository>();
        repoFalso
            .Setup(r => r.InsertAsync(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<CommerceRow>>()))
            .ReturnsAsync(2);
        var service = new CommerceService(repoFalso.Object);

        // Act
        var resultado = await service.UploadAsync(archivo);

        // Assert 1: devuelve lo que respondió el repositorio
        Assert.Equal(2, resultado);

        // Assert 2: llamó al repositorio una sola vez, con el nombre del archivo
        // y las 2 filas leídas correctamente del CSV
        repoFalso.Verify(r => r.InsertAsync(
            "commerce_07102026.csv",
            It.Is<IReadOnlyCollection<CommerceRow>>(filas =>
                filas.Count == 2 &&
                filas.First().PcNomComRed == "Tienda Sol" &&
                filas.First().PcProcessDate == new DateTime(2026, 10, 7))),
            Times.Once);
    }

    /// <summary>Crea un IFormFile en memoria, como si viniera del navegador.</summary>
    private static IFormFile CrearArchivo(string nombre, string contenido)
    {
        var bytes = Encoding.UTF8.GetBytes(contenido);
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", nombre);
    }
}