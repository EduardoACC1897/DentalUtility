using System.Collections.Generic;
using UnityEngine;

public class FoodDeck : MonoBehaviour
{
    public List<GameObject> foodCards; // Lista de prefabs de cartas de comida
    public Transform[] positions;      // Posiciones donde se colocar�n las cartas en la escena

    // Funci�n que instancia hasta 5 cartas de comida en posiciones espec�ficas
    public void SpawnFoodCards()
    {
        // Determina cu�ntas cartas se pueden colocar como m�ximo, sin superar el n�mero de posiciones ni el n�mero de cartas disponibles
        int maxCount = Mathf.Min(positions.Length, foodCards.Count);

        // Bucle para colocar una carta en cada posici�n disponible
        for (int i = 0; i < maxCount; i++)
        {
            // Selecciona aleatoriamente un prefab de carta de comida de la lista
            GameObject prefab = foodCards[Random.Range(0, foodCards.Count)];

            // Instancia el prefab en la posici�n correspondiente
            Instantiate(prefab, positions[i].position, Quaternion.identity);
        }
    }
}
