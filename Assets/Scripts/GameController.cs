using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public int phase = 1; // Fases: Comida = 1, Cuidado = 2

    public ToothDeck toothDeck;    // Mazo de dientes
    public FoodDeck foodDeck;      // Mazo de comida
    public CareDeck careDeck;      // Mazo de cuidado dental
    public GameObject discardPile; // Pila de descarte

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
    public float foodPhaseTimer; // Duraci�n total

    // Timer para la fase de cuidado
    public float carePhaseDuration;   // Duraci�n total
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

    // Bot�n
    public GameObject rerollButton;

    // Sprites de fondo y posici�n del renderer
    public Sprite bgSpriteFood, bgSpriteCare;
    public SpriteRenderer bgRenderer;
    private Vector2 bgCenterOffset = new Vector2(0, 0);

    public GameObject gameOverPanel; // Panel que contiene todos los elementos del Game Over
    public TMP_Text finalScoreText; // Texto para mostrar el puntaje final
    public bool isGameOver = false; // Bandera para controlar el estado del juego

    public bool isTransitioning = false; // Indica si se est� realizando una transici�n entre fases
    public bool hasTriggeredCarePhaseEnd = false; // Asegura que se llame solo una vez transici�n para cuando termina fase de cuidado
    public bool canUseCards = true; // Controla si el jugador puede interactuar con las cartas

    // Sprites de transici�n
    public Sprite transitionToFoodPhaseSprite;   // Transici�n visual para fase de comida
    public Sprite transitionToCarePhaseSprite;   // Transici�n visual para fase de cuidado
    public Sprite discardTransitionSprite;       // Transici�n visual al descartar cartas
    public Sprite cariesTransitionSprite;        // Transici�n visual cuando aparecen caries

    public AudioSource Bgm;
    public AudioResource bgmfood, bgmcare, foodTransition, careTransition, cariesTransition, discardTransition, bgmScore;
    public AudioClip endphaseClip, takeCard, shuffleBtn, cardShuffle, foodCardConsumed, careCardConsumed, cardShaking, discardSound, fractureSound, dropCard, correctUse, incorrectUse;

    void Start()
    {
        Phase();
    }

    void Update()
    {
        gameOver();
        if (isGameOver || isTransitioning) return;

        if (phase == 1)
        {
            foodPhaseTimer -= Time.deltaTime;
            foodPhaseTimer = Mathf.Max(0f, foodPhaseTimer);
        }
        else if (phase == 2)
        {
            carePhaseTimer -= Time.deltaTime;
            carePhaseTimer = Mathf.Max(0f, carePhaseTimer);

            // Solo ejecutar la transici�n una vez
            if (carePhaseTimer <= 0f && !hasTriggeredCarePhaseEnd)
            {
                hasTriggeredCarePhaseEnd = true;
                StartCoroutine(TransitionToNextPhase());
            }
        }

        UpdateUI();
    }
    private void playFoodMusic()
    {
        Bgm.loop = true;
        Bgm.resource = bgmfood;
        Bgm.Play();
    }
    private void playCareMusic()
    {
        Bgm.loop = true;
        Bgm.resource = bgmcare;
        Bgm.Play();
    }
    private void playEndPhaseSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(endphaseClip);
    }
    private void playFoodTransitionSound()
    {
        Bgm.loop = false;
        Bgm.resource = foodTransition;
        Bgm.Play();
    }
    private void playCareTransitionSound()
    {
        Bgm.loop = false;
        Bgm.resource = careTransition;
        Bgm.Play();
    }
    public void playCariesTransitionSound()
    {
        Bgm.loop = false;
        Bgm.resource = cariesTransition;
        Bgm.Play();
    }
    public void playDiscardTransitionSound()
    {
        Bgm.loop = false;
        Bgm.resource = discardTransition;
        Bgm.Play();
    }
    public void playTakeCardSound()    
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(takeCard);
    }
    public void playShuffleBtnSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(shuffleBtn);
    }
    public void playCardShuffleSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(cardShuffle);
    }
    public void playFoodCardConsumedSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(foodCardConsumed);
    }
    public void playCareCardConsumedSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(careCardConsumed);
    }
    public void playShakingSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(cardShaking);
    }
    public void playDiscardSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(discardSound);
    }
    public void playFractureSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(fractureSound);
    }
    public void playDropCardSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(dropCard);
    }
    public void playCorrectSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(correctUse);
    }
    public void playIncorrectSound()
    {
        Bgm.loop = false;
        Bgm.PlayOneShot(incorrectUse);
    }

    // Corrutina que realiza una pausa de 3 segundos antes de cambiar de fase.
    // Durante la pausa se bloquea el uso de cartas y se indica que est� en transici�n.
    private System.Collections.IEnumerator TransitionToNextPhase()
    {
        playEndPhaseSound();
        isTransitioning = true;
        canUseCards = false; // Bloquear uso de cartas

        yield return new WaitForSeconds(3f); // Esperar 3 segundos

        ChangePhase(); // Cambiar fase

        canUseCards = true; // Volver a permitir uso de cartas
        isTransitioning = false;
    }

    // Corrutina que realiza la transici�n visual entre fases del juego.
    // Elimina las cartas activas seg�n la fase actual, cambia el sprite de fondo a uno de transici�n,
    // desactiva temporalmente todos los mazos, paneles y textos UI, espera 5 segundos,
    // y luego activa la nueva fase correspondiente con todos los elementos visibles nuevamente.
    private System.Collections.IEnumerator PhaseTransitionCoroutine()
    {
        isTransitioning = true;
        canUseCards = false;

        // Eliminar cartas seg�n la fase actual
        if (phase == 1) // Fase de comida
        {
            FoodCard[] foodCards = Object.FindObjectsByType<FoodCard>(FindObjectsSortMode.None);
            foreach (FoodCard card in foodCards)
                Destroy(card.gameObject);
        }
        else if (phase == 2) // Fase de cuidado
        {
            CareCard[] careCards = Object.FindObjectsByType<CareCard>(FindObjectsSortMode.None);
            foreach (CareCard card in careCards)
                Destroy(card.gameObject);
        }

        // Siempre eliminar cartas de diente
        ToothCard[] toothCards = Object.FindObjectsByType<ToothCard>(FindObjectsSortMode.None);
        foreach (ToothCard card in toothCards)
            Destroy(card.gameObject);

        // Ocultar objetos durante la transici�n
        if (toothDeck != null) toothDeck.gameObject.SetActive(false);
        if (foodDeck != null) foodDeck.gameObject.SetActive(false);
        if (careDeck != null) careDeck.gameObject.SetActive(false);
        if (discardPile != null) discardPile.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (roundsText != null) roundsText.gameObject.SetActive(false);
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (rerollButton != null) rerollButton.SetActive(false);

        // Evaluar riesgos
        if (phase == 2)
        {
            yield return StartCoroutine(toothDeck.EvaluateDiscardRiskWithTransition(this));
            yield return StartCoroutine(toothDeck.EvaluateCariesRiskWithTransition(this));
        }

        // Cambiar sprite de transici�n
        if (phase == 1)
        {
            bgRenderer.sprite = transitionToCarePhaseSprite;
            playCareTransitionSound();
        }
        else if (phase == 2)
        {
            bgRenderer.sprite = transitionToFoodPhaseSprite;
            playFoodTransitionSound();
        }

        yield return new WaitForSeconds(5f); // Tiempo de transici�n

        // Avanzar de fase
        if (phase == 1)
        {
            phase = 2;
            CarePhase();
            // Ocultar rondas durante la fase de cuidado (2)
            roundsText.gameObject.SetActive(false);
        }
        else if (phase == 2)
        {
            phase = 1;
            FoodPhase();
            // Mostrar rondas durante la fase de cuidado (1)
            roundsText.gameObject.SetActive(true);
        }

        // Restaurar UI y objetos
        if (toothDeck != null) toothDeck.gameObject.SetActive(true);
        if (foodDeck != null) foodDeck.gameObject.SetActive(true);
        if (careDeck != null) careDeck.gameObject.SetActive(true);
        if (discardPile != null) discardPile.SetActive(true);
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(true);
        if (rerollButton != null) rerollButton.SetActive(true);

        canUseCards = true;
        isTransitioning = false;
        hasTriggeredCarePhaseEnd = false;
    }

    // Se muestra el panel de game over con el puntaje final
    private void ShowGameOverScreen()
    {
        Bgm.loop = true;
        Bgm.resource = bgmScore;
        Bgm.Play();
        canUseCards = false;
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


    // Funci�n para iniciar la fase de acuerdo al valor de phase
    void Phase()
    {
        if (phase == 1)
        {
            FoodPhase();
            // Mostrar las rondas solo en la fase de comida (1)
            roundsText.gameObject.SetActive(true);
        }
        else if (phase == 2)
        {
            CarePhase();
            // Ocultar las rondas solo en la fase de comida (2)
            roundsText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Fase no v�lida. Usa 1 para comida o 2 para cuidado.");
        }
    }

    // Funci�n para iniciar la fase 1 (fase de comida)
    private void FoodPhase()
    {
        playFoodMusic();
        // Reiniciar el temporizador para la fase de cuidado
        carePhaseTimer = carePhaseDuration;

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
        bgRenderer.sprite = bgSpriteFood;
        bgRenderer.transform.position = Vector3.zero + (Vector3)bgCenterOffset;
    }

    // Funci�n para iniciar la fase 2 (fase de cuidado dental)
    private void CarePhase()
    {
        playCareMusic();
        // Reiniciar rondas de comida
        currentFoodRounds = 0;

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
        bgRenderer.sprite = bgSpriteCare;
    }

    // Cambia entre la fase de comida y la de cuidado dental
    public void ChangePhase()
    {
        StartCoroutine(PhaseTransitionCoroutine());
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
                StartCoroutine(TransitionToNextPhase());
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
                StartCoroutine(TransitionToNextPhase()); // Cambio autom�tico a fase 2
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

    // Funci�n que actualiza los elementos visuales de texto en la UI
    private void UpdateUI()
    {
        // Mostrar el tiempo correspondiente a la fase actual
        if (phase == 1)
        {
            timerText.text = "Tiempo: " + ((int)foodPhaseTimer) + "s";
            roundsText.text = $"Ronda: {currentFoodRounds + 1}/{maxFoodRounds}";
        }
        else if (phase == 2)
        {
            timerText.text = "Tiempo: " + ((int)carePhaseTimer) + "s";
        }

        // Puntaje
        scoreText.text = "Puntaje: " + score.ToString();
    }

    // Vuelve a la escena llamada "Inicio"
    public void ReturnToStartScene()
    {
        DOTween.KillAll();
        Time.timeScale = 1f; // Asegurar que el tiempo est� normal
        SceneManager.LoadScene("Inicio");
    }

    // Reinicia la escena actual del juego
    public void RestartScene()
    {
        DOTween.KillAll();
        Time.timeScale = 1f; // Asegurar que el tiempo est� normal
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
