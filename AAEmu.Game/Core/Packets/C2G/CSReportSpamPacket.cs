using System.Collections.Generic;
using AAEmu.Commons.Network;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game;
using AAEmu.Game.Models.Game.Items;
using AAEmu.Game.Models.Game.Mails;

namespace AAEmu.Game.Core.Packets.C2G;

public class CSReportSpamPacket : GamePacket
{
    public CSReportSpamPacket() : base(CSOffsets.CSReportSpamPacket, 1)
    {
    }

    public override void Read(PacketStream stream)
    {
        var mailId = stream.ReadInt64();

        Logger.Debug("ReportSpam, mailId: {0}", mailId);
        
        var itemSlots = new List<(SlotType slotType, byte slot)>();
        for (var i = 0; i < MailBody.MaxMailAttachments; i++)
        {
            itemSlots.Add((0, 0));
        }

        var mailResult = Connection.ActiveChar.Mails.ReturnMail(mailId);
        
        if (mailResult == MailResult.Success)
        {
            Connection.ActiveChar.SendErrorMessage(ErrorMessageType.MailSuccess);
        }
        else
        {
            Connection.SendPacket(new SCMailFailedPacket(mailResult, itemSlots.ToArray(), false));
        }
    }
}
