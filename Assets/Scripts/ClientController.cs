﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using XO.NetworkMsg;

namespace XO.Controllers{
	
	public class ClientController : ServerController {
		protected NetworkClient _nc;
		/// <summary>
		/// проверяет подключился ли клиент к серверу
		/// </summary>
		void CheckClientConnection(){
			if (_nc == null) 
				ShowMsg("Игра не может соеденится с соперником"); 
			else if (!_nc.isConnected)
				ShowMsg("Игра не может соеденится с соперником"); 
		}

		protected void StartClient(){
			ShowMsg("Игра запущена как КЛИЕНТ ") ; 
			playerType = PlayerType.CLIENT;
			if (_nc != null) {
				_nc.Disconnect ();
				_nc = null;
			}
			_nc = new NetworkClient();
			_nc.RegisterHandler(new StartGameMsg().id, OnStartGame);
			_nc.RegisterHandler(new NewTurnMsg().id, OnServerTurn);
			_nc.RegisterHandler(new StopGameMsg().id, OnStopGame);
			_nc.Connect("localhost",port);
		}
		/// <summary>
		/// начало игры
		/// </summary>

		void OnStartGame (NetworkMessage msg){
			if (connectionId == -1) {
				connectionId = msg.conn.connectionId;
				ShowMsg ("Игра подключена к серверу: " + connectionId.ToString ()); 
			}
			StartGameMsg m = msg.reader.ReadMessage<StartGameMsg> ();
			StartGame(m.myTurn);
		}
		/// <summary>
		/// серевер сделал ход
		/// </summary>
		/// <param name="msg">Message.</param>
		void OnServerTurn (NetworkMessage msg){
			NewTurnMsg m = msg.reader.ReadMessage<NewTurnMsg> ();
			NewTurn (m.myTurn, m.capturedCell,CellSymbol.X);
		}

		/// <summary>
		/// ира закончена
		/// </summary>
		/// <param name="msg">Message.</param>
		void OnStopGame(NetworkMessage msg){
			StopGameMsg m = msg.reader.ReadMessage<StopGameMsg> ();
			StopGame (m.param);
		}

		/// <summary>
		/// отппавка сообщения
		/// </summary>
		/// <param name="msg">Message.</param>
		protected void SendMsgToServer(BaseXOMsg msg){
			_nc.Send( msg.id, msg);
		}

	}
}
