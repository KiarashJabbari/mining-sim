using UnityEngine;

[CreateAssetMenu(fileName = "NewPickaxe", menuName = "Cave Game/Pickaxe Data")]
public class PickaxeData : ScriptableObject
{
    public string pickaxeName = "Stone Pickaxe";
    public Sprite sprite;

    [Tooltip("Mining power determines which tiles can be broken")]
    public int miningPower = 1;

    [Tooltip("How many tiles away the player can mine")]
    public int reach = 1;

    [Tooltip("Seconds between each hit")]
    public float mineInterval = 0.3f;
}