using Assets.Scripts.DataStructures.Objects.Static.Environmental;
using UnityEngine;

public class GrassData : EnvironmentalObjectData
{
    public GrassData()
    {
        EnvironmentalObjectType = EnvironmentalObjectType.Grass;

        GrassScale = new Vector3(Random.Range(0.55f, 0.65f), Random.Range(0.1f, 0.25f), Random.Range(0.55f, 0.65f));
        GrassRotation = new Vector3(Random.Range(-3, +4), Random.Range(0, 360), Random.Range(-3, +4));
    }
    public Vector3 GrassScale { get; set; }
    public Vector3 GrassRotation { get; set; }
}
