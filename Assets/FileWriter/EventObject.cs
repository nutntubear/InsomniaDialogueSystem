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
		string evType = currentEvent.GetTemplatedType();
		if (evType == "NONE") return;
		if (evType == "Int32") {
			int attempt = 0;
			System.Int32.TryParse(eventParameter.text, out attempt);
			((DialogueEventTemplated<int>)currentEvent).parameter = attempt;
		} else if (evType == "String") {
			((DialogueEventTemplated<string>)currentEvent).parameter = eventParameter.text;
		} else if (evType == "Boolean") {
			((DialogueEventTemplated<bool>)currentEvent).parameter = (eventParameterBoolean.value == 0);
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
				currentEvent = new DialogueEventTemplated<int>(currentEvent);
				eventParameter.contentType = InputField.ContentType.IntegerNumber;
			} else if (val == 2) {
				currentEvent = new DialogueEventTemplated<string>(currentEvent);
				eventParameter.contentType = InputField.ContentType.Alphanumeric;
			} else if (val == 3) {
				currentEvent = new DialogueEventTemplated<bool>(currentEvent);
				eventParameter.gameObject.SetActive(false);
				eventParameterBoolean.gameObject.SetActive(true);
			}
		}
	}

	public void Setup (DialogueEvent ev) {
		currentEvent = ev;
		string evType = ev.GetTemplatedType();
		eventName.text = ev.key;
		if (evType == "Int32") {
			eventType.value = 1;
			eventParameter.text = ((DialogueEventTemplated<int>)ev).parameter.ToString();
		} else if (evType == "String") {
			eventType.value = 2;
			eventParameter.text = ((DialogueEventTemplated<string>)ev).parameter;
		} else if (evType == "Boolean") {
			eventType.value = 3;
			eventParameter.gameObject.SetActive(false);
			eventParameterBoolean.gameObject.SetActive(true);
			if (((DialogueEventTemplated<bool>)ev).parameter) {
				eventParameterBoolean.value = 0;
			} else {
				eventParameterBoolean.value = 1;
			}
		} else {
			SwitchByInt(0);
		}
	}

}
