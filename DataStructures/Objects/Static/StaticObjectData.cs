namespace Assets.Scripts.DataStructures.Objects.Static
{
    public class StaticObjectData : ObjectData
    {
        public StaticObjectData()
        {
            ObjectType = ObjectType.Static;
        }

        public StaticObjectType StaticObjectType { get; set; }
    }
}