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

	public void TriggerEvent (DialogueEvent devent) {
		string eventType = devent.GetTemplatedType();
		if (eventType == "base") {
			for (int i = 0; i < events.Count; ++i) {
				if (events[i].name == devent.key) {
					events[i].uEvent.Invoke();
				}
			}
		} else if (eventType == "int") {
			for (int i = 0; i < intEvents.Count; ++i) {
				if (intEvents[i].name == devent.key) {
					intEvents[i].uEvent.Invoke(((DialogueEventTemplated<int>)devent).parameter);
				}
			}
		} else if (eventType == "string") {
			for (int i = 0; i < stringEvents.Count; ++i) {
				if (stringEvents[i].name == devent.key) {
					stringEvents[i].uEvent.Invoke(((DialogueEventTemplated<string>)devent).parameter);
				}
			}
		} else if (eventType == "bool") {
			for (int i = 0; i < boolEvents.Count; ++i) {
				if (boolEvents[i].name == devent.key) {
					boolEvents[i].uEvent.Invoke(((DialogueEventTemplated<bool>)devent).parameter);
				}
			}
		}
	}

}
