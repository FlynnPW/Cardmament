using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static CardManager;

public class CardInHand
{
    public delegate void cardHandPositionChanged(CardInHand us, bool resultOfCardRemoval);
    private cardHandPositionChanged cardHandPositionChangedCallback = null;
    private int handPosition;
    private Card ourCard;
    private CardHand hand;

    public CardInHand(int handPositionSet, Card ourCardSet, CardHand hand)
    {
        handPosition = handPositionSet;
        ourCard = ourCardSet;
        this.hand = hand;
    }

    public Card getCard()
    {
        return ourCard;
    }

    public CardHand getHandIn()
    {
        return hand;
    }

    public bool cardPlayed(Vector2Int at)
    {
        return ourCard.attemptToPlayCard(at);
    }

    public int getHandPosition()
    {
        return handPosition;
    }

    public void changeHandPositionTo(int to, bool resultOfCardRemoval)
    {
        handPosition = to;
        cardHandPositionChangedCallback(this, resultOfCardRemoval);
    }

    public void subscribeToCardHandPositionChange(cardHandPositionChanged newCardPositionCallback)
    {
        cardHandPositionChangedCallback += newCardPositionCallback;
    }
}
