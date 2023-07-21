using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class DMSpawnStrategy : ISpawnStrategy<int>
	{
		private List<int> _spawnMap;
		public  List<int> _locked;

		public DMSpawnStrategy()
		{
			//_spawnMap = spawnMap;
			_locked = new List<int>();
		}

		public bool GetSpawnpoint(out int spawnPoint, Hashtable args)
		{
			spawnPoint = -1;
			if (_spawnMap.Count > 0)
			{
				spawnPoint = _spawnMap.Count > 1 ? _spawnMap[Random.Range(0,_spawnMap.Count)] : _spawnMap[0];
				//UnityEngine.Debug.Log(string.Format("[DMSpawnStrategy] Spawnpoint ={0} SpawnPoints Open : {1}", spawnPoint, _spawnMap.Count));
				return true;
			}
			return false;
		}

		public bool LockSpawnpoint(int spawnPoint)
		{
			_locked.Add(spawnPoint);
			_spawnMap.Remove(spawnPoint);
			//UnityEngine.Debug.Log(string.Format("[DMSpawnStrategy] LockSpawnpoint Locked {0} SpawnIndex : {1}",_locked.Count,spawnPoint));
			return true;

		}

		public bool ReleaseSpawnpoint(int spawnPoint)
		{
			_locked.Remove(spawnPoint);
			_spawnMap.Add(spawnPoint);
			//UnityEngine.Debug.LogError(string.Format("[DMSpawnStrategy] ReleaseSpawnpoint Count {0} SpawnIndex : {1}",_locked.Count,spawnPoint));
			return true;

		}


		public void SetSpawnMap(Hashtable args)
		{
			var spawnMap = args[SpawnerProps.SpawnMap] as List<int>;
			_spawnMap = spawnMap;
		}

		public void Reset()
		{
			_locked.Clear();
			_spawnMap.Clear();
		}
	}
	
}
