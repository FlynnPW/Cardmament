using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMoveUnitImpact : CardImpact
{
    private int moveDistance;

    public CardMoveUnitImpact(int moves)
    {
        moveDistance = moves;
    }

    bool CardImpact.attemptToPlay(Vector2Int atTile, Player playedBy)
    {
        UnitWorld unit = UnitManager.instance.getUnitAtTile(atTile);

        if (unit == null)
        {
            return false;
        }

        List<UnitManager.unitMove> potentialMovePositions = UnitManager.instance.potentialMovePositions(unit.getGridPosition(), moveDistance);

        foreach (UnitManager.unitMove potentialMove in potentialMovePositions)
        {
            SelectTileUiManager.instance.createTileSelectionPrefab(potentialMove.gridPosition, () => { tileSelected(unit, potentialMove.direction, potentialMove.distance, potentialMove.gridPosition); });
            List<UnitManager.battle> potentialBattlesOnThisTile = UnitManager.instance.getBattlesOnTile(potentialMove.gridPosition, unit);

            foreach (UnitManager.battle potentialBattle in potentialBattlesOnThisTile)
            {
                BattleUiManager.instance.createBattleIcon(potentialBattle.offensiveUnitPosition, potentialBattle.defensiveUnitPosition);
            }
        }

        return true;
    }

    void tileSelected(UnitWorld unitToMove, UnitManager.direction directionToMove, int by, Vector2Int tilePosition)
    {
        if (UnitManager.instance.moveUnitBy(unitToMove, directionToMove, by) == false)
        {
            Debug.LogError("Tile selected but move cannot be performed?");
            return;
        }

        SelectTileUiManager.instance.clearTilesUis();
        BattleUiManager.instance.battlesSelected(tilePosition);
        UnitManager.instance.attackNeighbouringUnits(unitToMove);
    }
}
