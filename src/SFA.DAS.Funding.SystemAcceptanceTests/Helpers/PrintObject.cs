using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Funding.SystemAcceptanceTests.Helpers
{
    public static class PrintObject
    {
        public static void PrintObjectProperties(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                object propertyValue = property.GetValue(obj);
                if (property.PropertyType.Namespace.StartsWith("System"))
                {
                    Console.WriteLine("{0}: {1}", propertyName, propertyValue);
                }
                else
                {
                    Console.WriteLine("{0}:", propertyName);
                    PrintObjectProperties(propertyValue);
                }
            }
        }
    }
}
