using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace xiaoye97
{
    public static class LDBTool
    {
        // Add data action
        public static Action PreAddDataAction, PostAddDataAction;

        // Action to modify data
        public static Action<Proto> EditDataAction;
        
        internal static bool Finshed;
        
        internal static Dictionary<ProtoType, List<Proto>> PreToAdd = new Dictionary<ProtoType, List<Proto>>();
        internal static Dictionary<ProtoType, List<Proto>> PostToAdd = new Dictionary<ProtoType, List<Proto>>();
        internal static Dictionary<ProtoType, List<Proto>> TotalDict = new Dictionary<ProtoType, List<Proto>>();


        private static ConfigFile CustomID = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomID.cfg", true);
        private static ConfigFile CustomGridIndex = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomGridIndex.cfg", true);
        private static ConfigFile CustomStringZHCN = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.ZHCN.cfg", true);
        private static ConfigFile CustomStringENUS = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.ENUS.cfg", true);
        private static ConfigFile CustomStringFRFR = new ConfigFile($"{Paths.ConfigPath}/LDBTool/LDBTool.CustomLocalization.FRFR.cfg", true);

        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>> IDDict = new Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>>();

        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>> GridIndexDict =
            new Dictionary<ProtoType, Dictionary<string, ConfigEntry<int>>>();

        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>> ZHCNDict =
            new Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>>();

        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>> ENUSDict =
            new Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>>();

        private static Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>> FRFRDict =
            new Dictionary<ProtoType, Dictionary<string, ConfigEntry<string>>>();
        
        private static Dictionary<int, Dictionary<int, int>> BuildBarDict = new Dictionary<int, Dictionary<int, int>>();

        /// <summary>
        /// 设置建造快捷栏
        /// </summary>
        /// <param name="category">第几栏</param>
        /// <param name="index">第几个格子</param>
        /// <param name="itemId">物品ID</param>
        public static void SetBuildBar(int category, int index, int itemId)
        {
            if (category < 1 || category > 12)
            {
                LDBToolPlugin.logger.LogWarning("SetBuildBar Fail. category must be between 1 and 12.");
                return;
            }

            if (index < 1 || index > 12)
            {
                LDBToolPlugin.logger.LogWarning("SetBuildBar Fail. index must be between 1 and 12.");
                return;
            }

            if (UIBuildMenu.staticLoaded && Finshed) // 如果已经加载
            {
                var item = LDB.items.Select(itemId);
                if (item != null)
                {
                    UIBuildMenu.protos[category, index] = item;
                    LDBToolPlugin.logger.LogInfo($"Set build bar at {category},{index} ID:{item.ID} name:{item.Name.Translate()}");
                }
                else
                {
                    LDBToolPlugin.logger.LogWarning($"SetBuildBar Fail. ItemProto with ID {itemId} not found.");
                }
            }
            else
            {
                if (!BuildBarDict.ContainsKey(category))
                {
                    BuildBarDict.Add(category, new Dictionary<int, int>());
                }

                BuildBarDict[category][index] = itemId;
            }
        }

        /// <summary>
        /// 自动设置建造快捷栏
        /// </summary>
        internal static void SetBuildBar()
        {
            foreach (var kv in BuildBarDict)
            {
                foreach (var kv2 in kv.Value)
                {
                    var item = LDB.items.Select(kv2.Value);
                    if (item != null)
                    {
                        UIBuildMenu.protos[kv.Key, kv2.Key] = item;
                        LDBToolPlugin.logger.LogInfo($" Set build bar at {kv.Key},{kv2.Key} ID:{item.ID} name:{item.Name.Translate()}");
                    }
                    else
                    {
                        LDBToolPlugin.logger.LogWarning($"SetBuildBar Fail. ItemProto with ID {kv2.Value} not found.");
                    }
                }
            }
        }

        /// <summary>
        /// 用户配置数据绑定
        /// </summary>
        private static void Bind(ProtoType protoType, Proto proto)
        {
            IdBind(protoType, proto);
            GridIndexBind(protoType, proto);
            StringBind(protoType, proto);
        }

        /// <summary>
        /// 通过配置文件绑定ID，允许玩家在冲突时自定义ID
        /// </summary>
        private static void IdBind(ProtoType protoType, Proto proto)
        {
            if (proto is StringProto) return;
            
            var entry = CustomID.Bind(protoType.ToString(), proto.Name, proto.ID);
            proto.ID = entry.Value;
            if (!IDDict.ContainsKey(protoType))
            {
                IDDict.Add(protoType, new Dictionary<string, ConfigEntry<int>>());
            }

            if (IDDict[protoType].ContainsKey(proto.Name))
            {
                LDBToolPlugin.logger.LogError($"[CustomID] ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
            }
            else
            {
                IDDict[protoType].Add(proto.Name, entry);
            }
        }

        /// <summary>
        /// 通过配置文件绑定GridIndex，允许玩家在冲突时自定义GridIndex
        /// 在自定义ID之后执行
        /// </summary>
        private static void GridIndexBind(ProtoType protoType, Proto proto)
        {
            ConfigEntry<int> entry = null;
            
            if (proto is ItemProto item)
            {
                entry = CustomGridIndex.Bind(protoType.ToString(), item.ID.ToString(), item.GridIndex, $"Item Name = {item.Name}");
                item.GridIndex = entry.Value;
            }
            else if (proto is RecipeProto recipe)
            {
                entry = CustomGridIndex.Bind(protoType.ToString(), recipe.ID.ToString(), recipe.GridIndex, $"Recipe Name = {recipe.Name}");
                recipe.GridIndex = entry.Value;
            }

            if (entry == null) return;
            
            if (!GridIndexDict.ContainsKey(protoType))
            {
                GridIndexDict.Add(protoType, new Dictionary<string, ConfigEntry<int>>());
            }

            if (GridIndexDict[protoType].ContainsKey(proto.Name))
            {
                LDBToolPlugin.logger.LogError($"[CustomGridIndex] ID:{proto.ID} Name:{proto.Name} There is a conflict, please check.");
            }
            else
            {
                GridIndexDict[protoType].Add(proto.Name, entry);
            }
        }

        /// <summary>
        /// 通过配置文件绑定翻译文件，允许玩家在翻译缺失或翻译不准确时自定义翻译
        /// </summary>
        private static void StringBind(ProtoType protoType, Proto proto)
        {
            if (!(proto is StringProto stringProto)) return;
            
            var zhcn = CustomStringZHCN.Bind(protoType.ToString(), stringProto.Name, stringProto.ZHCN, stringProto.Name);
            var enus = CustomStringENUS.Bind(protoType.ToString(), stringProto.Name, stringProto.ENUS, stringProto.Name);
            var frfr = CustomStringFRFR.Bind(protoType.ToString(), stringProto.Name, stringProto.FRFR, stringProto.Name);
                
            if (!String.Equals(zhcn.Value, ""))
                stringProto.ZHCN = zhcn.Value;
            else
                zhcn.Value = stringProto.ZHCN;

            if (!String.Equals(enus.Value, ""))
                stringProto.ENUS = enus.Value;
            else
                enus.Value = stringProto.ENUS;

            if (!String.Equals(frfr.Value, ""))
                stringProto.FRFR = frfr.Value;
            else
                frfr.Value = stringProto.FRFR;
                
            if (!string.IsNullOrEmpty(zhcn.Value))
            {
                if (!ZHCNDict.ContainsKey(protoType)) ZHCNDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                if (ZHCNDict[protoType].ContainsKey(stringProto.Name))
                {
                    LDBToolPlugin.logger.LogError($"[CustomLocalization.ZHCN] Name:{stringProto.Name} There is a conflict, please check.");
                }
                else ZHCNDict[protoType].Add(stringProto.Name, zhcn);
            }

            if (ENUSDict != null)
            {
                if (!ENUSDict.ContainsKey(protoType)) ENUSDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                if (ENUSDict[protoType].ContainsKey(stringProto.Name))
                {
                    LDBToolPlugin.logger.LogError($"[CustomLocalization.ENUS] Name:{stringProto.Name} There is a conflict, please check.");
                }
                else ENUSDict[protoType].Add(stringProto.Name, enus);
            }

            if (!string.IsNullOrEmpty(frfr.Value))
            {
                if (!FRFRDict.ContainsKey(protoType)) FRFRDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                if (FRFRDict[protoType].ContainsKey(stringProto.Name))
                {
                    LDBToolPlugin.logger.LogError($"[CustomLocalization.FRFR] Name:{stringProto.Name} There is a conflict, please check.");
                }
                else FRFRDict[protoType].Add(stringProto.Name, frfr);
            }
        }
        
        internal static bool HasStringIdRegisted(int id)
        {
            if (LDB.strings.dataIndices.ContainsKey(id)) return true;
            
            if (PreToAdd[ProtoType.String].Any(proto => proto.ID == id)) return true;
            if (PostToAdd[ProtoType.String].Any(proto => proto.ID == id)) return true;
            
            return false;
        }
        
        internal static int lastStringId = 1000;


        internal static int FindAvailableStringID()
        {
            int id = lastStringId + 1;

            while (true)
            {
                if (!HasStringIdRegisted(id))
                {
                    break;
                }

                if (id > 12000)
                {
                    LDBToolPlugin.logger.LogError("Failed to find free index!");
                    throw new ArgumentException("No free indices available!");
                }

                id++;
            }

            lastStringId = id;

            return id;
        }

        /// <summary>
        /// 在游戏数据加载之前添加数据
        /// </summary>
        /// <param name="protoType">要添加的Proto的类型</param>
        /// <param name="proto">要添加的Proto</param>
        public static void PreAddProto(ProtoType protoType, Proto proto)
        {
            if (!PreToAdd[protoType].Contains(proto))
            {
                if (proto is StringProto)
                {
                    int id = FindAvailableStringID();
                    proto.ID = id;
                }

                Bind(protoType, proto);
                PreToAdd[protoType].Add(proto);
                TotalDict[protoType].Add(proto);
            }
        }

        /// <summary>
        /// 在游戏数据加载之后添加数据
        /// </summary>
        /// <param name="protoType">要添加的Proto的类型</param>
        /// <param name="proto">要添加的Proto</param>
        public static void PostAddProto(ProtoType protoType, Proto proto)
        {
            if (!PostToAdd[protoType].Contains(proto))
            {
                if (proto is StringProto)
                {
                    int id = FindAvailableStringID();
                    proto.ID = id;
                }
                
                Bind(protoType, proto);
                PostToAdd[protoType].Add(proto);
                TotalDict[protoType].Add(proto);
            }
        }

        internal static void AddProtos(Dictionary<ProtoType, List<Proto>> datas)
        {
            foreach (var kv in datas)
            {
                if (kv.Value.Count > 0)
                {
                    if (kv.Key == ProtoType.AdvisorTip) AddProtosToSet(LDB.advisorTips, kv.Value);
                    else if (kv.Key == ProtoType.Audio) AddProtosToSet(LDB.audios, kv.Value);
                    else if (kv.Key == ProtoType.EffectEmitter) AddProtosToSet(LDB.effectEmitters, kv.Value);
                    else if (kv.Key == ProtoType.Item) AddProtosToSet(LDB.items, kv.Value);
                    else if (kv.Key == ProtoType.Model) AddProtosToSet(LDB.models, kv.Value);
                    else if (kv.Key == ProtoType.Player) AddProtosToSet(LDB.players, kv.Value);
                    else if (kv.Key == ProtoType.Recipe) AddProtosToSet(LDB.recipes, kv.Value);
                    else if (kv.Key == ProtoType.String) AddProtosToSet(LDB.strings, kv.Value);
                    else if (kv.Key == ProtoType.Tech) AddProtosToSet(LDB.techs, kv.Value);
                    else if (kv.Key == ProtoType.Theme) AddProtosToSet(LDB.themes, kv.Value);
                    else if (kv.Key == ProtoType.Tutorial) AddProtosToSet(LDB.tutorial, kv.Value);
                    else if (kv.Key == ProtoType.Vege) AddProtosToSet(LDB.veges, kv.Value);
                    else if (kv.Key == ProtoType.Vein) AddProtosToSet(LDB.veins, kv.Value);
                }
            }
        }

        /// <summary>
        /// 添加多个数据到数据表
        /// </summary>
        private static void AddProtosToSet<T>(ProtoSet<T> protoSet, List<Proto> protos) where T : Proto
        {
            var array = protoSet.dataArray;
            protoSet.Init(array.Length + protos.Count);
            for (int i = 0; i < array.Length; i++)
            {
                protoSet.dataArray[i] = array[i];
            }

            for (int i = 0; i < protos.Count; i++)
            {
                protoSet.dataArray[array.Length + i] = protos[i] as T;

                if (protos[i] is ItemProto item)
                {
                    item.index = array.Length + i;
                }

                if (protos[i] is RecipeProto)
                {
                    RecipeProto proto = protos[i] as RecipeProto;
                    if (proto.preTech != null)
                    {
                        ArrayAddItem(ref proto.preTech.UnlockRecipes, proto.ID);
                        ArrayAddItem(ref proto.preTech.unlockRecipeArray, proto);
                    }
                }

                LDBToolPlugin.logger.LogInfo($"Add {protos[i].ID} {protos[i].Name.Translate()} to {protoSet.GetType().Name}.");
            }

            var dataIndices = new Dictionary<int, int>();
            for (int i = 0; i < protoSet.dataArray.Length; i++)
            {
                protoSet.dataArray[i].sid = protoSet.dataArray[i].SID;
                dataIndices[protoSet.dataArray[i].ID] = i;
            }

            protoSet.dataIndices = dataIndices;
            if (protoSet is StringProtoSet stringProtoSet)
            {
                for (int i = array.Length; i < protoSet.dataArray.Length; i++)
                {
                    stringProtoSet.nameIndices[protoSet.dataArray[i].Name] = i;
                }
            }
        }

        /// <summary>
        /// 数组添加数据
        /// </summary>
        private static void ArrayAddItem<T>(ref T[] array, T item)
        {
            var list = array.ToList();
            list.Add(item);
            array = list.ToArray();
        }
    }
}