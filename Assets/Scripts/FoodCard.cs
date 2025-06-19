using UnityEngine;

public class FoodCard : MonoBehaviour
{
    public int dirtValue;    // Valor de suciedad que aplica al diente
    public int phImpact;     // Valor de impacto en el ph del diente que tiene el alimento
    public bool grindAction; // Indica si necesita la acción de moler
    public bool cutAction;   // Indica si necesita la acción de cortar
    public bool tearAction;  // Indica si necesita la acción de desgarrar
}
