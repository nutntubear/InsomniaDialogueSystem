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

	public void UpdateEvent () {
		currentEvent.key = eventName.text;
		string evType = Utilities.GetEventType(currentEvent);
		if (evType == "base") return;
		if (evType == "int") {
			int attempt = 0;
			System.Int32.TryParse(eventParameter.text, out attempt);
			((DialogueIntEvent)currentEvent).parameter = attempt;
		} else if (evType == "string") {
			((DialogueStringEvent)currentEvent).parameter = eventParameter.text;
		} else if (evType == "bool") {
			((DialogueBoolEvent)currentEvent).parameter = (eventParameterBoolean.value == 0);
		}
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

	public void Setup (DialogueEvent ev) {
		currentEvent = ev;
		string evType = Utilities.GetEventType(currentEvent);
		eventName.text = ev.key;
		if (evType == "int") {
			eventType.value = 0;
			eventParameter.text = ((DialogueIntEvent)ev).parameter.ToString();
		} else if (evType == "string") {
			eventType.value = 1;
			eventParameter.text = ((DialogueStringEvent)ev).parameter;
		} else if (evType == "bool") {
			eventType.value = 2;
			eventParameter.gameObject.SetActive(false);
			eventParameterBoolean.gameObject.SetActive(true);
			if (((DialogueBoolEvent)ev).parameter) {
				eventParameterBoolean.value = 0;
			} else {
				eventParameterBoolean.value = 1;
			}
		}
	}

}
