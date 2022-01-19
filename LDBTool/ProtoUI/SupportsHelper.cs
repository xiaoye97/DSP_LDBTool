using System;

namespace xiaoye97.UI
{
    public static class SupportsHelper
    {
        private static readonly Type _unityExplorerType = Type.GetType("UnityExplorer.InspectorManager", false);
        
        public static bool UnityExplorerInstalled { get; }

        static SupportsHelper()
        {
            UnityExplorerInstalled = _unityExplorerType != null;
        }
    }

    public interface ISkin
    {
        UnityEngine.GUISkin GetSkin();
    }
}
