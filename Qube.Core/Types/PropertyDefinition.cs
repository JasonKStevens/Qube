namespace Qube.Core.Types
{
    public class PropertyDefinition
    {
        public string Name { get; set; }
        public string TypeName { get; set; }

        public PropertyDefinition(string name, string typeName)
        {
            Name = name;
            TypeName = typeName;
        }
    }
}
