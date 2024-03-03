using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;
using System;
using System.Collections.Generic;

public class GeneticShip : ShipManager 
{
    // instead of generating multiple ships, the genetic ship generates a single ship

    // the ship reference
    bool [,] ship;

    public bool front = false;

    public void Init(Bot botRef, bool[,] ship, int k = 0) {
        // data validation
        this.dim = ship.GetLength(0);
        this.k = k;

        // set references
        this.botRef = botRef;
        this.ship = ship;

        // call next methods
        if(nodes == null || nodes.Count == 0) {
            CreateGrid();
            GenerateShip();
        }
    }

    void GenerateShip() {
        for (int i = 0; i < dim; i++) {
            for (int j = 0; j < dim; j++) {
                if (ship[i, j]) {
                    nodes[new Vector2(i, j)].Open();
                }
            }
        }
    }

    public void BringToFront() {
        front = true;
        foreach (var node in nodes.Values) {
            node.sr.sortingOrder = 3;
        }
        foreach (var alien in aliens) {
            alien.sr.sortingOrder = 4;
        }
        captain.sr.sortingOrder = 4;
        bot.sr.sortingOrder = 4;
    }

    public new void Ready() {
        base.Ready();

        if(front) {
            BringToFront();
        }
    }

    public void Showcase() {
        foreach (var node in nodes.Values) {
            node.sr.sortingOrder = 5;
        }
    }

}