using System.Collections.Generic;
using UnityEngine;

// Clase que representa una entrada en el mazo de dientes.
// Contiene un prefab de carta y un indicador de disponibilidad.
[System.Serializable]
public class ToothCardEntry
{
    public GameObject prefab;     // Prefab del diente que se puede instanciar
    public bool isAvailable;      // Si la carta está disponible para usarse en la ronda actual
}

public class ToothDeck : MonoBehaviour
{
    public List<ToothCardEntry> toothCards; // Lista de cartas de dientes (prefabs + disponibilidad)
    public List<ToothCardData> toothCardDataList; // Lista de los datos de las cartas de dientes
    public Transform[] positions;           // Posiciones sobre la mesa donde aparecerán las cartas

    // Función pública que instancia hasta 5 cartas de diente en las posiciones definidas
    public void SpawnToothCards()
    {
        int posIndex = 0; // Índice para recorrer las posiciones

        // Itera sobre cada posición disponible
        foreach (Transform pos in positions)
        {
            // Selecciona aleatoriamente una carta disponible del mazo
            ToothCardEntry cardEntry = GetRandomAvailableCard();
            if (cardEntry == null) break; // Si no hay más cartas disponibles, detener

            // Instancia una copia del prefab en la posición actual
            GameObject instance = Instantiate(cardEntry.prefab, pos.position, Quaternion.identity);

            // Marca la carta como no disponible para evitar reuso
            cardEntry.isAvailable = false;

            // Cargar datos si hay coincidencia de ID
            ToothCard card = instance.GetComponent<ToothCard>();
            if (card != null)
            {
                ToothCardData data = GetDataByID(card.cardID);
                if (data != null)
                {
                    card.LoadData(data);
                }
            }

            GameController gc = Object.FindFirstObjectByType<GameController>();
            if (gc != null)
                gc.RegisterToothCardCreated();

            posIndex++;
            // Si ya se llenaron todas las posiciones, salir del bucle
            if (posIndex >= positions.Length) break;
        }
    }

    // Función privada que encuentra si existe un objeto de datos (ToothCardData) con un ID específico en la lista
    private ToothCardData GetDataByID(string id)
    {
        return toothCardDataList.Find(d => d.cardID == id);
    }

    // Función privada que devuelve una carta aleatoria que esté disponible
    private ToothCardEntry GetRandomAvailableCard()
    {
        // Filtra las cartas disponibles (isAvailable == true)
        List<ToothCardEntry> available = toothCards.FindAll(card => card.isAvailable);

        // Si no hay cartas disponibles, devuelve null
        if (available.Count == 0) return null;

        // Selecciona una carta aleatoria del listado de disponibles
        int randomIndex = Random.Range(0, available.Count);
        return available[randomIndex];
    }

    // Función que restablece la disponibilidad de todas las cartas del mazo de dientes
    public void ResetCardAvailability()
    {
        foreach (ToothCardEntry card in toothCards)
        {
            card.isAvailable = true;
        }
    }

    // Función que marca como disponibles únicamente aquellas cartas de diente cuyos datos han cambiado
    // (dirtValue != 0, toothPH != 100 o state != 0)
    public void SetAvailabilityForModifiedCardsOnly()
    {
        // Recorremos todos los datos de las cartas de diente
        foreach (ToothCardData data in toothCardDataList)
        {
            // Comprobamos si esta carta tiene valores modificados
            bool isModified = data.dirtValue != 0 || data.toothPH != 100 || data.state != 0;

            // Buscar la carta correspondiente en la lista de toothCards por ID
            foreach (ToothCardEntry card in toothCards)
            {
                ToothCard cardComponent = card.prefab.GetComponent<ToothCard>();
                if (cardComponent != null && cardComponent.cardID == data.cardID)
                {
                    card.isAvailable = isModified;
                    break;
                }
            }
        }
    }

    // Función que reemplaza todas las cartas de diente activas por nuevas del mazo
    public void ReplaceAllToothCards()
    {
        // Buscar todas las cartas activas en la escena
        ToothCard[] existingCards = Object.FindObjectsByType<ToothCard>(FindObjectsSortMode.None);

        // Si no hay cartas activas, no hay nada que reemplazar
        if (existingCards.Length == 0)
        {
            Debug.Log("No hay cartas de diente activas para reemplazar.");
            return;
        }

        // Verificar que existan cartas disponibles para poder crear nuevas
        int availableCount = toothCards.FindAll(c => c.isAvailable).Count;
        if (availableCount == 0)
        {
            Debug.Log("No hay cartas disponibles en el mazo para generar nuevas cartas.");
            return;
        }

        // Guardar los IDs de las cartas que se eliminarán
        HashSet<string> idsToRestore = new HashSet<string>();
        foreach (ToothCard card in existingCards)
        {
            idsToRestore.Add(card.cardID);
            Destroy(card.gameObject);
        }

        // Crear nuevas cartas
        SpawnToothCards();

        // Luego de crear, marcamos como disponibles las cartas eliminadas
        GameController gc = Object.FindFirstObjectByType<GameController>();

        foreach (ToothCardEntry entry in toothCards)
        {
            ToothCard cardComponent = entry.prefab.GetComponent<ToothCard>();
            if (cardComponent != null && idsToRestore.Contains(cardComponent.cardID))
            {
                entry.isAvailable = true;

                // Restar al contador global si existe GameController
                if (gc != null)
                {
                    gc.UnregisterToothCardCreated();
                }
            }
        }
    }

    // Función para llamar desde un botón UI y reemplazar todas las cartas
    public void ReplaceCardsFromButton()
    {
        ReplaceAllToothCards();
    }

}
