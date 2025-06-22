using UnityEngine;

public struct Stage
{
    public int cardPerPlayer;
    public bool getLife;
    public bool getShuriken;
}

[System.Serializable]
public struct UserInfo
{
    public string userId;
    public string nickname;
    public int coin;
    public int winCount;
    public int loseCount;
    public float waitingSecondPerNumber;
    public int totalPlayedCard;
    public int[] unlockedCardBack;
}