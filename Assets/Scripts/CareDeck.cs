using System.Collections.Generic;
using UnityEngine;

// Clase que representa una entrada en el mazo de cuidado dental.
// Contiene un prefab de carta y un indicador de disponibilidad.
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

    // Funci�n que instancia hasta 5 cartas en la mesa desde las disponibles.
    public void SpawnCareCards()
    {
        int posIndex = 0; // �ndice para rastrear en qu� posici�n colocar la siguiente carta

        foreach (Transform pos in positions)
        {
            CareCardEntry cardEntry = GetRandomAvailableCard();
            if (cardEntry == null) break;

            GameObject instance = Instantiate(cardEntry.prefab, pos.position, Quaternion.identity);

            posIndex++;
            if (posIndex >= positions.Length) break;
        }
    }

    // Funci�n que busca aleatoriamente una carta disponible del mazo
    private CareCardEntry GetRandomAvailableCard()
    {
        List<CareCardEntry> available = careCards.FindAll(card => card.isAvailable);
        if (available.Count == 0) return null;

        int randomIndex = Random.Range(0, available.Count);
        return available[randomIndex];
    }

    // Funci�n que recibe el diccionario de datos para establecer que cartas de cuidados deben estar disponibles
    public void UpdateCareCardAvailabilityBasedOnToothState(Dictionary<string, ToothCardData> runtimeData)
    {
        bool hasCaries = false;
        bool hasFracture = false;

        // Recorre todos los datos del mazo para verificar caries o fractura
        foreach (ToothCardData data in runtimeData.Values)
        {
            if (data.state == 1 || data.state == 2)
                hasCaries = true;

            if (data.hasFracture)
                hasFracture = true;
        }

        // Establecer la disponibilidad de cada carta seg�n la posici�n y estado
        for (int i = 0; i < careCards.Count; i++)
        {
            if (i >= 0 && i <= 7)
            {
                careCards[i].isAvailable = true; // Siempre disponibles
            }
            else if (i >= 8 && i <= 10)
            {
                careCards[i].isAvailable = hasCaries; // Solo si hay caries
            }
            else if (i >= 11 && i <= 12)
            {
                careCards[i].isAvailable = hasFracture; // Solo si hay fractura
            }
        }
    }
}
