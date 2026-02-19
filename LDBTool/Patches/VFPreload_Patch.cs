using System;
using System.Collections.Generic;
using System.Reflection;
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

            if (LDBTool.EditDataAction != null)
            {
                foreach (PropertyInfo propertyInfo in typeof(LDB).GetProperties())
                {
                    Type setType = propertyInfo.PropertyType;
                    if (!setType.IsConstructedGenericType)
                    {
                        setType = setType.BaseType;
                    }

                    Type protoType = setType.GetGenericArguments()[0];

                    object protoSet = propertyInfo.GetValue(null);

                    MethodInfo method = typeof(VFPreload_Patch).GetMethod(nameof(EditAllProtos), AccessTools.all).MakeGenericMethod(protoType);
                    method.Invoke(null, new[] {protoSet});
                }
            }
            
            LDBToolPlugin.logger.LogInfo("Calling ItemProto init functions...");
            try
            {
                ItemProto.InitFuelNeeds();
                ItemProto.InitConstructableItems();
                ItemProto.InitItemIds();
                ItemProto.InitItemIndices();
                InitRecipeItems();
                SignalProtoSet.InitSignalKeyIdPairs();
            }
            catch (Exception ex)
            {
                LDBToolPlugin.logger.LogError($"Error calling ItemProto init functions\n{ex}");
            }

            GameMain.iconSet.loaded = false;
            GameMain.iconSet.Create();
            LDBTool.SetBuildBar();
            LDBTool.Finshed = true;
            LDBToolPlugin.logger.LogInfo("Done.");
        }
        
        // Code taken from ModFixerOne (https://github.com/starfi5h/DSP_Mod_Support/tree/main/ModFixerOne)
        private static void InitRecipeItems()
        {
            // To avoid conflicts caused by having the same Recipe ID, a modified function is used here.
            // 為了避免Recipe同樣id衝突的情況發生, 在這裡使用修改後的函式
            RecipeProto.recipeExecuteData = new Dictionary<int, RecipeExecuteData>();
            RecipeProto[] dataArray = LDB.recipes.dataArray;
            for (int i = 0; i < dataArray.Length; i++)
            {
                RecipeExecuteData recipeExecuteData = new RecipeExecuteData(dataArray[i].Items, dataArray[i].ItemCounts, dataArray[i].Results, dataArray[i].ResultCounts, dataArray[i].TimeSpend * 10000, dataArray[i].TimeSpend * 100000, dataArray[i].productive);
                if (!RecipeProto.recipeExecuteData.ContainsKey(dataArray[i].ID))
                {
                    RecipeProto.recipeExecuteData.Add(dataArray[i].ID, recipeExecuteData);
                }
                else
                {
                    // In case of conflict, the recipe registered first takes precedence. Print out the recipe ID and name of the later recipe.
                    // 有衝突時, 以先註冊的配方優先。印出後來的配方id和名稱
                    LDBToolPlugin.logger.LogWarning($"Duplicate RecipeId:{dataArray[i].ID} Name:{dataArray[i].Name}");
                }
            }
        }

        private static void EditAllProtos<T>(ProtoSet<T> protoSet)
            where T : Proto
        {
            foreach (T proto in protoSet.dataArray)
            {
                if (proto == null) continue;

                try
                {
                    LDBTool.EditDataAction(proto);
                }
                catch (Exception e)
                {
                    LDBToolPlugin.logger.LogWarning($"Edit Error: ID:{proto.ID} Type:{proto.GetType().Name} {e.Message}");
                }
            }
        }
    }
}