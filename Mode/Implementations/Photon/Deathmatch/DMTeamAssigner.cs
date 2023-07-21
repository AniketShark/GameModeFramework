using System;
using System.Collections;
using System.Collections.Generic;
using GameModules.Teams;
using UnityEngine;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class DMTeamAssigner : TDMTeamAssigner
	{
		public DMTeamAssigner(ICallbackHandler callbackHandler, int maxTeams, int maxPlayersPerTeam, ITeamJoinStrategy strategy) : base(callbackHandler, maxTeams, maxPlayersPerTeam, strategy)
		{
		}
	}
}
