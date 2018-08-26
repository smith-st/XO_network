using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace XO.Controllers{
	public class BaseController : MonoBehaviour {
		protected int port = 77777;
		protected int connectionId = -1;
		
		protected enum PlayerType{
			SERVER,
			CLIENT
		}
		protected PlayerType playerType;

		virtual public void InitGame () {
			connectionId = -1;
		}
		virtual protected void Start () {
			
		}
		virtual protected void StartGame (bool myTurn = false) {
			
		}

		virtual protected void NewTurn (bool myTurn, int capturedCell, CellSymbol graphic) {
			
		}

		virtual public void StopGame (StopGameParam param) {

		}
	}
}
