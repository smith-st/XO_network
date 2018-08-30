using System;
using System.Collections;
using UnityEngine.Networking;

namespace XO.Events{
	public class ClientEvent {

		public static event Action<NetworkMessage> 		OnStartGame;
		public static event Action<NetworkMessage> 		OnTurn;
		public static event Action<NetworkMessage> 		OnStopGame;
		
		public static void StartGame (NetworkMessage msg) {
			if (OnStartGame != null)
				OnStartGame (msg);
		}

		public static void Turn (NetworkMessage msg) {
			if (OnTurn != null)
				OnTurn (msg);
		}

		public static void StopGame (NetworkMessage msg) {
			if (OnStopGame != null)
				OnStopGame (msg);
		}
	}
}