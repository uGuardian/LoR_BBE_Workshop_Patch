using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using BepInEx;
using BepInEx.Harmony;
using BaseMod;
using UI;
#pragma warning disable IDE0051

namespace BBE_Workshop {
	[BepInPlugin("LoR.uGuardian.BBE_Workshop", "BBE_Workshop_Patch", "1.1.0")]
	[BepInDependency("Bong", BepInDependency.DependencyFlags.HardDependency)]
	public class BBE_Workshop : BaseUnityPlugin {
		void Awake() {
			Harmony harmony = new Harmony("LoR.uGuardian.BBE_Workshop");
			try {
				harmony.PatchAll();
				Debug.Log("General patches done");
				harmony.Unpatch(typeof(UISettingEquipPageScrollList).GetMethod("SetData"), HarmonyPatchType.Prefix, "BongBong Enterprises");
				harmony.Unpatch(typeof(UIEquipPageScrollList).GetMethod("SetData"), HarmonyPatchType.Prefix, "BongBong Enterprises");
				Debug.Log("Unpatching done");
				MethodInfo prefix = typeof(UIPageScrollListPatch).GetMethod("Prefix");
				MethodInfo transpiler = typeof(UIPageScrollListPatch).GetMethod("Transpiler");
				harmony.Patch(typeof(UISettingEquipPageScrollList).GetMethod("SetData", AccessTools.all), new HarmonyMethod(prefix), null, new HarmonyMethod(transpiler), null, null);
				Debug.Log("UISettingEquipPageScrollList patch done");
				harmony.Patch(typeof(UIEquipPageScrollList).GetMethod("SetData", AccessTools.all), new HarmonyMethod(prefix), null, new HarmonyMethod(transpiler), null, null);
				Debug.Log("UIEquipPageScrollList patch done");
				Debug.Log("All patches done");
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}
	}
	[HarmonyPatch(typeof(AssemblyManager))]
	[HarmonyPatch("LoadTypesFromAssembly")]
	class TypeDictPatch {
		static void Postfix(Assembly assembly) {
			foreach (Type type in assembly.GetTypes())
			{
				string name = type.Name;
				if (type.IsSubclassOf(typeof(DiceCardSelfAbilityBase)))
				{
					Harmony_Patch.DiceCardSelfAbilityBaseList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(DiceCardAbilityBase)))
				{
					Harmony_Patch.DiceCardAbilityBaseList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(BattleDialogueModel)))
				{
					Harmony_Patch.BattleDialogueList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(BehaviourActionBase)))
				{
					Harmony_Patch.BehaviourActionList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(DiceCardPriorityBase)))
				{
					Harmony_Patch.DiceCardPriorityBaseList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(EmotionCardAbilityBase)))
				{
					Harmony_Patch.EmotionCardAbilityBaseList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(EnemyTeamStageManager)))
				{
					Harmony_Patch.EnemyTeamStageManagerList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(PassiveAbilityBase)))
				{
					Harmony_Patch.PassiveAbilityBaseList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(EnemyUnitTargetSetter)))
				{
					Harmony_Patch.EnemyUnitTargetSetterList[name] = type;
				}
				else if (type.IsSubclassOf(typeof(EnemyUnitAggroSetter)))
				{
					Harmony_Patch.EnemyUnitAggroSetterList[name] = type;
				}
			}
		}
	}
	public class UIPageScrollListPatch {
		private static int num;
		public static void Prefix() {
			Harmony_Patch.ModEpMatch = new Dictionary<int, int>();
			num = Enum.GetValues(typeof(UIStoryLine)).Length - 1;
		}
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var startIndex = -1;
			var endIndex = -1;
			var scanIndex = 0;
			CodeInstruction[] copiedLocal = null;

			var codes = new List<CodeInstruction>(instructions);
			for (var i = scanIndex; i < codes.Count; i++) {
				if (codes[i].opcode == OpCodes.Ldloc_2
                    && codes[i+1].opcode == OpCodes.Stfld
                    && codes[i+2].opcode == OpCodes.Ldloc_S
                    && codes[i+3].opcode == OpCodes.Ldfld
                    && codes[i+4].opcode == OpCodes.Ldfld) {
						copiedLocal = new CodeInstruction[]{
							codes[i+3],
							codes[i+4]
						};
						scanIndex = i+5;
					}
			}
			for (var i = scanIndex; i < codes.Count; i++)
			{
				if (codes[i].opcode != OpCodes.Ldstr) {
					continue;
				}
				var strOperand = codes[i].operand as string;
				if (strOperand == "스토리 string enum 변환 오류")
				{
					startIndex = i-1;
					endIndex = i+3;
					break;
				}
			}

			if (copiedLocal != null && startIndex != -1 && endIndex != -1) {
				codes[startIndex].operand = (byte)4;
				var classType = typeof(UIEquipPageScrollList);
                var newInstructions = new List<CodeInstruction>
                {
					copiedLocal[0],
					copiedLocal[1],
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(classType, "currentBookModelList")),
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(classType, "totalkeysdata")),
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(classType, "currentStoryBooksDic")),
                    new CodeInstruction(OpCodes.Call, AccessTools.Method(
                        typeof(UIPageScrollListPatch),
                        "BBE_SetData",
                        new Type[] {
							typeof(BookModel),
							typeof(List<BookModel>),
							typeof(List<UIStoryKeyData>),
							typeof(Dictionary<UIStoryKeyData, List<BookModel>>)
						}
					))
                };
				codes.RemoveRange(startIndex + 1, endIndex - startIndex - 1);
                codes.InsertRange(startIndex + 1, newInstructions);
				Debug.Log("UIPageScrollListPatch transpiler successful");
			} else {
				Debug.LogError("UIPageScrollListPatch transpiler failed!");
			}
			return codes.AsEnumerable();
		}
		public static void BBE_SetData(BookModel bookModel, List<BookModel> currentBookModelList, List<UIStoryKeyData> totalkeysdata, Dictionary<UIStoryKeyData, List<BookModel>> currentStoryBooksDic) {
			string bookIcon = bookModel.ClassInfo.BookIcon;
			try {
				Harmony_Patch.ModEpMatch[bookModel.ClassInfo.episode] = bookModel.ClassInfo._id;
				UIStoryKeyData uistoryKeyData = totalkeysdata.Find((UIStoryKeyData x) => x.chapter == bookModel.ClassInfo.Chapter && x.StoryLine == (UIStoryLine)num);
				if (uistoryKeyData == null)
				{
					uistoryKeyData = new UIStoryKeyData(bookModel.ClassInfo.Chapter, (UIStoryLine)num);
					totalkeysdata.Add(uistoryKeyData);
				}
				List<BookModel> list5 = new List<BookModel>();
				foreach (BookModel bookModel2 in currentBookModelList)
				{
					if (bookModel2.ClassInfo.episode == bookModel.ClassInfo.episode)
					{
						list5.Add(bookModel2);
					}
				}
				if (!currentStoryBooksDic.ContainsKey(uistoryKeyData))
				{
					currentStoryBooksDic[uistoryKeyData] = list5;
				}
				else
				{
					currentStoryBooksDic[uistoryKeyData].AddRange(list5);
				}
				int num3 = num;
				num = num3 + 1;
			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}
	}
}