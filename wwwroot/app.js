const apiUrl = 'http://localhost:5000/api/autocomplete';

// Función para obtener las sugerencias de palabras
function getSuggestions() {
    const prefix = document.getElementById('prefix').value;
    if (prefix.length > 0) {
        fetch(`${apiUrl}?prefix=${prefix}`)
            .then(response => response.json())
            .then(suggestions => {
                const suggestionsDiv = document.getElementById('suggestions');
                suggestionsDiv.innerHTML = '';  // Limpiar las sugerencias anteriores

                // Mostrar las nuevas sugerencias
                suggestions.forEach(word => {
                    const div = document.createElement('div');
                    div.classList.add('suggestion-item');
                    // Si solo se retorna la palabra, se muestra así:
                    div.textContent = word;

                    // Agregar evento de clic para completar la palabra en el input
                    div.addEventListener('click', function() {
                        document.getElementById('prefix').value = word;  // Completar el campo con la palabra seleccionada
                        document.getElementById('suggestions').innerHTML = '';  // Limpiar las sugerencias

                        // Llamar al endpoint para actualizar la frecuencia
                        updateFrequency(word);
                    });

                    suggestionsDiv.appendChild(div);
                });
            })
            .catch(error => console.error('Error al obtener sugerencias:', error));
    } else {
        document.getElementById('suggestions').innerHTML = '';  // Limpiar las sugerencias cuando el campo está vacío
    }
}

// Función para llamar al endpoint updateFrequency
function updateFrequency(word) {
    fetch(`${apiUrl}/updateFrequency`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(word)
    })
    .then(response => response.text())
    .then(data => {
        console.log(data); // Puedes mostrar o registrar el mensaje de éxito
        // Opcional: actualizar las sugerencias para reflejar la nueva frecuencia
        getSuggestions();
    })
    .catch(error => {
        console.error('Error al actualizar la frecuencia:', error);
    });
}

// Función para mostrar el mensaje
function showMessage(message, isSuccess) {
    const messageElement = document.getElementById('message');
    messageElement.textContent = message;
    messageElement.style.display = 'block';
    messageElement.style.backgroundColor = isSuccess ? '#d4edda' : '#f8d7da';
    messageElement.style.color = isSuccess ? '#155724' : '#721c24';
    messageElement.style.border = isSuccess ? '1px solid #c3e6cb' : '1px solid #f5c6cb';
    setTimeout(() => {
        messageElement.style.display = 'none';
    }, 1500);
}

// Función para agregar una nueva palabra
function addWord() {
    const newWord = document.getElementById('newWord').value;
    if (newWord) {
        fetch(apiUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(newWord)
        })
        .then(response => response.text())
        .then(data => {
            showMessage("Palabra agregada correctamente", true);
            document.getElementById('newWord').value = '';
            getSuggestions();
        })
        .catch(error => {
            console.error('Error al agregar la palabra:', error);
            showMessage("Error al agregar la palabra", false);
        });
    }
}

// Función para eliminar una palabra
function deleteWord() {
    const wordToDelete = document.getElementById('deleteWord').value;
    if (wordToDelete) {
        fetch(`${apiUrl}?word=${wordToDelete}`, {
            method: 'DELETE'
        })
        .then(response => response.text())
        .then(data => {
            showMessage("Palabra eliminada correctamente", true);
            document.getElementById('deleteWord').value = '';
            getSuggestions();
        })
        .catch(error => {
            console.error('Error al eliminar la palabra:', error);
            showMessage("Error al eliminar la palabra", false);
        });
    }
}
