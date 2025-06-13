using UnityEngine;

public class GameController : MonoBehaviour
{
    public int phase = 1; // Fase actual: 1 = Comida, 2 = Cuidado

    public ToothDeck toothDeck; // Mazo de dientes
    public FoodDeck foodDeck;   // Mazo de comida
    public CareDeck careDeck;   // Mazo de cuidado dental

    // Contadores
    private int usedToothCount = 0;
    private int totalToothCardsCreated = 0;
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

            toothDeck.SpawnToothCards();
            foodDeck.SpawnFoodCards();
        }
        else if (phase == 2)
        {
            careDeck.transform.position = careDeckPosition;

            toothDeck.SpawnToothCards();
            careDeck.SpawnCareCards();
        }
        else
        {
            Debug.LogWarning("Fase no válida. Usa 1 para comida o 2 para cuidado.");
        }
    }

    // Registro de cada vez que se instancia una carta
    public void RegisterToothCardCreated()
    {
        totalToothCardsCreated++;
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
