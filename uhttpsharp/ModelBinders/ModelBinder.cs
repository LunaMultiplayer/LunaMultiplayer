using System;
using System.Linq;
using System.Reflection;
using uhttpsharp.Headers;

namespace uhttpsharp.ModelBinders
{
    public class ModelBinder : IModelBinder
    {
        private readonly IObjectActivator _activator;
        public ModelBinder(IObjectActivator activator)
        {
            _activator = activator;
        }


        public T Get<T>(byte[] raw, string prefix)
        {
            throw new NotSupportedException();
        }
        public T Get<T>(IHttpHeaders headers)
        {
            var retVal = _activator.Activate<T>(null);

            foreach (var prop in retVal.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string))
                {
                    string stringValue;
                    if (headers.TryGetByName(prop.Name, out stringValue))
                    {
                        var value = Convert.ChangeType(stringValue, prop.PropertyType);
                        prop.SetValue(retVal, value);
                    }
                }
                else
                {
                    var value = Get(prop.PropertyType, headers, prop.Name);
                    prop.SetValue(retVal, value);
                }

            }

            return retVal;
        }

        private object Get(Type type, IHttpHeaders headers, string prefix)
        {
            if (type.IsPrimitive || type == typeof(string))
            {
                string value;
                if (headers.TryGetByName(prefix, out value))
                {
                    return Convert.ChangeType(value, type);
                }

                return null;
            }

            var retVal = _activator.Activate(type, null);

            string val;
            var settedValues =
                retVal.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(p => headers.TryGetByName(prefix + "[" + p.Name + "]", out val)).ToList();

            if (settedValues.Count == 0)
            {
                return null;
            }


            foreach (var prop in settedValues)
            {
                string stringValue;
                if (headers.TryGetByName(prefix + "[" + prop.Name + "]", out stringValue))
                {
                    object value = prop.PropertyType.IsPrimitive || prop.PropertyType == typeof(string)
                        ? Convert.ChangeType(stringValue, prop.PropertyType)
                        : Get(prop.PropertyType, headers, prefix + "[" + prop.Name + "]");

                    prop.SetValue(retVal, value);
                }
            }

            return retVal;
        }

        public T Get<T>(IHttpHeaders headers, string prefix)
        {
            return (T)Get(typeof(T), headers, prefix);
        }
    }

    public class ObjectActivator : IObjectActivator
    {

        public object Activate(Type type, Func<string, Type, object> argumentGetter)
        {
            return Activator.CreateInstance(type);
        }
    }

    public interface IObjectActivator
    {

        object Activate(Type type, Func<string, Type, object> argumentGetter);

    }

    public static class ObjectActivatorExtensions
    {

        public static T Activate<T>(this IObjectActivator activator, Func<string, Type, object> argumentGetter)
        {
            return (T)activator.Activate(typeof(T), argumentGetter);
        }
    }
}
