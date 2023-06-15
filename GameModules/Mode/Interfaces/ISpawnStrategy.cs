using System.Collections;

namespace  GameModules
{
	public interface ISpawnStrategy<T>
	{
		void SetSpawnMap(Hashtable args);
		bool GetSpawnpoint(out T spawnPoint, Hashtable args);
		bool LockSpawnpoint(T spawnPoint);
		bool ReleaseSpawnpoint(T spawnPoint);
		void Reset();
	}

}
