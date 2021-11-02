namespace Assets.Scripts.DataStructures.Objects.Static.Environmental
{
    public class EnvironmentalObjectData : StaticObjectData
    {
        public EnvironmentalObjectData()
        {
            StaticObjectType = StaticObjectType.Environmental;
        }

        public EnvironmentalObjectType EnvironmentalObjectType { get; set; }
    }
}