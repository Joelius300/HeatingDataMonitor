using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataHandler
{
    public static class TypeExtensions
    {
        public static bool IsGenericTypeOf(this Type t, Type genericDefinition) =>
            IsGenericTypeOf(t, genericDefinition, out _);

        public static bool IsGenericTypeOf(this Type t, Type genericDefinition, out Type[] genericParameters)
        {
            genericParameters = new Type[0];
            if (!genericDefinition.IsGenericType)
            {
                return false;
            }

            bool isMatch = t.IsGenericType && t.GetGenericTypeDefinition() == genericDefinition.GetGenericTypeDefinition();
            if (!isMatch && t.BaseType != null)
            {
                isMatch = IsGenericTypeOf(t.BaseType, genericDefinition, out genericParameters);
            }

            if (!isMatch && genericDefinition.IsInterface && t.GetInterfaces().Any())
            {
                foreach (var i in t.GetInterfaces())
                {
                    if (i.IsGenericTypeOf(genericDefinition, out genericParameters))
                    {
                        isMatch = true;
                        break;
                    }
                }
            }

            if (isMatch && !genericParameters.Any())
            {
                genericParameters = t.GetGenericArguments();
            }

            return isMatch;
        }
    }
}
