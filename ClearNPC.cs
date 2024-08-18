using Oxide.Core.Plugins;
using UnityEngine;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("ClearNPC", "RustFlash", "1.0.0")]
    [Description("Allows the removal and restoration of NPCs")]
    public class ClearNPC : RustPlugin
    {
        private Stack<RemovedNPC> removedNPCs = new Stack<RemovedNPC>();

        [ChatCommand("clear")]
        private void ClearNPCCommand(BasePlayer player, string command, string[] args)
        {
            if (args.Length == 0)
            {
                player.ChatMessage("Please provide the name of the NPC you want to remove.");
                return;
            }

            string npcName = string.Join(" ", args).ToLower();

            var target = RaycastNPC(player);
            if (target == null)
            {
                player.ChatMessage("You are not looking at an NPC.");
                return;
            }

            if (target.ShortPrefabName.ToLower().Contains(npcName))
            {
                SaveRemovedNPC(target);
                target.Kill();
                player.ChatMessage($"NPC '{target.ShortPrefabName}' has been removed.");
            }
            else
            {
                player.ChatMessage($"The NPC you are looking at is named '{target.ShortPrefabName}' and does not match '{npcName}'.");
            }
        }

        [ChatCommand("clearundo")]
        private void ClearUndoCommand(BasePlayer player, string command, string[] args)
        {
            if (removedNPCs.Count == 0)
            {
                player.ChatMessage("No NPCs to restore.");
                return;
            }

            var removedNPC = removedNPCs.Pop();
            RestoreNPC(removedNPC);
            player.ChatMessage($"NPC '{removedNPC.Name}' has been restored.");
        }

        private BaseCombatEntity RaycastNPC(BasePlayer player)
        {
            var eyes = player.eyes;
            Ray ray = new Ray(eyes.position, eyes.HeadForward());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 10f))
            {
                var npc = hit.collider.GetComponentInParent<BaseCombatEntity>();
                if (npc != null)
                {
                    return npc;
                }
            }

            return null;
        }

        private void SaveRemovedNPC(BaseCombatEntity npc)
        {
            var npcData = new RemovedNPC
            {
                Name = npc.ShortPrefabName,
                Position = npc.transform.position,
                Rotation = npc.transform.rotation
            };
            removedNPCs.Push(npcData);
        }

        private void RestoreNPC(RemovedNPC npcData)
        {
            BaseCombatEntity restoredNpc = GameManager.server.CreateEntity(npcData.Name, npcData.Position, npcData.Rotation) as BaseCombatEntity;
            if (restoredNpc != null)
            {
                restoredNpc.Spawn();
            }
        }

        private class RemovedNPC
        {
            public string Name { get; set; }
            public Vector3 Position { get; set; }
            public Quaternion Rotation { get; set; }
        }
    }
}
