﻿using System;
using System.Linq;
using static SolastaCommunityExpansion.CustomDefinitions.IPerformAttackAfterMagicEffectUse;

namespace SolastaCommunityExpansion.CustomDefinitions;

public interface IPerformAttackAfterMagicEffectUse
{
    delegate bool CanAttackHandler(GameLocationCharacter caster, GameLocationCharacter target);

    delegate bool CanUseHandler(CursorLocationSelectTarget targeting, GameLocationCharacter caster,
        GameLocationCharacter target, out string failure);

    delegate CharacterActionParams GetAttackAfterUseHandler(CharacterActionMagicEffect actionMagicEffect);

    CanUseHandler CanBeUsedToAttack { get; set; }
    GetAttackAfterUseHandler PerformAttackAfterUse { get; set; }
    CanAttackHandler CanAttack { get; set; }
}

public class PerformAttackAfterMagicEffectUse : IPerformAttackAfterMagicEffectUse
{
    public static readonly IPerformAttackAfterMagicEffectUse MeleeAttack = new PerformAttackAfterMagicEffectUse();
    public RuleDefinitions.RollOutcome minOutcomeToAttack = RuleDefinitions.RollOutcome.Success;
    public RuleDefinitions.RollOutcome minSaveOutcomeToAttack = RuleDefinitions.RollOutcome.Failure;

    public PerformAttackAfterMagicEffectUse()
    {
        CanAttack = CanMeleeAttack;
        CanBeUsedToAttack = DefaultCanUseHandler;
        PerformAttackAfterUse = DefaultAttackHandler;
    }

    public CanUseHandler CanBeUsedToAttack { get; set; }

    public GetAttackAfterUseHandler PerformAttackAfterUse { get; set; }
    public CanAttackHandler CanAttack { get; set; }

    private bool CanMeleeAttack(GameLocationCharacter caster, GameLocationCharacter target)
    {
        var attackMode = caster.FindActionAttackMode(ActionDefinitions.Id.AttackMain);
        if (attackMode == null)
        {
            return false;
        }

        var battleService = ServiceRepository.GetService<IGameLocationBattleService>();
        if (battleService == null)
        {
            return false;
        }

        var attackModifier = new ActionModifier();
        var evalParams = new BattleDefinitions.AttackEvaluationParams();

        evalParams.FillForPhysicalReachAttack(caster, caster.LocationPosition, attackMode, target,
            target.LocationPosition, attackModifier);

        return battleService.CanAttack(evalParams);
    }

    private CharacterActionParams DefaultAttackHandler(CharacterActionMagicEffect effect)
    {
        if (effect == null) { return null; }

        var actionParams = effect.ActionParams;
        if (actionParams == null) { return null; }

        //Spell got countered or it failed
        if (effect.Countered || effect.ExecutionFailed)
        {
            return null;
        }

        //Attack outcome is worse that required
        if (effect.Outcome > minOutcomeToAttack) { return null; }

        //Target rolled saving throw and got better result
        if (effect.RolledSaveThrow && effect.SaveOutcome < minSaveOutcomeToAttack) { return null; }

        var caster = actionParams.ActingCharacter;
        var targets = actionParams.TargetCharacters;

        if (caster == null || targets.Empty()) { return null; }

        var attackMode = caster.FindActionAttackMode(ActionDefinitions.Id.AttackMain);
        if (attackMode == null) { return null; }

        var attackModifier = new ActionModifier();

        //TODO: option to limit attack to select target, while effect can have multiple
        var target = targets
            .FirstOrDefault(t => CanMeleeAttack(caster, t));

        if (target != null)
        {
            var attackActionParams =
                new CharacterActionParams(caster, ActionDefinitions.Id.AttackFree) { AttackMode = attackMode };

            attackActionParams.TargetCharacters.Add(target);
            attackActionParams.ActionModifiers.Add(attackModifier);

            return attackActionParams;
        }

        return null;
    }

    private bool DefaultCanUseHandler(CursorLocationSelectTarget targeting, GameLocationCharacter caster,
        GameLocationCharacter target, out string failure)
    {
        failure = String.Empty;
        //TODO: implement setting to tell how many targets must meet weapon attack requirements
        var maxTargets = targeting.maxTargets;
        var remainingTargets = targeting.remainingTargets;
        var selectedTargets = maxTargets - remainingTargets;
        if (selectedTargets > 0)
        {
            return true;
        }

        //TODO: add option for ranged attacks
        var canAttack = CanMeleeAttack(caster, target);
        if (!canAttack)
        {
            failure = "Failure/&FailureFlagTargetMeleeWeaponError";
        }

        return canAttack;
    }
}
