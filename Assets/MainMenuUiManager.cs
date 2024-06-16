using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUiManager : MonoBehaviour
{
    [SerializeField]
    private Button startButton;
    private PlayersManager playersManager;
    private CardUiManager cardUiManager;

    private void Start()
    {
        playersManager = GetComponent<PlayersManager>();
        cardUiManager = GetComponent<CardUiManager>();
    }

    public void startGame()
    {
        cardUiManager.onGameBegan();
        playersManager.onGameBegan();
        startButton.gameObject.SetActive(false);
    }
}
