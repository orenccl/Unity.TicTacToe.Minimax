using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridSpace : MonoBehaviour
{
    public Button button;
    public TextMeshProUGUI buttonText;

    private GameController gameController;

    public void SetSpace()
    {
        if(gameController.playerMove == true)
        {
            buttonText.text = gameController.GetPlayerSide().tmpPro.text;
            button.interactable = false;
            gameController.EndTurn();
        }
    }

    public void SetGameControllerReference(GameController controller)
    {
        gameController = controller;
    }
}
