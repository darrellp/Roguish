﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Roguish.Serialization;

internal static partial class Serialize
{
    private class CustomResolver() : DefaultContractResolver
    {
        private readonly IValueProvider _valueProvider = new SimpleTypeNameProvider();

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

            if (type.IsClass && typeof(EcsComponent).IsAssignableFrom(type))
            {
                // Add a phantom string property to every class which will resolve 
                // to the simple type name of the class (via the value provider)
                // during serialization.
                props.Insert(0, new JsonProperty
                {
                    DeclaringType = type,
                    PropertyType = typeof(string),
                    PropertyName = "ComponentType",
                    ValueProvider = _valueProvider,
                    Readable = true,
                    Writable = false
                });
            }

            return props;
        }
    }

    class SimpleTypeNameProvider : IValueProvider
    {
        public object GetValue(object target)
        {
            return target.GetType().Name;
        }

        void IValueProvider.SetValue(object target, object? value)
        {
        }
    }

}