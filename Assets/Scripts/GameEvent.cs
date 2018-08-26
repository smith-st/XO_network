using UnityEngine;
using System.Collections;
using System;

namespace XO.Events{
	public class GameEvent {

		public static event Action<Cell> 		OnClickOnCell;
		
		 public static void ClickOnCell(Cell cell) {
			if (OnClickOnCell != null)
				OnClickOnCell (cell);
		 }

		
	}
}