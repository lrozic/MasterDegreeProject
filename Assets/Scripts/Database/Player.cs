using System;

[Serializable]
public class Player
{
    public string playerName;
    public float finishTime;

    /// <summary>
    /// Empty constructor for the Player object
    /// </summary>
    public Player()
    {
    }

    /// <summary>
    /// Constructor for the Player object
    /// </summary>
    /// <param name="playerName">Name of the palyer</param>
    /// <param name="finishTime">Time needed to beat the boss</param>
    public Player(string playerName, float finishTime)
    {
        this.playerName = playerName;
        this.finishTime = finishTime;
    }
}

