﻿using System;

using AAEmu.Game.Models.Game.Skills.Effects;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.Skills.Buffs.Triggers;

public class DeathBuffTrigger : BuffTrigger
{
    public override void Execute(object sender, EventArgs eventArgs)
    {
        var args = eventArgs as OnDeathArgs;

        Logger.Trace("Buff[{0}] {1} executed. Applying {2}[{3}]!", _buff.Template.BuffId, this.GetType().Name, Template.Effect.GetType().Name, Template.Effect.Id);

        if (!(_owner is Unit owner))
        {
            Logger.Warn("AttackTrigger owner is not a Unit");
            return;
        }

        var target = owner;
        // if (Template.EffectOnSource)
        //     target = args.Attacker;

        Template.Effect.Apply(owner, new SkillCasterUnit(_owner.ObjId), target, new SkillCastUnitTarget(target.ObjId), new CastBuff(_buff),
            new EffectSource(), // TODO : EffectSource Type trigger 
            null, DateTime.UtcNow);
    }

    public DeathBuffTrigger(Buff owner, BuffTriggerTemplate template) : base(owner, template)
    {

    }
}
