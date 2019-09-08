namespace Qube.Core.Types
{
    public class PortableTypeDefinition
    {
        public string AssemblyName { get; set; }
        public string ModuleName { get; set; }

        public string ClassName { get; set; }
        public string BaseClassName { get; set; }
        public bool IsInterface { get; set; }
        public bool IsAbstract { get; set; }

        public PropertyDefinition[] Properties { get; set; }

        public bool IsEnum { get; set; }
        public string[] EnumNames { get; set; }
        public int[] EnumValues { get; set; }
    }
}
