using SolastaCommunityExpansion.Api;
using SolastaCommunityExpansion.Builders;
using SolastaCommunityExpansion.Builders.Features;
using static RuleDefinitions;
using static SolastaCommunityExpansion.Api.DatabaseHelper;

namespace SolastaCommunityExpansion.Classes.Witch.Features;

internal static class WitchFamiliarSummons
{
    private static FeatureDefinition _help;
    public static FeatureDefinitionPower WitchFamiliarInvisibilityPower { get; private set; }
    public static FeatureDefinitionPower WitchFamiliarScarePower { get; private set; }

    private static FeatureDefinition Help
    {
        get
        {
            if (_help == null && DatabaseRepository.GetDatabase<FeatureDefinition>()
                    .TryGetElement("HelpAction", out var help))
            {
                _help = help;
            }

            return _help;
        }
    }

    public static void buildWitchFamiliarInvisibilityPower()
    {
        var invisibilty = SpellDefinitions.Invisibility;
        var effectDescription = new EffectDescription();
        effectDescription.Copy(invisibilty.EffectDescription);

        WitchFamiliarInvisibilityPower = FeatureDefinitionPowerBuilder
            .Create("WitchFamiliarInvisibilityPower", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Power,
                SpellDefinitions.Invisibility.GuiPresentation.SpriteReference)
            .SetUsesFixed(1)
            .SetActivation(ActivationTime.Action, 0)
            .SetRechargeRate(RechargeRate.AtWill)
            .SetEffectDescription(new EffectDescriptionBuilder()
                .SetDurationData(DurationType.Permanent)
                .SetTargetingData(Side.Ally, RangeType.Self, 1, TargetType.Self)
                .SetEffectForms(new EffectFormBuilder()
                    .SetConditionForm(ConditionDefinitionBuilder
                            .Create(ConditionDefinitions.ConditionInvisible,
                                "WitchFamiliarInvisibilityCondition", DefinitionBuilder.CENamespaceGuid)
                            .SetGuiPresentation(ConditionDefinitions.ConditionInvisible
                                .GuiPresentation)
                            .SetConditionType(ConditionType.Beneficial)
                            .SetDuration(DurationType.Permanent)
                            .SetSpecialInterruptions(ConditionInterruption.Attacks, ConditionInterruption.CastSpell,
                                ConditionInterruption.UsePower, ConditionInterruption.Damaged)
                            .SetInterruptionDamageThreshold(1)
                            .AddToDB(), ConditionForm.ConditionOperation.Add
                    )
                    .Build()
                )
                .Build())
            .AddToDB();
    }

    public static void buildWitchFamiliarScarePower()
    {
        var fear = SpellDefinitions.Fear;

        WitchFamiliarScarePower = FeatureDefinitionPowerBuilder
            .Create("WitchFamiliarScarePower", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Power, fear.GuiPresentation.SpriteReference)
            .SetUsesFixed(1)
            .SetActivation(ActivationTime.Action, 1)
            .SetRechargeRate(RechargeRate.Dawn)
            .SetEffectDescription(new EffectDescriptionBuilder(fear.EffectDescription)
                .SetSavingThrowData(
                    true,
                    true,
                    AttributeDefinitions.Wisdom,
                    true,
                    EffectDifficultyClassComputation.FixedValue,
                    AttributeDefinitions.Wisdom
                )
                .SetTargetingData(Side.Enemy, RangeType.Distance, 4, TargetType.Individuals)
                .SetDurationData(DurationType.Minute, 1)
                .SetEffectForms(new EffectFormBuilder()
                    .HasSavingThrow(EffectSavingThrowType.Negates)
                    .CanSaveToCancel(TurnOccurenceType.EndOfTurn)
                    .SetConditionForm(ConditionDefinitions.ConditionFrightenedFear,
                        ConditionForm.ConditionOperation.Add)
                    .Build()
                )
                .Build())
            .AddToDB();
    }

    public static MonsterDefinition buildCustomOwl()
    {
        var baseMonster = MonsterDefinitions.Eagle_Matriarch;

        var talonAttack = MonsterAttackDefinitionBuilder
            .Create(MonsterAttackDefinitions.Attack_EagleMatriarch_Talons, "AttackWitchOwlTalons",
                DefinitionBuilder.CENamespaceGuid)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .SetToHitBonus(0)
            .SetEffectDescription(new EffectDescriptionBuilder()
                .SetEffectForms(new EffectFormBuilder()
                    .SetDamageForm(dieType: DieType.D1, diceNumber: 1, bonusDamage: 0,
                        damageType: DamageTypeSlashing)
                    .Build()
                )
                .Build()
            )
            .AddToDB();

        var monster = MonsterDefinitionBuilder
            .Create(baseMonster, "WitchOwl", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Monster, baseMonster.GuiPresentation.SpriteReference)
            .SetFeatures(
                FeatureDefinitionSenses.SenseNormalVision,
                FeatureDefinitionSenses.SenseDarkvision24,
                FeatureDefinitionMoveModes.MoveModeMove2,
                FeatureDefinitionMoveModes.MoveModeFly12,
                FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityKeenSight,
                FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityKeenHearing,
                FeatureDefinitionCombatAffinitys.CombatAffinityFlyby,
                FeatureDefinitionMovementAffinitys.MovementAffinityNoClimb,
                FeatureDefinitionMovementAffinitys.MovementAffinityNoSpecialMoves,
                FeatureDefinitionConditionAffinitys.ConditionAffinityProneImmunity
            )
            .SetAttackIterations(talonAttack)
            .SetSkillScores(
                (SkillDefinitions.Perception, 3),
                (SkillDefinitions.Stealth, 3)
            )
            .SetArmorClass(11)
            .SetAbilityScores(3, 13, 8, 2, 12, 7)
            .SetStandardHitPoints(1)
            .SetHitDice(DieType.D4, 1)
            .SetHitPointsBonus(-1)
            .SetSavingThrowScores()
            .SetDefaultBattleDecisionPackage(DecisionPackageDefinitions.DefaultSupportCasterWithBackupAttacksDecisions)
            .SetSizeDefinition(CharacterSizeDefinitions.Tiny)
            .SetAlignment(AlignmentDefinitions.Neutral.Name)
            .SetCharacterFamily(CharacterFamilyDefinitions.Fey.name)
            .SetChallengeRating(0)
            .SetLegendaryCreature(false)
            .SetDroppedLootDefinition(null)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction("Party")
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .SetInDungeonEditor(false)
            .SetCreatureTags("WitchFamiliar")
            .SetNoExperienceGain(false)
            .SetHasPhantomDistortion(true)
            .SetForceNoFlyAnimation(true)
            .SetGroupAttacks(true)
            .AddToDB();

        monster.MonsterPresentation.hasPrefabVariants = false;

        monster.MonsterPresentation.MonsterPresentationDefinitions.Empty();
        monster.MonsterPresentation.monsterPresentationDefinitions = MonsterDefinitions.Eagle_Matriarch
            .MonsterPresentation.MonsterPresentationDefinitions;
        monster.MonsterPresentation.useCustomMaterials = true;
        monster.MonsterPresentation.customMaterials = MonsterPresentationDefinitions
            .Eagle_Giant_Presentation.CustomMaterials;

        monster.MonsterPresentation.maleModelScale = 0.55f;
        monster.MonsterPresentation.femaleModelScale = 0.55f;
        monster.MonsterPresentation.malePrefabReference =
            MonsterDefinitions.Eagle_Matriarch.MonsterPresentation.malePrefabReference;
        monster.MonsterPresentation.femalePrefabReference =
            MonsterDefinitions.Eagle_Matriarch.MonsterPresentation.malePrefabReference;

        if (Help) { monster.Features.Add(Help); }

        return monster;
    }

    public static MonsterDefinition buildCustomPseudodragon()
    {
        var baseMonster = MonsterDefinitions.Young_GreenDragon;

        var biteAttack = MonsterAttackDefinitionBuilder
            .Create(MonsterAttackDefinitions.Attack_Wolf_Bite, "AttackWitchDragonBite",
                DefinitionBuilder.CENamespaceGuid)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .SetToHitBonus(0)
            .SetEffectDescription(new EffectDescriptionBuilder()
                .SetEffectForms(new EffectFormBuilder()
                    .SetDamageForm(dieType: DieType.D4, diceNumber: 1, bonusDamage: 2,
                        damageType: DamageTypePiercing)
                    .Build()
                )
                .Build()
            )
            .AddToDB();

        var stingAttack = MonsterAttackDefinitionBuilder
            .Create(MonsterAttackDefinitions.Attack_Badlands_Spider_Bite, "AttackWitchDragonSting",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.MonsterAttack)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .SetToHitBonus(0)
            .SetEffectDescription(new EffectDescriptionBuilder()
                .SetSavingThrowData(
                    true,
                    true,
                    AttributeDefinitions.Constitution,
                    false, EffectDifficultyClassComputation.FixedValue,
                    null,
                    11
                )
                .SetEffectForms(new EffectFormBuilder()
                        .SetDamageForm(dieType: DieType.D4, diceNumber: 1, bonusDamage: 2,
                            damageType: DamageTypePiercing)
                        .Build(),
                    new EffectFormBuilder()
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .SetConditionForm(
                            ConditionDefinitions.ConditionPoisoned,
                            ConditionForm.ConditionOperation.Add
                        )
                        .Build()
                )
                .Build()
            )
            .AddToDB();


        var monster = MonsterDefinitionBuilder
            .Create(baseMonster, "WitchPseudodragon", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation("WitchCustomPseudodragon", Category.Monster,
                baseMonster.GuiPresentation.SpriteReference)
            .SetFeatures(
                FeatureDefinitionMoveModes.MoveModeFly12,
                FeatureDefinitionMoveModes.MoveModeMove2,
                FeatureDefinitionSenses.SenseNormalVision,
                FeatureDefinitionSenses.SenseSuperiorDarkvision,
                FeatureDefinitionSenses.SenseBlindSight2,

                //FeatureDefinitionConditionAffinitys.ConditionAffinityProneImmunity
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinitySpellResistance,
                FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityKeenSight,
                FeatureDefinitionAbilityCheckAffinitys.AbilityCheckAffinityKeenHearing
            )
            .SetAttackIterations(stingAttack, biteAttack)
            .SetSkillScores(
                (SkillDefinitions.Perception, 3),
                (SkillDefinitions.Stealth, 4)
            )
            .SetArmorClass(13)
            .SetAbilityScores(6, 15, 13, 10, 12, 10)
            .SetStandardHitPoints(7)
            .SetHitDice(DieType.D4, 2)
            .SetHitPointsBonus(2)
            .SetSavingThrowScores()
            .SetDefaultBattleDecisionPackage(DecisionPackageDefinitions.DragonCombatDecisions)
            .SetSizeDefinition(CharacterSizeDefinitions.Tiny)
            .SetAlignment(AlignmentDefinitions.NeutralGood.Name)
            .SetCharacterFamily(CharacterFamilyDefinitions.Dragon.name)
            .SetChallengeRating(0)
            .SetLegendaryCreature(false)
            .SetDroppedLootDefinition(null)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction("Party")
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .SetInDungeonEditor(false)
            .SetModelScale(0.1f)
            .SetCreatureTags("WitchFamiliar")
            .SetNoExperienceGain(false)
            .SetHasPhantomDistortion(true)
            .SetForceNoFlyAnimation(true)
            .SetGroupAttacks(true)
            .AddToDB();


        monster.MonsterPresentation.hasPrefabVariants = false;
        monster.MonsterPresentation.MonsterPresentationDefinitions.Empty();
        monster.MonsterPresentation.useCustomMaterials = true;
        monster.MonsterPresentation.customMaterials = MonsterPresentationDefinitions
            .Young_Green_Dragon_Presentation.CustomMaterials;
        monster.MonsterPresentation.hasMonsterPortraitBackground = true;
        monster.MonsterPresentation.canGeneratePortrait = true;

        if (Help) { monster.Features.Add(Help); }

        return monster;
    }

    public static MonsterDefinition buildCustomSprite()
    {
        var baseMonster = MonsterDefinitions.Dryad;

        var swordAttack = MonsterAttackDefinitionBuilder
            .Create(MonsterAttackDefinitions.Attack_Veteran_Longsword, "AttackWitchSpriteSword",
                DefinitionBuilder.CENamespaceGuid)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .SetProximity(AttackProximity.Melee)
            .SetToHitBonus(0)
            .SetEffectDescription(new EffectDescriptionBuilder()
                .SetEffectForms(new EffectFormBuilder()
                    .SetDamageForm(dieType: DieType.D1, diceNumber: 1, bonusDamage: 0,
                        damageType: DamageTypeSlashing)
                    .Build()
                )
                .Build()
            )
            .AddToDB();

        var bowAttack = MonsterAttackDefinitionBuilder
            .Create(MonsterAttackDefinitions.Attack_Goblin_ShortBow, "AttackWitchSpriteBow",
                DefinitionBuilder.CENamespaceGuid)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .SetProximity(AttackProximity.Range)
            .SetToHitBonus(0)
            .SetEffectDescription(new EffectDescriptionBuilder()
                .SetSavingThrowData(
                    true,
                    true,
                    AttributeDefinitions.Constitution,
                    false, EffectDifficultyClassComputation.FixedValue,
                    null
                )
                .SetEffectForms(new EffectFormBuilder()
                        .SetDamageForm(dieType: DieType.D1, diceNumber: 1, bonusDamage: 0,
                            damageType: DamageTypePiercing)
                        .Build(),
                    new EffectFormBuilder()
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .SetConditionForm(
                            ConditionDefinitions.ConditionPoisoned,
                            ConditionForm.ConditionOperation.Add
                        )
                        .Build()
                )
                .Build()
            )
            .AddToDB();


        var monster = MonsterDefinitionBuilder
            .Create(baseMonster, "WitchSprite", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Monster, baseMonster.GuiPresentation.SpriteReference)
            .SetFeatures(
                FeatureDefinitionMoveModes.MoveModeFly8,
                FeatureDefinitionMoveModes.MoveModeMove2,
                FeatureDefinitionSenses.SenseNormalVision,
                WitchFamiliarInvisibilityPower
            )
            .SetAttackIterations(bowAttack, swordAttack)
            .SetSkillScores(
                (SkillDefinitions.Perception, 3),
                (SkillDefinitions.Stealth, 8)
            )
            .SetArmorClass(15)
            .SetArmor(ArmorTypeDefinitions.LeatherType.Name)
            .SetAbilityScores(3, 18, 10, 14, 12, 11)
            .SetStandardHitPoints(2)
            .SetHitDice(DieType.D4, 1)
            .SetHitPointsBonus(2)
            .SetSavingThrowScores()
            .SetDefaultBattleDecisionPackage(DecisionPackageDefinitions.DryadCombatDecisions)
            .SetSizeDefinition(CharacterSizeDefinitions.Tiny)
            .SetAlignment(AlignmentDefinitions.NeutralGood.Name)
            .SetCharacterFamily(CharacterFamilyDefinitions.Fey.name)
            .SetChallengeRating(0.25f)
            .SetLegendaryCreature(false)
            .SetDroppedLootDefinition(null)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction("Party")
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.None)
            .SetInDungeonEditor(false)
            .SetModelScale(0.4f)
            .SetCreatureTags("WitchFamiliar")
            .SetNoExperienceGain(false)
            .SetHasPhantomDistortion(true)
            .SetForceNoFlyAnimation(true)
            .SetGroupAttacks(false)
            .AddToDB();


        monster.MonsterPresentation.hasPrefabVariants = false;
        monster.MonsterPresentation.MonsterPresentationDefinitions.Empty();
        monster.MonsterPresentation.useCustomMaterials = true;
        // monster.MonsterPresentation.customMaterials = (MonsterPresentationDefinitions
        //     .Young_Green_Dragon_Presentation.CustomMaterials);
        monster.MonsterPresentation.hasMonsterPortraitBackground = true;
        monster.MonsterPresentation.canGeneratePortrait = true;

        if (Help) { monster.Features.Add(Help); }

        // monster.MonsterPresentation.SetOverrideCharacterShaderColors(true);
        // monster.MonsterPresentation.SetFirstCharacterShaderColor(MonsterDefinitions.FeyBear.MonsterPresentation.firstCharacterShaderColor);
        // monster.MonsterPresentation.SetSecondCharacterShaderColor(MonsterDefinitions.FeyBear.MonsterPresentation.secondCharacterShaderColor);
        //
        // // monster.CreatureTags.Clear();
        // monster.MonsterPresentation.MonsterPresentationDefinitions.Empty();
        // monster.MonsterPresentation.monsterPresentationDefinitions = (MonsterDefinitions.Goblin.MonsterPresentation.MonsterPresentationDefinitions);
        // monster.MonsterPresentation.useCustomMaterials = (true);
        // // //  monster.MonsterPresentation.customMaterials = (MonsterPresentationDefinitions.Silver_Dragon_Presentation.customMaterials);
        // //
        // monster.MonsterPresentation.maleModelScale = (0.4f);
        // monster.MonsterPresentation.femaleModelScale = (0.4f);
        // monster.MonsterPresentation.malePrefabReference = (MonsterDefinitions.Dryad.MonsterPresentation.malePrefabReference);
        // monster.MonsterPresentation.femalePrefabReference = (MonsterDefinitions.Dryad.MonsterPresentation.malePrefabReference);

        return monster;
    }

    public static MonsterDefinition buildCustomImp()
    {
        var baseMonster = MonsterDefinitions.Goblin;

        var stingAttack = MonsterAttackDefinitionBuilder
            .Create(MonsterAttackDefinitions.Attack_Badlands_Spider_Bite, "AttackWitchImpSting",
                DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.MonsterAttack)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .SetToHitBonus(0)
            .SetEffectDescription(new EffectDescriptionBuilder()
                .SetSavingThrowData(
                    true,
                    true,
                    AttributeDefinitions.Constitution,
                    false, EffectDifficultyClassComputation.FixedValue,
                    null,
                    11
                )
                .SetEffectForms(new EffectFormBuilder()
                        .SetDamageForm(dieType: DieType.D4, diceNumber: 1, bonusDamage: 3,
                            damageType: DamageTypePiercing)
                        .Build(),
                    new EffectFormBuilder()
                        .SetDamageForm(dieType: DieType.D6, diceNumber: 3, damageType: DamageTypePoison)
                        .HasSavingThrow(EffectSavingThrowType.HalfDamage)
                        .Build()
                )
                .Build()
            )
            .AddToDB();


        var monster = MonsterDefinitionBuilder
            .Create(baseMonster, "WitchImp", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Monster, baseMonster.GuiPresentation.SpriteReference)
            .SetFeatures(
                FeatureDefinitionMoveModes.MoveModeFly8,
                FeatureDefinitionMoveModes.MoveModeMove4,
                FeatureDefinitionSenses.SenseNormalVision,
                FeatureDefinitionSenses.SenseDarkvision24,
                //Todo: add devil's sight - magical darkness doesn't affect vision
                FeatureDefinitionDamageAffinitys.DamageAffinityFireImmunity,
                FeatureDefinitionDamageAffinitys.DamageAffinityPoisonImmunity,
                FeatureDefinitionDamageAffinitys.DamageAffinityColdResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityPiercingResistanceExceptSilver,
                FeatureDefinitionDamageAffinitys.DamageAffinitySlashingResistanceExceptSilver,
                FeatureDefinitionDamageAffinitys.DamageAffinityBludgeoningResistanceExceptSilver,
                FeatureDefinitionConditionAffinitys.ConditionAffinityPoisonImmunity,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinitySpellResistance,

                //TODO: can we implement shapechange for monsters at all?
                WitchFamiliarInvisibilityPower
            )
            .SetAttackIterations(stingAttack)
            .SetSkillScores(
                (SkillDefinitions.Deception, 4),
                (SkillDefinitions.Insight, 3),
                (SkillDefinitions.Persuasion, 4),
                (SkillDefinitions.Stealth, 5)
            )
            .SetArmorClass(13)
            .SetAbilityScores(6, 17, 13, 11, 12, 14)
            .SetStandardHitPoints(10)
            .SetHitDice(DieType.D4, 3)
            .SetHitPointsBonus(3)
            .SetSavingThrowScores()
            .SetDefaultBattleDecisionPackage(DecisionPackageDefinitions.DryadCombatDecisions)
            .SetSizeDefinition(CharacterSizeDefinitions.Tiny)
            .SetAlignment(AlignmentDefinitions.LawfulEvil.Name)
            .SetCharacterFamily(CharacterFamilyDefinitions.Fiend.name)
            .SetChallengeRating(1f)
            .SetLegendaryCreature(false)
            .SetDroppedLootDefinition(null)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction("Party")
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.Reference)
            .SetInDungeonEditor(false)
            .SetModelScale(0.4f)
            .SetCreatureTags("WitchFamiliar")
            .SetNoExperienceGain(false)
            .SetHasPhantomDistortion(true)
            .SetForceNoFlyAnimation(true)
            .SetGroupAttacks(false)
            .AddToDB();

        monster.MonsterPresentation.MonsterPresentationDefinitions.Empty();
        monster.MonsterPresentation.monsterPresentationDefinitions = MonsterDefinitions.Goblin
            .MonsterPresentation.MonsterPresentationDefinitions;
        monster.MonsterPresentation.useCustomMaterials = true;
        monster.MonsterPresentation.customMaterials = MonsterPresentationDefinitions
            .Orc_Female_Archer_RedScar.CustomMaterials;

        monster.MonsterPresentation.maleModelScale = 0.4f;
        monster.MonsterPresentation.femaleModelScale = 0.4f;
        monster.MonsterPresentation.malePrefabReference =
            MonsterDefinitions.Goblin.MonsterPresentation.malePrefabReference;
        monster.MonsterPresentation.femalePrefabReference =
            MonsterDefinitions.Goblin.MonsterPresentation.femalePrefabReference;

        monster.MonsterPresentation.hasPrefabVariants = false;

        if (Help) { monster.Features.Add(Help); }

        return monster;
    }

    public static MonsterDefinition buildCustomQuasit()
    {
        var baseMonster = MonsterDefinitions.Goblin;

        var clawAttack = MonsterAttackDefinitionBuilder
            .Create(MonsterAttackDefinitions.Attack_Zealot_Claw, "AttackWitchImpClaw",
                DefinitionBuilder.CENamespaceGuid)
            .SetActionType(ActionDefinitions.ActionType.Main)
            .SetToHitBonus(0)
            .SetEffectDescription(new EffectDescriptionBuilder()
                .SetSavingThrowData(
                    true,
                    true,
                    AttributeDefinitions.Constitution,
                    false, EffectDifficultyClassComputation.FixedValue,
                    null
                )
                .SetEffectForms(new EffectFormBuilder()
                        .SetDamageForm(dieType: DieType.D4, diceNumber: 1, bonusDamage: 3,
                            damageType: DamageTypePiercing)
                        .Build(),
                    new EffectFormBuilder()
                        .SetDamageForm(dieType: DieType.D4, diceNumber: 2, damageType: DamageTypePoison)
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .Build(),
                    new EffectFormBuilder()
                        .HasSavingThrow(EffectSavingThrowType.Negates)
                        .CanSaveToCancel(TurnOccurenceType.EndOfTurn)
                        .SetConditionForm(
                            ConditionDefinitions.ConditionPoisoned, //should be a minute duration
                            ConditionForm.ConditionOperation.Add
                        )
                        .Build()
                )
                .Build()
            )
            .AddToDB();

        var monster = MonsterDefinitionBuilder
            .Create(baseMonster, "WitchQuasit", DefinitionBuilder.CENamespaceGuid)
            .SetGuiPresentation(Category.Monster, baseMonster.GuiPresentation.SpriteReference)
            .SetFeatures(
                FeatureDefinitionMoveModes.MoveModeMove8,
                FeatureDefinitionSenses.SenseNormalVision,
                FeatureDefinitionSenses.SenseDarkvision24,
                FeatureDefinitionDamageAffinitys.DamageAffinityFireResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityLightningResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityColdResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityPiercingResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinitySlashingResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityBludgeoningResistance,
                FeatureDefinitionDamageAffinitys.DamageAffinityPoisonImmunity,
                FeatureDefinitionConditionAffinitys.ConditionAffinityPoisonImmunity,
                FeatureDefinitionSavingThrowAffinitys.SavingThrowAffinitySpellResistance,

                //TODO: can we implement shapechange for monsters at all?
                WitchFamiliarInvisibilityPower,
                WitchFamiliarScarePower
            )
            .SetAttackIterations(clawAttack)
            .SetSkillScores(
                (SkillDefinitions.Stealth, 5)
            )
            .SetArmorClass(13)
            .SetAbilityScores(5, 17, 10, 7, 10, 10)
            .SetStandardHitPoints(7)
            .SetHitDice(DieType.D4, 3)
            .SetHitPointsBonus(0)
            .SetSavingThrowScores()
            .SetDefaultBattleDecisionPackage(DecisionPackageDefinitions.GoblinCombatDecisions)
            .SetSizeDefinition(CharacterSizeDefinitions.Tiny)
            .SetAlignment(AlignmentDefinitions.ChaoticEvil.Name)
            .SetCharacterFamily(CharacterFamilyDefinitions.Fiend.name)
            .SetChallengeRating(1f)
            .SetLegendaryCreature(false)
            .SetDroppedLootDefinition(null)
            .SetFullyControlledWhenAllied(true)
            .SetDefaultFaction("Party")
            .SetBestiaryEntry(BestiaryDefinitions.BestiaryEntry.Reference)
            .SetInDungeonEditor(false)
            .SetModelScale(0.4f)
            .SetCreatureTags("WitchFamiliar")
            .SetNoExperienceGain(false)
            .SetHasPhantomDistortion(true)
            .SetForceNoFlyAnimation(true)
            .SetGroupAttacks(false)
            .AddToDB();

        monster.MonsterPresentation.hasPrefabVariants = false;

        monster.MonsterPresentation.MonsterPresentationDefinitions.Empty();
        monster.MonsterPresentation.monsterPresentationDefinitions = MonsterDefinitions.Goblin
            .MonsterPresentation.MonsterPresentationDefinitions;
        monster.MonsterPresentation.useCustomMaterials = true;
        monster.MonsterPresentation.customMaterials = MonsterPresentationDefinitions
            .Orc_Male_Chieftain_BladeFang.CustomMaterials;

        monster.MonsterPresentation.maleModelScale = 0.55f;
        monster.MonsterPresentation.femaleModelScale = 0.55f;
        monster.MonsterPresentation.malePrefabReference =
            MonsterDefinitions.Goblin.MonsterPresentation.malePrefabReference;
        monster.MonsterPresentation.femalePrefabReference =
            MonsterDefinitions.Goblin.MonsterPresentation.malePrefabReference;

        if (Help) { monster.Features.Add(Help); }

        return monster;
    }
}
