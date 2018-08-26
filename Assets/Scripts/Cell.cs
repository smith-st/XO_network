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

		public int index = -1;
		public List<CellSymbolAsset> allSymbols = new List<CellSymbolAsset>();
		public bool isUsed{get{ return _isUsed; }}
		public CellSymbol symbol{get{ return _symbol; }}


		SpriteRenderer _sr;
		[SerializeField]
		GameObject _highlight;
		bool _isUsed = false;
		CellSymbol _symbol = CellSymbol.NONE;

		void Awake () {
			_sr = GetComponent<SpriteRenderer>();
			Highlight (false);
		}

		void OnMouseDown() {
			if (!_isUsed)
				GameEvent.ClickOnCell (this);
		}

		public void ShowXO (CellSymbol symbol){
			_sr.sprite = ( 
				from g in allSymbols
				 where g.symbol == symbol
				 select g.sprite
			).Single<Sprite> ();
			_isUsed = true;
			_symbol = symbol;
		}

		public void Reset(){
			_isUsed = false;
			_sr.sprite = null;
			_symbol = CellSymbol.NONE;
			Highlight (false);
		}

		public void Highlight(bool show){
			if (_highlight)
				_highlight.SetActive (show);
		}
	}
}