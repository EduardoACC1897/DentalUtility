using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ToothCard : MonoBehaviour
{
    public string cardID; // ID de la carta
    public int dirtValue; // Valor de suciedad que tiene el diente
    public int toothPH; // Valor de ph que tiene el diente
    public int state; // Estado del diente: Limpio = 0, Caries1 = 1, Caries2 = 2
    public int durability; // Durabilidad del diente: Sano = 0, Comprometido = 1, Cr�tico = 2
    public bool grindAction; // Indica si realiza la acci�n de moler
    public bool cutAction; // Indica si realiza la acci�n de cortar
    public bool tearAction; // Indica si realiza la acci�n de desgarrar
    public bool isDirty; // Indica si el diente est� sucio
    public bool hasFracture; // Indica si el diente tiene una fractura
    public List<Sprite> stateSprites; // Lista de sprites disponibles para representar visualmente el estado del diente
    public List<Sprite> durabilitySprites; // Lista de sprites para representar la durabilidad
    public Vector3 startPosition; // Posici�n inicial del diente
    
    private bool isDragging = false; // Controlar si el diente est� siendo arrastrado
    private static bool cardInUse = false; // Controlar que solo una carta se use a la vez
    public bool isAnimating = false; // Indica si la carta est� siendo animada
    public bool isAnimatingButton = false; // Indicar si la carta est� siendo animada para usar el bot�n
    private Tween returnTween;       // Tween de retorno a posici�n
    private BoxCollider2D boxCollider; // Collider del objeto

    public Image dirtBarFill;   // Referencia a la imagen de relleno de la barra de suciedad
    public Image phBarFill;     // Referencia a la imagen de relleno de la barra de pH

    // M�todo de carga de datos
    public void LoadData(ToothCardData data)
    {
        dirtValue = data.dirtValue;
        toothPH = data.toothPH;
        state = data.state;
        durability = data.durability;
        isDirty = data.isDirty;
        hasFracture = data.hasFracture;

        UpdateToothSprite();         // Actualizar el sprite seg�n el estado
        UpdateDurabilityBasedOnPH(); // Actualizar la durabilidad de acuerdo al PH
        UpdateDurabilitySprite();    // Actualizar el sprite seg�n la durabilidad
        UpdateDirtVisual();          // Activar o desactivar visual de suciedad
        UpdateFractureVisual();      // Activar o desactivar visual de fractura                        
        UpdateProgressBars();        // Actualizar barras de suciedad y PH
    }

    // M�todo de guardado de datos
    public void SaveData(ToothCardData data)
    {
        data.dirtValue = dirtValue;
        data.toothPH = toothPH;
        data.state = state;
        data.durability = durability;
        data.isDirty = isDirty;
        data.hasFracture = hasFracture;      
    }

    // M�todo de inicio
    void Start()
    {
        // Obtener el BoxCollider2D para manejar las colisiones
        boxCollider = GetComponent<BoxCollider2D>();

        GameObject cutIcon = transform.Find("Cut").gameObject;
        GameObject tearIcon = transform.Find("Tear").gameObject;
        GameObject grindIcon = transform.Find("Grind").gameObject;

        if (grindAction == false)
        {
            grindIcon.SetActive(false);
        }
        if (tearAction == false)
        {
            tearIcon.SetActive(false);
        }
        if (cutAction == false)
        {
            cutIcon.SetActive(false);
        }
    }

    // M�todo de actualizaci�n
    void Update()
    {
        // Cancelar arrastre si el juego ha terminado
        GameController gc = Object.FindFirstObjectByType<GameController>();
        if (gc != null && (!gc.canUseCards || gc.isGameOver) && isDragging)
        {
            
            isDragging = false;
            cardInUse = false;
            boxCollider.enabled = true;
            transform.position = startPosition;
            AdjustOrderInLayer(-1);
            return;
        }

        // Si la carta est� siendo arrastrada, mover su posici�n seg�n la posici�n del mouse
        if (isDragging)
        {
            // Obtener la posici�n del mouse en el mundo y actualizar la posici�n de la carta
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0f);
        }
    }

    // M�todo al presionar el mouse
    void OnMouseDown()
    {
        // Cancelar arrastre si el juego ha terminado
        GameController gc = Object.FindFirstObjectByType<GameController>();
        if (gc != null && (!gc.canUseCards || gc.isGameOver)) return;

        // Si no hay ninguna carta siendo utilizada, se permite el arrastre
        if (!cardInUse)
        {
            isDragging = true;
            cardInUse = true;
            boxCollider.enabled = false;
            AdjustOrderInLayer(2);
        }
    }

    // M�todo al soltar el mouse
    void OnMouseUp()
    {
        isDragging = false;
        cardInUse = false;
        boxCollider.enabled = true;

        bool usedSuccessfully = false;
        bool usedOnFood = false;
        bool usedOnCare = false;

        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0f);

        foreach (var hit in hits)
        {
            if (hit == null || hit.gameObject == gameObject || hit.CompareTag("CardTooth")) continue;

            // Colisi�n con carta de comida
            if (hit.CompareTag("CardFood"))
            {
                FoodCard food = hit.GetComponent<FoodCard>();
                if (food != null)
                {
                    if (grindAction && food.grindAction)
                    {
                        food.grindAction = false;
                        Transform grindObj = food.transform.Find("Grind");
                        if (grindObj != null)
                        {
                            SpriteRenderer sr = grindObj.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                sr.color = new Color(0.5f, 0.5f, 0.5f); // Gris medio
                            }
                        }
                    }

                    if (cutAction && food.cutAction)
                    {
                        food.cutAction = false;
                        Transform cutObj = food.transform.Find("Cut");
                        if (cutObj != null)
                        {
                            SpriteRenderer sr = cutObj.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                sr.color = new Color(0.5f, 0.5f, 0.5f); // Gris medio
                            }
                        }
                    }

                    if (tearAction && food.tearAction)
                    {
                        food.tearAction = false;
                        Transform tearObj = food.transform.Find("Tear");
                        if (tearObj != null)
                        {
                            SpriteRenderer sr = tearObj.GetComponent<SpriteRenderer>();
                            if (sr != null)
                            {
                                sr.color = new Color(0.5f, 0.5f, 0.5f); // Gris medio
                            }
                        }
                    }

                    if (!food.grindAction && !food.cutAction && !food.tearAction)
                    {
                        food.PlayDestroyAnimation();
                        usedOnFood = true;

                        // Calcular y agregar puntos al puntaje global seg�n el estado y condici�n del diente
                        AwardScore();
                    }

                    usedSuccessfully = true;

                    dirtValue += food.dirtValue;
                    dirtValue = Mathf.Clamp(dirtValue, 0, 100);

                    if (dirtValue >= 50)
                    {
                        isDirty = true;
                    }
                    else
                    {
                        isDirty = false;
                    }

                    toothPH -= food.phImpact;
                    toothPH = Mathf.Clamp(toothPH, 0, 100);

                    // Evaluar el riesgo de fractura del diente en funci�n de su durabilidad y aplicar el resultado
                    EvaluateFractureRisk();
                }
            }

            // Colisi�n con carta de cuidado
            else if (hit.CompareTag("CardCare"))
            {
                CareCard care = hit.GetComponent<CareCard>();
                if (care != null)
                {
                    if (care.cureDirty)
                    {
                        dirtValue -= care.cleanValue;
                        dirtValue = Mathf.Clamp(dirtValue, 0, 100);
                    }

                    if (care.curePH)
                    {
                        toothPH += care.phRecovery;
                        toothPH = Mathf.Clamp(toothPH, 0, 100);
                    }

                    if (care.cureCaries)
                    {
                        // Solo reduce si el estado es 1 (Caries1) o 2 (Caries2)
                        if (state == 1 || state == 2)
                        {
                            state--;
                            state = Mathf.Clamp(state, 0, 2);
                        }
                    }

                    if (care.cureFracture && hasFracture)
                    {
                        hasFracture = false;
                    }

                    care.PlayDestroyAnimation();
                    usedSuccessfully = true;
                    usedOnCare = true;
                }
            }
        }

        if (usedSuccessfully)
        {
            // Actualizar barras de suciedad y PH
            UpdateProgressBars();

            // Actualizar la durabilidad de acuerdo al PH
            UpdateDurabilityBasedOnPH();

            // Si se uso la carta, realizar lo siguientes ajusten visuales en la carta
            UpdateToothSprite();      // Actualizar el sprite seg�n el estado
            UpdateDurabilitySprite(); // Actualizar el sprite seg�n la durabilidad
            UpdateDirtVisual();       // Activar o desactivar visual de suciedad
            UpdateFractureVisual();   // Activar o desactivar visual de fractura

            // Buscar la copia en runtime de los datos correspondiente
            ToothDeck deck = Object.FindFirstObjectByType<ToothDeck>();
            if (deck != null)
            {
                if (deck.runtimeData.TryGetValue(this.cardID, out ToothCardData runtimeCopy))
                {
                    SaveData(runtimeCopy); // Guardar los datos en la copia para no alterar el ScriptableObject original
                }
            }

            // Registro en el GameController
            GameController gc = Object.FindFirstObjectByType<GameController>();
            if (gc != null)
            {
                gc.RegisterToothCardUsed();
                if (usedOnFood) gc.RegisterFoodCardUsed();
                if (usedOnCare) gc.RegisterCareCardUsed();
                gc.CheckAndSpawnToothCardsIfNone();
            }

            PlayUseAnimation(); // Animaci�n al usar la carta
        }
        else
        {
            // Marcar como animando
            isAnimatingButton = true;

            // Desactivar el collider para evitar interacci�n durante la animaci�n
            boxCollider.enabled = false;

            // Animaci�n de volver la carta a la posici�n original
            // Adem�s, guardar el tween actual
            returnTween = transform.DOMove(startPosition, 0.3f)
                .SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    if (this != null)
                    {
                        AdjustOrderInLayer(-2);
                        boxCollider.enabled = true;
                        isAnimatingButton = false;
                        returnTween = null;
                    }
                });
        }
    }

    // Funci�n de animaci�n de uso de carta de diente
    public void PlayUseAnimation()
    {
        // Marcar como animando
        isAnimating = true;

        // Desactivar collider
        boxCollider.enabled = false;

        // Crear una secuencia de animaciones
        Sequence seq = DOTween.Sequence();

        float liftDuration = 0.15f;
        float fallDuration = 0.3f;
        Vector3 liftOffset = transform.position + new Vector3(0f, 1f, 0f);

        // Subir ligeramente antes de caer
        seq.Append(transform.DOMove(liftOffset, liftDuration).SetEase(Ease.OutQuad));

        // Movimiento hacia la posici�n objetivo
        seq.Append(transform.DOMove(new Vector3(-7.5f, -3f, 0f), fallDuration).SetEase(Ease.InBack));

        // Ca�da con rotaci�n y escala
        seq.Join(transform.DORotate(new Vector3(0f, 0f, 180f), fallDuration, RotateMode.FastBeyond360));
        seq.Join(transform.DOScale(Vector3.zero, fallDuration).SetEase(Ease.InQuad));

        // Destruir al finalizar
        seq.OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }

    // Cancela la animaci�n de retorno a la posici�n inicial si est� en curso
    public void CancelReturnAnimation()
    {
        if (returnTween != null && returnTween.IsActive())
        {
            returnTween.Kill();
            returnTween = null;
        }

        isAnimatingButton = false;
    }

    // Funci�n que actualiza el sprite visual del diente en base al estado actual
    private void UpdateToothSprite()
    {
        if (stateSprites != null && state >= 0 && state < stateSprites.Count)
        {
            // Buscar el hijo llamado "SplashArt"
            Transform splashArt = transform.Find("SplashArt");
            if (splashArt != null)
            {
                SpriteRenderer sr = splashArt.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = stateSprites[state];
                }
            }
        }
    }

    // Funci�n que actualiza el sprite de durabilidad seg�n el nivel actual de durabilidad
    private void UpdateDurabilitySprite()
    {
        if (durabilitySprites != null && durability >= 0 && durability < durabilitySprites.Count)
        {
            Transform durabilityObj = transform.Find("Durability");
            if (durabilityObj != null)
            {
                SpriteRenderer sr = durabilityObj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = durabilitySprites[durability];
                }
            }
        }
    }

    // Funci�n para activar o desactivar el objeto hijo "Dirt" seg�n el estado de isDirty
    private void UpdateDirtVisual()
    {
        Transform dirt = transform.Find("Dirt");
        if (dirt != null)
        {
            dirt.gameObject.SetActive(isDirty);
        }
    }

    // Funci�n para activar o desactivar el objeto hijo "Fracture" seg�n el estado de hasFracture
    private void UpdateFractureVisual()
    {
        Transform fracture = transform.Find("Fracture");
        if (fracture != null)
        {
            fracture.gameObject.SetActive(hasFracture);
        }
    }

    // Funci�n que actualiza la durabilidad del diente en base al valor actual de pH
    private void UpdateDurabilityBasedOnPH()
    {
        if (toothPH <= 25)
        {
            durability = 2;
        }
        else if (toothPH <= 50)
        {
            durability = 1;
        }
        else
        {
            durability = 0;
        }
    }


    // Funci�n que eval�a la probabilidad de que el diente sufra una fractura y actualiza el estado si ocurre
    private void EvaluateFractureRisk()
    {
        int probability = 0;

        // Asignar probabilidad seg�n la durabilidad
        if (durability == 1)
            probability = 25;
        else if (durability == 2)
            probability = 50;

        // Determinar si ocurre fractura
        int roll = Random.Range(0, 100);
        hasFracture = roll < probability;
    }

    // Funci�n para calcular y otorgar puntaje al GameController seg�n el estado y condici�n del diente
    private void AwardScore()
    {
        int points = 0;

        // Solo asignar puntos por el estado si el diente no est� fracturado
        if (!hasFracture)
        {
            switch (state)
            {
                case 0:
                    points += 100;
                    break;
                case 1:
                    points += 50;
                    break;
                case 2:
                    points += 25;
                    break;
            }
        }
        else
        {
            // Penalizaci�n por fractura
            points -= 100;
        }

        // Sumar al puntaje global, asegurando que nunca sea menor a 0
        GameController gc = Object.FindFirstObjectByType<GameController>();
        if (gc != null)
        {
            gc.score = Mathf.Max(0, gc.score + points);
        }
    }

    // Funci�n para ajustar din�micamente el orden de renderizado (sortingOrder)
    // para componentes SpriteRenderer y Canvas dentro del objeto y sus hijos.
    private void AdjustOrderInLayer(int delta)
    {
        // Ajustar el SpriteRenderer principal si existe
        SpriteRenderer rootRenderer = GetComponent<SpriteRenderer>();
        if (rootRenderer != null)
        {
            rootRenderer.sortingOrder += delta;
        }

        // Ajustar los SpriteRenderer hijos
        foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr != rootRenderer) // Evitar duplicado en el principal
            {
                sr.sortingOrder += delta;
            }
        }

        // Ajustar todos los Canvas del objeto y sus hijos
        foreach (Canvas canvas in GetComponentsInChildren<Canvas>())
        {
            canvas.sortingOrder += delta;
        }
    }

    // Funci�n para actualizar las barras visualmente
    public void UpdateProgressBars()
    {
        if (dirtBarFill != null)
            dirtBarFill.fillAmount = dirtValue / 100f;

        if (phBarFill != null)
            phBarFill.fillAmount = toothPH / 100f;
    }
}
