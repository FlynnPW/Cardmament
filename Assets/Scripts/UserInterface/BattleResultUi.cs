using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleResultUi : MonoBehaviour
{
    [SerializeField]
    private Image diceImage;
    [SerializeField]
    private Text diceAmount;
    [SerializeField]
    private Text attackAmount;
    [SerializeField]
    private Text totalAmount;
    [SerializeField]
    private Sprite[] diceFaces;
    private BattleUiManager.battleDisplayCompleted onDisplayCompletedCallback;
    private UnitManager.battleResult battleResultDisplaying;
    const float MOVE_DISTANCE_WHILE_SHAKING = 0.035f;
    const float DICE_FACE_CHANGE_EVERY = 0.2f;
    const float TIME_EACH_BATTLE_DISPLAYS = 4;
    const float DICE_SETTLE_TIME = 0.2f;
    const float DICE_OSCILLATE_EVERY = 0.25f;
    private float xToShakeAround;
    private float timeLeft = 0;
    private float timeUntilNextDiceFaceChange;
    private bool stopShaking;

    private void Start()
    {
        xToShakeAround = diceImage.transform.position.x;
        timeLeft = TIME_EACH_BATTLE_DISPLAYS;
    }

    private void Update()
    {
        if (timeLeft < DICE_SETTLE_TIME && timeLeft % DICE_OSCILLATE_EVERY < Time.deltaTime && stopShaking == false) //check to see if we are one frame away from returning to original position
        {
            stopShaking = true;
        }

        if (stopShaking == false)
        {
            diceImage.transform.position = new Vector2(xToShakeAround + MOVE_DISTANCE_WHILE_SHAKING * Mathf.Sin(timeLeft * 2 * Mathf.PI / DICE_OSCILLATE_EVERY), diceImage.transform.position.y);
        }
        
        timeLeft -= Time.deltaTime;
        timeUntilNextDiceFaceChange -= Time.deltaTime;

        if (timeUntilNextDiceFaceChange < 0)
        {
            if (timeLeft < DICE_FACE_CHANGE_EVERY)
            {
                completedDiceRollAnimation();
                setDiceFaceTo(battleResultDisplaying.diceRoll);
            }
            else
            {
                int diceRoll = Random.Range(1, 7);
                setDiceFaceTo(diceRoll);
            }

            timeUntilNextDiceFaceChange += DICE_FACE_CHANGE_EVERY;
        }

        if (timeLeft <= 0)
        {
            onDisplayCompletedCallback();
            enabled = false;
        }
    }

    void setDiceFaceTo(int to)
    {
        print(to);
        diceImage.sprite = diceFaces[to - 1];
    }

    public void dipslayBattleResults(UnitManager.battleResult result, BattleUiManager.battleDisplayCompleted onDisplayCompleted)
    {
        battleResultDisplaying = result;
        attackAmount.text = battleResultDisplaying.attack.ToString();
        onDisplayCompletedCallback += onDisplayCompleted;
    }

    private void completedDiceRollAnimation()
    {
        diceAmount.text = battleResultDisplaying.diceRoll.ToString();
        totalAmount.text = battleResultDisplaying.totalAttack.ToString();
    }
}
