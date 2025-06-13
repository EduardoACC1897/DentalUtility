using UnityEngine;

[CreateAssetMenu(menuName = "DentalUtility/Tooth Card Data")]
public class ToothCardData : ScriptableObject
{
    public string cardID;
    public int dirtValue;
    public int toothPH;
    public int state;
}
