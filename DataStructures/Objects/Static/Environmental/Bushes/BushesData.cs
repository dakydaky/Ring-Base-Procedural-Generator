using Assets.Scripts.DataStructures.Objects.Static.Environmental;
using UnityEngine;

public class BushesData : EnvironmentalObjectData
{
    public BushesData()
    {
        EnvironmentalObjectType = EnvironmentalObjectType.Bushes;

        BushesScale = new Vector3(Random.Range(0.25f, 0.45f), Random.Range(0.3f, 0.45f), Random.Range(0.25f, 0.45f));
        BushesRotation = new Vector3(Random.Range(-1, +2), Random.Range(0, 360), Random.Range(-1, +2));
    }
    public Vector3 BushesScale { get; set; }
    public Vector3 BushesRotation { get; set; }
}
