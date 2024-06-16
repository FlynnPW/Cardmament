using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBarUiManager : MonoBehaviour
{
    [SerializeField]
    private Image manaBar;
    [SerializeField]
    private Image manaBarMask;
    private Vector2 manaBarPosition;
    private PlayersManager playerManager;
    private Player playerDisplaying = null;
    const float waveSpeed = 15;

    void Start()
    {
        playerManager = GetComponent<PlayersManager>();
        playerManager.subscribeToHumanTurnOfPlayerCallback((player) => { nowTurnOfPlayer(player, player.getMana(), player.getMaxMana()); });
        manaBarPosition = manaBar.transform.position;
    }

    void Update()
    {
        Vector2 position = manaBarMask.transform.position;
        position += new Vector2(Time.deltaTime * waveSpeed, 0);

        if (position.x > manaBarPosition.x + (manaBar.rectTransform.sizeDelta.x / 2))
        {
            position -= new Vector2(manaBar.rectTransform.sizeDelta.x, 0);
        }

        setPosition(position);
    }

    private void setPosition(Vector2 to)
    {
        manaBarMask.transform.position = to;
        manaBar.transform.position = manaBarPosition;
    }

    private void nowTurnOfPlayer(Player player, int manaAt, int maxMana)
    {
        player.subscribeToManaAmountChanged(playerManaNowAt);

        if (playerDisplaying != null)
        {
            playerDisplaying.unsubscribeToManaAmountChanged(playerManaNowAt);
        }
        
        playerDisplaying = player;
        playerManaNowAt(manaAt, maxMana);
    }

    private void playerManaNowAt(int manaAt, int maxMana)
    {
        changeBarHeight((float)manaAt / maxMana);
    }

    private void changeBarHeight(float amount)
    {
        Vector2 setMaskBarPosition = manaBarPosition - new Vector2(0, manaBarMask.rectTransform.sizeDelta.y * (1 - amount));
        setPosition(setMaskBarPosition);
    }
}
