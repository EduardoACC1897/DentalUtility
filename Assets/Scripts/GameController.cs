using UnityEngine;

public class GameController : MonoBehaviour
{
    public int phase = 1; // Fases: Comida = 1, Cuidado = 2

    public ToothDeck toothDeck; // Mazo de dientes
    public FoodDeck foodDeck;   // Mazo de comida
    public CareDeck careDeck;   // Mazo de cuidado dental

    // Contadores
    public int totalToothCards = 0;
    public int totalToothCardsCreated = 0;
    public int usedToothCount = 0;
    private int usedFoodCount = 0;
    private int usedCareCount = 0;

    // Posiciones para mostrar mazos
    private Vector2 foodDeckPosition = new Vector2(8f, 3f);
    private Vector2 careDeckPosition = new Vector2(8f, 3f);

    void Start()
    {
        StartPhase();
    }

    void StartPhase()
    {
        if (phase == 1)
        {
            foodDeck.transform.position = foodDeckPosition;
            UpdateAvailableToothCardsCount();
            toothDeck.SpawnToothCards();
            foodDeck.SpawnFoodCards();
        }
        else if (phase == 2)
        {
            careDeck.transform.position = careDeckPosition;
            UpdateAvailableToothCardsCount();
            toothDeck.SpawnToothCards();
            careDeck.SpawnCareCards();
        }
        else
        {
            Debug.LogWarning("Fase no válida. Usa 1 para comida o 2 para cuidado.");
        }
    }

    // Función para actualizar el contador de cartas de diente disponibles
    public void UpdateAvailableToothCardsCount()
    {
        if (toothDeck != null)
        {
            totalToothCards = toothDeck.GetAvailableToothCardsCount();
        }
    }

    // Función para verificar si no quedan cartas de diente disponibles y, en ese caso,
    // reiniciar la disponibilidad del mazo y generar nuevas cartas automáticamente.
    public void CheckAndSpawnToothCardsIfNone()
    {
        totalToothCards--;
        if (totalToothCards == 0)
        {
            if (toothDeck != null)
            {
                usedToothCount = 0;
                totalToothCardsCreated = 0;
                toothDeck.ResetCardAvailability();
                UpdateAvailableToothCardsCount();
                toothDeck.SpawnToothCards();              
            }
        }
    }

    // Registro de cada vez que se instancia una carta
    public void RegisterToothCardCreated()
    {
        totalToothCardsCreated++;
    }

    // Función para reducir el contador de cartas de diente creadas
    public void UnregisterToothCardCreated()
    {
        totalToothCardsCreated = Mathf.Max(0, totalToothCardsCreated - 1);
    }

    // Registro de cada vez que se usa una carta de diente
    public void RegisterToothCardUsed()
    {
        usedToothCount++;

        if (usedToothCount >= totalToothCardsCreated && totalToothCardsCreated > 0)
        {
            usedToothCount = 0;
            totalToothCardsCreated = 0;
            toothDeck.SpawnToothCards();
        }
    }

    // Registro de cada vez que se usa una carta de comida
    public void RegisterFoodCardUsed()
    {
        if (phase != 1) return;

        usedFoodCount++;
        if (usedFoodCount >= 5)
        {
            usedFoodCount = 0;
            foodDeck.SpawnFoodCards();
        }
    }

    // Registro de cada vez que se usa una carta de cuidado
    public void RegisterCareCardUsed()
    {
        if (phase != 2) return;

        usedCareCount++;
        if (usedCareCount >= 5)
        {
            usedCareCount = 0;
            careDeck.SpawnCareCards();
        }
    }
}
