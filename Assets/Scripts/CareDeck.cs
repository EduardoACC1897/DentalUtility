using System.Collections.Generic;
using UnityEngine;

// Clase serializable que representa una entrada en el mazo de cuidado dental.
// Contiene un prefab (el objeto que se va a instanciar) y un booleano que indica si est� disponible.
[System.Serializable]
public class CareCardEntry
{
    public GameObject prefab;     // Prefab de la carta de cuidado dental
    public bool isAvailable;      // Indica si esta carta est� disponible para ser usada
}

public class CareDeck : MonoBehaviour
{
    public List<CareCardEntry> careCards; // Lista de cartas disponibles (pool)
    public Transform[] positions;         // Posiciones sobre la mesa donde se colocar�n las cartas

    // Funci�n que coloca hasta 5 cartas en la mesa desde las disponibles.
    public void SpawnCareCards()
    {
        int posIndex = 0; // �ndice para rastrear en qu� posici�n colocar la siguiente carta

        foreach (Transform pos in positions)
        {
            // Obtener una carta aleatoria que est� disponible
            CareCardEntry cardEntry = GetRandomAvailableCard();
            if (cardEntry == null) break; // Si no hay m�s cartas disponibles, se detiene

            // Instanciar una copia del prefab en la posici�n correspondiente
            GameObject instance = Instantiate(cardEntry.prefab, pos.position, Quaternion.identity);

            posIndex++;
            // Si ya se llenaron todas las posiciones, se detiene
            if (posIndex >= positions.Length) break;
        }
    }

    // Funci�n que busca aleatoriamente una carta disponible del mazo
    private CareCardEntry GetRandomAvailableCard()
    {
        // Filtra la lista para obtener solo las cartas disponibles
        List<CareCardEntry> available = careCards.FindAll(card => card.isAvailable);
        if (available.Count == 0) return null; // Si no hay cartas disponibles, retorna null

        // Selecciona una carta aleatoria del pool disponible
        int randomIndex = Random.Range(0, available.Count);
        return available[randomIndex];
    }
}
