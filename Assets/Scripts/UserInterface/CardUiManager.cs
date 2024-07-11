using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardUiManager : MonoBehaviour
{
    //CardUI = UI represenation of cards
    private static CardUI cardUiDragging = null;
    private Dictionary<CardInHand, CardUI> cardUIs = new Dictionary<CardInHand, CardUI>();
    private CardManager mainManager;
    private PlayersManager playerManager;
    [SerializeField]
    private Image cardPlacePositionIndicator;
    [SerializeField]
    private GameObject cardPrefab;
    [SerializeField]
    private Canvas ourCanvas;
    [SerializeField]
    private Camera mainCamera;
    private TileManager tileManager;
    private Vector2Int mouseGridPosition;
    const float Y_POSITION_OF_CARDS = 115;
    const float X_DISTANCE_BETWEEN_CARDS = 160;
    const float TRANSPARENCY_OF_CARD_WHEN_ABOVE_BOARD = 0.6f;

    // Start is called before the first frame update
    void Start()
    {
        tileManager = GetComponent<TileManager>();
        mainManager = GetComponent<CardManager>();
        playerManager = GetComponent<PlayersManager>();
    }

    public void onGameBegan()
    {
        playerManager.subcribeToHumanPlayerCallbacks(newCard, removedCard, newHandToDisplay);
    }

    // Update is called once per frame
    void Update()
    {
        mouseGridPosition = Vector2Int.RoundToInt((Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition));

        if (tileManager.isTileWithinBounds(mouseGridPosition))
        {
            cardPlacePositionIndicator.enabled = true;
            cardPlacePositionIndicator.transform.position = (Vector2)mouseGridPosition;

            if (cardUiDragging != null)
            {
                cardUiDragging.setTransparency(TRANSPARENCY_OF_CARD_WHEN_ABOVE_BOARD);
            }
        }
        else
        {
            cardPlacePositionIndicator.enabled = false;

            if (cardUiDragging != null)
            {
                cardUiDragging.setTransparency(1);
            }
        }

        if (cardUiDragging != null)
        {
            cardUiDragging.transform.position = Input.mousePosition;
        }
    }

    public static void cardStartDragging(CardUI dragging)
    {
        cardUiDragging = dragging;
    }

    public static void cardStopDragging(CardUI dragging)
    {
        if (cardUiDragging != dragging)
        {
            Debug.LogError("Card stopped dragging even though its not currently the card being dragged");
            return;
        }

        cardUiDragging.setTransparency(1);
        cardUiDragging = null;
    }

    public void newCard(CardInHand newCard)
    {
        CardUI newCardUI = Instantiate(cardPrefab, ourCanvas.transform).GetComponent<CardUI>();
        cardUIs.Add(newCard, newCardUI);
        newCardUI.setCardRepresenting(newCard);
        Card card = newCard.getCard();
        newCardUI.setNameText(card.getName());
        newCardUI.setDescriptionText(card.getDescription());
        newCardUI.setImageSprite(card.getImage());
        //subscribe to hand position change callback, when our position in the hand changes, change it in the ui UNLESS it is done as result of card removal
        //this is already handeled in the removed card function
        newCard.subscribeToCardHandPositionChange((card, cardRemoval) => { if (cardRemoval == false) { setFixedPositionOfCard(newCardUI); } });
        newCardUI.subscribeToCardDraggingStoppedCallback(cardUiDragged);
        setAllFixedPositions();
    }

    public void removedCard(CardInHand cardRemoved)
    {
        CardUI cardUI = cardUIs[cardRemoved];
        Destroy(cardUI.gameObject);
        cardUIs.Remove(cardRemoved);
        setAllFixedPositions();
    }

    public void newHandToDisplay(List<CardInHand> handCards)
    {
        while (cardUIs.Values.Count > 0)
        {
            removedCard(cardUIs.Values.ElementAt(0).getCardRepresenting());
        }

        foreach (CardInHand cardToAdd in handCards)
        {
            newCard(cardToAdd);
        }
    }

    private void setAllFixedPositions()
    {
        for (int i = 0; i < cardUIs.Count; i++)
        {
            setFixedPositionOfCard(cardUIs.Values.ElementAt(i));
        }
    }

    public void cardUiDragged(CardUI cardUi)
    {
        mainManager.playCard(cardUi.getCardRepresenting(), mouseGridPosition);
    }

    public void setFixedPositionOfCard(CardUI toSet)
    {
        CardInHand inHandCard = toSet.getCardRepresenting();
        toSet.setFixedPosition(new Vector2(X_DISTANCE_BETWEEN_CARDS * (-(cardUIs.Count - 1) / (float)2 + inHandCard.getHandPosition()), Y_POSITION_OF_CARDS));
    }
}
