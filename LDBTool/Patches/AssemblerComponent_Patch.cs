using HarmonyLib;

namespace xiaoye97.Patches
{
    // Code taken from ModFixerOne (https://github.com/starfi5h/DSP_Mod_Support/tree/main/ModFixerOne)
    
    [HarmonyPatch]
    public class AssemblerComponent_Patch
    {
        [HarmonyPostfix, HarmonyPriority(Priority.High)]
        [HarmonyPatch(typeof(AssemblerComponent), nameof(AssemblerComponent.Import))]
        private static void ValidateAndFixRecipeData(ref AssemblerComponent __instance)
        {
            if (__instance.recipeId > 0)
            {
                if (__instance.recipeExecuteData == null)
                {
                    // The original game assumes that `recipeExecuteData` will always have a value if `recipeId > 0`.
                    // Therefore, for recipes that have been removed, the machine will be set to a state without recipes.
                    // 原版遊戲假設了recipeId>0時recipeExecuteData必有值
                    // 因此對於已經移除的配方, 將機器設置為無配方狀態
                    LDBToolPlugin.logger.LogWarning($"Importing assembler {__instance.recipeType}: Can't find recipeId {__instance.recipeId}");
                    __instance.recipeId = 0;
                    __instance.recipeType = ERecipeType.None;
                    __instance.recipeExecuteData = null;
                    __instance.served = null;
                    __instance.incServed = null;
                    __instance.needs = null;
                    __instance.produced = null;
                }
                else
                {
                    // Ensure that the length of the array containing the item quantity is consistent with the length of the array in `recipeExecuteData` to avoid errors in `UpdateNeeds()`.
                    // 確保物品數量的數組和recipeExecuteData中的數組長度一致, 避免在UpdateNeeds()中報錯
                    if (__instance.served.Length != __instance.recipeExecuteData.requireCounts.Length ||
                        __instance.incServed.Length != __instance.recipeExecuteData.requireCounts.Length)
                    {
                        LDBToolPlugin.logger.LogWarning($"Importing assembler {__instance.recipeType}: requireCounts mismatch");
                        __instance.served = new int[__instance.recipeExecuteData.requireCounts.Length];
                        __instance.incServed = new int[__instance.recipeExecuteData.requireCounts.Length];
                    }

                    if (__instance.produced.Length != __instance.recipeExecuteData.productCounts.Length)
                    {
                        LDBToolPlugin.logger.LogWarning($"Importing assembler {__instance.recipeType}: productCounts mismatch");
                        __instance.produced = new int[__instance.recipeExecuteData.productCounts.Length];
                    }
                }
            }
        }
    }
}