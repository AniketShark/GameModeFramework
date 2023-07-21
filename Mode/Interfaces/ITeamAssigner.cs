using System;
using System.Collections.Generic;

namespace GameModules.Teams
{
	public interface ITeamAssigner : IDisposable
	{
		event Action<IDictionary<string, object>> onPlayerJoinedTeam;
		event Action<IDictionary<string, object>> onPlayerLeftTeam;
		event Action<IList<object>> onPlayerListRefresh;

		List<Team> GetAllTeams();
		void JoinTeam(Team team,int slot, Action<bool> callback);
		void LeaveTeam(Team team, Action<bool> callback);
		void SwitchTeam(Team team,int slot, Action<bool> callback); 
		void GetAvailableTeam(Action<Team,int> callback);
		bool GetTeamByName(string name,out Team team);
		bool GetTeamByCode(int code,out Team team);
	}
}
