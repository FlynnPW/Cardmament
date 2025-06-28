using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CardHand;
using static Player;

public class PlayersManager : MonoBehaviour
{
    private int nextPlayerIndex = 0;
    private Player currentPlayer = null;
    private Player[] players;
    private CardManager cardManager;
    private BoardDisplayManager boardDisplayManager;
    public enum playerType { human, cpu }

    public struct playerSetupData 
    {
        public playerSetupData(playerType type, Color colour)
        {
            this.type = type;
            this.colour = colour;
        }

        public playerType type;
        public Color colour;
    }

    private void Start()
    {
        cardManager = GetComponent<CardManager>();
        boardDisplayManager = GetComponent<BoardDisplayManager>();
    }

    public void setupPlayers(playerSetupData[] playersToSetup)
    {
        players = new Player[playersToSetup.Length];
        
        for (int i = 0; i < players.Length; i++)
        {
            switch (playersToSetup[i].type)
            {
                case playerType.human:
                    players[i] = new HumanPlayer(cardManager.getDeck(), playersToSetup[i].colour);
                    break;
                case playerType.cpu:
                    break;
            }

            boardDisplayManager.playerAddedToDisplay(players[i], playersToSetup[i].colour);
        }
    }

    public void onGameBegan()
    {
        beginTurnOfNextPlayerIndex();
    }

    public Player getCurrentPlayer()
    {
        return currentPlayer;
    }

    public Player[] getAllPlayers()
    {
        return players;
    }

    public void beginTurnOfNextPlayerIndex()
    {
        currentPlayer = players[nextPlayerIndex];
        currentPlayer.ourTurnBegun();
        nextPlayerIndex++;

        if (nextPlayerIndex == players.Length)
        {
            nextPlayerIndex = 0;
        }
    }

    public void endTurn()
    {
        beginTurnOfNextPlayerIndex();
    }

    public void subcribeToHumanPlayerCallbacks(newCard newCardSubscribe, removedCard removedCardSubscribe, ourHandTurn ourHandTurnSubscribe)
    {
        foreach (Player player in players)
        {
            if (player.isHuman() == false)
            {
                continue;
            }

            CardHand hand = player.getOurHand();
            hand.subscribeToCardAddedCallback(newCardSubscribe);
            hand.subscribeToCardRemovedCallback(removedCardSubscribe);
            hand.subscribeToHandTurnCallback(ourHandTurnSubscribe);
        }
    }

    public void subscribeToHumanTurnOfPlayerCallback(turnOfPlayer turnOfPlayerSubscribe)
    {
        foreach (Player player in players)
        {
            if (player.isHuman() == false)
            {
                continue;
            }

            player.subscribeToTurnOfPlayer(turnOfPlayerSubscribe);
        }
    }
}
