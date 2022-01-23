using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace xiaoye97.UI
{
    public static class SupportsHelper
    {
        internal static readonly Type unityExplorerType;
        internal static readonly MethodInfo inspectMethod;
        
        public static bool UnityExplorerInstalled => unityExplorerType != null;

        static SupportsHelper()
        {
            unityExplorerType = AccessTools.TypeByName("UnityExplorer.InspectorManager");
            inspectMethod = unityExplorerType.GetMethods().First(info => info.Name == "Inspect" && info.GetParameters().Length == 2);
        }
    }

    public interface ISkin
    {
        UnityEngine.GUISkin GetSkin();
    }
}
