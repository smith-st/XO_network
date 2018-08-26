using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XO.NetworkMsg;

namespace XO.Controllers{
	public class ServerController : UIController {
	protected bool StartServer(){
			NetworkServer.Reset ();
			playerType = PlayerType.SERVER;
			bool status = NetworkServer.Listen(port);
			if (!status){
				ShowMsg("Порт уже занят. Запускаем клиента ") ; 
			}else{
				ShowMsg ("Игра запущена как СЕРВЕР");
				NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnect);
				NetworkServer.RegisterHandler(new NewTurnMsg().id, OnClientTurn);
			}
			return status;
		}

		void OnClientConnect (NetworkMessage msg){
			if (connectionId == -1){
				connectionId = msg.conn.connectionId;
				ShowMsg ("К игре подключился клиент: " + connectionId.ToString ());
				StartGame();


			}
		}

		void OnClientTurn (NetworkMessage msg){
			NewTurnMsg m = msg.reader.ReadMessage<NewTurnMsg> ();
			NewTurn (m.myTurn, m.capturedCell,CellSymbol.O);
		}

		protected void SendMsgToClient(BaseXOMsg msg){
			NetworkServer.SendToClient (connectionId, msg.id, msg);
		}
		
	}
}
