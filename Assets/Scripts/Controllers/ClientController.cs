using UnityEngine.Networking;
using XO.Events;
using XO.NetworkMsg;

namespace XO.Controllers{
	
	public class ClientController  {
		protected NetworkClient _nc;
		public void Start(){

			if (_nc != null) {
				_nc.Disconnect ();
				_nc = null;
			}
			_nc = new NetworkClient();
			_nc.RegisterHandler(new StartGameMsg().id, OnStartGame);
			_nc.RegisterHandler(new NewTurnMsg().id, OnServerTurn);
			_nc.RegisterHandler(new StopGameMsg().id, OnStopGame);
			_nc.Connect("localhost",Constants.PORT);
			
		}


		public void Reset (){
			if (_nc != null && _nc.isConnected)
				_nc.Disconnect ();
		}


		/// <summary>
		/// начало игры
		/// </summary>

		void OnStartGame (NetworkMessage msg){
			ClientEvent.StartGame (msg);
		}
		/// <summary>
		/// серевер сделал ход
		/// </summary>
		/// <param name="msg">Message.</param>
		void OnServerTurn (NetworkMessage msg){
			ClientEvent.Turn (msg);
		}

		/// <summary>
		/// ира закончена
		/// </summary>
		/// <param name="msg">Message.</param>
		void OnStopGame(NetworkMessage msg){
			ClientEvent.StopGame (msg);
		}

		/// <summary>
		/// отппавка сообщения
		/// </summary>
		/// <param name="msg">Message.</param>
		public void SendMsg(BaseXOMsg msg){
			_nc.Send( msg.id, msg);
		}

	}
}
