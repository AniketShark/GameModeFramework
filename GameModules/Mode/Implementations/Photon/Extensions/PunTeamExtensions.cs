using System;
using System.Collections;
using System.Collections.Generic;
using GameModules.Teams;
using UnityEngine;
using PunPlayer = global::Photon.Realtime.Player;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public static class PunTeamExtensions
	{
		public static int GetTeam(this PunPlayer player)
		{
			object teamId;
			if (player.CustomProperties.TryGetValue(TeamProps.TeamCode, out teamId))
			{
				return Convert.ToInt32(teamId);
			}

			return Convert.ToInt32(-1);
		}

		public static int GetTeamSlot(this PunPlayer player)
		{
			object slot;
			if (player.CustomProperties.TryGetValue(TeamProps.TeamSlot, out slot))
			{
				return Convert.ToInt32(slot);
			}

			return Convert.ToInt32(-1);
		}

		public static string GetTeamName(this PunPlayer player)
		{
			object teamName;
			if (player.CustomProperties.TryGetValue(TeamProps.TeamName, out teamName))
			{
				return teamName.ToString();
			}

			return string.Empty;
		}
	
	}
}
