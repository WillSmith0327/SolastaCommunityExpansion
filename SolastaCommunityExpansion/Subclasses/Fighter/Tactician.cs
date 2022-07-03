﻿using SolastaCommunityExpansion.Api.Extensions;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using SolastaCommunityExpansion.CustomDefinitions;
using static SolastaCommunityExpansion.Api.DatabaseHelper;
using static SolastaCommunityExpansion.Api.DatabaseHelper.CharacterSubclassDefinitions;

namespace SolastaCommunityExpansion.Subclasses.Fighter;

internal sealed class Tactician : AbstractSubclass
{
    private CharacterSubclassDefinition Subclass;

    internal override FeatureDefinitionSubclassChoice GetSubclassChoiceList()
    {
        return FeatureDefinitionSubclassChoices.SubclassChoiceFighterMartialArchetypes;
    }

    internal override CharacterSubclassDefinition GetSubclass()
    {
        return Subclass ??= TacticianFighterSubclassBuilder.BuildAndAddSubclass();
    }
}

internal static class KnockDownPowerBuilder
{
    private const string KnockDownPowerName = "KnockDownPower";
    private const string KnockDownPowerNameGuid = "90dd5e81-40d7-4824-89b4-45bcf4c05218";

    private static FeatureDefinitionPowerSharedPool Build(string name, string guid)
    {
        //Create the damage form - TODO make it do the same damage as the wielded weapon?  This doesn't seem possible
        var damageEffect = new EffectForm
        {
            DamageForm = new DamageForm
            {
                DiceNumber = 1,
                DieType = RuleDefinitions.DieType.D6,
                BonusDamage = 2,
                DamageType = "DamageBludgeoning"
            },
            SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.None
        };

        //Create the prone effect - Weirdly enough the motion form seems to also automatically apply the prone condition
        var proneMotionEffect = new EffectForm {FormType = EffectForm.EffectFormType.Motion};
        var proneMotion = new MotionForm {type = MotionForm.MotionType.FallProne, distance = 1};
        proneMotionEffect.motionForm = proneMotion;
        proneMotionEffect.SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.Negates;

        //Add to our new effect
        var newEffectDescription =
            FeatureDefinitionPowers.PowerFighterActionSurge.EffectDescription.Copy();
        newEffectDescription.SetEffectForms(damageEffect, proneMotionEffect);
        newEffectDescription.SetSavingThrowDifficultyAbility(AttributeDefinitions.Strength);
        newEffectDescription.SetDifficultyClassComputation(RuleDefinitions.EffectDifficultyClassComputation
            .AbilityScoreAndProficiency);
        newEffectDescription.SavingThrowAbility = AttributeDefinitions.Strength;
        newEffectDescription.HasSavingThrow = true;
        newEffectDescription.DurationType = RuleDefinitions.DurationType.Round;

        var builder = new FeatureDefinitionPowerSharedPoolBuilder(name, guid,
            TacticianFighterSubclassBuilder.GambitResourcePool, RuleDefinitions.RechargeRate.ShortRest,
            RuleDefinitions.ActivationTime.OnAttackHit,
            1, true, true, AttributeDefinitions.Strength, newEffectDescription,
            new GuiPresentationBuilder("Feature/&KnockDownPowerTitle", "Feature/&KnockDownPowerDescription")
                .SetSpriteReference(FeatureDefinitionPowers.PowerFighterActionSurge.GuiPresentation.SpriteReference)
                .Build(), false);

        return builder.AddToDB();
    }

    internal static FeatureDefinitionPowerSharedPool CreateAndAddToDB()
    {
        return Build(KnockDownPowerName, KnockDownPowerNameGuid);
    }
}

internal static class InspirePowerBuilder
{
    private const string InspirePowerName = "InspirePower";
    private const string InspirePowerNameGuid = "163c28de-48e5-4f75-bdd0-d42374a75ef8";

    private static FeatureDefinitionPowerSharedPool Build(string name, string guid)
    {
        //Create the temp hp form
        var healingEffect = new EffectForm {FormType = EffectForm.EffectFormType.TemporaryHitPoints};
        var tempHpForm = new TemporaryHitPointsForm
        {
            DiceNumber = 1, DieType = RuleDefinitions.DieType.D6, BonusHitPoints = 2
        };
        healingEffect.temporaryHitPointsForm = tempHpForm;

        //Create the bless effect - A fun test, unfortunately the two effects can't have varying durations AFAIK so a bless or similar effect might be overpowered (was thinking a bless for 1 round).  Alternatively both could last 1 minute instead and be intended for in battle.
        //EffectForm blessEffect = new EffectForm();
        //blessEffect.ConditionForm = new ConditionForm();
        //blessEffect.FormType = EffectForm.EffectFormType.Condition;
        //blessEffect.ConditionForm.Operation = ConditionForm.ConditionOperation.Add;
        //blessEffect.ConditionForm.ConditionDefinition = DatabaseHelper.ConditionDefinitions.ConditionBlessed;

        //Add to our new effect
        var newEffectDescription = new EffectDescription();
        newEffectDescription.Copy(FeatureDefinitionPowers.PowerDomainLifePreserveLife.EffectDescription);
        newEffectDescription.EffectForms.Clear();
        newEffectDescription.EffectForms.Add(healingEffect);
        //newEffectDescription.EffectForms.Add(blessEffect);
        newEffectDescription.HasSavingThrow = false;
        newEffectDescription.DurationType = RuleDefinitions.DurationType.Day;
        newEffectDescription.SetTargetSide(RuleDefinitions.Side.Ally);
        newEffectDescription.SetTargetType(RuleDefinitions.TargetType.Individuals);
        newEffectDescription.SetTargetProximityDistance(12);
        newEffectDescription.SetCanBePlacedOnCharacter(true);
        newEffectDescription.SetRangeType(RuleDefinitions.RangeType.Distance);

        var builder = new FeatureDefinitionPowerSharedPoolBuilder(name, guid,
            TacticianFighterSubclassBuilder.GambitResourcePool, RuleDefinitions.RechargeRate.ShortRest,
            RuleDefinitions.ActivationTime.BonusAction,
            1, true, true, AttributeDefinitions.Strength, newEffectDescription,
            new GuiPresentationBuilder("Feature/&InspirePowerTitle", "Feature/&InspirePowerDescription")
                .SetSpriteReference(FeatureDefinitionPowers.PowerDomainLifePreserveLife.GuiPresentation
                    .SpriteReference).Build(), false);

        builder.SetShortTitle("Feature/&InspirePowerTitle");

        return builder.AddToDB();
    }

    internal static FeatureDefinitionPowerSharedPool CreateAndAddToDB()
    {
        return Build(InspirePowerName, InspirePowerNameGuid);
    }
}

internal static class CounterStrikePowerBuilder
{
    private const string CounterStrikePowerName = "CounterStrikePower";
    private const string CounterStrikePowerNameGuid = "88c294ce-14fa-4f7e-8b81-ea4d289e3d8b";

    private static FeatureDefinitionPowerSharedPool Build(string name, string guid)
    {
        //Create the damage form - TODO make it do the same damage as the wielded weapon (seems impossible with current tools, would need to use the AdditionalDamage feature but I'm not sure how to combine that with this to make it a reaction ability).
        var damageEffect = new EffectForm
        {
            DamageForm = new DamageForm
            {
                DiceNumber = 1,
                DieType = RuleDefinitions.DieType.D6,
                BonusDamage = 2,
                DamageType = "DamageBludgeoning"
            },
            SavingThrowAffinity = RuleDefinitions.EffectSavingThrowType.None
        };

        //Add to our new effect
        var newEffectDescription = new EffectDescription();
        newEffectDescription.Copy(FeatureDefinitionPowers.PowerDomainLawHolyRetribution.EffectDescription);
        newEffectDescription.EffectForms.Clear();
        newEffectDescription.EffectForms.Add(damageEffect);

        var builder = new FeatureDefinitionPowerSharedPoolBuilder(name, guid,
            TacticianFighterSubclassBuilder.GambitResourcePool, RuleDefinitions.RechargeRate.ShortRest,
            RuleDefinitions.ActivationTime.Reaction,
            1, true, true, AttributeDefinitions.Strength, newEffectDescription,
            new GuiPresentationBuilder("Feature/&CounterStrikePowerTitle", "Feature/&CounterStrikePowerDescription")
                .SetSpriteReference(FeatureDefinitionPowers.PowerDomainLawHolyRetribution.GuiPresentation
                    .SpriteReference).Build(), false);
        builder.SetReaction(RuleDefinitions.ReactionTriggerContext.HitByMelee, "CounterStrike");

        return builder.AddToDB();
    }

    internal static FeatureDefinitionPowerSharedPool CreateAndAddToDB()
    {
        return Build(CounterStrikePowerName, CounterStrikePowerNameGuid);
    }
}

internal static class GambitResourcePoolAddBuilder
{
    private const string GambitResourcePoolAddName = "GambitResourcePoolAdd";
    private const string GambitResourcePoolAddNameGuid = "056d786a-2611-4981-a652-704fa5056375";

    private const string GambitResourcePoolAdd10Name = "GambitResourcePoolAdd10";
    private const string GambitResourcePoolAdd10Guid = "52b74360-eecf-407c-9445-4515cbb372f3";

    private const string GambitResourcePoolAdd15Name = "GambitResourcePoolAdd15";
    private const string GambitResourcePoolAdd15Guid = "b4307074-cd80-4376-96f0-46f7a3a79b5a";

    private const string GambitResourcePoolAdd18Name = "GambitResourcePoolAdd18";
    private const string GambitResourcePoolAdd18Guid = "c7ced45a-572f-4af0-8ec5-2add074dd7c3";

    private static FeatureDefinitionPower CreateAndAddToDB(string name, string guid)
    {
        return FeatureDefinitionPowerPoolModifierBuilder.Create(name, guid)
            .Configure(1, RuleDefinitions.UsesDetermination.Fixed, AttributeDefinitions.Dexterity,
                TacticianFighterSubclassBuilder.GambitResourcePool)
            .SetGuiPresentation(new GuiPresentationBuilder("Feature/&GambitResourcePoolAddTitle",
                "Feature/&GambitResourcePoolAddDescription").Build())
            .AddToDB();
    }

    internal static FeatureDefinitionPower GambitResourcePoolAdd()
    {
        return CreateAndAddToDB(GambitResourcePoolAddName, GambitResourcePoolAddNameGuid);
    }

    internal static FeatureDefinitionPower GambitResourcePoolAdd10()
    {
        return CreateAndAddToDB(GambitResourcePoolAdd10Name, GambitResourcePoolAdd10Guid);
    }

    internal static FeatureDefinitionPower GambitResourcePoolAdd15()
    {
        return CreateAndAddToDB(GambitResourcePoolAdd15Name, GambitResourcePoolAdd15Guid);
    }

    internal static FeatureDefinitionPower GambitResourcePoolAdd18()
    {
        return CreateAndAddToDB(GambitResourcePoolAdd18Name, GambitResourcePoolAdd18Guid);
    }
}

internal static class TacticianFighterSubclassBuilder
{
    private const string TacticianFighterSubclassName = "TacticianFighter";
    private const string TacticianFighterSubclassNameGuid = "9d32577d-d3ec-4859-b66d-451d071bb117";

    internal static readonly FeatureDefinitionPower GambitResourcePool = FeatureDefinitionPowerPoolBuilder
        .Create("GambitResourcePool", "00da2b27-139a-4ca0-a285-aaa70d108bc8")
        .Configure(4, RuleDefinitions.UsesDetermination.Fixed, AttributeDefinitions.Dexterity,
            RuleDefinitions.RechargeRate.ShortRest)
        .SetGuiPresentation(Category.Feature)
        .AddToDB();

    private static readonly FeatureDefinitionPower GambitResourcePoolAdd10 =
        GambitResourcePoolAddBuilder.GambitResourcePoolAdd10();

    private static readonly FeatureDefinitionPower GambitResourcePoolAdd15 =
        GambitResourcePoolAddBuilder.GambitResourcePoolAdd15();

    private static readonly FeatureDefinitionPower GambitResourcePoolAdd18 =
        GambitResourcePoolAddBuilder.GambitResourcePoolAdd18();

    private static readonly FeatureDefinitionPowerSharedPool KnockDownPower =
        KnockDownPowerBuilder.CreateAndAddToDB();

    private static readonly FeatureDefinitionPowerSharedPool InspirePower = InspirePowerBuilder.CreateAndAddToDB();

    private static readonly FeatureDefinitionPowerSharedPool CounterStrikePower =
        CounterStrikePowerBuilder.CreateAndAddToDB();

    internal static CharacterSubclassDefinition BuildAndAddSubclass()
    {
        return CharacterSubclassDefinitionBuilder
            .Create(TacticianFighterSubclassName, TacticianFighterSubclassNameGuid)
            .SetGuiPresentation("TactitionFighterSubclass", Category.Subclass,
                RoguishShadowCaster.GuiPresentation.SpriteReference)
            .AddFeatureAtLevel(GambitResourcePool, 3)
            .AddFeatureAtLevel(KnockDownPower, 3)
            .AddFeatureAtLevel(InspirePower, 3)
            .AddFeatureAtLevel(CounterStrikePower, 3)
            .AddFeatureAtLevel(FeatureDefinitionFeatureSets.FeatureSetChampionRemarkableAthlete,
                7) //Wasn't sure what to do for level mostly a ribbon feature
            .AddFeatureAtLevel(GambitResourcePoolAdd10, 10)
            .AddFeatureAtLevel(GambitResourcePoolAdd15, 15)
            .AddFeatureAtLevel(GambitResourcePoolAdd18, 18)
            .AddToDB();
    }
}
