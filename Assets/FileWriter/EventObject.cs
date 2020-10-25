using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using InsomniaSystemTypes;

public class EventObject : MonoBehaviour
{

	public InputField eventName;
	public Dropdown eventType;
	public InputField eventParameter;
	public Dropdown eventParameterBoolean;

	[HideInInspector]
	public DialogueEvent currentEvent = new DialogueEvent();

	public void SwitchType (Dropdown drop) {
		SwitchByInt(drop.value);
	}

	// Switch the UI based on the type of destination given:
	// 		0 = Base
	//		1 = Int
	//		2 = String
	//		3 = Bool
	void SwitchByInt (int val) {
		eventParameter.gameObject.SetActive(true);
		eventParameterBoolean.gameObject.SetActive(false);
		if (val == 0) {
			currentEvent = new DialogueEvent(currentEvent.key);
			eventParameter.text = "";
			eventParameter.interactable = false;
		} else {
			eventParameter.text = "";
			eventParameter.interactable = true;
			if (val == 1) {
				currentEvent = new DialogueIntEvent(currentEvent);
				eventParameter.contentType = InputField.ContentType.IntegerNumber;
			} else if (val == 2) {
				currentEvent = new DialogueStringEvent(currentEvent);
				eventParameter.contentType = InputField.ContentType.Alphanumeric;
			} else if (val == 3) {
				currentEvent = new DialogueBoolEvent(currentEvent);
				eventParameter.gameObject.SetActive(false);
				eventParameterBoolean.gameObject.SetActive(true);
			}
		}
	}

}
