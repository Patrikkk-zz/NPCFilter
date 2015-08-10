using System;
using System.Collections.Generic;
using System.IO;
using TerrariaApi.Server;
using TShockAPI;
using Terraria;
using Newtonsoft.Json;
using System.Linq;

namespace NPCFilter
{
	[ApiVersion(1, 21)]
	public class NPCFilter : TerrariaPlugin
	{
		public override Version Version
		{
			get { return new Version(1,0); }
		}

		public override string Name
		{
			get { return "NPC Filter"; }
		}

		public override string Author
		{
			get { return "Patrikk"; }
		}

		public override string Description
		{
			get { return "Filters NPC spawning."; }
		}

		public NPCFilter(Main game)
			: base(game)
		{
		}

		public static FilterStorage filterStorage = new FilterStorage();
		public string path = Path.Combine(TShock.SavePath, "NPCFilter.json");

		public override void Initialize()
		{
			TShockAPI.Commands.ChatCommands.Add(new Command("npcfilter.use", FilterNPC, "npcfilter"));
			ServerApi.Hooks.NpcSpawn.Register(this, OnSpawn);
			ServerApi.Hooks.NpcTransform.Register(this, OnTransform);

			if (File.Exists(path))
			{
				 filterStorage = FilterStorage.Read(path);
			}
			filterStorage.Write(path);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.NpcSpawn.Deregister(this, OnSpawn);
			}
			base.Dispose(disposing);
		}

		private void FilterNPC(CommandArgs args)
		{
			if (args.Parameters.Count == 1 && args.Parameters[0] == "list")
			{
				args.Player.SendInfoMessage("Banned NPC IDs: " + string.Join(", ", filterStorage.FilteredNPCs) + ".");
				return;
			}
			else if (args.Parameters.Count == 1 && args.Parameters[0] == "reload")
			{
				FilterStorage.Read(path);
				args.Player.SendSuccessMessage("Attempted to reload NPCFliter list!");
				return;
			}
			else if (args.Parameters.Count == 2)
			{
				NPC npc;
				List<NPC> matchedNPCs = TShock.Utils.GetNPCByIdOrName(args.Parameters[1]);
				if (matchedNPCs.Count == 0)
				{
					args.Player.SendErrorMessage("Invalid NPC: '{0}'!", args.Parameters[1]);
					return;
				}
				else if (matchedNPCs.Count > 1)
				{
					TShock.Utils.SendMultipleMatchError(args.Player, matchedNPCs.Select(i => i.name));
					return;
				}
				else
				{
					npc = matchedNPCs[0];
				}

				switch (args.Parameters[0])
				{
					case "add":
						{
							if (filterStorage.FilteredNPCs.Contains(npc.netID))
							{
								args.Player.SendErrorMessage("NPC ID {0} is already on the filter list!", npc.netID);
								return;
							}
							filterStorage.FilteredNPCs.Add(npc.netID);
							File.WriteAllText(path, JsonConvert.SerializeObject(filterStorage, Formatting.Indented));
							args.Player.SendSuccessMessage("Successfully added NPC ID to filter list: {0}!", npc.netID);
							break;
						}
					case "delete":
					case "del":
					case "remove":
						{
							if (!filterStorage.FilteredNPCs.Contains(npc.netID))
							{
								args.Player.SendErrorMessage("NPC ID {0} is not on the filter list!", npc.netID);
								return;
							}
							filterStorage.FilteredNPCs.Remove(npc.netID);
							File.WriteAllText(path, JsonConvert.SerializeObject(filterStorage, Formatting.Indented));
							args.Player.SendSuccessMessage("Successfully removed NPC ID from filter list: {0}!", npc.netID);
							break;
						}
					default:
						{
							args.Player.SendErrorMessage("Syntax: /npcfilter <add/remove> [name or ID]");
							break;
						}
				}
			}
			else
			{
				args.Player.SendInfoMessage("Available NPCFilter commands:");
				args.Player.SendInfoMessage("/npcfilter <add/remove> [name or ID]");
				args.Player.SendInfoMessage("/npcfilter list");
				args.Player.SendInfoMessage("/npcfilter reload");
				return;
			}
			
		}

		private void OnTransform(NpcTransformationEventArgs args)
		{
			if (args.Handled)
				return;
			if (filterStorage.FilteredNPCs.Contains(Main.npc[args.NpcId].netID))
			{
				Main.npc[args.NpcId].active = false;
			}
		}

		private void OnSpawn( NpcSpawnEventArgs args)
		{
			if (args.Handled)
				return;
			if (filterStorage.FilteredNPCs.Contains(Main.npc[args.NpcId].netID))
			{
				args.Handled = true;
				Main.npc[args.NpcId].active = false;
				args.NpcId = 200;
			}
		}
	}
}
