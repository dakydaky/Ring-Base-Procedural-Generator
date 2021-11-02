using Assets.Scripts.DataStructures.Objects.Interactive;
using Assets.Scripts.DataStructures.Objects.Interactive.Artifacts;

public class ArtifactData : InteractiveObjectData
{
    public ArtifactData(ArtifactType artifactType)
    {
        InteractiveObjectType = InteractiveObjectType.Artifact;
        ArtifactType = artifactType;
    }

    public ArtifactType ArtifactType { get; set; }
}