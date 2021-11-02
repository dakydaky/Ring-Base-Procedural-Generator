using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataStructures.Biomes;
using Assets.Scripts.DataStructures.Objects.Static.Environmental;
using Assets.Scripts.DataStructures.Objects.Static.Structural.Puzzles;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New BiomeData", menuName = "Biome Data", order = 52)]
public class BiomeData : ScriptableObject
{
    [Header("Spawnable structures (& puzzles)")]
    [SerializeField]
    [Tooltip("Will enforce the spawning of this specified puzzles in this biome, 1 entry in this list - 1 spawned")]
    private List<PuzzleType> requiredPuzzles;

    //TODO: Implement this, but with biome-related frequency associated to each structure - future
    [SerializeField]
    [Tooltip("Number of non-unique structures to be displaced on the biome (from the list below)")]
    [Range(0,100)]
    private int numAdditionalStructures;

    [Header("Additional information")]
    // TODO: utilize this only maybe, might even be a Texture instead of Material
    [SerializeField] [Tooltip("The ground texture in the biome")]
    private GameObject groundTexture;

    [SerializeField] [Tooltip("The type of the biome")]
    private BiomeType biomeType;

    [Header("Environmental spawn frequency")]
    [Tooltip("The frequency of rocks being spawned relative to other elements (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int rockFreq;
    [Tooltip("The frequency of trees being spawned relative to other elements (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int treeFreq;
    [Tooltip("The frequency of grass being spawned relative to other elements (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int grassFreq;
    [Tooltip("The frequency of bushes being spawned relative to other elements (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int bushFreq;
    [Tooltip("The frequency of logs being spawned relative to other elements (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int logFreq;
    [Tooltip("The frequency of nothing being spawned relative to other elements (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int nothingFreq;

    [Header("Tree type spawn frequency")]
    [Tooltip("The frequency of healthy, full trees being spawned relative to other types (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int healthyTreeFreq;
    [Tooltip("The frequency of dead trees being spawned relative to other types (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int deadTreeFreq;
    [Tooltip("The frequency of tree stumps being spawned relative to other types (in %)")]
    [Range(0,100)]
    [SerializeField]
    private int treeStumpFreq;

    public List<PuzzleType> RequiredPuzzles => requiredPuzzles;
    public int NumAdditionalStructures => numAdditionalStructures;

    public BiomeType BiomeType => biomeType;
    public GameObject GroundTexture => groundTexture;

    public TreeType GetTreeTypeByFrequency()
    {
        var upperLimits = new List<(TreeType type, int freq)>
        {
            (TreeType.Grown, healthyTreeFreq),
            (TreeType.Dead, deadTreeFreq + healthyTreeFreq),
            (TreeType.Stump, treeStumpFreq + deadTreeFreq + healthyTreeFreq),
        };

        if (upperLimits.Any(type => type.freq > 100) || upperLimits.All(type => type.freq != 100))
            throw new Exception("Percentages of spawning frequency don't add up to 100 for a biome. Check if you calculated it correctly.");

        var rand = Random.Range(1, 101);
        var closestMatch = upperLimits.Where(obj => obj.freq >= rand).OrderBy(item => Math.Abs(rand - item.freq)).First();
        return closestMatch.type;

    }

    public EnvironmentalObjectType GetEnvironmentalObjectFrequency()
    {
        var upperLimits = new List<(EnvironmentalObjectType type, int freq)>
        {
            (EnvironmentalObjectType.Rock, rockFreq),
            (EnvironmentalObjectType.Tree, treeFreq + rockFreq),
            (EnvironmentalObjectType.Grass, grassFreq + treeFreq + rockFreq),
            (EnvironmentalObjectType.Bushes, bushFreq + grassFreq + treeFreq + rockFreq),
            (EnvironmentalObjectType.Log, logFreq + bushFreq + grassFreq + treeFreq + rockFreq),
            (EnvironmentalObjectType.None, nothingFreq + logFreq + bushFreq + grassFreq + treeFreq + rockFreq)
        };

        if (upperLimits.Any(type => type.freq > 100) || upperLimits.All(type => type.freq != 100))
            throw new Exception("Percentages of spawning frequency don't add up to 100 for a biome. Check if you calculated it correctly.");

        var rand = Random.Range(1, 101);
        var closestMatch = upperLimits.Where(obj => obj.freq >= rand).OrderBy(item => Math.Abs(rand - item.freq)).First();

        return closestMatch.type;
    }
}

