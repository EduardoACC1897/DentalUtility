using System.Collections.Generic;
using UnityEngine;

// Clase que representa una entrada en el mazo de cuidado dental.
// Contiene un prefab de carta y un indicador de disponibilidad.
[System.Serializable]
public class CareCardEntry
{
    public GameObject prefab;     // Prefab de la carta de cuidado dental
    public bool isAvailable;      // Indica si esta carta está disponible para ser usada
}

public class CareDeck : MonoBehaviour
{
    public List<CareCardEntry> careCards; // Lista de cartas disponibles (pool)
    public Transform[] positions;         // Posiciones sobre la mesa donde se colocarán las cartas

    // Función que instancia hasta 5 cartas en la mesa desde las disponibles.
    public void SpawnCareCards()
    {
        int posIndex = 0; // Índice para rastrear en qué posición colocar la siguiente carta

        foreach (Transform pos in positions)
        {
            // Obtener una carta aleatoria que esté disponible
            CareCardEntry cardEntry = GetRandomAvailableCard();
            if (cardEntry == null) break; // Si no hay más cartas disponibles, se detiene

            // Instanciar una copia del prefab en la posición correspondiente
            GameObject instance = Instantiate(cardEntry.prefab, pos.position, Quaternion.identity);

            posIndex++;
            // Si ya se llenaron todas las posiciones, se detiene
            if (posIndex >= positions.Length) break;
        }
    }

    // Función que busca aleatoriamente una carta disponible del mazo
    private CareCardEntry GetRandomAvailableCard()
    {
        // Filtra la lista para obtener solo las cartas disponibles
        List<CareCardEntry> available = careCards.FindAll(card => card.isAvailable);
        if (available.Count == 0) return null; // Si no hay cartas disponibles, retorna null

        // Selecciona una carta aleatoria del pool disponible
        int randomIndex = Random.Range(0, available.Count);
        return available[randomIndex];
    }

    // Función que actualiza la disponibilidad de cartas de cuidado dental
    // según si hay dientes con caries (state = 2 o 3) o fractura (state = 4) en la lista de datos de dientes.
    public void UpdateCareCardAvailabilityBasedOnToothState(List<ToothCardData> toothCardDataList)
    {
        bool hasCaries = false;
        bool hasFracture = false;

        // Buscar si existe alguna carta de diente con caries o fractura
        foreach (ToothCardData data in toothCardDataList)
        {
            if (data.state == 2 || data.state == 3)
                hasCaries = true;

            if (data.state == 4)
                hasFracture = true;
        }

        // Actualizar la disponibilidad según las condiciones
        for (int i = 0; i < careCards.Count; i++)
        {
            if (i >= 0 && i <= 7)
            {
                // Siempre disponibles
                careCards[i].isAvailable = true;
            }
            else if (i >= 8 && i <= 10)
            {
                // Disponibles solo si hay caries
                careCards[i].isAvailable = hasCaries;
            }
            else if (i >= 11 && i <= 12)
            {
                // Disponibles solo si hay fractura
                careCards[i].isAvailable = hasFracture;
            }
        }
    }

}
