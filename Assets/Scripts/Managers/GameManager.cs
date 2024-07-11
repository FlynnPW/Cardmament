using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private TileManager tileManager;
    private PlayersManager playerManager;
    private CardUiManager cardUiManager;
    private ManaBarUiManager manaBarUiManager;
    const int MAX_SEED = 999;

    private void Start()
    {
        tileManager = GetComponent<TileManager>();
        playerManager = GetComponent<PlayersManager>();
        cardUiManager = GetComponent<CardUiManager>();
        manaBarUiManager = GetComponent<ManaBarUiManager>();
    }

    public void startGame()
    {
        int seed;
        seed = Random.Range(0, MAX_SEED);
        Debug.Log("Seed is: " + seed);
        playerManager.setupPlayers(new PlayersManager.playerType[] { PlayersManager.playerType.human, PlayersManager.playerType.human });
        tileManager.generateMap(seed);
        cardUiManager.onGameBegan();
        playerManager.onGameBegan();
        manaBarUiManager.onGameBegan();
    }
}
