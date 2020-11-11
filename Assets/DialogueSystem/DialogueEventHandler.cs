using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;

public class DialogueEventHandler : MonoBehaviour
{
	//When you hover over the variables in the inspector these tool tips pop up 
	//Explaining how each variable is different one another
	[Tooltip("Events called from dialogue that don't take parameters from dialogue.")]
	public List<EventPair> events = new List<EventPair>();
	[Tooltip("Events called from dialogue that can take int parameters from dialogue.")]
	public List<EventPairInt> intEvents = new List<EventPairInt>();
	[Tooltip("Events called from dialogue that can take string parameters from dialogue.")]
	public List<EventPairString> stringEvents = new List<EventPairString>();
	[Tooltip("Events called from dialogue that can take bool parameters from dialogue.")]
	public List<EventPairBool> boolEvents = new List<EventPairBool>();

	/*
	 The purpose of the DialogueEventHandler is with the TriggerEvent function takes in the 
	 event with a parameter of type T. Finds out what type the parameter is then finds the
	 corressponding event in the stored list of events. And invokes that Unity Event
	*/
	public void TriggerEvent (DialogueEvent devent) {
		string eventType = devent.GetTemplatedType();
		//Unity events that don't have a parameter
		if (eventType == "NONE") {
			for (int i = 0; i < events.Count; ++i) {
				if (events[i].name == devent.key) {
					events[i].uEvent.Invoke();
				}
			}
		//Unity events that have an int parameter required
		} else if (eventType == "Int32") {
			for (int i = 0; i < intEvents.Count; ++i) {
				if (intEvents[i].name == devent.key) {
					intEvents[i].uEvent.Invoke(((DialogueEventTemplated<int>)devent).parameter);
				}
			}
		//Unity events that have a string parameter required
		} else if (eventType == "String") {
			for (int i = 0; i < stringEvents.Count; ++i) {
				if (stringEvents[i].name == devent.key) {
					stringEvents[i].uEvent.Invoke(((DialogueEventTemplated<string>)devent).parameter);
				}
			}
		//Unity events that have a boolean parameter required
		} else if (eventType == "Boolean") {
			for (int i = 0; i < boolEvents.Count; ++i) {
				if (boolEvents[i].name == devent.key) {
					boolEvents[i].uEvent.Invoke(((DialogueEventTemplated<bool>)devent).parameter);
				}
			}
		}
	}

}
