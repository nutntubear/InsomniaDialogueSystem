using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

	public CameraControl control;

	public void OnPointerEnter (PointerEventData eventData) {
		control.hoveringOver = true;
	}

	public void OnPointerExit (PointerEventData eventData) {
		control.hoveringOver = false;
	}

	void OnDisable () {
		control.hoveringOver = false;
	}

}
