using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class Player
{
    public delegate void turnOfPlayer(Player turnOf);
    private turnOfPlayer turnOfPlayerCallback;
    public delegate void manaAmountChanged(int to, int max);
    private manaAmountChanged manaAmountChangedCallback;
    protected CardHand ourHand;
    const int baseManaIncome = 10;
    const int baseMana = 100;
    const int maxMana = 200;
    private int manaIncome = baseManaIncome;
    private int mana = baseMana;

    public Player(Card[] deck)
    {
        ourHand = new CardHand(deck);
    }

    public CardHand getOurHand()
    {
        return ourHand;
    }

    public void ourTurnBegun()
    {
        setMana(Math.Min(mana + manaIncome, maxMana));
        ourHand.playerTurnBegan();

        if (turnOfPlayerCallback != null)
        {
            turnOfPlayerCallback(this);
        }
    }

    public void changeManaIncome(int byAmount)
    {
        manaIncome += byAmount;
    }

    public bool sufficientMana(int amount)
    {
        return mana - amount >= 0;
    }

    public void spendMana(int amount)
    {
        setMana(mana - amount);
    }

    private void setMana(int to)
    {
        mana = to;

        if (manaAmountChangedCallback != null)
        {
            manaAmountChangedCallback(to, maxMana);
        }
    }

    public int getMana()
    {
        return mana;
    }

    public int getMaxMana()
    {
        return maxMana;
    }

    //this leaves the possibilities open for a hand that all players could play from under certain conditions
    public bool canPlayFromHand(CardHand hand)
    {
        return hand == ourHand;
    }

    public abstract void takeTurnActions();

    public abstract bool isHuman();

    public void subscribeToTurnOfPlayer(turnOfPlayer subscribe)
    {
        turnOfPlayerCallback += subscribe;
    }

    public void subscribeToManaAmountChanged(manaAmountChanged subcribe)
    {
        manaAmountChangedCallback += subcribe;
    }

    public void unsubscribeToManaAmountChanged(manaAmountChanged unsubcribe)
    {
        manaAmountChangedCallback -= unsubcribe;
    }
}
