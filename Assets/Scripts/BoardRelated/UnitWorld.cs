using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitWorld : MonoBehaviour
{
    private bool positionSet = false;
    private int health;
    private Unit unitAttributes;
    private Vector2Int gridPosition;
    private Player allegiance;

    public struct Unit {
        public int health;
        public int attack;

        public Unit(int health, int attack)
        {
            this.health = health;
            this.attack = attack;
        }
    }

    public void unitCreated(Unit attributes, Player allegiance)
    {
        unitAttributes = attributes;
        health = attributes.health;
        this.allegiance = allegiance;
    }

    public void unitMovedTo(Vector2Int moveTo)
    {
        gridPosition = moveTo;
        transform.position = (Vector2)moveTo;
        positionSet = true;
    }

    public bool isUnitPoisitionSet()
    {
        return positionSet;
    }

    public Vector2Int getGridPosition()
    {
        return gridPosition;
    }

    public Player getAllegiance()
    {
        return allegiance;
    }

    public void dealDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            die();
        }
    }

    private void die()
    {

    }
}
