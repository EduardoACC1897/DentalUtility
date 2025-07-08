using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    public List<ToothCardEntry> toothCards;             // Lista de cartas de dientes (prefabs + disponibilidad)
    public List<ToothCardData> toothCardDataList;       // Lista de datos base (ScriptableObjects)
    public Dictionary<string, ToothCardData> runtimeData = new Dictionary<string, ToothCardData>(); // Copias de los datos en runtime
    public Transform[] positions;                       // Posiciones sobre la mesa donde aparecerán las cartas

    void Start()
    {
        // Crear copias de todos los datos originales para uso en tiempo de ejecución
        foreach (ToothCardData original in toothCardDataList)
        {
            runtimeData[original.cardID] = original.Clone();
        }
    }

    // Función para obtener la cantidad de cartas disponibles en la lista
    public int GetAvailableToothCardsCount()
    {
        return toothCards.FindAll(card => card.isAvailable).Count;
    }

    // Función que instancia hasta 5 cartas de diente en las posiciones definidas
    public void SpawnToothCards()
    {
        int posIndex = 0; // Índice para recorrer las posiciones

        foreach (Transform pos in positions)
        {
            ToothCardEntry cardEntry = GetRandomAvailableCard();
            if (cardEntry == null) break;

            // Instanciar en la posición del mazo (la posición del objeto ToothDeck)
            Vector3 startPos = transform.position;

            GameObject instance = Instantiate(cardEntry.prefab, startPos, Quaternion.identity);
            cardEntry.isAvailable = false;

            ToothCard card = instance.GetComponent<ToothCard>();
            if (card != null && runtimeData.TryGetValue(card.cardID, out ToothCardData data))
            {
                card.LoadData(data);
            }

            // Animación hacia la posición destino
            float duration = 0.3f;
            instance.transform.DOMove(pos.position, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Asignar la posición final como startPosition después de que la animación termina
                    if (card != null)
                    {
                        card.startPosition = pos.position;
                    }
                });

            GameController gc = Object.FindFirstObjectByType<GameController>();
            if (gc != null)
                gc.RegisterToothCardCreated();

            posIndex++;
            if (posIndex >= positions.Length) break;
        }
    }

    // Función que devuelve una carta aleatoria que esté disponible
    private ToothCardEntry GetRandomAvailableCard()
    {
        List<ToothCardEntry> available = toothCards.FindAll(card => card.isAvailable);
        if (available.Count == 0) return null;

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
        foreach (KeyValuePair<string, ToothCardData> pair in runtimeData)
        {
            string id = pair.Key;
            ToothCardData data = pair.Value;

            bool isModified = data.dirtValue != 0 || data.toothPH != 100 || data.state != 0;

            if (!isModified)
            {
                Debug.Log($"[SetAvailability] Carta NO modificada = ID: {id} | Dirt: {data.dirtValue}, PH: {data.toothPH}, State: {data.state}");
            }

            foreach (ToothCardEntry card in toothCards)
            {
                ToothCard cardComponent = card.prefab.GetComponent<ToothCard>();
                if (cardComponent != null && cardComponent.cardID == id)
                {
                    card.isAvailable = isModified;
                    Debug.Log($"[SetAvailability] Carta SI modificada = ID: {id} | Dirt: {data.dirtValue}, PH: {data.toothPH}, State: {data.state}");
                    break;
                }
            }
        }
    }

    // Función que reemplaza todas las cartas de diente activas por nuevas del mazo
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

    // Función para llamar desde un botón UI y reemplazar todas las cartas
    public void ReplaceCardsFromButton()
    {
        ReplaceAllToothCards();
    }

    // Función que evalúa el riesgo de desarrollo de caries en todas las cartas
    public void EvaluateCariesRiskForAllCards()
    {
        foreach (var pair in runtimeData)
        {
            ToothCardData data = pair.Value;
            int probability = 0;

            // Probabilidad basada en PH
            if (data.toothPH <= 0)
                probability += 50;
            else if (data.toothPH <= 25)
                probability += 30;
            else if (data.toothPH <= 50)
                probability += 20;
            else if (data.toothPH <= 75)
                probability += 10;

            // Probabilidad basada en suciedad
            if (data.dirtValue >= 100)
                probability += 50;
            else if (data.dirtValue >= 75)
                probability += 30;
            else if (data.dirtValue >= 50)
                probability += 20;
            else if (data.dirtValue >= 25)
                probability += 10;

            // Tirada de probabilidad (de 0 a 99)
            int roll = Random.Range(0, 100);

            // Si la tirada está dentro de la probabilidad y el estado es menor a 2, se incrementa
            if (roll < probability && data.state < 2)
            {
                data.state += 1;
                Debug.Log($"Caries: Carta con ID '{data.cardID}' aumentó su estado a {data.state} (prob: {probability}%, roll: {roll})");
            }
        }
    }

    // Función que evalúa el riesgo de descarte de las cartas de diente y elimina hasta 5 del mazo si corresponde.
    public void EvaluateDiscardRiskForAllCards()
    {
        List<string> idsToRemove = new List<string>();                     // IDs para eliminar de runtimeData
        List<ToothCardEntry> entriesToRemove = new List<ToothCardEntry>(); // Entradas para eliminar de toothCards

        int discardCount = 0; // Contador de descartes realizados

        foreach (var pair in runtimeData)
        {
            if (discardCount >= 5)
                break;

            string id = pair.Key;
            ToothCardData data = pair.Value;

            int probability = 0;

            // Probabilidad base según el estado
            if (data.state == 1)
                probability += 10;
            else if (data.state == 2)
                probability += 25;

            // Aumentar probabilidad si tiene fractura
            if (data.hasFracture)
                probability += 25;

            int roll = Random.Range(0, 100);

            if (roll < probability)
            {
                // Marcar para eliminar de runtimeData
                idsToRemove.Add(id);

                // Marcar para eliminar de toothCards
                ToothCardEntry entry = toothCards.Find(card =>
                {
                    ToothCard c = card.prefab.GetComponent<ToothCard>();
                    return c != null && c.cardID == id;
                });

                if (entry != null)
                    entriesToRemove.Add(entry);

                Debug.Log($"Descarte: Carta con ID '{id}' fue descartada (prob: {probability}%, roll: {roll})");
                discardCount++;
            }
        }

        // Aplicar eliminaciones
        foreach (string id in idsToRemove)
        {
            runtimeData.Remove(id);
        }

        foreach (var entry in entriesToRemove)
        {
            toothCards.Remove(entry);
        }
    }
}
