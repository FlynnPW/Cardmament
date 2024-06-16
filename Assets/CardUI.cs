using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public delegate void cardDraggingStopped(CardUI cardUiStoppedDragging);
    private cardDraggingStopped cardDraggingStoppedCallback;
    //this is the position that the card starts at, can be dragged away from it
    private Vector2 fixedPosition;
    [SerializeField]
    private RectTransform ourRectTransform;
    [SerializeField]
    private Text ourNameText;
    [SerializeField]
    private Text ourDescriptionText;
    [SerializeField]
    private Image ourBackground;
    [SerializeField]
    private Image ourImage;
    private CardInHand cardRepresenting;

    void Update()
    {
        
    }

    public void subscribeToCardDraggingStoppedCallback(cardDraggingStopped newCardDraggingCallback)
    {
        cardDraggingStoppedCallback += newCardDraggingCallback;
    }

    public void setCardRepresenting(CardInHand representing)
    {
        cardRepresenting = representing;
    }

    public CardInHand getCardRepresenting() 
    {
        return cardRepresenting; 
    }

    public void setFixedPosition(Vector2 to)
    {
        fixedPosition = to;
        setRealPositionTo(fixedPosition);
    }

    public void setRealPositionTo(Vector2 to)
    {
        ourRectTransform.anchoredPosition = fixedPosition;
    }

    public void setNameText(string to)
    {
        ourNameText.text = to;
    }

    public void setDescriptionText(string to)
    {
        ourDescriptionText.text = to;
    }

    public void setImageSprite(Sprite to)
    {
        ourImage.sprite = to;
    }

    public void startDragging()
    {
        CardUiManager.cardStartDragging(this);
    }

    public void setTransparency(float amount)
    {
        ourBackground.color = new Color(ourBackground.color.r, ourBackground.color.g, ourBackground.color.b, amount);
        ourImage.color = new Color(ourImage.color.r, ourImage.color.g, ourImage.color.b, amount);
    }

    public void endDragging()
    {
        CardUiManager.cardStopDragging(this);
        setRealPositionTo(fixedPosition);
        cardDraggingStoppedCallback(this);
    }
}
