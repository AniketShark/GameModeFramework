using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using PunPlayer = global::Photon.Realtime.Player;
using ExitHashtable = global::ExitGames.Client.Photon.Hashtable;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public static class PunPlayerExtensions
	{
		public static int GetLastRoomId(this PunPlayer player)
		{
			object roomId;
			if (player.CustomProperties.TryGetValue(PlayerProps.LastRoomId, out roomId))
			{
				return (int)roomId;
			}
			return -1;
		}

		public static int GetPlayerViewId(this PunPlayer player)
		{
			object viewId;
			if (player.CustomProperties.TryGetValue(PlayerProps.PlayerViewId, out viewId))
			{
				return (int)viewId;
			}
			return -1;
		}

		public static int GetPlayerId(this PunPlayer player)
		{
			object viewId;
			if (player.CustomProperties.TryGetValue(PlayerProps.PlayerId, out viewId))
			{
				return (int)viewId;
			}
			return -1;
		}

		public static string GetPlayerName(this PunPlayer player)
		{
			object name;
			if (player.CustomProperties.TryGetValue(PlayerProps.PlayerName, out name))
			{
				return name.ToString();
			}
			return string.Empty;
		}

		public static int GetSpawnpoint(this PunPlayer player)
		{
			object spn;
			if (player.CustomProperties.TryGetValue(PlayerProps.PlayerSpawn, out spn))
			{
				return (int)spn;
			}
			return -1;
		}

		public static bool SetSpawnpoint(this PunPlayer player,int spawnPoint)
		{
			return player.SetCustomProperties(new ExitHashtable(){{ PlayerProps.PlayerSpawn, spawnPoint } });
		}

		public static int GetTeamScoreContribution(this PunPlayer player)
		{
			object tsc;
			if (player.CustomProperties.TryGetValue(ScoringProps.TeamScoreContribution, out tsc))
			{
				return (int)tsc;
			}
			return 0;
		}

		public static int GetTotalScoreContribution(this PunPlayer player)
		{
			object tsc;
			if (player.CustomProperties.TryGetValue(ScoringProps.TotalScoreContribution, out tsc))
			{
				return (int)tsc;
			}
			return 0;
		}

		public static int GetPersonalScoreContribution(this PunPlayer player)
		{
			object psc;
			if (player.CustomProperties.TryGetValue(ScoringProps.PersonalScoreContribution, out psc))
			{
				return (int)psc;
			}
			return 0;
		}

		public static bool SetSelectedCharacter(this PunPlayer player, string characterName) {
			return player.SetCustomProperties(new ExitHashtable() {{ PlayerProps.SelecterCharacter, characterName}});
		}

		public static string GetSelectedCharacter(this PunPlayer player)
		{
			object characterName;
			if (player.CustomProperties.TryGetValue(PlayerProps.SelecterCharacter, out characterName))
			{
				return characterName.ToString();
			}
			return string.Empty;
		}

		public static bool SetOutFitInfo(this PunPlayer player, string outfitData)
		{
			return player.SetCustomProperties(new ExitHashtable(){{ PlayerProps.OutFit, outfitData } });
		}

		public static string GetOutFitInfo(this PunPlayer player)
		{
			object outfitData;
			if (player.CustomProperties.TryGetValue(PlayerProps.OutFit, out outfitData))
			{
				return outfitData.ToString();
			}
			return string.Empty;
		}

		public static bool SetOutFitHash(this PunPlayer player, int outfitHash)
		{
			return player.SetCustomProperties(new ExitHashtable(){{ PlayerProps.OutFitHash, outfitHash } });
		}

		public static int GetOutfitHash(this PunPlayer player)
		{
			object outfitData;
			if (player.CustomProperties.TryGetValue(PlayerProps.OutFitHash, out outfitData))
			{
				return Convert.ToInt32(outfitData);
			}
			return -1;
		}

		public static int GetLastEquippedItem(this PunPlayer player,int slot)
		{
			object equippedItems;
			if (player.CustomProperties.TryGetValue(PlayerProps.EquippedItems, out equippedItems))
			{
				int[] allItems = (int[]) equippedItems;
				if (allItems.Length > slot)
					return allItems[slot]; 
			}
			return -1;
		}

		public static int[] GetLastEquippedItems(this PunPlayer player)
		{
			object equippedItems;
			if (player.CustomProperties.TryGetValue(PlayerProps.EquippedItems, out equippedItems))
			{
				int[] allItems = (int[]) equippedItems;
				return allItems; 
			}
			return null;
		}

		public static void SetLastEquippedItems(this PunPlayer player,int[] equippedItems)
		{
			player.SetCustomProperties(new ExitHashtable() {{PlayerProps.EquippedItems, equippedItems}});
		}

		public static void WipeCleanCustomProperties(this PunPlayer player)
		{
			player.CustomProperties = new ExitHashtable();
		}
	}
}
