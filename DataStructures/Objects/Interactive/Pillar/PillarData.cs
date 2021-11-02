using Assets.Scripts;
using Assets.Scripts.DataStructures.Objects.Interactive;
using Assets.Scripts.DataStructures.Objects.Interactive.Pillar;
using UnityEngine;

public class PillarData : InteractiveObjectData
{
    private int BonfireXY;
    public PillarData(PillarType pillarType)
    {
        InteractiveObjectType = InteractiveObjectType.Pillar;
        PillarType = pillarType;
        BonfireXY = MapGenerator.Instance.MapSize / 2;
        // TODO: Add rotational logic maths
        //PillarRotation = Transform.Lookat
        PillarRotation = new Vector3(Random.Range(-2, +3), Random.Range(0, 360), Random.Range(-2, +3));

    }

    public Vector3 PillarRotation { get; set; }
    public PillarType PillarType { get; set; }
}