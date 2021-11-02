using Assets.Scripts.DataStructures.Objects.Static.Environmental;
using UnityEngine;
public class RockData : EnvironmentalObjectData
{
    public RockData()
    {
        EnvironmentalObjectType = EnvironmentalObjectType.Rock;

        RockScale = new Vector3(Random.Range(0.95f, 1.05f), Random.Range(0.75f, 1.25f), Random.Range(0.95f, 1.15f));
        RockRotation = new Vector3(Random.Range(-3, +4), Random.Range(0, 360), Random.Range(-3, +4));
    }
    public Vector3 RockScale { get; set; }
    public Vector3 RockRotation { get; set; }
}