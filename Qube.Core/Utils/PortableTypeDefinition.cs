namespace Qube.Core.Utils
{
    public class PortableTypeDefinition
    {
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string ModuleName { get; set; }
        public PropertyDefinition[] Properties { get; set; }
    }
}
