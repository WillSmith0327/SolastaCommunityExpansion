using SolastaCommunityExpansion.Api;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using SolastaCommunityExpansion.Models;
using SolastaCommunityExpansion.Properties;
using SolastaCommunityExpansion.Utils;

namespace SolastaCommunityExpansion.Classes.Witch.Features;

internal class WitchFamiliarFeatureSetBuilder : FeatureDefinitionFeatureSetBuilder
{
    private const string WitchFamiliarFeatureSetName = "WitchFamiliarFeatureSet";

    internal static readonly FeatureDefinitionFeatureSet WitchFamiliarFeatureSet =
        CreateAndAddToDB(WitchFamiliarFeatureSetName);

    protected WitchFamiliarFeatureSetBuilder(string name) : base(
        DatabaseHelper.FeatureDefinitionFeatureSets.FeatureSetGreenmageWardenOfTheForest, name, CENamespaceGuid)
    {
        Definition.GuiPresentation.Title = "Feature/&WitchFamiliarFeatureSetTitle";
        Definition.GuiPresentation.Description = "Feature/&WitchFamiliarFeatureSetDescription";

        Definition.FeatureSet.Clear();

        WitchFamiliarSummons.buildWitchFamiliarInvisibilityPower();
        WitchFamiliarSummons.buildWitchFamiliarScarePower();
        var owl = WitchFamiliarSummons.buildCustomOwl();
        var pseudodragon = WitchFamiliarSummons.buildCustomPseudodragon();
        var sprite = WitchFamiliarSummons.buildCustomSprite();
        var imp = WitchFamiliarSummons.buildCustomImp();
        var quasit = WitchFamiliarSummons.buildCustomQuasit();

        var effectDescriptionOwl = new EffectDescriptionBuilder()
            .SetDurationData(RuleDefinitions.DurationType.UntilLongRest, 1,
                RuleDefinitions.TurnOccurenceType.EndOfTurn)
            .SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Distance, 2,
                RuleDefinitions.TargetType.Position)
            .AddEffectForm(new EffectFormBuilder()
                .SetSummonCreatureForm(1, owl.name, false,
                    DatabaseHelper.ConditionDefinitions.ConditionFlyingBootsWinged)
                .Build()
            )
            .SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.ConjureElementalAir.EffectDescription
                .EffectParticleParameters);

        var WitchFamiliarOwlGui = new GuiPresentationBuilder(
            "Spell/&WitchFamiliarOwlTitle",
            "Spell/&WitchFamiliarOwlDescription");
        WitchFamiliarOwlGui.SetSpriteReference(owl.GuiPresentation.SpriteReference);


        var effectDescriptionPseudodragon = new EffectDescriptionBuilder()
            .SetDurationData(RuleDefinitions.DurationType.UntilLongRest, 1,
                RuleDefinitions.TurnOccurenceType.EndOfTurn)
            .SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Distance, 2,
                RuleDefinitions.TargetType.Position)
            .AddEffectForm(new EffectFormBuilder()
                .SetSummonCreatureForm(1, pseudodragon.name, false,
                    DatabaseHelper.ConditionDefinitions.ConditionFlyingBootsWinged)
                .Build()
            )
            .SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.ConjureElementalAir.EffectDescription
                .EffectParticleParameters);

        var WitchFamiliarPseudodragonGui = new GuiPresentationBuilder(
            "Spell/&WitchFamiliarPseudodragonTitle",
            "Spell/&WitchFamiliarPseudodragonDescription");
        WitchFamiliarPseudodragonGui.SetSpriteReference(pseudodragon.GuiPresentation.SpriteReference);


        var effectDescriptionSprite = new EffectDescriptionBuilder();
        effectDescriptionSprite.SetDurationData(RuleDefinitions.DurationType.UntilLongRest, 1,
            RuleDefinitions.TurnOccurenceType.EndOfTurn);
        effectDescriptionSprite.SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Distance, 2,
            RuleDefinitions.TargetType.Position, 1, 1, ActionDefinitions.ItemSelectionType.Equiped);
        effectDescriptionSprite.AddEffectForm(new EffectFormBuilder().SetSummonCreatureForm(1, sprite.name, false,
            DatabaseHelper.ConditionDefinitions.ConditionFlyingBootsWinged).Build());
        effectDescriptionSprite.SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.ConjureElementalAir
            .EffectDescription.EffectParticleParameters);

        var WitchFamiliarSpriteGui = new GuiPresentationBuilder(
            "Spell/&WitchFamiliarSpriteTitle",
            "Spell/&WitchFamiliarSpriteDescription");
        WitchFamiliarSpriteGui.SetSpriteReference(sprite.GuiPresentation.SpriteReference);


        var effectDescriptionImp = new EffectDescriptionBuilder()
            .SetDurationData(RuleDefinitions.DurationType.UntilLongRest, 1,
                RuleDefinitions.TurnOccurenceType.EndOfTurn)
            .SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Distance, 2,
                RuleDefinitions.TargetType.Position, 1, 1, ActionDefinitions.ItemSelectionType.Equiped)
            .AddEffectForm(new EffectFormBuilder().SetSummonCreatureForm(1, imp.name, false,
                DatabaseHelper.ConditionDefinitions.ConditionFlyingBootsWinged).Build())
            .SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.ConjureElementalAir.EffectDescription
                .EffectParticleParameters);

        var WitchFamiliarImpGui = new GuiPresentationBuilder(
            "Spell/&WitchFamiliarImpTitle",
            "Spell/&WitchFamiliarImpDescription");
        WitchFamiliarImpGui.SetSpriteReference(imp.GuiPresentation.SpriteReference);


        var effectDescriptionQuasit = new EffectDescriptionBuilder();
        effectDescriptionQuasit.SetDurationData(RuleDefinitions.DurationType.UntilLongRest, 1,
            RuleDefinitions.TurnOccurenceType.EndOfTurn);
        effectDescriptionQuasit.SetTargetingData(RuleDefinitions.Side.Ally, RuleDefinitions.RangeType.Distance, 2,
            RuleDefinitions.TargetType.Position, 1, 1, ActionDefinitions.ItemSelectionType.Equiped);
        effectDescriptionQuasit.AddEffectForm(new EffectFormBuilder().SetSummonCreatureForm(1, quasit.name)
            .Build());
        effectDescriptionQuasit.SetParticleEffectParameters(DatabaseHelper.SpellDefinitions.ConjureElementalAir
            .EffectDescription.EffectParticleParameters);

        var WitchFamiliarQuasitGui = new GuiPresentationBuilder(
            "Spell/&WitchFamiliarQuasitTitle",
            "Spell/&WitchFamiliarQuasitDescription");
        WitchFamiliarQuasitGui.SetSpriteReference(quasit.GuiPresentation.SpriteReference);


        var WitchFamiliarOwlPowerBuilder = FeatureDefinitionPowerBuilder
            .Create("WitchFamiliarOwlPower", CENamespaceGuid)
            .SetGuiPresentation(WitchFamiliarOwlGui.Build())
            .Configure(
                1,
                RuleDefinitions.UsesDetermination.Fixed,
                AttributeDefinitions.Charisma,
                RuleDefinitions.ActivationTime.Hours1,
                1,
                RuleDefinitions.RechargeRate.AtWill,
                false,
                false,
                AttributeDefinitions.Charisma,
                effectDescriptionOwl.Build(),
                true);
        var WitchFamiliarOwlPower = WitchFamiliarOwlPowerBuilder.AddToDB();

        var WitchFamiliarPseudodragonPowerBuilder = FeatureDefinitionPowerBuilder
            .Create("WitchFamiliarPseudodragonPower", CENamespaceGuid)
            .SetGuiPresentation(WitchFamiliarPseudodragonGui.Build())
            .Configure(
                1,
                RuleDefinitions.UsesDetermination.Fixed,
                AttributeDefinitions.Charisma,
                RuleDefinitions.ActivationTime.Hours1,
                1,
                RuleDefinitions.RechargeRate.AtWill,
                false,
                false,
                AttributeDefinitions.Charisma,
                effectDescriptionPseudodragon.Build(),
                true);
        var WitchFamiliarPseudodragonPower = WitchFamiliarPseudodragonPowerBuilder.AddToDB();

        var WitchFamiliarSpritePowerBuilder = FeatureDefinitionPowerBuilder
            .Create("WitchFamiliarSpritePower", CENamespaceGuid)
            .SetGuiPresentation(WitchFamiliarSpriteGui.Build())
            .Configure(
                1,
                RuleDefinitions.UsesDetermination.Fixed,
                AttributeDefinitions.Charisma,
                RuleDefinitions.ActivationTime.Hours1,
                1,
                RuleDefinitions.RechargeRate.AtWill,
                false,
                false,
                AttributeDefinitions.Charisma,
                effectDescriptionSprite.Build(),
                true);
        var WitchFamiliarSpritePower = WitchFamiliarSpritePowerBuilder.AddToDB();

        var WitchFamiliarImpPowerBuilder = FeatureDefinitionPowerBuilder
            .Create("WitchFamiliarImpPower", CENamespaceGuid)
            .SetGuiPresentation(WitchFamiliarImpGui.Build())
            .Configure(
                1,
                RuleDefinitions.UsesDetermination.Fixed,
                AttributeDefinitions.Charisma,
                RuleDefinitions.ActivationTime.Hours1,
                1,
                RuleDefinitions.RechargeRate.AtWill,
                false,
                false,
                AttributeDefinitions.Charisma,
                effectDescriptionImp.Build(),
                true);
        var WitchFamiliarImpPower = WitchFamiliarImpPowerBuilder.AddToDB();

        var WitchFamiliarQuasitPowerBuilder = FeatureDefinitionPowerBuilder
            .Create("WitchFamiliarQuasitPower", CENamespaceGuid)
            .SetGuiPresentation(WitchFamiliarQuasitGui.Build())
            .Configure(
                1,
                RuleDefinitions.UsesDetermination.Fixed,
                AttributeDefinitions.Charisma,
                RuleDefinitions.ActivationTime.Hours1,
                1,
                RuleDefinitions.RechargeRate.AtWill,
                false,
                false,
                AttributeDefinitions.Charisma,
                effectDescriptionQuasit.Build(),
                true);
        var WitchFamiliarQuasitPower = WitchFamiliarQuasitPowerBuilder.AddToDB();

        var WitchFamiliarPowerBundle = FeatureDefinitionPowerPoolBuilder
            .Create("WitchFamiliarBundlePower", CENamespaceGuid)
            .SetGuiPresentation(Category.Power,
                CustomIcons.CreateAssetReferenceSprite("WitchChainSummon",
                    Resources.WarlockChainSummon, 128, 64))
            .SetActivation(RuleDefinitions.ActivationTime.Hours1, 1)
            .AddToDB();


        PowerBundleContext.RegisterPowerBundle(WitchFamiliarPowerBundle, false,
            WitchFamiliarOwlPower,
            WitchFamiliarPseudodragonPower,
            WitchFamiliarSpritePower,
            WitchFamiliarImpPower,
            WitchFamiliarQuasitPower
        );
        Definition.FeatureSet.Add(WitchFamiliarPowerBundle);

        GlobalUniqueEffects.AddToGroup(GlobalUniqueEffects.Group.Familiar,
            WitchFamiliarOwlPower,
            WitchFamiliarPseudodragonPower,
            WitchFamiliarSpritePower,
            WitchFamiliarImpPower,
            WitchFamiliarQuasitPower
        );

        Definition.mode = FeatureDefinitionFeatureSet.FeatureSetMode.Union;
    }

    internal static FeatureDefinitionFeatureSet CreateAndAddToDB(string name)
    {
        return new WitchFamiliarFeatureSetBuilder(name).AddToDB();
    }
}
