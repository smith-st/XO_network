using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using XO.NetworkMsg;
using XO.Events;
using Random = UnityEngine.Random;

namespace XO.Controllers{
	public class GameController : MonoBehaviour {
		public UIController _ui;
		/// <summary>
		/// тип игры
		/// </summary>
		enum PlayerType{
			NONE,
			SERVER,
			CLIENT
		}

		PlayerType _playerType = PlayerType.NONE;

		bool _myTurn = false;
		bool _playGame = false;
		int _grid;
		int _countWin;
		int _countLose;
		int _countPlay;
		//находм все ячейки
		Cell[] _allCells;
		ServerController _server;
		ClientController _client;

		#region STRUCT
		/// <summary>
		/// Проверяет является ли заданая лини выиграшной
		/// </summary>
		struct LineChecker {
			List<int> _line;
			Cell [] _cells;
			bool _isWinLine;
			public bool hasWinLine {
				get{ return _isWinLine;}
			}

			public int lineLength {
				get{ return _line.Count;}
			}

			public bool isWinLine{
				get{
					bool f = true;
					for (int i = 0; i < _line.Count - 1; i++) {
						if (_cells [_line[i]].symbol != _cells [_line[i + 1]].symbol) {
							f = false;
							break;
						}
					}
					if (f && _cells [_line [0]].symbol == CellSymbol.NONE)
						f = false;
					return _isWinLine = f;
				}
			}
			public List<int> line{
				get{
					return _line;
				}
			}

			public LineChecker(ref Cell[] cells){
				_line = new List<int> ();
				_isWinLine = false;
				_cells = cells;
			}

			/// <summary>
			/// новая ячейка
			/// </summary>
			/// <param name="index">Index.</param>
			public void AddCell(int index){
				_line.Add (index);
			}

			public void Reset(){
				if (_line.Count > 0) {
					_line.Clear ();
					_isWinLine = false;
				}
			}
			
		}
		#endregion
		void Start(){
			_allCells = FindObjectsOfType<Cell> ();
			//размер сетки
			_grid = (int)Mathf.Sqrt((float)_allCells.Length);
			//сортируем
			Array.Sort(
				_allCells, delegate(Cell a, Cell b) {
					if (a.transform.position.y > b.transform.position.y){
						return -1;
					}else if (a.transform.position.y < b.transform.position.y){
						return 1;
					}else{
						if (a.transform.position.x > b.transform.position.x){
							return 1;
						}else if (a.transform.position.x < b.transform.position.x){
							return -1;
						}else{
							return 0;
						}	
					}
				}
			);
			//присваиваем id
			for (int i = 0; i < _allCells.Length; i++) 
				_allCells [i].index = i;

			GameEvent.OnClickOnCell += GameEvent_OnClickOnCell;
			InitGame ();
		}
		/// <summary>
		/// инициализация начальный парметров
		/// </summary>
		public void InitGame (){

			if (_playerType == PlayerType.SERVER) {
				ServerEvent.OnClientConnected -= ServerEvent_OnClientConnected;
				ServerEvent.OnTurn -= ServerEvent_OnTurn;
				_server.Reset ();
				_server = null;
			} else if (_playerType == PlayerType.CLIENT){
				ClientEvent.OnStartGame -= ClientEvent_OnStartGame;
				ClientEvent.OnStopGame -= ClientEvent_OnStopGame;
				ClientEvent.OnTurn -= ClientEvent_OnTurn;
				_client.Reset ();
				_client = null;
			}
			_playerType = PlayerType.NONE;

			_countWin = _countLose = _countPlay = 0;
			_ui.CountLevel (0);
			_ui.CountX (0);
			_ui.CountO (0);
			//стартуем сервер
			_server = new ServerController();
			if (_server.Start ()) {
				_playerType = PlayerType.SERVER;
				_ui.ShowMsg ("Игра запущена. Ожидание соперника");
				ServerEvent.OnClientConnected += ServerEvent_OnClientConnected;
				ServerEvent.OnTurn += ServerEvent_OnTurn;
			} else {
				_playerType = PlayerType.CLIENT;
				_server = null;
				//если сервер не удалось запустить, возможно он уже запущен
				//стартуем клиента
				_client = new ClientController();
				_client.Start();
				_ui.ShowMsg("Не найден соперник ") ; 
				ClientEvent.OnStartGame += ClientEvent_OnStartGame;
				ClientEvent.OnStopGame += ClientEvent_OnStopGame;
				ClientEvent.OnTurn += ClientEvent_OnTurn;
			}
		}
		void ClientEvent_OnStartGame (NetworkMessage msg){
			StartGameMsg m = msg.reader.ReadMessage<StartGameMsg> ();
			StartGame(m.myTurn);
		}
		void ClientEvent_OnStopGame (NetworkMessage msg){
			StopGameMsg m = msg.reader.ReadMessage<StopGameMsg> ();
			StopGame (m.param);
		}
		void ClientEvent_OnTurn (NetworkMessage msg){
			NewTurnMsg m = msg.reader.ReadMessage<NewTurnMsg> ();
			NewTurn (m.myTurn, m.capturedCell, CellSymbol.X);
		}

		void ServerEvent_OnClientConnected (NetworkMessage msg){
			_ui.ShowMsg ("К игре подключился клиент");
			StartGame ();
		}

		void ServerEvent_OnTurn (NetworkMessage msg){
			NewTurnMsg m = msg.reader.ReadMessage<NewTurnMsg> ();
			NewTurn (m.myTurn, m.capturedCell,CellSymbol.O);
		}



		/// <summary>
		/// проверяем на заполнение вертикальных и горизонтальных линий
		/// проверка сделана таким образом, что может проверять сетку лубой размерности
		/// </summary>
		/// <returns><c>true</c> если есть победитель, <c>false</c> нету</returns>
		bool ChekForWinner (){
			if (CheckForDraw ()) {
				DrawGame ();
				return false;
			}

			bool f = false;
			LineChecker lc = new LineChecker (ref _allCells);
			int i, j;
			for (i = 0; i < _grid; i++) {
				//горизонтальные 
				lc.Reset ();
				for (j = _grid * i; lc.lineLength < _grid; j++)
					lc.AddCell (j);
				if (lc.isWinLine)
					break;
				//вертикальные
				lc.Reset ();
				for (j = i; lc.lineLength < _grid; j += _grid)
					lc.AddCell (j);
				if (lc.isWinLine)
					break;
			}
			if (lc.hasWinLine) {
				WinGame (lc.line);
				f = true;
			}else {
				lc.Reset ();
				//проверяем диагональные линии (слева на право)
				for (i = 0; i < _grid; i++)
					lc.AddCell (_grid * i + i);
				if (lc.isWinLine) {
					WinGame (lc.line);
					f = true;
				}else {
					lc.Reset ();
					for (i = 0; i < _grid; i++)
						lc.AddCell (_grid-1 + _grid * i - i);

					if (lc.isWinLine) {
						WinGame (lc.line);
						f = true;
					}
				}
			}
			return f;
		}
		/// <summary>
		/// проверка на ничью
		/// </summary>
		/// <returns><c>true</c>, if for draw was checked, <c>false</c> otherwise.</returns>
		bool CheckForDraw(){
			bool f = true;
			for (int i = 0; i < _allCells.Length; i++) {
				if (_allCells [i].symbol == CellSymbol.NONE) {
					f = false;
					break;
				}
			}
			return f;
		}
		/// <summary>
		/// игра закончилась ничьей
		/// </summary>
		void DrawGame(){
			StopGameMsg m = new StopGameMsg ();
			StopGameParam p = new StopGameParam ();
			p.totalPlay = ++_countPlay;
			p.totalX = _countWin;
			p.totalO = _countLose;
			p.draw = true;
			StopGame (p);
			m.param = p;
			_server.SendMsg(m);
			Invoke ("StartGame", 3f);
		}
		/// <summary>
		/// кто-то выиграл игру
		/// </summary>
		/// <param name="line">Line.</param>
		void WinGame(List<int> line){
			StopGameMsg m = new StopGameMsg ();
			StopGameParam p = new StopGameParam ();

			p.totalPlay = ++_countPlay;
			if (_allCells [line [0]].symbol == CellSymbol.X) {
				_countWin++;
				p.win = true;
			} else {
				_countLose++;
				p.win = false;
			}
			p.totalX = _countWin;
			p.totalO = _countLose;
			p.line = line.ToArray();
			p.draw = false;

			StopGame (p);
			m.param = p;
			m.param.win = !p.win;
			_server.SendMsg (m);
			Invoke ("StartGame", 3f);
		}
		/// <summary>
		/// когда нажали на ячейку
		/// </summary>
		/// <param name="cell">Cell.</param>
		void GameEvent_OnClickOnCell (Cell cell){
			if (_myTurn && _playGame) {
				_myTurn = false;
				NewTurnMsg m = new NewTurnMsg ();
				m.capturedCell = cell.index;
				m.myTurn = !_myTurn;
				_ui.ShowMsg ("Ход соперника");
				if (_playerType == PlayerType.SERVER) {
					cell.ShowXO (CellSymbol.X);
					_server.SendMsg (m);
					ChekForWinner ();
				} else {
					cell.ShowXO (CellSymbol.O);
					_client.SendMsg(m);
				}
			}
		}

		/// <summary>
		/// перезагружка сделана что бы этот метод можно было вызвать через Invoke
		/// </summary>
		void StartGame(){
			StartGame (false);
		}

		/// <summary>
		/// Начало игры
		/// </summary>
		/// <param name="myTurn">сервер не обращает внимания на этот параметр, так как он решает кто первым делает ход. А клиенту этот параметр указывает его ход или сервера.</param>
		void StartGame(bool myTurn = false){
			_playGame = true;
			//обнуляем ячейки
			for (int i = 0; i < _allCells.Length; i++) {
				_allCells [i].Reset ();
			}

			if (_playerType == PlayerType.SERVER) {
				StartGameMsg m = new StartGameMsg ();
				//решаем кто ходит случайным образом
				if (Random.Range (0, 2) == 1) {
					//первый ход сервера, играет крестиками
					_myTurn = true;
					_ui.ShowMsg ("Ваш ход");
				} else {
					//первый ход клиента, играет ноликами
					_myTurn = false;
					_ui.ShowMsg ("Ход соперника");
				}
				m.myTurn = !_myTurn;
				_server.SendMsg (m);
			} else {
				_myTurn = myTurn;
				if (_myTurn) 
					_ui.ShowMsg ("Ваш ход");
				else
					_ui.ShowMsg ("Ход соперника");
			}
		}
		/// <summary>
		/// игра окончена
		/// </summary>
		/// <param name="p">параметры </param>
		void StopGame (StopGameParam p){
			_playGame = false;
			if (p.draw) 
				_ui.ShowMsg ("Ничья");
			else if (p.win) 
				_ui.ShowMsg ("Вы выиграли");
			else 
				_ui.ShowMsg ("Вы проиграли");
			if (p.line != null){
				for (int i = 0; i < p.line.Length; i++) 
					_allCells [p.line [i]].Highlight (true);
			}
			_ui.CountLevel (p.totalPlay);
			_ui.CountX (p.totalX);
			_ui.CountO (p.totalO);
		}

		/// <summary>
		/// новый ход
		/// </summary>
		/// <param name="myTurn"><c>true</c>  - мой ход</param>
		/// <param name="capturedCell">захваченная ячейка соперником, надо отобразить на своем поле</param>
		/// <param name="graphic">Символ соперника</param>
		void NewTurn (bool myTurn, int capturedCell, CellSymbol graphic){
			_myTurn = myTurn;
			if (_myTurn)
				_ui.ShowMsg ("Ваш ход");
			else
				_ui.ShowMsg ("Ход соперника");
			if (capturedCell != -1) {
				Cell cell = (
				                from c in _allCells
				                where c.index == capturedCell
				                select c
				            ).Single<Cell> ();
				cell.ShowXO (graphic);
				///если ход сделал клиент, тогда на стороне сервера проверяем условия выиграша
				if (_playerType == PlayerType.SERVER)
					ChekForWinner ();
			}
		}
	}
}