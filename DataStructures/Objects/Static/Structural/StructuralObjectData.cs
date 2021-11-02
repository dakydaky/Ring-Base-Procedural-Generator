namespace Assets.Scripts.DataStructures.Objects.Static.Structural
{
    public class StructuralObjectData : StaticObjectData
    {
        public StructuralObjectData()
        {
            StaticObjectType = StaticObjectType.Structural;
        }

        public StructuralObjectType StructuralObjectType { get; set; }

        //How many Tiles have to be reserved for the specified structure 
        public int StructuralObjectSize { get; set; }
    }
}