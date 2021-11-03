using System;
using System.Reflection;
using HarmonyLib;
using BepInEx;
using BepInEx.Harmony;
using BaseMod;

namespace BBE_Workshop {
	[BepInPlugin("LoR.uGuardian.BBE_Workshop", "BBE_Workshop_Patch", "1.0")]
	public class BBE_Workshop : BaseUnityPlugin {
		void Awake() {
			Harmony harmony = new Harmony("LoR.uGuardian.BBE_Workshop");
			harmony.PatchAll();
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
}