using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios necesarios
builder.Services.AddCors();
builder.Services.AddSingleton<Trie>();
builder.Services.AddControllers();

var app = builder.Build();

// Cargar el diccionario
LoadDictionary(app.Services.GetRequiredService<Trie>());

// CORS
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Rutas de controladores (más moderno)
app.MapControllers();

// Archivos estáticos (si los usas)
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();

// Cargar el diccionario desde el archivo
void LoadDictionary(Trie trie)
{
    var path = Path.Combine(Directory.GetCurrentDirectory(), "diccionario.txt");

    if (File.Exists(path))
    {
        foreach (var line in File.ReadLines(path))
        {
            trie.Insert(line.Trim());
        }
    }
    else
    {
        Console.WriteLine("El archivo de diccionario no se encontró.");
    }
}
    