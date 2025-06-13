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
    public List<ToothCardEntry> toothCards; // Lista de cartas de dientes (prefabs + disponibilidad)
    public List<ToothCardData> toothCardDataList; // Lista de los datos de las cartas de dientes
    public Transform[] positions;           // Posiciones sobre la mesa donde aparecer�n las cartas

    // Funci�n p�blica que instancia hasta 5 cartas de diente en las posiciones definidas
    public void SpawnToothCards()
    {
        int posIndex = 0; // �ndice para recorrer las posiciones

        // Itera sobre cada posici�n disponible
        foreach (Transform pos in positions)
        {
            // Selecciona aleatoriamente una carta disponible del mazo
            ToothCardEntry cardEntry = GetRandomAvailableCard();
            if (cardEntry == null) break; // Si no hay m�s cartas disponibles, detener

            // Instancia una copia del prefab en la posici�n actual
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

    // Funci�n privada que encuentra si existe un objeto de datos (ToothCardData) con un ID espec�fico en la lista
    private ToothCardData GetDataByID(string id)
    {
        return toothCardDataList.Find(d => d.cardID == id);
    }

    // Funci�n privada que devuelve una carta aleatoria que est� disponible
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
}
