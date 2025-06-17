using System.Collections.Generic;
using UnityEngine;

public class FoodDeck : MonoBehaviour
{
    public List<GameObject> foodCards; // Lista de prefabs de cartas de comida
    public Transform[] positions;      // Posiciones donde se colocarán las cartas en la escena

    // Función que instancia hasta 5 cartas de comida en posiciones específicas
    public void SpawnFoodCards()
    {
        // Determina cuántas cartas se pueden colocar como máximo, sin superar el número de posiciones ni el número de cartas disponibles
        int maxCount = Mathf.Min(positions.Length, foodCards.Count);

        // Bucle para colocar una carta en cada posición disponible
        for (int i = 0; i < maxCount; i++)
        {
            // Selecciona aleatoriamente un prefab de carta de comida de la lista
            GameObject prefab = foodCards[Random.Range(0, foodCards.Count)];

            // Instancia el prefab en la posición correspondiente
            Instantiate(prefab, positions[i].position, Quaternion.identity);
        }
    }
}
