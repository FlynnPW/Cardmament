using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    private Card[] deck;
    private PlayersManager playerManager;

    private void Start()
    {
        playerManager = GetComponent<PlayersManager>();
    }

    public void recieveCards(Card[] cards)
    {
        deck = cards;
    }

    public Card[] getDeck()
    {
        return deck;
    }

    public bool playCard(CardInHand cardToPlay, Vector2Int at)
    {
        Player currentPlayer = playerManager.getCurrentPlayer();
        CardHand handOfCard = cardToPlay.getHandIn();
        Card cardAttributes = cardToPlay.getCard();

        if (currentPlayer.sufficientMana(cardAttributes.getManaCost()) == false)
        {
            return false;
        }

        if (currentPlayer.canPlayFromHand(handOfCard) == false)
        {
            return false;
        }

        //bit weird to play from hand we get from card, maybe change this?
        if (handOfCard.playCard(cardToPlay, at) == false)
        {
            return false;
        }

        currentPlayer.spendMana(cardAttributes.getManaCost());
        return true;
    }
}
