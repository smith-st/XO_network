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
	public class GameController : ClientController {
		bool _myTurn = false;
		bool _playGame = false;
		int _grid;
		int _countWin;
		int _countLose;
		int _countPlay;
		//находм все ячейки
		Cell[] _allCells;

		#region STRUCT
		struct LineChecker {
			List<int> _line;
			Cell [] _cells;
			bool _isWinLine;
			public bool hasWinLine {
				get{ return _isWinLine;}
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
		override protected void Start(){
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

		public override void InitGame (){
			base.InitGame ();
			CancelInvoke("CheckClientConnection");
			_countWin = _countLose = _countPlay = 0;
			CountLevel (0);
			CountX (0);
			CountO (0);
			//стартуем сервер
			if (!StartServer()){
				//если сервер не удалось запустить, возможно он уже запущен
				//стартуем клиента
				NetworkServer.Reset();
				StartClient();
				Invoke("CheckClientConnection",1f);
			}
		}

		/// <summary>
		/// роверяем на заполнение вертикальных и горизонтальных линий
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
				for (j = 1 + _grid * i; j <= _grid + _grid * i; j++)
					lc.AddCell (j-1);
				if (lc.isWinLine)
					break;
				//вертикальные
				lc.Reset ();
				for (j = 1 + i; j <= _grid * 2 + 1 + i; j += _grid)
					lc.AddCell (j-1);
				if (lc.isWinLine)
					break;
			}
			if (lc.hasWinLine) {
				WinGame (lc.line);
				f = true;
			}
			else {
				lc.Reset ();
				//проверяем диагональные линии
				for (i = 0; i < _grid; i++)
					lc.AddCell (_grid * i + i);
				if (lc.isWinLine) {
					WinGame (lc.line);
					f = true;
				}else {
					lc.Reset ();
					for (i = 0; i < _grid; i++)
						lc.AddCell (2 + _grid * i - i);

					if (lc.isWinLine) {
						WinGame (lc.line);
						f = true;
					}
				}
			}
			return f;
		}

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

		void DrawGame(){
			StopGameMsg m = new StopGameMsg ();
			StopGameParam p = new StopGameParam ();
			p.totalPlay = ++_countPlay;
			p.totalX = _countWin;
			p.totalO = _countLose;
			p.draw = true;
			StopGame (p);
			m.param = p;
			SendMsgToClient (m);
			Invoke ("StartGame", 3f);
		}

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
			SendMsgToClient (m);
			Invoke ("StartGame", 3f);
		}

		void GameEvent_OnClickOnCell (Cell cell){
			if (_myTurn && _playGame) {
				_myTurn = false;
				NewTurnMsg m = new NewTurnMsg ();
				m.capturedCell = cell.index;
				m.myTurn = !_myTurn;
				ShowMsg ("Ход соперника");
				if (playerType == PlayerType.SERVER) {
					cell.ShowXO (CellSymbol.X);
					SendMsgToClient (m);
					ChekForWinner ();
				} else {
					cell.ShowXO (CellSymbol.O);
					SendMsgToServer (m);
				}
			}
		}

		protected void StartGame(){
			StartGame (false);
		}

		override protected void StartGame(bool myTurn = false){
			_playGame = true;
			for (int i = 0; i < _allCells.Length; i++) {
				_allCells [i].Reset ();
			}
			if (playerType == PlayerType.SERVER) {
				StartGameMsg m = new StartGameMsg ();
				if (Random.Range (0, 2) == 1) {
					//первый ход сервера, играет крестиками
					_myTurn = true;
					ShowMsg ("Ваш ход");
				} else {
					//первый ход клиента, играет ноликами
					_myTurn = false;
					ShowMsg ("Ход соперника");
				}
				m.myTurn = !_myTurn;
				SendMsgToClient (m);
			} else {
				_myTurn = myTurn;
				if (_myTurn) 
					ShowMsg ("Ваш ход");
				else
					ShowMsg ("Ход соперника");
			}
		}

		public override void StopGame (StopGameParam p){
			base.StopGame (p);
			_playGame = false;
			if (p.draw) 
				ShowMsg ("Ничья");
			else if (p.win) 
				ShowMsg ("Вы выиграли");
			else 
				ShowMsg ("Вы проиграли");
			if (p.line != null){
				for (int i = 0; i < p.line.Length; i++) 
					_allCells [p.line [i]].Highlight (true);
			}
			CountLevel (p.totalPlay);
			CountX (p.totalX);
			CountO (p.totalO);

		}

		protected override void NewTurn (bool myTurn, int capturedCell, CellSymbol graphic){
			base.NewTurn (myTurn, capturedCell,graphic);
			_myTurn = myTurn;
			if (_myTurn)
				ShowMsg ("Ваш ход");
			else
				ShowMsg ("Ход соперника");
			if (capturedCell != -1) {
				Cell cell = (
				                from c in _allCells
				                where c.index == capturedCell
				                select c
				            ).Single<Cell> ();
				cell.ShowXO (graphic);
				if (playerType == PlayerType.SERVER)
					ChekForWinner ();
			}
		}

		void NewLevel(){
			
		}

	
	

	}
}
	
