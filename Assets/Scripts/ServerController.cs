using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XO.NetworkMsg;
using XO.Events;

namespace XO.Controllers{
	public class ServerController  {
		int _connectionId = -1;
		/// <summary>
		/// запускает сервер
		/// </summary>
		/// <returns><c>true</c>, запустился, <c>false</c> ошибка</returns>
		public bool Start(){
			bool status = NetworkServer.Listen(Constants.PORT);
			if (!status){
				NetworkServer.Reset ();
			}else{
				NetworkServer.RegisterHandler(MsgType.Connect, OnClientConnect);
				NetworkServer.RegisterHandler(new NewTurnMsg().id, OnClientTurn);
			}
			return status;
		}
		/// <summary>
		////сброс
		/// </summary>
		public void Reset (){
			NetworkServer.Reset ();
		}

		/// <summary>
		/// При подключении клиента
		/// </summary>
		void OnClientConnect (NetworkMessage msg){
			if (_connectionId == -1){
				_connectionId = msg.conn.connectionId;
				ServerEvent.ClientConnected (msg);
			}
		}
		/// <summary>
		/// когда клиент сделал ход
		/// </summary>
		/// <param name="msg">Message.</param>
		void OnClientTurn (NetworkMessage msg){
			ServerEvent.Turn (msg);

		}
		/// <summary>
		/// отправляет сообщение клиенту
		/// </summary>
		/// <param name="msg">Message.</param>
		public void SendMsg(BaseXOMsg msg){
			if (_connectionId != -1)
				NetworkServer.SendToClient (_connectionId, msg.id, msg);
		}
		
	}
}
