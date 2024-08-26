using System;
using System.Collections.Generic;
using AAEmu.Commons.Network;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Models.StaticValues;

namespace AAEmu.Game.Models.Game.Faction;

public class SystemFaction : PacketMarshaler
{
    public FactionsEnum Id { get; set; }
    public string Name { get; set; }
    public uint OwnerId { get; set; }
    public string OwnerName { get; set; }
    public FactionsEnum MotherId { get; set; }
    public sbyte UnitOwnerType { get; set; }
    public byte PoliticalSystem { get; set; }
    public bool DiplomacyTarget { get; set; }
    public bool AggroLink { get; set; }
    public bool GuardHelp { get; set; }
    public byte AllowChangeName { get; set; }
    public DateTime Created { get; set; }

    public Dictionary<FactionsEnum, FactionRelation> Relations { get; set; } = new();

    /// <summary>
    /// Get Root faction
    /// character -> race faction -> union faction
    /// </summary>
    /// <returns></returns>
    public SystemFaction GetRootMother()
    {
        if (MotherId == FactionsEnum.Invalid)
        {
            return this;
        }

        var motherFaction = FactionManager.Instance.GetFaction(MotherId);
        return motherFaction?.GetRootMother() ?? this;
    }


    public RelationState GetRelationState(SystemFaction otherFaction)
    {
        if (otherFaction == null) return RelationState.Neutral;

        var faction = GetRootMother();
        var faction2 = otherFaction.GetRootMother();

        // Handle Root Factions
        switch (faction.Id)
        {
            case FactionsEnum.Neutral:
                return RelationState.Neutral;
            case FactionsEnum.Friendly:
                return RelationState.Friendly;
            case FactionsEnum.Hostile:
                return RelationState.Hostile;
        }

        // Handle Target Root Factions
        switch (faction2.Id)
        {
            case FactionsEnum.Neutral:
                return RelationState.Neutral;
            case FactionsEnum.Friendly:
                return RelationState.Friendly;
            case FactionsEnum.Hostile:
                return RelationState.Hostile;
        }

        if (faction.Id == faction2.Id)
            return RelationState.Friendly;
        
        // 判断是否正在Duel
        // 如果正在PK，判断双方是否红蓝，且处于同一个Duel
        // 如果不是同一个Duel则判断种族

        return faction.Relations.TryGetValue(faction2.Id, out var relation) ? relation.State : RelationState.Neutral;
    }

    public override PacketStream Write(PacketStream stream)
    {
        stream.Write((uint)Id);
        stream.Write(AggroLink);
        stream.Write((uint)MotherId);
        stream.Write(Name);
        stream.Write(OwnerId);
        stream.Write(OwnerName);
        stream.Write(UnitOwnerType);
        stream.Write(PoliticalSystem);
        stream.Write(Created);
        stream.Write(DiplomacyTarget);
        stream.Write(AllowChangeName);
        return stream;
    }
}
