using UnityEngine;
using DG.Tweening;

public class FoodCard : MonoBehaviour
{
    public int dirtValue;    // Valor de suciedad que aplica al diente
    public int phImpact;     // Valor de impacto en el ph del diente que tiene el alimento
    public bool grindAction; // Indica si necesita la acción de moler
    public bool cutAction;   // Indica si necesita la acción de cortar
    public bool tearAction;  // Indica si necesita la acción de desgarrar

    private BoxCollider2D boxCollider; // Referencia al collider de la carta

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Función para animar la destrucción de la carta
    public void PlayDestroyAnimation()
    {
        GameController gc = Object.FindFirstObjectByType<GameController>();
        // Desactivar el collider para evitar interacción durante la animación
        boxCollider.enabled = false;

        // Mover hacia arriba y luego destruir
        transform.DOMoveY(transform.position.y + 5f, 0.3f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                Destroy(gameObject);
            });
        gc.playFoodCardConsumedSound();
    }
}
