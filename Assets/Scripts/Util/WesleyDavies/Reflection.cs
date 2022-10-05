using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace WesleyDavies
{
    public static class Reflection
    {
        public static object CreateInstanceFromName(Type classType, Assembly assembly = default)
        {
            if (assembly == default)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            Type type = assembly.GetTypes().First(t => t.GetType() == classType);

            return Activator.CreateInstance(type);
        }

        public static object CreateInstanceFromName(string className, Assembly assembly = default)
        {
            if (assembly == default)
            {
                assembly = Assembly.GetExecutingAssembly();
            }

            Type type = assembly.GetTypes().First(t => t.Name == className);

            return Activator.CreateInstance(type);
        }
    }
}