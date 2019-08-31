using System;

namespace Qube.Core.Utils
{
    public class PropertyDefinition
    {
        public string Name { get; set; }
        public Type Type { get; set; }

        public PropertyDefinition(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
