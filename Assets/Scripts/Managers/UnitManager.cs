using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;
using static UnitManager;

public class UnitManager : MonoBehaviour
{
    public static UnitManager instance { get; private set; }
    public delegate void battlesHappened(List<battleResult> battleResults);
    private battlesHappened battlesHappenedCallback;
    public enum direction { down, left, right, up }
    private UnitWorld[,] unitsAtPositions;
    private TileManager tileManager;
    [SerializeField]
    private GameObject unitPrefab;

    void Start()
    {
        instance = GetComponent<UnitManager>();
        tileManager = GetComponent<TileManager>();
    }

    public void subscribeToBattlesHappenedCallback(battlesHappened subscribe)
    {
        battlesHappenedCallback += subscribe;
    }

    public void mapSet(Vector2Int size)
    {
        unitsAtPositions = new UnitWorld[size.x, size.y];
        //createUnitAt(new Vector2Int(0, 0));
    }

    public List<unitMove> potentialMovePositions(Vector2Int position, int maxBy)
    {
        direction[] directionsToMove = new direction[] { direction.down, direction.left, direction.right, direction.up };
        List<unitMove> movePositions = new List<unitMove>();

        foreach (direction moveInDirection in directionsToMove)
        {
            for (int distance = 1; distance <= maxBy; distance++)
            {
                Vector2Int potentialPosition = position + TileManager.getDirectionVector(moveInDirection) * distance;

                if (tileManager.isTileCurrentlyPassable(potentialPosition))
                {
                    movePositions.Add(new unitMove(moveInDirection, distance, potentialPosition));
                }
                else
                {
                    break;
                }
            }
        }

        return movePositions;
    }

    public struct unitMove
    {
        public direction direction;
        public int distance;
        public Vector2Int gridPosition;

        public unitMove(direction unitDirection, int unitDistance, Vector2Int position)
        {
            direction = unitDirection;
            distance = unitDistance;
            gridPosition = position;
        }
    }

    public bool moveUnitBy(UnitWorld unitToMove, direction inDirection, int by)
    {
        Vector2Int unitPosition = unitToMove.getGridPosition();
        Vector2Int unitMoveTo = unitPosition + TileManager.getDirectionVector(inDirection) * by;
        TileOnBoard[] tilesOnPath = tileManager.getPathTo(unitPosition, inDirection, by);

        if (tilesOnPath == null)
        {
            return false;
        }

        //we stepped on each point between this and the last tile
        for (int i = 0; i < tilesOnPath.Length - 1; i++)
        {
            TileOnBoard tile = tilesOnPath[i];
            tile.unitSteppedOn(unitToMove);
        }

        setUnitToPosition(unitToMove, unitMoveTo);
        return true;
    }

    private void setUnitToPosition(UnitWorld unit, Vector2Int nowAt)
    {
        //kind weird but whatever
        void exertInfluenceFrom(Vector2Int tilePos, bool noLonger)
        {
            List<Vector2Int> tilesNoLongerInfluencing = new List<Vector2Int>();
            tilesNoLongerInfluencing.Add(tilePos);
            tilesNoLongerInfluencing.AddRange(tileManager.getNeighbouringTiles(tilePos));

            foreach (Vector2Int changeInfluence in tileManager.getNeighbouringTiles(tilePos))
            {
                TileOnBoard tile = tileManager.getTile(changeInfluence);
                
                if (noLonger)
                {
                    tile.tileNoLongerInInfluenceOf(unit);
                }
                else
                {
                    tile.tileInInfluenceOf(unit);
                }         
            }
        }

        Vector2Int oldPosition = unit.getGridPosition();

        if (unit.isUnitPoisitionSet())
        {
            unitsAtPositions[oldPosition.x, oldPosition.y] = null;
            tileManager.unitLeftTile(oldPosition);
            exertInfluenceFrom(oldPosition, true);
        }
        
        unitsAtPositions[nowAt.x, nowAt.y] = unit;
        tileManager.unitEnteredTile(nowAt);
        unit.unitMovedTo(nowAt);

        //inform tile
        TileOnBoard tileAt = tileManager.getTile(nowAt);
        tileAt.unitSteppedOn(unit);
        exertInfluenceFrom(nowAt, false);
    }

    private void unitAffectTiles(UnitWorld unit, Vector2Int at)
    {
        List<Vector2Int> neigbours = tileManager.getNeighbouringTiles(at);

        //neigbours.RemoveAll(neighbour => tileManager.getNeighbouringTiles(neigbour));
    }

    public void unitHasDied(UnitWorld unit)
    {
        Vector2Int positionOfUnit = unit.getGridPosition();
        unitsAtPositions[positionOfUnit.x, positionOfUnit.y] = null;
        tileManager.unitLeftTile(positionOfUnit);
    }

    public struct battle
    {
        public Vector2Int offensiveUnitPosition;
        public Vector2Int defensiveUnitPosition;

        public battle(Vector2Int offensiveUnitPosition, Vector2Int defensiveUnitPosition)
        {
            this.offensiveUnitPosition = offensiveUnitPosition;
            this.defensiveUnitPosition = defensiveUnitPosition;
        }
    }

    public List<battle> getBattlesOnTile(Vector2Int position, UnitWorld unit)
    {
        List<battle> potentialBattles = new List<battle>();
        List<Vector2Int> potentialBattleTiles = tileManager.getNeighbouringTiles(position);

        foreach (Vector2Int potentialBattleTile in potentialBattleTiles)
        {
            UnitWorld unitToPotentiallyAttack = getUnitAtTile(potentialBattleTile);

            if (unitToPotentiallyAttack == null || unitToPotentiallyAttack.getAllegiance() == unit.getAllegiance())
            {
                continue;
            }

            potentialBattles.Add(new battle(position, potentialBattleTile));
        }

        return potentialBattles;
    } 

    public struct battleResult
    {
        public UnitWorld attacker;
        public UnitWorld defender;
        public int diceRoll;
        public int attack;
        public int totalAttack;

        public battleResult(UnitWorld attacker, UnitWorld defender)
        {
            this.attacker = attacker;
            this.defender = defender;
            diceRoll = Random.Range(1, 7);
            UnitWorld.Unit attackerAttributes = attacker.getUnitAttributes();
            UnitWorld.Unit defenderAttributes = defender.getUnitAttributes();
            attack = attackerAttributes.attack;
            totalAttack = attack + diceRoll;
        }

        public void executeResult()
        {          
            defender.dealDamage(totalAttack);
            print("Attack: " + attack + " Dice roll " + diceRoll);
        }
    }

    public void attackNeighbouringUnits(UnitWorld attacker)
    {
        List<battle> battles = getBattlesOnTile(attacker.getGridPosition(), attacker);
        List<battleResult> battleResults = new List<battleResult>();

        foreach (battle battle in battles)
        {
            Vector2Int defenderPosition = battle.defensiveUnitPosition;
            UnitWorld defendingUnit = unitsAtPositions[defenderPosition.x, defenderPosition.y];
            battleResult result = new battleResult(attacker, defendingUnit);
            battleResults.Add(result);
            result.executeResult();
        }

        battlesHappenedCallback(battleResults);
    }

    public UnitWorld getUnitAtTile(Vector2Int tile)
    {
        return unitsAtPositions[tile.x, tile.y];
    }

    public void createUnitAt(Vector2Int at, UnitWorld.Unit unit, Player allegiance)
    {
        UnitWorld newUnit = Instantiate(unitPrefab).GetComponent<UnitWorld>();
        setUnitToPosition(newUnit, at);
        newUnit.unitCreated(unit, allegiance);
    }
}
