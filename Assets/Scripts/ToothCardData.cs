using UnityEngine;

[CreateAssetMenu(menuName = "DentalUtility/Tooth Card Data")]
public class ToothCardData : ScriptableObject
{
    public string cardID;      // ID �nico para enlazar datos con la carta
    public int dirtValue;      // Nivel de suciedad del diente
    public int toothPH;        // Nivel de pH del diente (0-100)
    public int state;          // Estado del diente: Limpio = 0, Sucio = 1, Caries1 = 2, Caries2 = 3
    public int durability;     // Durabilidad del diente: Sano = 0, Comprometido = 1, Cr�tico = 2
    public bool hasFracture;   // Fractura o no del diente
    public bool isDirty;       // Si el diente est� sucio o no

    public ToothCardData Clone()
    {
        ToothCardData copy = ScriptableObject.CreateInstance<ToothCardData>();
        copy.cardID = this.cardID;
        copy.dirtValue = this.dirtValue;
        copy.toothPH = this.toothPH;
        copy.state = this.state;
        copy.durability = this.durability;
        copy.hasFracture = this.hasFracture;
        copy.isDirty = this.isDirty;
        return copy;
    }
}
