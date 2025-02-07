﻿/*
 * Path Of The Rage Mage is a Third Caster for Barbarians, inspired by the subclass of the same name from Valdas Spire Of Secrets.
 * Created by William Smith AKA DemonSlayer730
 * Ver. 1.0
 * 07/08/2022 (MM/DD/YYYY)
    */

using System;
using SolastaCommunityExpansion.Api.Extensions;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SolastaCommunityExpansion.CustomDefinitions;
using SolastaCommunityExpansion.CustomInterfaces;
using static RuleDefinitions;
using static SolastaCommunityExpansion.Api.DatabaseHelper;
using static SolastaCommunityExpansion.Api.DatabaseHelper.CharacterSubclassDefinitions;
using SolastaCommunityExpansion.Models;
using static SolastaCommunityExpansion.Api.DatabaseHelper.ConditionDefinitions;
using static SolastaCommunityExpansion.Api.DatabaseHelper.FeatureDefinitionFightingStyleChoices;
using static SolastaCommunityExpansion.Api.DatabaseHelper.FeatureDefinitionAdditionalDamages;
using static SolastaCommunityExpansion.Api.DatabaseHelper.FeatureDefinitionPowers;

namespace SolastaCommunityExpansion.Subclasses.Barbarian;

// creating subclass Class, don't forget to add subclass to Models/SubclassesContext.cs and create a txt file in Translations
internal sealed class PathOfTheRageMage : AbstractSubclass
{

    private readonly CharacterSubclassDefinition Subclass;
    private static readonly Guid SubclassNamespace = new("6a9ec115-29db-40b4-9b1d-ad55abede214"); // GUID generated online
    internal PathOfTheRageMage()
    {
        var magicAffinity = FeatureDefinitionMagicAffinityBuilder // Adds some rules for magic
            .Create("MagicAffinityBarbarianPathOfTheRageMage", SubclassNamespace)
            .SetHandsFullCastingModifiers(true, true, true) // lets character cast spells withut somatic components and sets weapon as focus 
            .SetGuiPresentationNoContent(true) // prevents anything from showing on GUI
            .SetCastingModifiers(0, RuleDefinitions.SpellParamsModifierType.None, 0,
                RuleDefinitions.SpellParamsModifierType.FlatValue, true, false, false) // no additional attack or DC modifiers, no disadvantage from enemies in close proximity
            .AddToDB();

        var spellCasting = FeatureDefinitionCastSpellBuilder // allows spell casting 
            .Create("CastSpellPathOfTheRageMage", SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheRageMageSpellcasting", Category.Feature)
            .SetSpellCastingOrigin(FeatureDefinitionCastSpell.CastingOrigin.Subclass)
            .SetSpellCastingAbility(AttributeDefinitions.Charisma) // Charisma is spellcasting modifier
            .SetSpellList(SpellListDefinitions.SpellListSorcerer) // all spells from Sorcerer list
            .SetSpellKnowledge(RuleDefinitions.SpellKnowledge.Selection) // you learn new spells at certain levels
            .SetSpellReadyness(RuleDefinitions.SpellReadyness.AllKnown)
            .SetSlotsRecharge(RuleDefinitions.RechargeRate.LongRest) // Spell slots back at long rest
            .SetKnownCantrips(2, 3, FeatureDefinitionCastSpellBuilder.CasterProgression.THIRD_CASTER) // know 2 cantrips at level 3, gain at rate of third caster
            .SetKnownSpells(3, 3, FeatureDefinitionCastSpellBuilder.CasterProgression.THIRD_CASTER) // start with 2 level 1 spells, gain at rate of third caster
            .SetSlotsPerLevel(3, FeatureDefinitionCastSpellBuilder.CasterProgression.THIRD_CASTER); // gain spell slots at rate of third caster

        var skillProf = FeatureDefinitionProficiencyBuilder
            .Create("ProficiencySkillPathOfTheRageMage", SubclassNamespace)
            .SetGuiPresentation("ProficiencySkillPathOfTheRageMage", Category.Feature)
            .SetProficiencies(RuleDefinitions.ProficiencyType.Skill, SkillDefinitions.Arcana) // gain proficiency in Arcana
            .AddToDB();

        var supernaturalExploits = FeatureDefinitionBuilder // A general definition of the Supernatural Exploits feature at level up
            .Create("supernaturalExploitsPathOfTheRagemage", SubclassNamespace)
            .SetGuiPresentation("supernaturalExploitsPathOfTheRagemage", Category.Feature)
            .AddToDB();

        var supernaturalExploitsDarkvision = FeatureDefinitionPowerBuilder // lets you cast Darkvision once per long rest
            .Create("supernaturalExploitsDarkvisionPathOfTheRagemage", SubclassNamespace)
            .SetGuiPresentation("supernaturalExploitsDarkvisionPathOfTheRagemage", Category.Feature)
            .SetGuiPresentation(Category.Feature, SpellDefinitions.Darkvision.GuiPresentation.SpriteReference) // setting sprite of power to that of Darkvision spell
            .SetEffectDescription(SpellDefinitions.Darkvision.EffectDescription.Copy()) // copies the effect of darkvision spell
            .SetActivationTime(RuleDefinitions.ActivationTime.Action) // requires action
            .SetFixedUsesPerRecharge(1) // once per long rest
            .SetRechargeRate(RuleDefinitions.RechargeRate.LongRest)
            .SetCostPerUse(1)
            .SetShowCasting(true)
            .AddToDB();

        var supernaturalExploitsFeatherfall = FeatureDefinitionPowerBuilder // lets you cast Featherfal once per long rest
            .Create("supernaturalExploitsFeatherfallPathOfTheRagemage", SubclassNamespace)
            .SetGuiPresentation(Category.Feature, SpellDefinitions.FeatherFall.GuiPresentation.SpriteReference) // setting sprite of power to that of Featherfal spell
            .SetEffectDescription(SpellDefinitions.FeatherFall.EffectDescription.Copy()) // copies the effect of Featherfal spell
            .SetActivationTime(RuleDefinitions.ActivationTime.Action) // requires action
            .SetFixedUsesPerRecharge(1)
            .SetRechargeRate(RuleDefinitions.RechargeRate.LongRest) // once per long rest
            .SetCostPerUse(1)
            .SetShowCasting(true)
            .AddToDB();

        var supernaturalExploitsJump = FeatureDefinitionPowerBuilder // lets you cast Jump once per long rest
            .Create("supernaturalExploitsJumpPathOfTheRagemage", SubclassNamespace)
            .SetGuiPresentation(Category.Feature, SpellDefinitions.Jump.GuiPresentation.SpriteReference) // setting sprite of power to that of Jump spell
            .SetEffectDescription(SpellDefinitions.Jump.EffectDescription.Copy()) // copies the effect of Jump spell
            .SetActivationTime(RuleDefinitions.ActivationTime.Action) // requires action
            .SetFixedUsesPerRecharge(1)
            .SetRechargeRate(RuleDefinitions.RechargeRate.LongRest) // once per long rest
            .SetCostPerUse(1)
            .SetShowCasting(true)
            .AddToDB();

        var supernaturalExploitsSeeInvisibility = FeatureDefinitionPowerBuilder // lets you cast See Invisibility once per long rest
            .Create("supernaturalExploitsSeeInvisibilityPathOfTheRagemage", SubclassNamespace)
            .SetGuiPresentation(Category.Feature, SpellDefinitions.SeeInvisibility.GuiPresentation.SpriteReference) // setting sprite of power to that of See Invisibility spell
            .SetEffectDescription(SpellDefinitions.SeeInvisibility.EffectDescription.Copy()) // copies the effect of See Invisibility spell
            .SetActivationTime(RuleDefinitions.ActivationTime.Action) // requires action
            .SetFixedUsesPerRecharge(1)
            .SetRechargeRate(RuleDefinitions.RechargeRate.LongRest) // once per long rest
            .SetCostPerUse(1)
            .SetShowCasting(true)
            .AddToDB();

        // TODO: I'd like one of two bonus effects for this subclass. Either
        // A.) You can cast spells while raging, this keeps your rage goin same as hitting opponent or
        // B.) at 6th level, when you cast a cantrip you can attack once AND at 14th level, when you cast spell of 1st level or higher, you can attack once

        /* var bonusAttack = FeatureDefinitionOnMagicalAttackDamageEffectBuilder
             .Create("ArcaneRampagePathOfTheRagemage", SubclassNamespace)
             .SetGuiPresentation(Category.Feature)
             .AddFeatures(FeatureDefinitionAdditionalActionBuilder
             .SetActionType(ActionDefinitions.ActionType.Bonus)
             .SetRestrictedActions(ActionDefinitions.Id.AttackMain)
             .SetMaxAttacksNumber(-1)
             .AddToDB())
             .AddToDB(); */

        var arcaneExplosion = FeatureDefinitionAdditionalDamageBuilder // deal additional damage while raging
            .Create("arcaneExplosionPathOfTheMageRage", SubclassNamespace)
            .SetGuiPresentation("arcaneExplosionPathOfTheMageRage", Category.Feature)
            .SetDamageDice(RuleDefinitions.DieType.D6, 1) // deal 1d6 force damage while raging
            .SetAdvancement(RuleDefinitions.AdditionalDamageAdvancement.ClassLevel, // damage increases to 2d6 at 14th level
                        (6, 1),
                        (7, 1),
                        (8, 1),
                        (9, 1),
                        (10, 1),
                        (11, 1),
                        (12, 1),
                        (13, 1),
                        (14, 2),
                        (15, 2),
                        (16, 2),
                        (17, 2),
                        (18, 2),
                        (19, 2),
                        (20, 2)
                    )
                .SetFrequencyLimit(RuleDefinitions.FeatureLimitedUsage.OncePerTurn) // only first hit each turn
                .SetTriggerCondition(RuleDefinitions.AdditionalDamageTriggerCondition.Raging) // only while raging
                .SetSpecificDamageType(DamageTypeForce).AddToDB();

        var enhancedArcaneExplosion = FeatureDefinitionPowerBuilder // dummy feture to include in GUI
            .Create("enhancedArcaneExplosionPathOfTheMageRage", SubclassNamespace)
            .SetGuiPresentation("enhancedArcaneExplosionPathOfTheMageRage", Category.Feature)
            .AddToDB();


        Subclass = CharacterSubclassDefinitionBuilder // adds all of the above features to the subclass at respective levels
            .Create("BarbarianPathOfTheRageMage", SubclassNamespace)
            .SetGuiPresentation("BarbarianPathOfTheRageMage", Category.Subclass, DomainBattle.GuiPresentation.SpriteReference)
            .AddFeatureAtLevel(spellCasting.AddToDB(), 3)
            .AddFeatureAtLevel(magicAffinity, 3)
            .AddFeatureAtLevel(skillProf, 3)
            .AddFeatureAtLevel(arcaneExplosion, 6)
            .AddFeatureAtLevel(supernaturalExploits, 10)
            .AddFeatureAtLevel(supernaturalExploitsDarkvision, 10)
            .AddFeatureAtLevel(supernaturalExploitsFeatherfall, 10)
            .AddFeatureAtLevel(supernaturalExploitsJump, 10)
            .AddFeatureAtLevel(supernaturalExploitsSeeInvisibility, 10)
            .AddFeatureAtLevel(enhancedArcaneExplosion, 14).AddToDB();

    }



    internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList() // required method by abstract class, gets which class this subclass belongs to 
    {
        return FeatureDefinitionSubclassChoices.SubclassChoiceBarbarianPrimalPath;
    }

    internal override CharacterSubclassDefinition GetSubclass() // required method by abstract class, returns subclass object
    {
        return Subclass;
    }
}
