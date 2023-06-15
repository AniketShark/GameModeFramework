using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class DMRoomState : TDMRoomState
	{
		public DMRoomState(ITimer<double> timer, IScoring<int> scoring) : base(timer, scoring)
		{
		}
	}
}
