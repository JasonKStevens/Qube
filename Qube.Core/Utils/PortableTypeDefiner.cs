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
            var moduleName = type.Assembly.GetModules().FirstOrDefault()?.Name ?? "";
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(p => new PropertyDefinition(p.Name, p.PropertyType.FullName));

            return new PortableTypeDefinition
            {
                AssemblyName = type.Assembly.FullName,
                ModuleName = moduleName,

                ClassName = type.FullName,
                IsInterface = type.IsInterface,
                IsAbstract = type.IsAbstract,
                Properties = type.IsEnum ? new PropertyDefinition[0] : properties.ToArray(),

                IsEnum = type.IsEnum,
                EnumNames = type.IsEnum ? Enum.GetNames(type) : new string[0],
                EnumValues = type.IsEnum ? Enum.GetValues(type).Cast<int>().ToArray() : new int[0],

                BaseClassName = type.BaseType?.FullName,
            };
        }

        public PortableTypeDefinition[] BuildDefinitions(Type[] type)
        {
            return type.Select(BuildDefinition).ToArray();
        }
    }
}
