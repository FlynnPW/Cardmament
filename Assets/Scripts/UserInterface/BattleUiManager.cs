using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleUiManager : MonoBehaviour
{
    public static BattleUiManager instance { get; private set; }
    public delegate void battleDisplayCompleted();
    private battleDisplayCompleted battleDisplayedCompletedCallback;
    [SerializeField]
    private GameObject battleIconPrefab;
    [SerializeField]
    private GameObject battleResultPrefab;
    [SerializeField]
    private Canvas worldSpaceCanvas;
    private Dictionary<(Vector2Int attackerPosition, Vector2Int defenderPosition), Image> battleIcons = new Dictionary<(Vector2Int, Vector2Int), Image>();
    private List<UnitManager.battleResult> battleResultsQueue = new List<UnitManager.battleResult>();
    const float BATTLE_RESULT_HOVER_ABOVE = 0.5f;

    void Start()
    {
        instance = GetComponent<BattleUiManager>();
        UnitManager.instance.subscribeToBattlesHappenedCallback(battlesHappened);
    }

    public void createBattleIcon(Vector2Int atCoordinate, Vector2Int toward)
    {
        Image newBattleIcon = Instantiate(battleIconPrefab, worldSpaceCanvas.transform).GetComponent<Image>();
        newBattleIcon.transform.position = atCoordinate + new Vector2(toward.x - atCoordinate.x, toward.y - atCoordinate.y) / 2;
        battleIcons.Add((atCoordinate, toward), newBattleIcon);
    }

    public void battlesSelected(Vector2Int onCoordinate)
    {
        List<(Vector2Int attackerPosition, Vector2Int defenderPosition)> toRemove = battleIcons.Keys.ToList();
        toRemove.RemoveAll(p => p.attackerPosition == onCoordinate);

        foreach ((Vector2Int, Vector2Int) battlePosition in toRemove)
        {
            Image toDestroy = battleIcons[battlePosition];
            battleIcons.Remove(battlePosition);
            Destroy(toDestroy);
        }
    }

    public void battlesHappened(List<UnitManager.battleResult> battleResults)
    {
        battleResultsQueue.AddRange(battleResults);
        displayBattleResult();
    }

    public void displayBattleResult()
    {
        if (battleResultsQueue.Count == 0)
        {
            return;
        }

        BattleResultUi battleResultDisplay = Instantiate(battleResultPrefab, worldSpaceCanvas.transform).GetComponent<BattleResultUi>();
        battleResultDisplay.transform.position = battleResultsQueue[0].attacker.getGridPosition() + new Vector2(0, BATTLE_RESULT_HOVER_ABOVE);
        battleResultDisplay.dipslayBattleResults(battleResultsQueue[0], displayBattleResult);
        battleResultsQueue.RemoveAt(0);
    }
}
