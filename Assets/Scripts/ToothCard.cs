using UnityEngine;

public class ToothCard : MonoBehaviour
{
    // Variables públicas
    public bool grindAction;  // Indica si realiza la acción de moler
    public bool cutAction;    // Indica si realiza la acción de cortar
    public bool tearAction;   // Indica si realiza la acción de desgarrar
    public int dirtValue;     // Valor de suciedad que tiene el diente
    public int toothPH;     // Valor de ph que tiene el diente

    // Variables privadas
    private int state; // Estado del diente: Limpio = 0, Sucio = 1, Caries1 = 2, Caries2 = 3, Fractura = 4
    private Vector3 startPosition; // Posición inicial del diente
    private bool isDragging = false; // Controlar si el diente está siendo arrastrado
    private static bool cardInUse = false; // Controlar que solo una carta se use a la vez
    private BoxCollider2D boxCollider; // Collider del objeto

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
        // Detener el arrastre
        isDragging = false;
        cardInUse = false; // Marcar que ya no hay cartas en uso
        boxCollider.enabled = true; // Reactivar el collider para detectar colisiones nuevamente

        // Comprobar manualmente las colisiones del objeto
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0f);

        // Recorrer todos los objetos que colisionan con el diente
        foreach (var hit in hits)
        {
            // Evitar que procese si el collider es null, es el mismo objeto o tiene el mismo tag
            if (hit == null || hit.gameObject == gameObject || hit.CompareTag("CardTooth")) continue;

            // Colisión con carta de comida ("CardFood")
            if (hit.CompareTag("CardFood"))
            {
                // Obtener el componente CardFood de la carta con la que colisionó
                FoodCard food = hit.GetComponent<FoodCard>();
                if (food != null)
                {
                    // Interacción entre acciones
                    if (grindAction && food.grindAction) food.grindAction = false;
                    if (cutAction && food.cutAction) food.cutAction = false;
                    if (tearAction && food.tearAction) food.tearAction = false;

                    // Si todas las acciones se cumplieron, destruir la comida
                    if (!food.grindAction && !food.cutAction && !food.tearAction)
                    {
                        Destroy(hit.gameObject); // Destruir la carta de comida
                    }

                    // Aumentar la suciedad del diente según la comida
                    dirtValue += food.dirtValue;
                    dirtValue = Mathf.Clamp(dirtValue, 0, 100);
                    // Disminuir el ph del diente según la comida
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

                    Destroy(hit.gameObject); // Destruir la carta una vez aplicada
                }
            }
        }

        // Volver la carta a la posición inicial
        transform.position = startPosition;
    }
}
