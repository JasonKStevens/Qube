using System;
using System.Linq;
using System.Reflection;

namespace Qube.Core.Utils
{
    /// <summary>
    /// Defines a type that the server-side linq expression can use.
    /// Used for strong-typing.
    /// Paired with <see cref="PortableTypeBuilder"/>
    /// </summary>
    public class PortableTypeDefiner
    {
        public PortableTypeDefinition BuildDefinition(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new PropertyDefinition(p.Name, p.PropertyType));
            var moduleName = type.Assembly.GetModules().FirstOrDefault()?.Name ?? "";

            return new PortableTypeDefinition
            {
                AssemblyName = type.Assembly.FullName,
                ClassName = type.FullName,
                ModuleName = moduleName,
                Properties = properties.ToArray()
            };
        }
    }
}
