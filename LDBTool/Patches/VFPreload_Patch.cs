using System;
using System.Collections.Generic;
using HarmonyLib;

namespace xiaoye97.Patches
{
    [HarmonyPatch]
    public static class VFPreload_Patch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        private static void VFPreloadPrePatch()
        {
            if (LDBTool.Finshed) return;
            LDBToolPlugin.logger.LogInfo("Pre Loading...");
            if (LDBTool.PreAddDataAction != null)
            {
                LDBTool.PreAddDataAction();
                LDBTool.PreAddDataAction = null;
            }

            LDBTool.AddProtos(LDBTool.PreToAdd);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        private static void VFPreloadPostPatch()
        {
            if (LDBTool.Finshed) return;
            LDBToolPlugin.logger.LogInfo("Post Loading...");
            if (LDBTool.PostAddDataAction != null)
            {
                LDBTool.PostAddDataAction();
                LDBTool.PostAddDataAction = null;
            }

            LDBTool.AddProtos(LDBTool.PostToAdd);
            List<Proto> allProto = new List<Proto>();
            foreach (var p in LDB.advisorTips.dataArray) allProto.Add(p);
            foreach (var p in LDB.audios.dataArray) allProto.Add(p);
            foreach (var p in LDB.effectEmitters.dataArray) allProto.Add(p);
            foreach (var p in LDB.items.dataArray) allProto.Add(p);
            foreach (var p in LDB.models.dataArray) allProto.Add(p);
            foreach (var p in LDB.players.dataArray) allProto.Add(p);
            foreach (var p in LDB.recipes.dataArray) allProto.Add(p);
            foreach (var p in LDB.strings.dataArray) allProto.Add(p);
            foreach (var p in LDB.techs.dataArray) allProto.Add(p);
            foreach (var p in LDB.themes.dataArray) allProto.Add(p);
            foreach (var p in LDB.tutorial.dataArray) allProto.Add(p);
            foreach (var p in LDB.veges.dataArray) allProto.Add(p);
            foreach (var p in LDB.veins.dataArray) allProto.Add(p);
            if (LDBTool.EditDataAction != null)
            {
                foreach (var p in allProto)
                {
                    if (p != null)
                    {
                        try
                        {
                            LDBTool.EditDataAction(p);
                        }
                        catch (Exception e)
                        {
                            LDBToolPlugin.logger.LogWarning($"Edit Error: ID:{p.ID} Type:{p.GetType().Name} {e.Message}");
                        }
                    }
                }
            }

            GameMain.iconSet.loaded = false;
            GameMain.iconSet.Create();
            LDBTool.SetBuildBar();
            LDBTool.Finshed = true;
            LDBToolPlugin.logger.LogInfo("Done.");
        }
    }
}