using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Para LINQ

[ApiController]
[Route("api/[controller]")]
public class AutocompleteController : ControllerBase
{
    private readonly Trie _trie;
    // Cola de prioridad para ordenar las sugerencias (prioridad = -frecuencia)
    private readonly PriorityQueue<string, int> _priorityQueue;
    // Diccionario para llevar la frecuencia de búsqueda de cada palabra (clave: palabra normalizada)
    private readonly Dictionary<string, int> _wordFrequency;
    private readonly string dictionaryPath = Path.Combine(Directory.GetCurrentDirectory(), "diccionario.txt");

    public AutocompleteController(Trie trie)
    {
        _trie = trie;
        _priorityQueue = new PriorityQueue<string, int>();
        _wordFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    }

    // Devuelve las sugerencias ordenadas por frecuencia de búsqueda (mayor frecuencia = mayor prioridad)
    [HttpGet]
    public IActionResult GetSuggestions([FromQuery] string prefix)
    {
        var suggestions = _trie.GetWordsWithPrefix(prefix.ToLower());
        var prioritizedSuggestions = new List<string>();

        // Vaciar la cola de prioridad para recalcularla
        while (_priorityQueue.Count > 0)
            _priorityQueue.Dequeue();

        // Encolar cada palabra con prioridad = -frecuencia (si no existe, se usa 0)
        foreach (var word in suggestions)
        {
            int priority = GetWordPriority(word); // prioridad = -frecuencia
            _priorityQueue.Enqueue(word, priority);
        }

        // Extraer las palabras de la cola en orden
        while (_priorityQueue.Count > 0)
        {
            prioritizedSuggestions.Add(_priorityQueue.Dequeue());
        }

        return Ok(prioritizedSuggestions);
    }

    [HttpPost]
    public IActionResult AddWord([FromBody] string word)
    {
        string normalized = _trie.NormalizeWord(word);
        _trie.Insert(normalized);

        // Asegurarse de tener un registro de frecuencia para la palabra (inicialmente 0)
        if (!_wordFrequency.ContainsKey(normalized))
            _wordFrequency[normalized] = 0;

        // Agregar la palabra al archivo si no existe ya (comparación ignorando mayúsculas)
        var existingWords = new HashSet<string>(System.IO.File.ReadAllLines(dictionaryPath), StringComparer.OrdinalIgnoreCase);
        if (!existingWords.Contains(normalized))
        {
            System.IO.File.AppendAllText(dictionaryPath, normalized + Environment.NewLine);
        }

        return Ok($"Palabra '{normalized}' añadida correctamente.");
    }

    [HttpDelete]
    public IActionResult DeleteWord([FromQuery] string word)
    {
        string normalized = _trie.NormalizeWord(word);
        bool removed = _trie.Delete(normalized);

        if (!removed)
            return NotFound($"La palabra '{normalized}' no se encontró.");

        // Reescribir el archivo sin la palabra eliminada
        var updated = System.IO.File.ReadAllLines(dictionaryPath)
            .Where(line => !string.Equals(line.Trim(), normalized, StringComparison.OrdinalIgnoreCase));
        System.IO.File.WriteAllLines(dictionaryPath, updated);

        // Eliminar la palabra del diccionario de frecuencia
        if (_wordFrequency.ContainsKey(normalized))
            _wordFrequency.Remove(normalized);

        return Ok($"Palabra '{normalized}' eliminada correctamente.");
    }

    // Endpoint para actualizar la frecuencia de búsqueda de una palabra (por ejemplo, al seleccionarla)
    [HttpPost("updateFrequency")]
    public IActionResult UpdateFrequency([FromBody] string word)
    {
        string normalized = _trie.NormalizeWord(word);
        if (_wordFrequency.ContainsKey(normalized))
        {
            _wordFrequency[normalized]++;
        }
        else
        {
            _wordFrequency[normalized] = 1;
        }
        return Ok($"Frecuencia de '{normalized}' actualizada a {_wordFrequency[normalized]}.");
    }

    [HttpGet("trie")]
    public IActionResult GetTrie()
    {
        var trieStructure = _trie.GetTrieStructure();
        return Ok(trieStructure);
    }

    // Devuelve la prioridad basada en la frecuencia: si la palabra se ha buscado 'freq' veces, 
    // la prioridad será -freq (para que mayor frecuencia aparezca primero)
    private int GetWordPriority(string word)
    {
        return _wordFrequency.TryGetValue(word, out int freq) ? -freq : 0;
    }
}
