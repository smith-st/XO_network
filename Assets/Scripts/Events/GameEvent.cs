using System;
using System.Collections;
using UnityEngine;

namespace XO.Events{
	public class GameEvent {

		public static event Action<Cell> 		OnClickOnCell;
		
		 public static void ClickOnCell(Cell cell) {
			if (OnClickOnCell != null)
				OnClickOnCell (cell);
		 }
	}
}