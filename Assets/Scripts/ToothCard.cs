using System.Collections.Generic;
using UnityEngine;

public class ToothCard : MonoBehaviour
{
    public string cardID; // ID de la carta
    public bool grindAction; // Indica si realiza la acción de moler
    public bool cutAction; // Indica si realiza la acción de cortar
    public bool tearAction; // Indica si realiza la acción de desgarrar
    public bool hasFracture; // Indica si el diente tiene una fractura
    public bool isDirty; // Indica si el diente está sucio
    public int dirtValue; // Valor de suciedad que tiene el diente
    public int toothPH; // Valor de ph que tiene el diente
    public int state; // Estado del diente: Limpio = 0, Caries1 = 2, Caries2 = 3
    public int durability; // Durabilidad del diente: Sano = 0, Comprometido = 1, Crítico = 2
    public List<Sprite> stateSprites; // Lista de sprites disponibles para representar visualmente el estado del diente

    private Vector3 startPosition; // Posición inicial del diente
    private bool isDragging = false; // Controlar si el diente está siendo arrastrado
    private static bool cardInUse = false; // Controlar que solo una carta se use a la vez
    private BoxCollider2D boxCollider; // Collider del objeto

    // Método de carga de datos
    public void LoadData(ToothCardData data)
    {
        dirtValue = data.dirtValue;
        toothPH = data.toothPH;
        state = data.state;
        durability = data.durability;
        hasFracture = data.hasFracture;
        isDirty = data.isDirty;

        UpdateToothSprite();    // Actualizar el sprite según el estado
        UpdateFractureVisual(); // Activar o desactivar visual de fractura
        UpdateDirtVisual();     // Activar o desactivar visual de suciedad
    }

    // Método de guardado de datos
    public void SaveData(ToothCardData data)
    {
        data.dirtValue = dirtValue;
        data.toothPH = toothPH;
        data.state = state;
        data.durability = durability;
        data.hasFracture = hasFracture;
        data.isDirty = isDirty;
    }

    // Método de inicio
    void Start()
    {
        // Guardamos la posición inicial del diente
        startPosition = transform.position;
        // Obtener el BoxCollider2D para manejar las colisiones
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Método de actualización
    void Update()
    {
        // Si la carta está siendo arrastrada, mover su posición según la posición del mouse
        if (isDragging)
        {
            // Obtener la posición del mouse en el mundo y actualizar la posición de la carta
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, 0f);
        }
    }

    // Método al presionar el mouse
    void OnMouseDown()
    {
        // Si no hay ninguna carta siendo utilizada, se permite el arrastre
        if (!cardInUse)
        {
            isDragging = true;
            cardInUse = true; // Marcar que una carta está en uso
            boxCollider.enabled = false; // Desactivar el collider para evitar colisiones mientras se arrastra
        }
    }

    // Método al soltar el mouse
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

            // Colisión con carta de comida
            if (hit.CompareTag("CardFood"))
            {
                FoodCard food = hit.GetComponent<FoodCard>();
                if (food != null)
                {
                    if (grindAction && food.grindAction) food.grindAction = false;
                    if (cutAction && food.cutAction) food.cutAction = false;
                    if (tearAction && food.tearAction) food.tearAction = false;

                    if (!food.grindAction && !food.cutAction && !food.tearAction)
                    {
                        Destroy(hit.gameObject);
                        usedOnFood = true;
                    }

                    usedSuccessfully = true;

                    dirtValue += food.dirtValue;
                    dirtValue = Mathf.Clamp(dirtValue, 0, 100);

                    // Verificación de suciedad
                    if (dirtValue >= 50)
                    {
                        SetDirtyState(true);
                    }
                    else
                    {
                        SetDirtyState(false);
                    }

                    toothPH -= food.phImpact;
                    toothPH = Mathf.Clamp(toothPH, 0, 100);
                }
            }

            // Colisión con carta de cuidado
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

                    Destroy(hit.gameObject);
                    usedSuccessfully = true;
                    usedOnCare = true;
                }
            }
        }

        if (usedSuccessfully)
        {   
            // Si se uso la carta, cambiar el sprite de acuerdo al estado en el que quedo
            UpdateToothSprite();

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
                if (gc.totalToothCards > 0)
                {
                    gc.RegisterToothCardUsed();
                }
                gc.CheckAndSpawnToothCardsIfNone();       
                if (usedOnFood) gc.RegisterFoodCardUsed();
                if (usedOnCare) gc.RegisterCareCardUsed();
            }

            Destroy(gameObject); // Eliminar carta de la escena
        }
        else
        {
            transform.position = startPosition;
        }
    }

    // Función que actualiza el sprite visual del diente en base al estado actual
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

    // Función que permite activar o desactivar la fractura del diente
    public void SetFractureState(bool value)
    {
        hasFracture = value;
        UpdateFractureVisual();
    }

    // Activa o desactiva el objeto hijo "Fracture" según el estado de hasFracture
    private void UpdateFractureVisual()
    {
        Transform fracture = transform.Find("Fracture");
        if (fracture != null)
        {
            fracture.gameObject.SetActive(hasFracture);
        }
    }

    // Función que permite activar o desactivar la suciedad del diente
    public void SetDirtyState(bool value)
    {
        isDirty = value;
        UpdateDirtVisual();
    }

    // Activa o desactiva el objeto hijo "Dirt" según el estado de isDirty
    private void UpdateDirtVisual()
    {
        Transform dirt = transform.Find("Dirt");
        if (dirt != null)
        {
            dirt.gameObject.SetActive(isDirty);
        }
    }

}
