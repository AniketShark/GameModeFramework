using System;
using System.Collections;

namespace GameModules
{
	public interface ISpawner<T>
	{
		void LoadSpawnMap(Hashtable args);
		void SetSpawnpoint(Hashtable args,Action<T> callback);
		void ReleaseSpawnpoint(T spawnPoint, Action<bool> callback);
		void Spawn(Hashtable args,Action<UnityEngine.Object> callback);
		void Remove(Hashtable args,Action<bool> callback);
		void Reset();
	}
}
