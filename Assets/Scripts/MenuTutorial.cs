using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuTutorial : MonoBehaviour
{
    public Image tutorialImage; // Referencia al componente Image de Unity que muestra el tutorial
    public List<Sprite> tutorialSprites; // Lista de imágenes del tutorial
    public GameObject leftBtn, rightBtn; // Botones para navegar
    public int counter = 0; // Índice de imagen actual

    void Start()
    {
        ShowCurrentImage();
        UpdateButtons();
    }

    public void leftButton()
    {
        if (counter > 0)
        {
            counter--;
            ShowCurrentImage();
            UpdateButtons();
        }
    }

    public void rightButton()
    {
        if (counter < tutorialSprites.Count - 1)
        {
            counter++;
            ShowCurrentImage();
            UpdateButtons();
        }
    }

    public void backButton()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Inicio");
    }

    void ShowCurrentImage()
    {
        tutorialImage.sprite = tutorialSprites[counter];
    }

    void UpdateButtons()
    {
        leftBtn.SetActive(counter > 0);
        rightBtn.SetActive(counter < tutorialSprites.Count - 1);
    }
}
