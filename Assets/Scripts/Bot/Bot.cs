using System.Collections.Generic;
using UnityEngine;

public abstract class Bot: MonoBehaviour
{
    public Vector2 pos { get; set; }
    public abstract string botName { get; }

    // computes the next step and moves the bot
    public abstract void computeNextStep(ShipManager ship);
}