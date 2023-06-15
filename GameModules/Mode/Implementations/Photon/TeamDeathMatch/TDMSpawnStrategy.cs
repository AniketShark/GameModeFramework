using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace GameModules.Teams
{
	public class TDMSpawnStrategy : ISpawnStrategy<int>
	{
		private Dictionary<int,List<int>> _teamToSpawnPoints;
		private List<int> _locked;
		private System.Random _random;

		public TDMSpawnStrategy()
		{
			_random = new System.Random();
			// Initialized locked list with -1 because when we try to get current spawn point for player
			// from custom properties. we return -1 for no spawn point.
			_locked = new List<int>{-1};
			_teamToSpawnPoints = new Dictionary<int, List<int>>();
		}

		#region PUBLIC METHODS

		public bool GetSpawnpoint(out int spawnPoint,Hashtable args)
		{
			spawnPoint     = -1;
			int spawnFound = -1;
			bool random = false;

			if (args.Count <= 0)
			{
				//UnityEngine.Debug.LogErrorFormat("[TDMSpawnStrategy] teamName= paramater[0] must be passed to get spawnPoint");
				return false;
			}
			
			int teamCode = (int)args[PlayerProps.PlayerTeam];
			if (args.ContainsKey(SpawnerProps.SpawnIndex))
				spawnFound = (int)args[SpawnerProps.SpawnIndex];
			if (args.ContainsKey(SpawnerProps.SelectRandom))
				random = (bool)args[SpawnerProps.SelectRandom];

			if (!_teamToSpawnPoints.ContainsKey(teamCode))
			{
				//UnityEngine.Debug.LogErrorFormat("[TDMSpawnStrategy] Wrong teamCode= {0}",teamCode);
				return false;
			}

			if(random)
			{
				//GM_NORMAL : Check if we can make this snippet more performant
				List<int> spawnsAvailable = _teamToSpawnPoints[teamCode].FindAll(item => !_locked.Contains(item));
				if (spawnsAvailable.Count > 0)
				{
					spawnPoint = spawnsAvailable[0];
				}
				//UnityEngine.Debug.LogErrorFormat("[TDMSpawnStrategy] Spawnpoint teamName={0} spawnPoint={1} spawnPoints Open : {2}",teamCode,spawnPoint,spawnsAvailable.Count);
				return true;
			}

			if (!_locked.Contains(spawnFound))
			{
				//UnityEngine.Debug.LogError(string.Format("[TDMSpawnStrategy] Spawnpoint teamName= {0} spawnIndex= {1} is locked", teamCode, spawnFound));
				return false;
			}
			else
			{
				if (_teamToSpawnPoints[teamCode].Contains(spawnFound))
				{
					spawnPoint = spawnFound;
					return true;
				}
				else
				{
					//UnityEngine.Debug.LogErrorFormat("[TDMSpawnStrategy] Spawnpoint is not alloted to teamName= {0}", teamCode);
				}
			}
			return false;
		}

		public bool LockSpawnpoint(int spawnIndex)
		{
			_locked.Add(spawnIndex);
			//UnityEngine.Debug.LogErrorFormat("[LockSpawnpoint] Locked Count {0} SpawnIndex : {1}",_locked.Count,spawnIndex);
			return true;
		}

		public bool ReleaseSpawnpoint(int spawnIndex)
		{
			_locked.Remove(spawnIndex);
			//UnityEngine.Debug.LogErrorFormat("[ReleaseSpawnpoint] Locked Count {0} SpawnIndex : {1}",_locked.Count,spawnIndex);
			return true;
		}

		public void Reset()
		{
			_locked.Clear();
			_teamToSpawnPoints.Clear();
		}

		public void SetSpawnMap(Hashtable args)
		{
			var spawnMap = args[SpawnerProps.SpawnMap] as Dictionary<int, List<int>>;
			_teamToSpawnPoints = spawnMap;
		}

		#endregion
	}
}
