using System.Collections.Generic;
using UnityEngine;

public abstract class Bot: MonoBehaviour
{
    public Vector2 pos { get; set; }

    // computes the next step and moves the bot
    public abstract void computeNextStep(ShipManager ship);
}