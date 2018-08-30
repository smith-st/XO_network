using System;
using System.Collections;
using UnityEngine.Networking;

namespace XO.Events{
	public class ServerEvent {

		public static event Action<NetworkMessage> 		OnClientConnected;
		public static event Action<NetworkMessage> 		OnTurn;
		
		public static void ClientConnected(NetworkMessage msg) {
			if (OnClientConnected != null)
				OnClientConnected (msg);
		}

		public static void Turn(NetworkMessage msg) {
			if (OnTurn != null)
				OnTurn (msg);
		}

	}
}