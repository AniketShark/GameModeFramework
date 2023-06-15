using System;
using System.Collections;
using System.Collections.Generic;
using GameModules.Networking;
using GameModules.Networking.Implementations.PhotonV2;
using GameModules.Teams;

namespace GameModules
{
	public abstract class Mode
	{
		#region CONSTRUCTOR
		private Mode() {}
		protected Mode(Hashtable modeParameters)
		{
		}
		#endregion

		#region PROPERTIES

		public ITeamAssigner TeamAssigner { get; protected set; }
		public IScoring<int> Scoring { get; protected set; }
		public ITimer<double> Timer { get; protected set; }
		public ISpawner<int> Spawner { get; protected set; }
		public IRoomState State { get; protected set; }
		protected ICallbackHandler CallbackHandler { get; set; }

		#endregion

		#region PUBLIC METHODS

		public abstract void Start();
		public abstract void JoinMatch(Hashtable arguments);
		public abstract void Reset();

		#endregion
		
	}
}
