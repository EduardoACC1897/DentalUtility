using UnityEngine;

public class CareCard : MonoBehaviour
{
    public bool cureDirty;    // Indica si cura la suciedad
    public bool curePH;       // Indica si cura el ph
    public bool cureCaries;   // Indica si cura las caries
    public bool cureFracture; // Indica si cura las fracturas
    public int cleanValue;    // Cantidad de suciedad que elimina del diente
    public int phRecovery;    // Cantidad de ph que recupera del diente

}
