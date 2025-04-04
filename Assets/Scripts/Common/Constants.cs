using UnityEngine;
using System;
using System.Collections.Generic;

public class Constants
{
    #region Network
    
    public const string ServerURL = "http://localhost:3000";
    public const string GameServerURL = "ws://localhost:3000";
    
    #endregion
    
}

public static class GameBaseData
{
    public static readonly Dictionary<int, int> startingLife = new Dictionary<int, int>()
    {
        {2, 2},
        {3, 3},
        {4, 4}
    };
    public const int startingShuriken = 1;
    public static readonly Dictionary<int, int> lastStage = new Dictionary<int, int>()
    {
        // index 고려해서 -1씩
        {2, 11},
        {3, 9},
        {4, 7}
    };
}

public static class StageData
{
    public static readonly List<Stage> stages = new List<Stage>()
    {
        new Stage { cardPerPlayer = 1, getLife = false, getShuriken = false },
        new Stage { cardPerPlayer = 2, getLife = false, getShuriken = true },
        new Stage { cardPerPlayer = 3, getLife = true, getShuriken = false },
        new Stage { cardPerPlayer = 4, getLife = false, getShuriken = false },
        new Stage { cardPerPlayer = 5, getLife = false, getShuriken = true },
        new Stage { cardPerPlayer = 6, getLife = true, getShuriken = false },
        new Stage { cardPerPlayer = 7, getLife = false, getShuriken = false },
        new Stage { cardPerPlayer = 8, getLife = false, getShuriken = true },
        new Stage { cardPerPlayer = 9, getLife = true, getShuriken = false },
        new Stage { cardPerPlayer = 10, getLife = false, getShuriken = false },
        new Stage { cardPerPlayer = 11, getLife = false, getShuriken = false },
        new Stage { cardPerPlayer = 12, getLife = false, getShuriken = false },
    };
}
