using System.Collections.Generic;
using UnityEngine;

public class ToothCard : MonoBehaviour
{
    // Variables públicas
    public string cardID; // ID de la carta
    public bool grindAction; // Indica si realiza la acción de moler
    public bool cutAction; // Indica si realiza la acción de cortar
    public bool tearAction; // Indica si realiza la acción de desgarrar
    public bool hasFracture; // Indica si el diente tiene una fractura
    public int dirtValue; // Valor de suciedad que tiene el diente
    public int toothPH; // Valor de ph que tiene el diente
    public int state; // Estado del diente: Limpio = 0, Sucio = 1, Caries1 = 2, Caries2 = 3
    public int durability; // Durabilidad del diente: Sano = 0, Comprometido = 1, Crítico = 2
    public List<Sprite> stateSprites; // Lista de sprites disponibles para representar visualmente el estado del diente

    // Variables privadas
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

        UpdateToothSprite(); // Actualizar el sprite según el estado
    }

    // Método de guardado de datos
    public void SaveData(ToothCardData data)
    {
        data.dirtValue = dirtValue;
        data.toothPH = toothPH;
        data.state = state;
        data.durability = durability;
        data.hasFracture = hasFracture;
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
            // Buscar el objeto de datos correspondiente
            ToothDeck deck = Object.FindFirstObjectByType<ToothDeck>();
            if (deck != null)
            {
                ToothCardData data = deck.toothCardDataList.Find(d => d.cardID == this.cardID);
                if (data != null)
                {
                    SaveData(data); // Guardar los datos antes de destruir
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
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = stateSprites[state];
            }
        }
    }

    // Función que permite activar o desactivar la fractura del diente
    public void SetFractureState(bool value)
    {
        hasFracture = value;
    }
}
