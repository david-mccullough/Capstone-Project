using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Faction {

    public string name;
    public Color color;
    private List<Unit> myUnits = new List<Unit>();
    private int startingNumUnits;
    private int numActiveUnits;
    [SerializeField]
    private bool IS_AI = false;

    public bool isActive;

    public delegate void DeactivateDelegate();
    public event DeactivateDelegate FactionDeactivateEvent;

    public Faction(string name)
        : this(name, Color.black, 1) {
    }

    public Faction(string name, Color color, int numUnits) {
        this.name = name;
        this.color = color;
        this.startingNumUnits = numUnits;
        this.numActiveUnits = numUnits;
        isActive = true;
        myUnits = new List<Unit>();

        FactionDeactivateEvent = null;
    }

    public Faction(string name, Color color, int numUnits, bool isAI)
        : this(name, color, numUnits) {
        this.IS_AI = isAI;
    }

    public void AddUnit(Unit unit) {
        if (myUnits == null) {
            myUnits = new List<Unit>();
        }
        myUnits.Add(unit);
        numActiveUnits++;
        isActive = true;
    }

    public void RemoveUnit(Unit unit) {
        Debug.Log("removing unit");
        //myUnits.Remove(unit);
        numActiveUnits--;
        if (numActiveUnits <= 0) {
            SetIsActive(false);
            FactionDeactivateEvent();
        }
    }

    public bool IsAI() {
        return IS_AI;
    }

    public bool IsActive() {
        return isActive;
    }

    public void SetIsActive(bool boolean) {
        isActive = boolean;
    }

    public Unit[] GetUnits() {
        return myUnits.ToArray();
    }

    public int GetStartingNumUnits() {
        return startingNumUnits;
    }

    public static bool operator ==(Faction f1, Faction f2) {
        return f1.Equals(f2);
    }

    public static bool operator !=(Faction f1, Faction f2) {
        return !f1.Equals(f2);
    }

}
