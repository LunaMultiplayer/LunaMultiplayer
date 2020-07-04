using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LmpClient.Events.Base
{
    public abstract class LmpBaseEvent
    {
        public static void Awake()
        {
            var lmpEventClasses = Assembly.GetExecutingAssembly().GetTypes().Where(myType => myType.IsClass && myType.IsSubclassOf(typeof(LmpBaseEvent)));
            Parallel.ForEach(lmpEventClasses, lmpEventClass =>
            {
                var eventFields = lmpEventClass.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToArray();
                if (eventFields.Any())
                {
                    foreach (var eventField in eventFields)
                    {
                        var val = Activator.CreateInstance(eventField.FieldType, eventField.Name);
                        eventField.SetValue(null, val);
                    }
                }
            });
        }
    }
}
