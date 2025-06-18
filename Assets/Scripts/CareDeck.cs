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
            CareCardEntry cardEntry = GetRandomAvailableCard();
            if (cardEntry == null) break;

            GameObject instance = Instantiate(cardEntry.prefab, pos.position, Quaternion.identity);

            posIndex++;
            if (posIndex >= positions.Length) break;
        }
    }

    // Función que busca aleatoriamente una carta disponible del mazo
    private CareCardEntry GetRandomAvailableCard()
    {
        List<CareCardEntry> available = careCards.FindAll(card => card.isAvailable);
        if (available.Count == 0) return null;

        int randomIndex = Random.Range(0, available.Count);
        return available[randomIndex];
    }

    // ACTUALIZADO: Recibe el diccionario de datos modificables en tiempo real
    public void UpdateCareCardAvailabilityBasedOnToothState(Dictionary<string, ToothCardData> runtimeData)
    {
        bool hasCaries = false;
        bool hasFracture = false;

        foreach (ToothCardData data in runtimeData.Values)
        {
            if (data.state == 2 || data.state == 3)
                hasCaries = true;

            if (data.state == 4)
                hasFracture = true;
        }

        for (int i = 0; i < careCards.Count; i++)
        {
            if (i >= 0 && i <= 7)
            {
                careCards[i].isAvailable = true;
            }
            else if (i >= 8 && i <= 10)
            {
                careCards[i].isAvailable = hasCaries;
            }
            else if (i >= 11 && i <= 12)
            {
                careCards[i].isAvailable = hasFracture;
            }
        }
    }
}
