using System;
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
        public Type BuildType(PortableTypeDefinition typeDefinition)
        {
            var typeBuilder = CreateTypeBuilder(typeDefinition);

            AddConstructor(typeBuilder);
            AddProperties(typeBuilder, typeDefinition.Properties);
            var type = CreateType(typeBuilder);

            return type;
        }

        private TypeBuilder CreateTypeBuilder(PortableTypeDefinition typeDefinition)
        {
            var assemblyName = new AssemblyName(typeDefinition.AssemblyName);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(typeDefinition.ModuleName);

            var typeBuilder = moduleBuilder.DefineType(typeDefinition.ClassName,
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null);
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
                CreateProperty(typeBuilder, property.Name, property.Type);
            }
        }

        private Type CreateType(TypeBuilder typeBuilder)
        {
            return typeBuilder.CreateTypeInfo().AsType();
        }

        private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
        {
            var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getMethodBuilder = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = getMethodBuilder.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            var setMethodBuilder = tb.DefineMethod("set_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });
            var setIl = setMethodBuilder.GetILGenerator();
            var modifyProperty = setIl.DefineLabel();
            var exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getMethodBuilder);
            propertyBuilder.SetSetMethod(setMethodBuilder);
        }
    }
}
