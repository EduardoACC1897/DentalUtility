using UnityEngine;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

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
    private Vector2 foodDeckPosition = new Vector2(7.5f, 3.25f);
    private Vector2 careDeckPosition = new Vector2(7.5f, 3.25f);

    // Timer para la fase de comida
    public float foodPhaseTimer; // Duración total

    // Timer para la fase de cuidado
    public float carePhaseDuration;   // Duración total
    public float carePhaseTimer = 0f; // Temporizador

    // Rondas de fase de comida
    public int maxFoodRounds;         // Rondas necesarias para pasar a fase de cuidado
    public int currentFoodRounds = 0; // Rondas de comida actuales

    // Puntaje total acumulado
    public int score = 0;

    // Textos UI
    public TMP_Text timerText;
    public TMP_Text roundsText;
    public TMP_Text scoreText;

    // Sprites de fondo y posición del renderer
    public Sprite bgSpriteFood, bgSpriteCare;
    public SpriteRenderer bgRenderer;
    private Vector2 bgCenterOffset = new Vector2(0, 0);

    public GameObject gameOverPanel; // Panel que contiene todos los elementos del Game Over
    public TMP_Text finalScoreText; // Texto para mostrar el puntaje final
    public bool isGameOver = false; // Bandera para controlar el estado del juego


    void Start()
    {
        Phase();
    }

    void Update()
    {
        gameOver();
        if (isGameOver) return;

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
                ChangePhase(); // Cambiar automáticamente a la fase de comida
            }
        }

        // Actualizar UI
        UpdateUI();
    }

    // Se muestra el panel de game over con el puntaje final
    private void ShowGameOverScreen()
    {
        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + score.ToString();
        }
    }

    // Verifica si el timer llega a 0
    void gameOver()
    {
        if (foodPhaseTimer <= 0f && !isGameOver)
        {
            isGameOver = true;
            ShowGameOverScreen();
        }
    }


    // Función para iniciar la fase de acuerdo al valor de phase
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
            Debug.LogWarning("Fase no válida. Usa 1 para comida o 2 para cuidado.");
        }
    }

    // Función para iniciar la fase 1 (fase de comida)
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

        // Reubicación de mazos
        careDeck.transform.position = new Vector2(13f, 3f);
        foodDeck.transform.position = foodDeckPosition;

        // Mostrar todos los dientes
        toothDeck.ResetCardAvailability();

        UpdateAvailableToothCardsCount();
        toothDeck.SpawnToothCards();
        foodDeck.SpawnFoodCards();
        bgRenderer.sprite = bgSpriteFood;
        bgRenderer.transform.position = Vector3.zero + (Vector3)bgCenterOffset;
    }

    // Función para iniciar la fase 2 (fase de cuidado dental)
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

        // Reubicación de mazos
        foodDeck.transform.position = new Vector2(13f, 3f);
        careDeck.transform.position = careDeckPosition;

        // Configurar qué cartas de cuidado pueden aparecer
        if (toothDeck != null && careDeck != null)
        {
            careDeck.UpdateCareCardAvailabilityBasedOnToothState(toothDeck.runtimeData);
        }

        // Mostrar solo dientes modificados
        toothDeck.SetAvailabilityForModifiedCardsOnly();

        UpdateAvailableToothCardsCount();
        toothDeck.SpawnToothCards();
        careDeck.SpawnCareCards();
        bgRenderer.sprite = bgSpriteCare;
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

            // Evaluar el riesgo de descartar dientes dañados y aparición de caries 
            toothDeck.EvaluateDiscardRiskForAllCards();
            toothDeck.EvaluateCariesRiskForAllCards();

            // Cambiar a la fase de comida
            FoodPhase();
        }
    }

    // Función para actualizar el contador de cartas de diente disponibles
    public void UpdateAvailableToothCardsCount()
    {
        if (toothDeck != null)
        {
            totalToothCards = toothDeck.GetAvailableToothCardsCount();

            // Si estamos en la fase de cuidado, sumar 1 extra
            if (phase == 2)
            {
                totalToothCards += 1;
            }
        }
    }

    // Función para verificar si no quedan cartas de diente disponibles.
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
            currentFoodRounds++;

            if (currentFoodRounds >= maxFoodRounds)
            {
                maxFoodRounds++;  // Aumenta la dificultad (más rondas para la próxima vez)
                ChangePhase();    // Cambio automático a fase 2
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

    // Función que actualiza los elementos visuales de texto en la UI
    private void UpdateUI()
    {
        // Mostrar el tiempo correspondiente a la fase actual
        if (phase == 1)
        {
            timerText.text = "Tiempo: " + ((int)foodPhaseTimer) + "s";
            roundsText.text = $"Ronda: {currentFoodRounds + 1}/{maxFoodRounds}";
            roundsText.gameObject.SetActive(true); // Mostrar las rondas solo en la fase de comida (1)
        }
        else if (phase == 2)
        {
            timerText.text = "Tiempo: " + ((int)carePhaseTimer) + "s";
            roundsText.gameObject.SetActive(false); // Ocultar rondas durante la fase de cuidado (2)
        }

        // Puntaje
        scoreText.text = "Puntaje: " + score.ToString();
    }
}
