using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHand
{
    public delegate void newCard(CardInHand newCard);
    public delegate void removedCard(CardInHand removedCard);
    public delegate void ourHandTurn(List<CardInHand> cardsInHand);
    private newCard cardAddedCallback;
    private removedCard cardRemovedCallback;
    private ourHandTurn ourHandTurnCallback;
    [SerializeField]
    private Card[] deck;
    private List<CardInHand> cardsInHand = new List<CardInHand>();

    public CardHand(Card[] deck)
    {
        this.deck = deck;
    }

    public void subscribeToCardAddedCallback(newCard newCardCallback)
    {
        cardAddedCallback += newCardCallback;
    }

    public void subscribeToCardRemovedCallback(removedCard newCardCallback)
    {
        cardRemovedCallback += newCardCallback;
    }

    public void subscribeToHandTurnCallback(ourHandTurn newOurHandTurnCallback)
    {
        ourHandTurnCallback += newOurHandTurnCallback;
    }

    public void playerTurnBegan()
    {
        if (ourHandTurnCallback != null)
        {
            ourHandTurnCallback(cardsInHand);
        }
        
        drawCardFromDeck();
    }

    private void drawCardFromDeck()
    {
        int cardToChooseIndex = Random.Range(0, deck.Length);
        Card newCard = deck[cardToChooseIndex];
        //The card in hand handles an instance of a card created (when the player actually has it as opposed to in the deck)
        CardInHand newCardInstance = new CardInHand(cardsInHand.Count, newCard, this);
        cardsInHand.Add(newCardInstance);

        if (cardAddedCallback != null)
        {
            cardAddedCallback(newCardInstance);
        }  
    }

    public bool playCard(CardInHand toPlay, Vector2Int position)
    {
        if (cardsInHand.Contains(toPlay) == false)
        {
            Debug.LogError("Cannot play card as it isn't in hand!");
            return false;
        }

        if (toPlay.cardPlayed(position) == false)
        {
            return false;
        }

        //shift each card back one
        for (int i = cardsInHand.IndexOf(toPlay) + 1; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].changeHandPositionTo(i - 1, true);
        }

        cardsInHand.Remove(toPlay);

        if (cardRemovedCallback != null)
        {
            cardRemovedCallback(toPlay);
        }
        
        return true;
    }
}
