using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Reactive.Testing;
using DeepEqual.Syntax;
using NUnit.Framework;
using Qube.Core.Types;

namespace Qube.Test
{
    // TODO: Add test for interface properties

    public class PortableTypeFixture : ReactiveTest
    {
        private PortableTypeDefiner _sutAlice;
        private PortableTypeBuilder _sutBob;

        [SetUp]
        public void Setup()
        {
            _sutAlice = new PortableTypeDefiner();
            _sutBob = new PortableTypeBuilder();
        }

        [TestCase(typeof(Test1Class))]
        [TestCase(typeof(Test2Class))]
        [TestCase(typeof(Test3Class))]
        public void should_transfer_classes(Type classType)
        {
            // Act
            var reconstructedType = TransferType(classType);

            // Assert
            AssertClassEquals(reconstructedType, classType);
        }

        [Test]
        public void should_transfer_inherited_classes()
        {
            // Arrange
            var types = new [] { typeof(ITestInterface), typeof(TestBaseClass), typeof(TestDerivedClass) };

            // Act
            var reconstructedTypes = TransferTypes(types);

            // Assert
            AssertClassesEquals(reconstructedTypes, types);
        }

        [TestCase(typeof(Test1Enum))]
        [TestCase(typeof(Test2Enum))]
        [TestCase(typeof(Test3Enum))]
        public void should_transfer_enums(Type enumType)
        {
            // Act
            var reconstructedType = TransferType(enumType);

            // Assert
            AssertEnumEquals(reconstructedType, enumType);
        }

        private Type TransferType(Type typeIn)
        {
            var def = _sutAlice.BuildDefinition(typeIn);
            var typeOut = _sutBob.BuildType(def);
            return typeOut;
        }

        private Type[] TransferTypes(Type[] typesIn)
        {
            var defs = _sutAlice.BuildDefinitions(typesIn);
            var typesOut = _sutBob.BuildTypes(defs);
            return typesOut;
        }

        private static void AssertClassesEquals(Type[] actuals, Type[] expecteds)
        {
            Assert.That(actuals.Length, Is.EqualTo(expecteds.Length));

            for (int i = 0; i < actuals.Length; i++)
            {
                AssertClassEquals(actuals[i], expecteds[i]);
            }
        }

        private static void AssertClassEquals(Type actual, Type expected)
        {
            if (actual == null && expected == null)
            {
                Assert.Pass();
                return;
            }

            Assert.That(actual.FullName, Is.EqualTo(expected.FullName));
            actual.GetFields().ShouldDeepEqual(expected.GetFields());
            
            actual.GetProperties()
                .Select(p => new { p.Name, p.PropertyType.FullName })
                .ShouldDeepEqual(
                    expected.GetProperties()
                        .Select(p => new { p.Name, p.PropertyType.FullName })
                );

            AssertClassesEquals(actual.GetInterfaces(), expected.GetInterfaces());
            AssertClassEquals(actual.BaseType, expected.BaseType);
        }

        private static void AssertEnumEquals(Type actual, Type expected)
        {
            Assert.That(actual.FullName, Is.EqualTo(expected.FullName));
            actual.GetEnumNames().ShouldDeepEqual(expected.GetEnumNames());
            actual.GetEnumValues().Cast<int>().ToArray().ShouldDeepEqual(expected.GetEnumValues().Cast<int>().ToArray());
        }
    }

    public class Test1Class { }
    public class Test2Class { public int P1 { get; set; } public string P2 { get; set; } public decimal? P3 { get; set; } public Guid P4 { get; set; } public DateTimeOffset P5 { get; set; } }
    public class Test3Class { public byte[] P1 { get; set; } public Int64 P2 { get; set; } public List<String> P3 { get; set; } }

    public interface ITestInterface {}
    public abstract class TestBaseClass : ITestInterface { public Dictionary<string, object> Bag { get; set; } }
    public class TestDerivedClass : Test1Class { }

    public enum Test1Enum {}
    public enum Test2Enum { A = 0, B, C }
    public enum Test3Enum { X, Y = 100, Z }
}