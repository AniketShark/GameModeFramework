using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GameModules.Teams;
using GameModules.Utils;
using Photon.Pun;
using Photon.Realtime;
using ExitHashtable = ExitGames.Client.Photon.Hashtable;
using Hashtable = System.Collections.Hashtable;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class DMKillsToWin : TDMScoreToWin
	{
		public DMKillsToWin(ICallbackHandler callbackHandler) : base(callbackHandler)
		{
		}
	}
}
