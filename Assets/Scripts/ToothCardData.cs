using UnityEngine;

[CreateAssetMenu(menuName = "DentalUtility/Tooth Card Data")]
public class ToothCardData : ScriptableObject
{
    public string cardID;      // ID único para enlazar datos con la carta
    public int dirtValue;      // Nivel de suciedad del diente
    public int toothPH;        // Nivel de pH del diente (0-100)
    public int state;          // Estado del diente: Limpio = 0, Sucio = 1, Caries1 = 2, Caries2 = 3
    public int durability;     // Durabilidad del diente: Sano = 0, Comprometido = 1, Crítico = 2
    public bool isDirty;       // Si el diente está sucio o no
    public bool hasFracture;   // Fractura o no del diente

    public ToothCardData Clone()
    {
        ToothCardData copy = ScriptableObject.CreateInstance<ToothCardData>();
        copy.cardID = this.cardID;
        copy.dirtValue = this.dirtValue;
        copy.toothPH = this.toothPH;
        copy.state = this.state;
        copy.durability = this.durability;
        copy.isDirty = this.isDirty;
        copy.hasFracture = this.hasFracture;  
        return copy;
    }
}
