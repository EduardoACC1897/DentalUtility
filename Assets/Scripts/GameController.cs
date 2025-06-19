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

    // Timer para la fase de cuidado
    public float carePhaseDuration;    // Duraci�n total
    public float carePhaseTimer = 0f;  // Temporizador

    // Timer para la fase de comida
    public float foodPhaseTimer;    // Duraci�n total

    // Rondas de fase de comida
    public int maxFoodRounds;          // Rondas necesarias para pasar a fase de cuidado
    public int currentFoodRounds = 0; // Rondas de comida actuales

    void Start()
    {
        Phase();
    }

    void Update()
    {
        if (phase == 1)
        {
            foodPhaseTimer -= Time.deltaTime;
            foodPhaseTimer = Mathf.Max(0f, foodPhaseTimer); // Prevenir que sea negativo
        }
        else if (phase == 2)
        {
            carePhaseTimer -= Time.deltaTime;

            if (carePhaseTimer <= 0f)
            {
                carePhaseTimer = 0f;
                ChangePhase(); // Cambiar autom�ticamente a la fase de comida
            }
        }
    }


    // Funci�n para iniciar la fase de acuerdo al valor de phase
    void Phase()
    {
        if (phase == 1)
        {
            FoodPhase();
        }
        else if (phase == 2)
        {
            CarePhase();
        }
        else
        {
            Debug.LogWarning("Fase no v�lida. Usa 1 para comida o 2 para cuidado.");
        }
    }

    // Funci�n para iniciar la fase 1 (fase de comida)
    private void FoodPhase()
    {
        // Reiniciar el temporizador para la fase de cuidado
        carePhaseTimer = carePhaseDuration;

        // Eliminar cartas de cuidado y diente activas
        CareCard[] careCards = Object.FindObjectsByType<CareCard>(FindObjectsSortMode.None);
        foreach (CareCard card in careCards)
        {
            Destroy(card.gameObject);
        }

        ToothCard[] toothCards = Object.FindObjectsByType<ToothCard>(FindObjectsSortMode.None);
        foreach (ToothCard card in toothCards)
        {
            Destroy(card.gameObject);
        }

        // Reinicio de contadores
        totalToothCards = 0;
        totalToothCardsCreated = 0;
        usedToothCount = 0;
        usedCareCount = 0;

        // Reubicaci�n de mazos
        careDeck.transform.position = new Vector2(13f, 3f);
        foodDeck.transform.position = foodDeckPosition;

        // Mostrar todos los dientes
        toothDeck.ResetCardAvailability();
        UpdateAvailableToothCardsCount();
        toothDeck.SpawnToothCards();
        foodDeck.SpawnFoodCards();
    }

    // Funci�n para iniciar la fase 2 (fase de cuidado dental)
    private void CarePhase()
    {
        // Reiniciar rondas de comida
        currentFoodRounds = 0;

        // Eliminar cartas de comida y diente activas
        FoodCard[] foodCards = Object.FindObjectsByType<FoodCard>(FindObjectsSortMode.None);
        foreach (FoodCard card in foodCards)
        {
            Destroy(card.gameObject);
        }

        ToothCard[] toothCards = Object.FindObjectsByType<ToothCard>(FindObjectsSortMode.None);
        foreach (ToothCard card in toothCards)
        {
            Destroy(card.gameObject);
        }

        // Reinicio de contadores
        totalToothCards = 0;
        totalToothCardsCreated = 0;
        usedToothCount = 0;
        usedFoodCount = 0;

        // Reubicaci�n de mazos
        foodDeck.transform.position = new Vector2(13f, 3f);
        careDeck.transform.position = careDeckPosition;

        // Configurar qu� cartas de cuidado pueden aparecer
        if (toothDeck != null && careDeck != null)
        {
            careDeck.UpdateCareCardAvailabilityBasedOnToothState(toothDeck.runtimeData);
        }
        // Mostrar solo dientes modificados
        toothDeck.SetAvailabilityForModifiedCardsOnly();

        UpdateAvailableToothCardsCount();
        toothDeck.SpawnToothCards();
        careDeck.SpawnCareCards();
    }

    // Cambia entre la fase de comida y la de cuidado dental
    public void ChangePhase()
    {
        if (phase == 1)
        {
            phase = 2;

            // Cambiar a la fase de cuidado dental
            CarePhase();
        }
        else if (phase == 2)
        {
            phase = 1;

            // Evaluar el riesgo de aparici�n de caries y descartar dientes da�ados
            toothDeck.EvaluateCariesRiskForAllCards();
            toothDeck.EvaluateDiscardRiskForAllCards();

            // Cambiar a la fase de comida
            FoodPhase();
        }
    }


    // Funci�n para actualizar el contador de cartas de diente disponibles
    public void UpdateAvailableToothCardsCount()
    {
        if (toothDeck != null)
        {
            totalToothCards = toothDeck.GetAvailableToothCardsCount();
        }
    }

    // Funci�n para verificar si no quedan cartas de diente disponibles.
    // Si la fase es 2, simplemente cambia de fase. Si es 1, reinicia el mazo y genera nuevas cartas.
    public void CheckAndSpawnToothCardsIfNone()
    {
        totalToothCards--;

        if (totalToothCards == 0)
        {
            if (phase == 2)
            {
                ChangePhase(); // Cambiar a fase 1
            }
            else if (phase == 1 && toothDeck != null)
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

    // Funci�n para reducir el contador de cartas de diente creadas
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
            currentFoodRounds++;

            if (currentFoodRounds >= maxFoodRounds)
            {
                maxFoodRounds++;  // Aumenta la dificultad (m�s rondas para la pr�xima vez)
                ChangePhase();    // Cambio autom�tico a fase 2
            }
            else
            {
                foodDeck.SpawnFoodCards(); // Avanza a la siguiente ronda dentro de fase 1
            }
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
