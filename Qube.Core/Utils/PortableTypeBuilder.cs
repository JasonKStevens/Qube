using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Qube.Core.Utils
{
    /// <summary>
    /// Dynamically creates a type that the server-side linq expression can use.
    /// Used for strong-typing.
    /// Paired with <see cref="PortableTypeDefiner"/>
    /// </summary>
    public class PortableTypeBuilder
    {
        private static IDictionary<string, Type> _typeCache = new Dictionary<string, Type>();

        public Type BuildType(PortableTypeDefinition typeDefinition)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(typeDefinition.ModuleName);

            if (typeDefinition.IsEnum)
            {
                var enumType = CreateEnumType(moduleBuilder, typeDefinition);
                return enumType;
            }

            var typeBuilder = CreateTypeBuilder(moduleBuilder, typeDefinition);

            if (!typeBuilder.IsInterface)
            {
                AddConstructor(typeBuilder);
            }
                
            AddProperties(typeBuilder, typeDefinition.Properties);
            var type = typeBuilder.CreateTypeInfo().AsType();
            
            _typeCache[type.FullName] = type;

            return type;
        }

        public Type[] BuildTypes(PortableTypeDefinition[] typeDefinitions)
        {
            return typeDefinitions.Select(BuildType).ToArray();
        }

        private Type CreateEnumType(ModuleBuilder moduleBuilder, PortableTypeDefinition typeDefinition)
        {
            var enumBuilder = moduleBuilder.DefineEnum(typeDefinition.ClassName, TypeAttributes.Public, typeof(int));

            for (int i = 0; i < typeDefinition.EnumNames.Length; i++)
            {
                enumBuilder.DefineLiteral(typeDefinition.EnumNames[i], typeDefinition.EnumValues[i]);
            }

            return enumBuilder.CreateTypeInfo().AsType();
        }

        private TypeBuilder CreateTypeBuilder(ModuleBuilder moduleBuilder, PortableTypeDefinition typeDefinition)
        {
            var baseType = GetType(typeDefinition.BaseClassName);

            var typeAttributes =
                TypeAttributes.Public |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout |
                TypeAttributes.Serializable;

            if (typeDefinition.IsAbstract)
            {
                typeAttributes |= TypeAttributes.Abstract;
            }
            
            if (typeDefinition.IsInterface)
            {
                typeAttributes |= TypeAttributes.Interface;
            }
            else
            {
                typeAttributes |= TypeAttributes.Class | TypeAttributes.AutoClass | TypeAttributes.AnsiClass;
            }

            var typeBuilder = moduleBuilder.DefineType(typeDefinition.ClassName, typeAttributes, baseType);
            return typeBuilder;
        }

        private void AddConstructor(TypeBuilder typeBuilder)
        {
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        }

        private void AddProperties(TypeBuilder typeBuilder, PropertyDefinition[] properties)
        {
            foreach (var property in properties)
            {
                var propertyType = GetType(property.TypeName);
                CreateProperty(typeBuilder, property.Name, propertyType);
            }
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            // Getter
            var getter = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = getter.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getter);

            // Setter
            var setter = tb.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });
            var setIl = setter.GetILGenerator();

            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetSetMethod(setter);
        }

        private static Type GetType(string typeName)
        {
            if (typeName == null)
            {
                return null;
            }

            return _typeCache.ContainsKey(typeName) ? _typeCache[typeName] : Type.GetType(typeName);
        }
    }
}
