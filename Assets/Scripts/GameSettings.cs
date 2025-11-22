using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "The-Duelist/Game Settings")]
public class GameSettings : ScriptableObject
{
    public int totalRounds = 5;
    public float startingHP = 50f;
    public string p1Name = "Player 1";
    public string p2Name = "Player 2";

    public string selectedMap = "Void";
}
