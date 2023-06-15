using System;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using ExitHashtable = ExitGames.Client.Photon.Hashtable;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class TDMSpawner : ISpawner<int>,IDisposable
	{
		private ISpawnStrategy<int> _strategy;
		private PhotonCallbackHandler _callbackHandler;

		public TDMSpawner(ICallbackHandler callbackHandler, ISpawnStrategy<int> strategy)
		{
			_strategy = strategy;
			_callbackHandler = (PhotonCallbackHandler)callbackHandler;
			_callbackHandler.onPlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
			_callbackHandler.onJoinedRoom += OnRoomJoined;
		}

		#region PUBLIC METHODS

		void ISpawner<int>.LoadSpawnMap(Hashtable args)
		{
			_strategy.SetSpawnMap(args);
		}

		void ISpawner<int>.SetSpawnpoint(Hashtable args, Action<int> callback)
		{
			int spawnPoint = -1;
			if (_strategy.GetSpawnpoint(out spawnPoint, args))
			{
				PhotonNetwork.LocalPlayer.SetSpawnpoint(spawnPoint);
			}
			callback?.Invoke(spawnPoint);
		}

		void ISpawner<int>.ReleaseSpawnpoint(int spawnPoint, Action<bool> callback)
		{
			bool release = _strategy.ReleaseSpawnpoint(spawnPoint);
			//UnityEngine.Debug.LogErrorFormat("[TDMSpawner] Releasing Spawnpoint {0} = {1}",spawnPoint,release);
			callback?.Invoke(release);
		}

		void ISpawner<int>.Spawn(Hashtable args,Action<UnityEngine.Object> callback)
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
				
				PhotonNetwork.LocalPlayer.TagObject = player;
				callback?.Invoke(player);
			}
			else
			{
				UnityEngine.Debug.LogError("No Prefabs name provided");
			}
		}

		void ISpawner<int>.Remove(Hashtable args, Action<bool> callback)
		{
			if (args.ContainsKey(SpawnerProps.SpawnRelease))
			{
				int spawnPoint = (int)args[SpawnerProps.SpawnIndex];
				if(_strategy.ReleaseSpawnpoint(spawnPoint))
					PhotonNetwork.LocalPlayer.SetSpawnpoint(-1);
			}
			PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

			callback?.Invoke(true);
		}

		public void Dispose()
		{
			_strategy = null;
			_callbackHandler.onPlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
			_callbackHandler.onJoinedRoom -= OnRoomJoined;
		}

		#endregion

		#region CALLBACKS

		void OnPlayerPropertiesUpdate(Player targetPlayer, ExitHashtable changedProps)
		{
			if (changedProps.ContainsKey(PlayerProps.PlayerSpawn))
			{
				int spawnPoint = (int) changedProps[PlayerProps.PlayerSpawn];
				if (spawnPoint > -1)
				{
					//UnityEngine.Debug.LogError("[TDMSpawner] Locking Spawn Point " + spawnPoint);
					_strategy.LockSpawnpoint(spawnPoint);
				}
			}
		}

		private void OnRoomJoined()
		{
			foreach(Player player in PhotonNetwork.PlayerList)
			{
				int spawnIndex = player.GetSpawnpoint();
				if( spawnIndex != -1)
				{ 
					_strategy.LockSpawnpoint(spawnIndex);
				}
			}
		}

		public void Reset()
		{
			_strategy.Reset();
		}

		#endregion

	}
}
