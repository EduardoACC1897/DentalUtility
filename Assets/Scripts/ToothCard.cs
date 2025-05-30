using UnityEngine;

public class ToothCard : MonoBehaviour
{
    // Variables p�blicas
    public bool grindAction;  // Indica si realiza la acci�n de moler
    public bool cutAction;    // Indica si realiza la acci�n de cortar
    public bool tearAction;   // Indica si realiza la acci�n de desgarrar
    public int dirtValue;     // Valor de suciedad que tiene el diente
    public int toothPH;     // Valor de ph que tiene el diente

    // Variables privadas
    private int state; // Estado del diente: Limpio = 0, Sucio = 1, Caries1 = 2, Caries2 = 3, Fractura = 4
    private Vector3 startPosition; // Posici�n inicial del diente
    private bool isDragging = false; // Controlar si el diente est� siendo arrastrado
    private static bool cardInUse = false; // Controlar que solo una carta se use a la vez
    private BoxCollider2D boxCollider; // Collider del objeto

    // M�todo de inicio
    void Start()
    {
        // Guardamos la posici�n inicial del diente
        startPosition = transform.position;
        // Obtener el BoxCollider2D para manejar las colisiones
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // M�todo de actualizaci�n
    void Update()
    {
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
        // Si no hay ninguna carta siendo utilizada, se permite el arrastre
        if (!cardInUse)
        {
            isDragging = true;
            cardInUse = true; // Marcar que una carta est� en uso
            boxCollider.enabled = false; // Desactivar el collider para evitar colisiones mientras se arrastra
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

                    Destroy(hit.gameObject);
                    usedSuccessfully = true;
                    usedOnCare = true;
                }
            }
        }

        if (usedSuccessfully)
        {
            // Registro del uso en el GameController
            GameController gc = Object.FindFirstObjectByType<GameController>();
            if (gc != null)
            {
                gc.RegisterToothCardUsed();

                if (usedOnFood)
                    gc.RegisterFoodCardUsed();

                if (usedOnCare)
                    gc.RegisterCareCardUsed();
            }

            Destroy(gameObject); // Destruir la carta de diente
        }
        else
        {
            transform.position = startPosition;
        }
    }
}
