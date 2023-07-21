using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModules.Utils.PhotonPun
{
	public class PhotonUtils
	{
		public static ExitGames.Client.Photon.Hashtable SystemToPhotonHashtable(Hashtable systemHashTable)
		{
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			foreach (var key in systemHashTable.Keys)
				properties.Add(key,systemHashTable[key]);
			return properties;
		}

		public static Hashtable ExitToSystemHashtable(ExitGames.Client.Photon.Hashtable photonHashTable)
		{
			Hashtable properties = new Hashtable();
			foreach (var parameter in photonHashTable)
				properties.Add(parameter.Key,parameter.Value);
			return properties;
		}
	}
}

