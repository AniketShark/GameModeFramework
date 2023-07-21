using System;
using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = System.Collections.Hashtable;
using Object = UnityEngine.Object;
using ExitHashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class DMSpawner : ISpawner<int>
	{
		private ISpawnStrategy<int> _spawnStrategy;
		private PhotonCallbackHandler _callbackHandler;

		#region CONSTRUCTOR
		public DMSpawner(ICallbackHandler callbackHandler, ISpawnStrategy<int> strategy)
		{
			_spawnStrategy = strategy;
			_callbackHandler = (PhotonCallbackHandler)callbackHandler;
			_callbackHandler.onPlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
		}
		#endregion

		#region PUBLIC METHODS
		public void LoadSpawnMap(Hashtable args)
		{
			_spawnStrategy.SetSpawnMap(args);
		}

		public void Reset()
		{
			_spawnStrategy.Reset();
		}

		public void SetSpawnpoint(Hashtable args, Action<int> callback)
		{
			int spawnPoint;
			if (_spawnStrategy.GetSpawnpoint(out spawnPoint, args))
			{
				PhotonNetwork.LocalPlayer.SetSpawnpoint(spawnPoint);
			}

			callback?.Invoke(spawnPoint);
		}

		public void ReleaseSpawnpoint(int spawnPoint, Action<bool> callback)
		{
			bool release = _spawnStrategy.ReleaseSpawnpoint(spawnPoint);
			callback?.Invoke(release);
		}

		public void Spawn(Hashtable args, Action<Object> callback)
		{
			if(args.ContainsKey(SpawnerProps.PrefabName))
			{
				string prefab = args[SpawnerProps.PrefabName] as string;
				Vector3 position = Vector3.zero;
				Quaternion rotation = Quaternion.identity;

				if (args.ContainsKey(SpawnerProps.SpawnPosition))
					position = (Vector3)args[SpawnerProps.SpawnPosition];
				if (args.ContainsKey(SpawnerProps.SpawnRotation))
					rotation = (Quaternion)args[SpawnerProps.SpawnRotation];

				var player = PhotonNetwork.Instantiate(prefab, position, rotation);

				callback?.Invoke(player);
			}
			else
			{
				UnityEngine.Debug.LogError("No Prefabs name provided");
			}
		}

		public void Remove(Hashtable args, Action<bool> callback)
		{
			if (args.ContainsKey(SpawnerProps.SpawnRelease))
			{
				int spawnPoint = (int)args[SpawnerProps.SpawnIndex];
				UnityEngine.Debug.Log("Releasing Spawn Point : " + spawnPoint);
				if(_spawnStrategy.ReleaseSpawnpoint(spawnPoint))
					PhotonNetwork.LocalPlayer.SetSpawnpoint(-1);
			}
			PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

			callback?.Invoke(true);
		}
		#endregion

		#region CALLBACKS

		private void OnPlayerPropertiesUpdate(Photon.Realtime.Player obj, ExitHashtable changedProps)
		{
			if (changedProps.ContainsKey(PlayerProps.PlayerSpawn))
			{
				int spawnPoint = (int)changedProps[PlayerProps.PlayerSpawn];
				if (spawnPoint > -1)
				{
					_spawnStrategy.LockSpawnpoint(spawnPoint);
				}
			}
		}

		

		#endregion

	}
}
