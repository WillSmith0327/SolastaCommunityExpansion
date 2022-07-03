﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using SolastaCommunityExpansion.Classes.Warlock;
using SolastaCommunityExpansion.Models;
using static SolastaCommunityExpansion.Level20.SpellsHelper;

namespace SolastaCommunityExpansion.Patches.CustomFeatures.SharedSpells;

[HarmonyPatch(typeof(RulesetCharacter), "ApplyRest")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class RulesetCharacter_ApplyRest
{
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var restoreAllSpellSlotsMethod = typeof(RulesetSpellRepertoire).GetMethod("RestoreAllSpellSlots");
        var myRestoreAllSpellSlotsMethod = typeof(RulesetCharacter_ApplyRest).GetMethod("RestoreAllSpellSlots");

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(restoreAllSpellSlotsMethod))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0); // rulesetCharacter
                yield return new CodeInstruction(OpCodes.Ldarg_1); // restType
                yield return new CodeInstruction(OpCodes.Call, myRestoreAllSpellSlotsMethod);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static void RestoreAllSpellSlots(RulesetSpellRepertoire __instance, RulesetCharacter rulesetCharacter,
        RuleDefinitions.RestType restType)
    {
        if (restType == RuleDefinitions.RestType.LongRest
            || rulesetCharacter is not RulesetCharacterHero heroWithSpellRepertoire)
        {
            rulesetCharacter.RestoreAllSpellSlots();

            return;
        }

        var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(heroWithSpellRepertoire);
        var slotsToRestore = SharedSpellsContext.GetWarlockUsedSlots(heroWithSpellRepertoire);

        foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires
                     .Where(x => x.SpellCastingRace == null))
        {
            var usedSpellsSlots =
                spellRepertoire.usedSpellsSlots;

            for (var i = WarlockSpells.PactMagicSlotTabIndex; i <= warlockSpellLevel; i++)
            {
                if (usedSpellsSlots.ContainsKey(i))
                {
                    usedSpellsSlots[i] -= slotsToRestore;
                }
            }

            spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
        }
    }
}

// logic to correctly offer / calculate spell slots on all different scenarios
[HarmonyPatch(typeof(RulesetCharacter), "RefreshSpellRepertoires")]
[SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Patch")]
internal static class RulesetCharacter_RefreshSpellRepertoires
{
    private static readonly Dictionary<int, int> affinityProviderAdditionalSlots = new();

    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var computeSpellSlotsMethod = typeof(RulesetSpellRepertoire).GetMethod("ComputeSpellSlots");
        var myComputeSpellSlotsMethod =
            typeof(RulesetCharacter_RefreshSpellRepertoires).GetMethod("ComputeSpellSlots");
        var finishRepertoiresRefreshMethod =
            typeof(RulesetCharacter_RefreshSpellRepertoires).GetMethod("FinishRepertoiresRefresh");

        foreach (var instruction in instructions)
        {
            if (instruction.Calls(computeSpellSlotsMethod))
            {
                yield return new CodeInstruction(OpCodes.Pop); // featureDefinitions
                yield return new CodeInstruction(OpCodes.Call, myComputeSpellSlotsMethod);
            }
            else if (instruction.opcode == OpCodes.Brtrue_S)
            {
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call, finishRepertoiresRefreshMethod);
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static void ComputeSpellSlots(RulesetSpellRepertoire spellRepertoire)
    {
        // will calculate additional slots from features later
        spellRepertoire.ComputeSpellSlots(null);
    }

    public static void FinishRepertoiresRefresh(RulesetCharacter rulesetCharacter)
    {
        if (rulesetCharacter is not RulesetCharacterHero heroWithSpellRepertoire)
        {
            return;
        }

        // calculates additional slots from features
        affinityProviderAdditionalSlots.Clear();

        foreach (var spellCastingAffinityProvider in rulesetCharacter.FeaturesToBrowse
                     .OfType<ISpellCastingAffinityProvider>())
        {
            foreach (var additionalSlot in spellCastingAffinityProvider.AdditionalSlots)
            {
                var slotLevel = additionalSlot.SlotLevel;

                affinityProviderAdditionalSlots.TryAdd(slotLevel, 0);
                affinityProviderAdditionalSlots[slotLevel] += additionalSlot.SlotsNumber;
            }
        }

        // calculates shared slots system across all repertoires except for Race and Warlock
        var isSharedCaster = SharedSpellsContext.IsSharedcaster(heroWithSpellRepertoire);

        foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires
                     .Where(x => x.SpellCastingRace == null &&
                                 x.SpellCastingClass != IntegrationContext.WarlockClass))
        {
            var spellsSlotCapacities =
                spellRepertoire.spellsSlotCapacities;

            // replaces standard caster slots with shared slots system
            if (isSharedCaster)
            {
                var sharedCasterLevel = SharedSpellsContext.GetSharedCasterLevel(heroWithSpellRepertoire);
                var sharedSpellLevel = SharedSpellsContext.GetSharedSpellLevel(heroWithSpellRepertoire);

                spellsSlotCapacities.Clear();

                // adds shared slots
                for (var i = 1; i <= sharedSpellLevel; i++)
                {
                    spellsSlotCapacities[i] = FullCastingSlots[sharedCasterLevel - 1].Slots[i - 1];
                }
            }

            // adds affinity provider slots collected in my custom compute spell slots
            foreach (var slot in affinityProviderAdditionalSlots)
            {
                spellsSlotCapacities.TryAdd(slot.Key, 0);
                spellsSlotCapacities[slot.Key] += slot.Value;
            }

            spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
        }

        // collects warlock and non warlock repertoires for consolidation
        var warlockRepertoire = SharedSpellsContext.GetWarlockSpellRepertoire(heroWithSpellRepertoire);
        var anySharedRepertoire = heroWithSpellRepertoire.SpellRepertoires.Find(sr =>
            !SharedSpellsContext.IsWarlock(sr.SpellCastingClass) &&
            (sr.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Class ||
             sr.SpellCastingFeature.SpellCastingOrigin == FeatureDefinitionCastSpell.CastingOrigin.Subclass));

        // combines the Shared Slot System and Warlock Pact Magic
        if (warlockRepertoire == null || anySharedRepertoire == null)
        {
            return;
        }

        {
            var warlockSpellLevel = SharedSpellsContext.GetWarlockSpellLevel(heroWithSpellRepertoire);
            var warlockSlotsCapacities =
                warlockRepertoire.spellsSlotCapacities;
            var anySharedSlotsCapacities =
                anySharedRepertoire.spellsSlotCapacities;

            // first consolidates under Warlock repertoire
            for (var i = 1; i <= Math.Max(warlockSlotsCapacities.Count, anySharedSlotsCapacities.Count); i++)
            {
                warlockSlotsCapacities.TryAdd(i, 0);

                if (anySharedSlotsCapacities.ContainsKey(i))
                {
                    warlockSlotsCapacities[i] += anySharedSlotsCapacities[i];
                }
            }

            // then copy over Warlock repertoire to all others
            foreach (var spellRepertoire in heroWithSpellRepertoire.SpellRepertoires
                         .Where(x => x.SpellCastingRace == null &&
                                     x.SpellCastingClass != IntegrationContext.WarlockClass))
            {
                var spellsSlotCapacities =
                    spellRepertoire.spellsSlotCapacities;

                spellsSlotCapacities.Clear();

                foreach (var warlockSlotCapacities in warlockSlotsCapacities)
                {
                    spellsSlotCapacities[warlockSlotCapacities.Key] = warlockSlotCapacities.Value;
                }

                spellRepertoire.RepertoireRefreshed?.Invoke(spellRepertoire);
            }

            warlockRepertoire?.RepertoireRefreshed?.Invoke(warlockRepertoire);
        }
    }
}
