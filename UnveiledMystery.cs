using System.IO;
using Terraria.ModLoader;
using UnveiledMystery.Enemies.Boss;
using Terraria;
using Terraria.ID;
using System.Linq;

namespace UnveiledMystery
{
	public class UnveiledMystery : Mod
	{
        internal enum MessageType : byte
        {
            SpawnNPCByPlayer,
            ReplaceStalactite,
            SpawnNPCByNPC
        }
        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            if(Main.netMode == NetmodeID.Server)
            {
                MessageType msgType = (MessageType)reader.ReadByte();
                switch (msgType)
                {
                    case MessageType.SpawnNPCByPlayer:
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();
                        int type = reader.ReadInt32();
                        int index = NPC.NewNPC(Main.player[whoAmI].GetSource_FromAI(), x, y, type);
                        NetMessage.SendData(MessageID.SyncNPC, number: index);
                        Logger.Info("NPC Spawned by player");

                        break;
                    case MessageType.SpawnNPCByNPC:
                        byte entity = reader.ReadByte();
                        x = reader.ReadInt32();
                        y = reader.ReadInt32();
                        type = reader.ReadInt32();
                        index = NPC.NewNPC(Main.npc[entity].GetSource_FromAI(), x, y, type);
                        NetMessage.SendData(MessageID.SyncNPC, number: index);
                        Logger.Info("NPC Spawned by NPC");

                        break;
                    case MessageType.ReplaceStalactite:
                        entity = reader.ReadByte();
                        x = reader.ReadInt32();
                        y = reader.ReadInt32();
                        type = reader.ReadInt32();
                        index = NPC.NewNPC(Main.npc[entity].GetSource_FromAI(), x, y, type);
                        NPC Hand = Main.npc.FirstOrDefault(i => i.active && i.type == ModContent.NPCType<LivingTrapBossHand>());
                        LivingTrapBossHand HandScript = (LivingTrapBossHand)Hand.ModNPC;
                        Logger.Info(Hand);
                        Logger.Info(entity);
                        HandScript.Stalactites.Add(Main.npc[index]);
                        HandScript.StalactitesScript.Add((StalactiteNPC)Main.npc[index].ModNPC);
                        Logger.Info("stalactite spawned");
                        NetMessage.SendData(MessageID.SyncNPC, number: index);
                        break;
                    default:
                        Logger.WarnFormat("MyMod: Unknown Message type: {0}", msgType);
                        break;
                }
            }

        }
    }
}