using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Player
{
    public Image panel;
    public TextMeshProUGUI tmpPro;
    public Button button;
}

[System.Serializable]
public class PlayerColor
{
    public Color panelColor;
    public Color textColor;
}

public struct Move
{
    public Move(int i, int j)
    {
        row = i;
        col = j;
    }

    public int row { get; }
    public int col { get; }
}

public class GameController : MonoBehaviour
{
    // 從 inspector 抓取 3x3 格子位置
    public TextMeshProUGUI[] buttonList;
    // 儲存 3x3 格子位置的2維陣列
    public TextMeshProUGUI[,] buttonRowCol;

    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public GameObject restartButton;
    public GameObject startInfo;

    public Player playerX;
    public Player playerO;
    public PlayerColor activePlayerColor;
    public PlayerColor inactivePlayerColor;

    public float delay;
    public bool playerMove;
    private float delayTimer;
    private bool isRunning;
    private int moveCount;

    private int rowValue;
    private int colValue;
    
    private Player playerSide;
    private Player computerSide;
    private Dictionary<string, float> scoresLookup;

    private void Awake()
    {
        // 將1維轉換成2維方便處理資料
        SetButtonRowCol();
        SetGameControllerReferenceOnButton();
        SetBoardInteractable(false);

        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);
        playerMove = true;
        isRunning = false;
        moveCount = 0;

        foreach (var button in buttonRowCol)
        {
            button.text = "";
        }
    }

    private void Update()
    {
        if (!playerMove && isRunning)
        {
            delayTimer += delay * Time.deltaTime;
            if(delayTimer >= 100)
            {
                AIBestMove();
            }
        }
    }

    // 用MinMax演算法找出最佳解
    private void AIBestMove()
    {
        // 如果讓電腦下第一步，下中間
        if (moveCount == 0)
        {
            if (buttonRowCol[1, 1].text == "")
            {
                buttonRowCol[1, 1].text = GetComputerSide().tmpPro.text;
                buttonRowCol[1, 1].GetComponentInParent<Button>().interactable = false;
                EndTurn();
                return;
            }
        }

        float bestScore = -10000; // -Infinity

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (buttonRowCol[i, j].text == "")
                {
                    buttonRowCol[i, j].text = GetComputerSide().tmpPro.text;
                    float score = MinMax(0, false);
                    buttonRowCol[i, j].text = "";

                    Debug.Log((i * 3 + j) + " : " + score);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        rowValue = i;
                        colValue = j;
                    }
                }
            }
        }

        buttonRowCol[rowValue, colValue].text = GetComputerSide().tmpPro.text;
        buttonRowCol[rowValue, colValue].GetComponentInParent<Button>().interactable = false;
        EndTurn();
    }

    // 算出MinMax分數
    private float MinMax(int depth, bool isMaximizing)
    {
        string winner = CheckWinner();

        if (winner != "")
        {
            // 依照深度加成分數，越深分數越低
            return scoresLookup[winner] - depth;
        }

        if (isMaximizing)
        {
            float bestScore = -10000; // -Infinity;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (buttonRowCol[i, j].text == "")
                    {
                        buttonRowCol[i, j].text = GetComputerSide().tmpPro.text;
                        float score = MinMax(depth + 1, false);
                        buttonRowCol[i, j].text = "";

                        bestScore = Mathf.Max(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
        else
        {
            float bestScore = 10000; // Infinity;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (buttonRowCol[i, j].text == "")
                    {
                        buttonRowCol[i, j].text = GetPlayerSide().tmpPro.text;
                        float score = MinMax(depth + 1, true);
                        buttonRowCol[i, j].text = "";
                        bestScore = Mathf.Min(score, bestScore);
                    }
                }
            }
            return bestScore;
        }
    }

    public void SetStartSide(string startingSide)
    {
        if(startingSide == "O")
        {
            playerSide = playerO;
            computerSide = playerX;
            scoresLookup = new Dictionary<string, float>()
            {
                { "X", 10 },
                { "O", -10 },
                { "Draw", 0 }
            };
            playerMove = true;
        }
        else
        {
            computerSide = playerO;
            playerSide = playerX;
            scoresLookup = new Dictionary<string, float>()
            {
                { "X", -10 },
                { "O", 10 },
                { "Draw", 0 }
            };
            playerMove = false;
        }
        SetPlayerColors(playerO, playerX);
        StartGame();
    }

    public void StartGame()
    {
        SetBoardInteractable(true);
        SetPlayerButtons(false);
        startInfo.SetActive(false);
        isRunning = true;
    }

    public void EndTurn()
    {
        moveCount++;
        ChangePlayerSides();

        string winner = CheckWinner();
        if (winner != "")
        {
            GameOver(winner);
            isRunning = false;
        }
    }

    public void GameOver(string winnerPlayer)
    {
        SetBoardInteractable(false);

        if (winnerPlayer == "Draw")
        {
            SetPlayerColorInactive();
            SetGameOverText("It's a draw!");
        }
        else
        {
            SetGameOverText(winnerPlayer + " Wins");
        }
    }

    public void RestartGame()
    {
        SetPlayerButtons(true);
        SetPlayerColorInactive();

        startInfo.SetActive(true);
        gameOverPanel.SetActive(false);
        restartButton.SetActive(false);

        delayTimer = 0;
        moveCount = 0;
        playerMove = true;
        isRunning = false;

        foreach (var button in buttonRowCol)
        {
            button.text = "";
        }
    }

    public Player GetComputerSide()
    {
        return computerSide;
    }

    public Player GetPlayerSide()
    {
        return playerSide;
    }

    private string CheckWinner()
    {
        string winner = "";

        // Check Col
        for (int i = 0; i < 3; i++)
        {
            if(buttonRowCol[i, 0].text == "")
            {
                continue;
            }

            if (buttonRowCol[i, 0].text == buttonRowCol[i, 1].text &&
                buttonRowCol[i, 0].text == buttonRowCol[i, 2].text)
            {
                winner = buttonRowCol[i, 0].text;
            }
        }

        // Check Row
        for (int i = 0; i < 3; i++)
        {
            if (buttonRowCol[0, i].text == "")
            {
                continue;
            }

            if (buttonRowCol[0, i].text == buttonRowCol[1, i].text &&
                buttonRowCol[0, i].text == buttonRowCol[2, i].text)
            {
                winner = buttonRowCol[0, i].text;
            }
        }

        // Check Diagonal
        if (buttonRowCol[1, 1].text != "")
        {
            if (buttonRowCol[0, 0].text == buttonRowCol[1, 1].text &&
                buttonRowCol[1, 1].text == buttonRowCol[2, 2].text)
            {
                winner = buttonRowCol[1, 1].text;
            }
            else if (buttonRowCol[0, 2].text == buttonRowCol[1, 1].text &&
                        buttonRowCol[1, 1].text == buttonRowCol[2, 0].text)
            {
                winner = buttonRowCol[1, 1].text;
            }
        }

        int openSpots = 0;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (buttonRowCol[i, j].text == "")
                {
                    openSpots++;
                }
            }
        }

        if (winner == "" && openSpots == 0)
        {
            return "Draw";
        }
        else
        {
            return winner;
        }
    }

    // 尋找可以下棋的位置
    private List<Move> GetAvailableMove()
    {
        List<Move> availableMove = new List<Move>();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (buttonRowCol[i, j].text == "")
                {
                    availableMove.Add(new Move(i, j));
                }
            }
        }
        return availableMove;
    }

    private void ChangePlayerSides()
    {
        playerMove = (playerMove == true) ? false : true;

        if (playerMove == true)
        {
            SetPlayerColors(playerSide, computerSide);
        }
        else
        {
            SetPlayerColors(computerSide, playerSide);
        }
    }

    private void SetPlayerColors(Player newPlayer, Player oldPlayer)
    {
        newPlayer.panel.color = activePlayerColor.panelColor;
        newPlayer.tmpPro.color = activePlayerColor.textColor;
        oldPlayer.panel.color = inactivePlayerColor.panelColor;
        oldPlayer.tmpPro.color = inactivePlayerColor.textColor;
    }

    private void SetGameOverText(string value)
    {
        gameOverPanel.SetActive(true);
        restartButton.SetActive(true);
        gameOverText.text = value;
    }

    private void SetBoardInteractable(bool toggle)
    {
        foreach (var button in buttonRowCol)
        {
            button.GetComponentInParent<Button>().interactable = toggle;
            if (toggle)
            {
                button.text = "";
            }
        }
    }

    private void SetPlayerButtons(bool toggle)
    {
        playerX.button.interactable = toggle;
        playerO.button.interactable = toggle;
    }

    private void SetPlayerColorInactive()
    {
        playerX.panel.color = inactivePlayerColor.panelColor;
        playerX.tmpPro.color = inactivePlayerColor.textColor;
        playerO.panel.color = inactivePlayerColor.panelColor;
        playerO.tmpPro.color = inactivePlayerColor.textColor;
    }

    private void SetGameControllerReferenceOnButton()
    {
        foreach (var button in buttonRowCol)
        {
            button.GetComponent<GridSpace>().SetGameControllerReference(this);
        }
    }

    private void SetButtonRowCol()
    {
        buttonRowCol = new TextMeshProUGUI[3, 3]
        {
            {buttonList[0], buttonList[1], buttonList[2] },
            {buttonList[3], buttonList[4], buttonList[5] },
            {buttonList[6], buttonList[7], buttonList[8] }
        };

        buttonList = null;
    }
}
