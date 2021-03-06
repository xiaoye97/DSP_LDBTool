using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace xiaoye97
{
    public static class ProtoIndex
    {
        private static Dictionary<Type, int> index = new Dictionary<Type, int>();
        private static Type[] protoTypes;
        
        internal static void InitIndex()
        {
            LDBToolPlugin.logger.LogDebug($"Generating Proto type list:");
            protoTypes = (
                from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
                from assemblyType in GetTypesSafe(domainAssembly)
                where assemblyType != null && typeof(Proto).IsAssignableFrom(assemblyType) && assemblyType != typeof(Proto)
                select assemblyType
                ).ToArray();

            for (int i = 0; i < protoTypes.Length; i++)
            {
                index.Add(protoTypes[i], i);
                LDBToolPlugin.logger.LogDebug($"Found Proto type: {protoTypes[i].FullName}");
            }
        }

        private static Type[] GetTypesSafe(Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            return types;
        }

            public static int GetProtosCount()
        {
            return index.Count;
        }
        
        public static int GetIndex(Proto proto)
        {
            return GetIndex(proto.GetType());
        }

        public static int GetIndex(Type type)
        {
            if (!typeof(Proto).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Can't get index because type {type.FullName} does not extend Proto type.");
            }

            if (index.ContainsKey(type))
            {
                return index[type];
            }
            
            throw new ArgumentException($"Unknown Proto type: {type.FullName}");
        }

        public static Type GetProtoTypeAt(int num)
        {
            return protoTypes[num];
        }

        public static string GetProtoName(Proto proto)
        {
            return proto.GetType().Name.Replace("Proto", "");
        }
        
        public static string[] GetProtoNames()
        {
            return protoTypes.Select(type => type.Name.Replace("Proto", "")).ToArray();
        }

        internal static Type[] GetAllProtoTypes()
        {
            return protoTypes;
        }
    }
}