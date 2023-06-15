using System;
using System.Collections.Generic;

namespace GameModules.Teams
{
	public interface ITeamJoinStrategy
	{
		void Init(List<Team> teamList);
		bool IsTeamJoinValid(string userId,Team team,int slotIndex);
		void GetAvailableTeam(string playerId, Action<Team, int> callback);
		bool ReleaseSlot(string playerId,Team team, int slot);
	}
}
