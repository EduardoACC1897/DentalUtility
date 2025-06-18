using System.Collections.Generic;
using UnityEngine;

// Clase que representa una entrada en el mazo de dientes.
// Contiene un prefab de carta y un indicador de disponibilidad.
[System.Serializable]
public class ToothCardEntry
{
    public GameObject prefab;     // Prefab del diente que se puede instanciar
    public bool isAvailable;      // Si la carta est� disponible para usarse en la ronda actual
}

public class ToothDeck : MonoBehaviour
{
    public List<ToothCardEntry> toothCards;             // Lista de cartas de dientes (prefabs + disponibilidad)
    public List<ToothCardData> toothCardDataList;       // Lista de datos base (ScriptableObjects)
    public Dictionary<string, ToothCardData> runtimeData = new Dictionary<string, ToothCardData>(); // Copias de los datos en runtime
    public Transform[] positions;                       // Posiciones sobre la mesa donde aparecer�n las cartas

    void Start()
    {
        // Crear copias de todos los datos originales para uso en tiempo de ejecuci�n
        foreach (ToothCardData original in toothCardDataList)
        {
            runtimeData[original.cardID] = original.Clone();
        }
    }

    // Funci�n para obtener la cantidad de cartas disponibles en la lista
    public int GetAvailableToothCardsCount()
    {
        return toothCards.FindAll(card => card.isAvailable).Count;
    }

    // Funci�n que instancia hasta 5 cartas de diente en las posiciones definidas
    public void SpawnToothCards()
    {
        int posIndex = 0; // �ndice para recorrer las posiciones

        // Itera sobre cada posici�n disponible
        foreach (Transform pos in positions)
        {
            ToothCardEntry cardEntry = GetRandomAvailableCard();
            if (cardEntry == null) break;

            GameObject instance = Instantiate(cardEntry.prefab, pos.position, Quaternion.identity);
            cardEntry.isAvailable = false;

            ToothCard card = instance.GetComponent<ToothCard>();
            if (card != null && runtimeData.TryGetValue(card.cardID, out ToothCardData data))
            {
                card.LoadData(data);
            }

            GameController gc = Object.FindFirstObjectByType<GameController>();
            if (gc != null)
                gc.RegisterToothCardCreated();

            posIndex++;
            if (posIndex >= positions.Length) break;
        }
        Debug.Log("se crearon las cartas");
    }

    // Funci�n que devuelve una carta aleatoria que est� disponible
    private ToothCardEntry GetRandomAvailableCard()
    {
        List<ToothCardEntry> available = toothCards.FindAll(card => card.isAvailable);
        if (available.Count == 0) return null;

        int randomIndex = Random.Range(0, available.Count);
        return available[randomIndex];
    }

    // Funci�n que restablece la disponibilidad de todas las cartas del mazo de dientes
    public void ResetCardAvailability()
    {
        foreach (ToothCardEntry card in toothCards)
        {
            card.isAvailable = true;
        }
    }

    // Funci�n que marca como disponibles �nicamente aquellas cartas de diente cuyos datos han cambiado
    // (dirtValue != 0, toothPH != 100 o state != 0)
    public void SetAvailabilityForModifiedCardsOnly()
    {
        foreach (KeyValuePair<string, ToothCardData> pair in runtimeData)
        {
            string id = pair.Key;
            ToothCardData data = pair.Value;

            bool isModified = data.dirtValue != 0 || data.toothPH != 100 || data.state != 0;

            foreach (ToothCardEntry card in toothCards)
            {
                ToothCard cardComponent = card.prefab.GetComponent<ToothCard>();
                if (cardComponent != null && cardComponent.cardID == id)
                {
                    card.isAvailable = isModified;
                    break;
                }
            }
        }
    }

    // Funci�n que reemplaza todas las cartas de diente activas por nuevas del mazo
    public void ReplaceAllToothCards()
    {
        ToothCard[] existingCards = Object.FindObjectsByType<ToothCard>(FindObjectsSortMode.None);
        if (existingCards.Length == 0)
        {
            Debug.Log("No hay cartas de diente activas para reemplazar.");
            return;
        }

        int availableCount = toothCards.FindAll(c => c.isAvailable).Count;
        if (availableCount == 0)
        {
            Debug.Log("No hay cartas disponibles en el mazo para generar nuevas cartas.");
            return;
        }

        HashSet<string> idsToRestore = new HashSet<string>();
        foreach (ToothCard card in existingCards)
        {
            idsToRestore.Add(card.cardID);
            Destroy(card.gameObject);
        }

        SpawnToothCards();

        GameController gc = Object.FindFirstObjectByType<GameController>();

        foreach (ToothCardEntry entry in toothCards)
        {
            ToothCard cardComponent = entry.prefab.GetComponent<ToothCard>();
            if (cardComponent != null && idsToRestore.Contains(cardComponent.cardID))
            {
                entry.isAvailable = true;

                if (gc != null)
                {
                    gc.UnregisterToothCardCreated();
                }
            }
        }
    }

    // Funci�n para llamar desde un bot�n UI y reemplazar todas las cartas
    public void ReplaceCardsFromButton()
    {
        ReplaceAllToothCards();
    }
}
