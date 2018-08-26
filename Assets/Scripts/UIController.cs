using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XO.Controllers{

	public class UIController : BaseController {
		[Header("UI")]
		[SerializeField]
		Text _txtMsg;
		[SerializeField]
		Text _txtX;
		[SerializeField]
		Text _txtO;
		[SerializeField]
		Text _txtLevel;

		protected void ShowMsg(string txt){
			_txtMsg.text = txt ; 
		}

		protected void CountX(int count){
			_txtX.text = count.ToString (); ; 
		}

		protected void CountO(int count){
			_txtO.text = count.ToString (); ; 
		}

		protected void CountLevel(int count){
			_txtLevel.text = "Количество игр: " + count.ToString (); ; 
		}

	}
}
