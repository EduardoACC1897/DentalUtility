using UnityEngine;
using DG.Tweening;

public class CareCard : MonoBehaviour
{
    public int cleanValue;    // Cantidad de suciedad que elimina del diente
    public int phRecovery;    // Cantidad de ph que recupera del diente
    public bool cureDirty;    // Indica si cura la suciedad
    public bool curePH;       // Indica si cura el ph
    public bool cureCaries;   // Indica si cura las caries
    public bool cureFracture; // Indica si cura las fracturas

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
        gc.playCareCardConsumedSound();
    }
}
