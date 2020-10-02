using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;

public class DialogueEventHandler : MonoBehaviour
{

	[Tooltip("Events called from dialogue that don't take parameters from dialogue.")]
	public List<EventPair> events = new List<EventPair>();
	[Tooltip("Events called from dialogue that can take int parameters from dialogue.")]
	public List<EventPairInt> intEvents = new List<EventPairInt>();
	[Tooltip("Events called from dialogue that can take string parameters from dialogue.")]
	public List<EventPairString> stringEvents = new List<EventPairString>();
	[Tooltip("Events called from dialogue that can take bool parameters from dialogue.")]
	public List<EventPairBool> boolEvents = new List<EventPairBool>();

	// Temp event for type checking and use in TriggerEvent.
	DialogueIntEvent tempEventInt = new DialogueIntEvent();
	DialogueStringEvent tempEventString = new DialogueStringEvent();
	DialogueBoolEvent tempEventBool = new DialogueBoolEvent();

	public void TriggerEvent (DialogueEvent devent) {
		if (devent.GetType() == tempEventInt.GetType()) {
			// Int events.
			tempEventInt = (DialogueIntEvent)devent;
			for (int i = 0; i < intEvents.Count; ++i) {
				if (intEvents[i].name == tempEventInt.key) {
					intEvents[i].uEvent.Invoke(tempEventInt.parameter);
				}
			}
		} else if (devent.GetType() == tempEventString.GetType()) {
			// String events.
			tempEventString = (DialogueStringEvent)devent;
			for (int i = 0; i < stringEvents.Count; ++i) {
				if (stringEvents[i].name == tempEventString.key) {
					stringEvents[i].uEvent.Invoke(tempEventString.parameter);
				}
			}
		} else if (devent.GetType() == tempEventBool.GetType()) {
			// Bool events.
			tempEventBool = (DialogueBoolEvent)devent;
			for (int i = 0; i < boolEvents.Count; ++i) {
				if (boolEvents[i].name == tempEventBool.key) {
					boolEvents[i].uEvent.Invoke(tempEventBool.parameter);
				}
			}
		} else {
			// Base events.
			for (int i = 0; i < events.Count; ++i) {
				if (events[i].name == devent.key) {
					events[i].uEvent.Invoke();
				}
			}
		}
	}

}
