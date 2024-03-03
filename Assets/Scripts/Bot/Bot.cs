using System.Collections.Generic;
using UnityEngine;

public abstract class Bot: MonoBehaviour
{
    public SpriteRenderer sr;

    // bot's position
    public Vector2 pos { get; set; }
    
    // bot's name
    public abstract string botName { get; }

    // computes the next step and moves the bot
    public abstract void computeNextStep(ShipManager ship, bool front = true);
}