using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEvents : MonoBehaviour
{

	public void SetOpacity (int opacity) {
		GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, opacity / 10f);
	}

	public void TurnOff () {
		gameObject.SetActive(false);
	}

	public void PrintOut (string s) {
		Debug.Log(s);
	}

}
