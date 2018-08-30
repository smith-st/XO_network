using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XO.Events;

namespace XO{
	
[RequireComponent(typeof(SpriteRenderer))]
	public class Cell : MonoBehaviour {

		[System.Serializable]
		public struct CellSymbolAsset{
			public Sprite sprite;
			public CellSymbol symbol;
		}

		public List<CellSymbolAsset> allSymbols = new List<CellSymbolAsset>();
		public CellSymbol symbol{get{ return _symbol; }}
		public int index {
			get {return _index;	}
			set {_index = value;}
		}
		public bool isUsed{
			get{ return _isUsed; }
		}


		SpriteRenderer _sr;
		[SerializeField]
		GameObject _highlight;
		bool _isUsed = false;
		CellSymbol _symbol = CellSymbol.NONE;
		int _index = -1;

		void Awake () {
			/*comment*/
			_sr = GetComponent<SpriteRenderer>();
			Highlight (false);
		}
		/// <summary>
		/// клик мышки по ячейке
		/// </summary>
		void OnMouseDown() {
			if (!_isUsed)
				GameEvent.ClickOnCell (this);
		}
		/// <summary>
		/// отображает указанный символ
		/// </summary>
		/// <param name="symbol">символ</param>
		public void ShowXO (CellSymbol symbol){
			_sr.sprite = ( 
				from g in allSymbols
				 where g.symbol == symbol
				 select g.sprite
			).Single<Sprite> ();
			_isUsed = true;
			_symbol = symbol;
		}
		/// <summary>
		/// сброс в начальное состояние
		/// </summary>
		public void Reset(){
			_isUsed = false;
			_sr.sprite = null;
			_symbol = CellSymbol.NONE;
			Highlight (false);
		}
		/// <summary>
		/// подсевтка ячейки
		/// </summary>
		/// <param name="show">If set to <c>true</c> show.</param>
		public void Highlight(bool show){
			if (_highlight)
				_highlight.SetActive (show);
		}
	}
}