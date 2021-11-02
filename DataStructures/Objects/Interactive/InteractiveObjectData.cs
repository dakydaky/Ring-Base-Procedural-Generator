namespace Assets.Scripts.DataStructures.Objects.Interactive
{
    public class InteractiveObjectData : ObjectData
    {
        public InteractiveObjectData()
        {
            ObjectType = ObjectType.Interactive;
        }

        public InteractiveObjectType InteractiveObjectType { get; set; }
    }
}