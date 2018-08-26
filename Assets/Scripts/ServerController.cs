using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XO.NetworkMsg;

namespace XO.Controllers{
	public class ServerController : UIController {
		/// <summary>
		/// запускает сервер
		/// </summary>
		/// <returns><c>true</c>, запустился, <c>false</c> ошибка</returns>
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
		/// <summary>
		/// При подключении клиента
		/// </summary>
		void OnClientConnect (NetworkMessage msg){
			if (connectionId == -1){
				connectionId = msg.conn.connectionId;
				ShowMsg ("К игре подключился клиент: " + connectionId.ToString ());
				StartGame();


			}
		}
		/// <summary>
		/// когда клиент сделал ход
		/// </summary>
		/// <param name="msg">Message.</param>
		void OnClientTurn (NetworkMessage msg){
			NewTurnMsg m = msg.reader.ReadMessage<NewTurnMsg> ();
			NewTurn (m.myTurn, m.capturedCell,CellSymbol.O);
		}
		/// <summary>
		/// отправляет сообщение клиенту
		/// </summary>
		/// <param name="msg">Message.</param>
		protected void SendMsgToClient(BaseXOMsg msg){
			NetworkServer.SendToClient (connectionId, msg.id, msg);
		}
		
	}
}
