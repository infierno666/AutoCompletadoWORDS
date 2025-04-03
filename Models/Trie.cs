using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Linq;

public class Trie
{
    // Método para eliminar una palabra del Trie
    public bool Delete(string word)
    {
        return Delete(root, word, 0);
    }

    private bool Delete(TrieNode node, string word, int depth)
    {
        if (node == null)
            return false;

        if (depth == word.Length)
        {
            if (!node.IsEnd)
                return false;

            node.IsEnd = false;
            return node.Children.All(child => child == null);
        }

        int index = word[depth] - 'a';
        if (Delete(node.Children[index], word, depth + 1))
        {
            node.Children[index] = null;
            return !node.IsEnd && node.Children.All(child => child == null);
        }

        return false;
    }

    // Clase interna para los nodos del Trie
    private class TrieNode
    {
        public bool IsEnd = false;
        public TrieNode[] Children = new TrieNode[26]; // Considerando solo letras de la a-z
    }

    private readonly TrieNode root = new TrieNode();

    // Insertar una palabra en el Trie
    public void Insert(string word)
    {
        word = NormalizeWord(word); // Normalización agregada

        var node = root;
        foreach (var c in word)
        {
            if (c < 'a' || c > 'z') continue; // Validar que esté entre a-z

            int index = c - 'a';
            if (node.Children[index] == null)
                node.Children[index] = new TrieNode();
            node = node.Children[index];
        }
        node.IsEnd = true;
    }

    // Obtener palabras que comienzan con un prefijo
    public List<string> GetWordsWithPrefix(string prefix)
    {
        prefix = NormalizeWord(prefix); // Normalización agregada

        var result = new List<string>();
        var node = root;
        foreach (var c in prefix)
        {
            if (c < 'a' || c > 'z') return result;

            int index = c - 'a';
            if (node.Children[index] == null)
                return result;
            node = node.Children[index];
        }
        DFS(node, prefix, result);
        return result;
    }

    // Función recursiva para realizar la búsqueda DFS
    private void DFS(TrieNode node, string prefix, List<string> result)
    {
        if (node.IsEnd)
            result.Add(prefix);

        for (int i = 0; i < 26; i++)
        {
            if (node.Children[i] != null)
                DFS(node.Children[i], prefix + (char)(i + 'a'), result);
        }
    }

    // Función para normalizar palabras
    public string NormalizeWord(string input)
    {
        input = input.ToLowerInvariant();
        string normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark && Char.IsLetter(c))
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    // *** NUEVA PARTE PARA LA VISUALIZACIÓN DEL TRIE ***
    // Método público para obtener la estructura del Trie en formato JSON
    public Dictionary<string, object> GetTrieStructure()
    {
        return GetTrieStructure(root);
    }

    private Dictionary<string, object> GetTrieStructure(TrieNode node)
    {
        var result = new Dictionary<string, object>();

        for (int i = 0; i < 26; i++)
        {
            if (node.Children[i] != null)
            {
                char c = (char)('a' + i);
                result[c.ToString()] = GetTrieStructure(node.Children[i]);
            }
        }

        if (node.IsEnd)
        {
            result["end"] = true;
        }

        return result;
    }

    // Función recursiva que convierte el Trie a un árbol de objetos con "name" y "children"
    private object BuildD3Tree(TrieNode node, string value)
    {
        var result = new Dictionary<string, object>();
        result["name"] = value;
        var children = new List<object>();
        for (int i = 0; i < 26; i++)
        {
            if (node.Children[i] != null)
            {
                char letter = (char)(i + 'a');
                children.Add(BuildD3Tree(node.Children[i], letter.ToString()));
            }
        }
        result["children"] = children;
        return result;
    }

}
