using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace InsomniaSystemTypes {

	/*
	Utilities
	*/
	public class Utilities {
		// Base types (save memory for comparisons)
		public static Destination destination = new Destination();
		public static MemoryDestination memoryDestination = new MemoryDestination();
		public static MemoryDestinationInt memoryDestinationInt = new MemoryDestinationInt();
		public static MemoryDestinationString memoryDestinationString = new MemoryDestinationString();
		public static MemoryDestinationBool memoryDestinationBool = new MemoryDestinationBool();
		public static DialogueEvent dialogueEvent = new DialogueEvent();
		public static DialogueIntEvent dialogueIntEvent = new DialogueIntEvent();
		public static DialogueStringEvent dialogueStringEvent = new DialogueStringEvent();
		public static DialogueBoolEvent dialogueBoolEvent = new DialogueBoolEvent();
		public static IntMemory intMemory = new IntMemory();
		public static StringMemory stringMemory = new StringMemory();
		public static BoolMemory boolMemory = new BoolMemory();

		public static int SortNode (Node a, Node b) {
			return a.id.CompareTo(b.id);
		}

		// Used to give the type of a destination without needing to run these checks every time.
		public static string GetDestinationType (Destination dest) {
			if (dest.GetType() == memoryDestinationInt.GetType()) {
				return "int";
			} else if (dest.GetType() == memoryDestinationString.GetType()) {
				return "string";
			} else if (dest.GetType() == memoryDestinationBool.GetType()) {
				return "bool";
			} else {
				return "base";
			}
		}

		// Used to give the type of an event without needing to run these checks every time.
		public static string GetEventType (DialogueEvent ev) {
			if (ev.GetType() == dialogueIntEvent.GetType()) {
				return "int";
			} else if (ev.GetType() == dialogueStringEvent.GetType()) {
				return "string";
			} else if (ev.GetType() == dialogueBoolEvent.GetType()) {
				return "bool";
			} else {
				return "base";
			}
		}

		// For the loading of Destinations, DialogueEvents, and Memories when loading Nodes.
		static Regex numValueCheck = new Regex(System.String.Format(@"{0}value{0}:(-?[0-9]+)", "\""));
		static Regex stringValueCheck = new Regex(System.String.Format(@"{0}value{0}:{0}(.+?){0}", "\""));
		static Regex boolValueCheck = new Regex(System.String.Format(@"{0}value{0}:(true|false)", "\""));
		static Regex numParamCheck = new Regex(System.String.Format(@"{0}parameter{0}:(-?[0-9]+)", "\""));
		static Regex stringParamCheck = new Regex(System.String.Format(@"{0}parameter{0}:{0}(.+?){0}", "\""));
		static Regex boolParamCheck = new Regex(System.String.Format(@"{0}parameter{0}:(true|false)", "\""));

		public static List<Destination> GetDestinations (string node) {
			List<Destination> dests = new List<Destination>();
			// Find the section of the node JSON string where destinations are found.
			int start = node.IndexOf("],\"destinations\":[") + 2;
			int end = node.IndexOf("],\"memories\":[") + 1;
			string[] destinations = node.Substring(start, end - start).Split('{');
			string destString;
			for (int i = 1; i < destinations.Length; ++i) {
				destString = "{" + destinations[i].Substring(0, destinations[i].Length - 1);
				if (!destString.Contains("memoryKey")) {
					dests.Add(JsonUtility.FromJson<Destination>(destString));
					continue;
				}
				// If it is a memory destination, use regex to check each type:
				MatchCollection mc = numValueCheck.Matches(destString);
				if (mc.Count > 0) {
					dests.Add(JsonUtility.FromJson<MemoryDestinationInt>(destString));
				} else {
					mc = stringValueCheck.Matches(destString);
					if (mc.Count > 0) {
						dests.Add(JsonUtility.FromJson<MemoryDestinationString>(destString));
					} else {
						mc = boolValueCheck.Matches(destString);
						if (mc.Count > 0) {
							dests.Add(JsonUtility.FromJson<MemoryDestinationBool>(destString));
						} else {
							// If it somehow gets to this point, add it as a generic MemoryDestination.
							dests.Add(JsonUtility.FromJson<MemoryDestination>(destString));
						}
					}
				}
			}
			return dests;
		}

		public static List<DialogueEvent> GetEvents (string node) {
			List<DialogueEvent> events = new List<DialogueEvent>();
			// Find the section of the node JSON string where dEvents are found.
			int start = node.IndexOf(",\"events\":[") + 2;
			int end = node.IndexOf("],\"destinations\":[") + 1;
			string[] dEvents = node.Substring(start, end - start).Split('{');
			string eventString;
			for (int i = 1; i < dEvents.Length; ++i) {
				eventString = "{" + dEvents[i].Substring(0, dEvents[i].Length - 1);
				if (!eventString.Contains("parameter")) {
					events.Add(JsonUtility.FromJson<DialogueEvent>(eventString));
					continue;
				}
				// If it is a memory destination, use regex to check each type:
				MatchCollection mc = numParamCheck.Matches(eventString);
				if (mc.Count > 0) {
					events.Add(JsonUtility.FromJson<DialogueIntEvent>(eventString));
				} else {
					mc = stringParamCheck.Matches(eventString);
					if (mc.Count > 0) {
						events.Add(JsonUtility.FromJson<DialogueStringEvent>(eventString));
					} else {
						mc = boolParamCheck.Matches(eventString);
						if (mc.Count > 0) {
							events.Add(JsonUtility.FromJson<DialogueBoolEvent>(eventString));
						} else {
							// If it somehow gets to this point, add it as a generic MemoryDestination.
							events.Add(JsonUtility.FromJson<DialogueEvent>(eventString));
						}
					}
				}
			}
			return events;
		}

	}

	/*
	Nodes
		Contains all of the information for a dialogue node.
		Type is determined by the number of destinations:
		one destination makes a node a normal node (type == 'n'),
		more than one destination makes a node a branching node (type == 'b'),
		zero nodes makes a node an ending node (type == 'e').
	*/
	[System.Serializable]
	public class Node {
		public int id;
		public char type;
		public string body;
		public string speaker;
		public bool player;
		public List<DialogueEvent> events;
		public List<Destination> destinations;
		public List<Memory> memories;
		public string spareData;
		// Used for visual file writer.
		public Vector2 position;

		public Node () {
			type = 'e';
			player = false;
			events = new List<DialogueEvent>();
			destinations = new List<Destination>();
			memories = new List<Memory>();
		}

		public bool HasDestination (int dest) {
			if (dest == id) return true;
			for (int i = 0; i < destinations.Count; ++i) {
				if (destinations[i].dest == dest) return true;
			}
			return false;
		}

		// SetType sets the char type variable. Run when producing the JSON dialogue file.
		void SetType () {
			if (destinations.Count == 0) {
				type = 'e';
			} else if (destinations.Count == 1) {
				type = 'n';
			} else {
				type = 'b';
			}
		}
		// Returns a JSON formatted string containing the Node information.
		public string SaveNode () {
			SetType();
			string checkType;
			// Get JSON for Destinations:
			string destinationsString = "";
			List<Destination> tempDests = new List<Destination>();
			for (int i = 0; i < destinations.Count; ++i) {
				tempDests.Add(destinations[i]);
				checkType = Utilities.GetDestinationType(destinations[i]);
				if (checkType == "int") {
					destinationsString += JsonUtility.ToJson((MemoryDestinationInt)destinations[i]);
				} else if (checkType == "string") {
					destinationsString += JsonUtility.ToJson((MemoryDestinationString)destinations[i]);
				} else if (checkType == "bool") {
					destinationsString += JsonUtility.ToJson((MemoryDestinationBool)destinations[i]);
				} else {
					destinationsString += JsonUtility.ToJson(destinations[i]);
				}
				if (i != destinations.Count - 1) {
					destinationsString += ",";
				}
			}
			destinations = new List<Destination>();
			destinations.Add(new Destination(-1));
			// Get JSON for Events:
			string eventsString = "";
			List<DialogueEvent> tempEvents = new List<DialogueEvent>();
			for (int i = 0; i < events.Count; ++i) {
				tempEvents.Add(events[i]);
				checkType = Utilities.GetEventType(events[i]);
				if (checkType == "int") {
					eventsString += JsonUtility.ToJson((DialogueIntEvent)events[i]);
				} else if (checkType == "string") {
					eventsString += JsonUtility.ToJson((DialogueStringEvent)events[i]);
				} else if (checkType == "bool") {
					eventsString += JsonUtility.ToJson((DialogueBoolEvent)events[i]);
				} else {
					eventsString += JsonUtility.ToJson(events[i]);
				}
				if (i != events.Count - 1) {
					eventsString += ",";
				}
			}
			events = new List<DialogueEvent>();
			events.Add(new DialogueEvent());
			string jsonText = JsonUtility.ToJson(this);
			jsonText = jsonText.Replace("{\"dest\":-1}", destinationsString);
			jsonText = jsonText.Replace("{\"key\":\"\"}", eventsString);
			// Reset the replaced lists:
			destinations = tempDests;
			events = tempEvents;
			return jsonText;
		}

		public string ToTextBox () {
			string line1 = id.ToString() + " - ";
			line1 += speaker.Substring(0, (int)Mathf.Clamp(11 - line1.Length, 0, speaker.Length));
			string line2;
			if (memories.Count > 9) line2 = "9+ Memories";
			else line2 = memories.Count.ToString() + " Memories";
			string line3;
			if (events.Count > 9) line3 = "9+ Events";
			else line3 = events.Count.ToString() + " Events";
			string line4 = "";
			int bodyLength = body.Length;
			bool addEllipsis = false;
			if (bodyLength > 33) {
				bodyLength = 30;
				addEllipsis = true;
			}
			for (int i = 1; i <= bodyLength; ++i) {
				line4 += body[i - 1];
				if (i % 11 == 0) {
					line4 += "\n";
				}
			}
			if (addEllipsis) {
				line4 += "...";
			}
			return System.String.Format("{0}\n{1}\n{2}\n{3}", line1, line2, line3, line4);
		}

	}

	/*
	UnityEvent Types
		Used for handling Dialogue Events, but each UnityEvent<type> can be used
		for other purposes in-game.
	*/

	[System.Serializable]
	public class EventPair {
		public string name;
		public UnityEvent uEvent;

		public EventPair () {
			uEvent = new UnityEvent();
		}
	}

	[System.Serializable]
	public class UnityEventInt : UnityEvent<int> {}
	[System.Serializable]
	public class EventPairInt {
		public string name;
		public UnityEventInt uEvent;

		public EventPairInt () {
			uEvent = new UnityEventInt();
		}
	}

	[System.Serializable]
	public class UnityEventString : UnityEvent<string> {}
	[System.Serializable]
	public class EventPairString {
		public string name;
		public UnityEventString uEvent;

		public EventPairString () {
			uEvent = new UnityEventString();
		}
	}

	[System.Serializable]
	public class UnityEventBool : UnityEvent<bool> {}
	[System.Serializable]
	public class EventPairBool {
		public string name;
		public UnityEventBool uEvent;

		public EventPairBool () {
			uEvent = new UnityEventBool();
		}
	}
	
	/*
	Dialogue Events
		Used to trigger events in the DialogueEventHandler when the node is reached.
		Int, String, and Bool events are used to trigger events with different parameters.
		The base DialogueEvent class is used to trigger events without parameters.
	*/
	[System.Serializable]
	public class DialogueEvent {
		public string key;

		public DialogueEvent (string key_="") {
			key = key_;
		}

		public DialogueEvent (DialogueEvent c) {
			key = c.key;
		}

        public override string ToString()
        {
            return key;
        }

    }
	[System.Serializable]
	public class DialogueIntEvent : DialogueEvent {
		public int parameter;

		public DialogueIntEvent (string key_="", int param=0) {
			key = key_;
			parameter = param;
		}

		public DialogueIntEvent (DialogueEvent c) {
			key = c.key;
			if (Utilities.GetEventType(c) == "int") {
				parameter = ((DialogueIntEvent)c).parameter;
			}
		}

    }
	[System.Serializable]
	public class DialogueStringEvent : DialogueEvent {
		public string parameter;

		public DialogueStringEvent (string key_="", string param="") {
			key = key_;
			parameter = param;
		}

		public DialogueStringEvent (DialogueEvent c) {
			key = c.key;
			if (Utilities.GetEventType(c) == "string") {
				parameter = ((DialogueStringEvent)c).parameter;
			}
		}
	}
	[System.Serializable]
	public class DialogueBoolEvent : DialogueEvent {
		public bool parameter;

		public DialogueBoolEvent (string key_="", bool param=false) {
			key = key_;
			parameter = param;
		}

		public DialogueBoolEvent (DialogueEvent c) {
			key = c.key;
			if (Utilities.GetEventType(c) == "bool") {
				parameter = ((DialogueBoolEvent)c).parameter;
			}
		}
	}

	/*
	Destinations
		Places that the node can lead.
		Contains an int for the destination node.
		MemoryDestinations contain an int for the destination node, a memory key, a
		memory check code (eq, lt, gt, leq, geq), and a value to be compared to.
		Only int uses all five of those comparator codes, as string and bool use eq exclusively.
	*/
	[System.Serializable]
	public class Destination {
		public int dest;

		public Destination (int dest_=-1) {
			dest = dest_;
		}

    }
	[System.Serializable]
	public class MemoryDestination : Destination {
		[SerializeField]
		public string memoryKey;
		[SerializeField]
		public string checkCode = "eq";
		[SerializeField]
		public bool forced = false;
	}
	[System.Serializable]
	public class MemoryDestinationInt : MemoryDestination {
		[SerializeField]
		public int value;

		public MemoryDestinationInt (int dest_=-1) {
			dest = dest_;
		}

		public MemoryDestinationInt (Destination c) {
			dest = c.dest;
			if (c.GetType() == Utilities.memoryDestination.GetType()) {
				memoryKey = ((MemoryDestination)c).memoryKey;
				checkCode = "=";
			}
		}
	}
	[System.Serializable]
	public class MemoryDestinationString : MemoryDestination {
		[SerializeField]
		public string value;

		public MemoryDestinationString (int dest_=-1) {
			dest = dest_;
		}

		public MemoryDestinationString (Destination c) {
			dest = c.dest;
			if (c.GetType() == Utilities.memoryDestination.GetType()) {
				memoryKey = ((MemoryDestination)c).memoryKey;
				checkCode = "=";
			}
		}
	}
	[System.Serializable]
	public class MemoryDestinationBool : MemoryDestination {
		[SerializeField]
		public bool value;

		public MemoryDestinationBool (int dest_=-1) {
			dest = dest_;
		}

		public MemoryDestinationBool (Destination c) {
			dest = c.dest;
			if (c.GetType() == Utilities.memoryDestination.GetType()) {
				memoryKey = ((MemoryDestination)c).memoryKey;
				checkCode = "=";
			}
		}
	}

	/*
	Memories
		Contain information that can be saved and checked against.
		Int, String, and Bool memories are used to save different types of data.
	*/
	[System.Serializable]
	public class Memory {
		public string key;
	}
	[System.Serializable]
	public class IntMemory : Memory {
		public int value;

		public IntMemory (string key_="", int value_=0) {
			key = key_;
			value = value_;
		}
	}
	[System.Serializable]
	public class StringMemory : Memory {
		public string value;

		public StringMemory (string key_="", string value_="") {
			key = key_;
			value = value_;
		}
	}
	[System.Serializable]
	public class BoolMemory : Memory {
		public bool value;

		public BoolMemory (string key_="", bool value_=false) {
			key = key_;
			value = value_;
		}
	}

	/*
	Memory Dictionary
		Used by the DialogueMemory system, where variables can be saved and checked through
		dialogue nodes.
		Contains three different types of memories: integer, string, and boolean.
	*/
	[System.Serializable]
	public class MemoryDictionary {
		public List<IntMemory> intMemories;
		public List<StringMemory> stringMemories;
		public List<BoolMemory> boolMemories;

		// Constructor
		public MemoryDictionary () {
			intMemories = new List<IntMemory>();
			stringMemories = new List<StringMemory>();
			boolMemories = new List<BoolMemory>();
		}

		// Memory Addition and Alteration
		public void SetMemory (Memory memory) {
			if (this.Contains(memory.key)) {
				throw new Exception("Insomnia Dialogue System Exception - Memory already exists!");
			}
			if (memory.GetType() == Utilities.intMemory.GetType()) {
				SetIntMemory((IntMemory)memory);
			} else if (memory.GetType() == Utilities.stringMemory.GetType()) {
				SetStringMemory((StringMemory)memory);
			} else if (memory.GetType() == Utilities.boolMemory.GetType()) {
				SetBoolMemory((BoolMemory)memory);
			}
		}
		// Type Based Memory Setters; called from internal methods suck as SetMemory.
		void SetIntMemory (IntMemory memory) {
			for (int i = 0; i < intMemories.Count; ++i) {
				if (intMemories[i].key == memory.key) {
					intMemories[i].value = memory.value;
					return;
				}
			}
			intMemories.Add(memory);
		}
		void SetStringMemory (StringMemory memory) {
			for (int i = 0; i < stringMemories.Count; ++i) {
				if (stringMemories[i].key == memory.key) {
					stringMemories[i].value = memory.value;
					return;
				}
			}
			stringMemories.Add(memory);
		}
		void SetBoolMemory (BoolMemory memory) {
			for (int i = 0; i < boolMemories.Count; ++i) {
				if (boolMemories[i].key == memory.key) {
					boolMemories[i].value = memory.value;
					return;
				}
			}
			boolMemories.Add(memory);
		}

		// Public Setters; available for other scripts outside of the dialogue system to use.
		public void SetInt (string name, int val) {
			this.SetMemory(new IntMemory(name, val));
		}
		public void SetString (string name, string val) {
			this.SetMemory(new StringMemory(name, val));
		}
		public void SetBool (string name, bool val) {
			this.SetMemory(new BoolMemory(name, val));
		}

		// Value Accessors
		public bool Contains (string key) {
			for (int i = 0; i < intMemories.Count; ++i) {
				if (intMemories[i].key == key) return true;
			}
			for (int i = 0; i < stringMemories.Count; ++i) {
				if (stringMemories[i].key == key) return true;
			}
			for (int i = 0; i < boolMemories.Count; ++i) {
				if (boolMemories[i].key == key) return true;
			}
			return false;
		}
		// Contains with an out value, giving the string of the value.
		public bool Contains (string key, out string val) {
			for (int i = 0; i < intMemories.Count; ++i) {
				if (intMemories[i].key == key) {
					val = intMemories[i].value.ToString();
					return true;
				}
			}
			for (int i = 0; i < stringMemories.Count; ++i) {
				if (stringMemories[i].key == key){
					val = stringMemories[i].value;
					return true;
				}
			}
			for (int i = 0; i < boolMemories.Count; ++i) {
				if (boolMemories[i].key == key) {
					val = boolMemories[i].value.ToString();
					return true;
				}
			}
			val = "";
			return false;
		}
		// Getters for specific types.
		public int GetIntMemoryValue (string key) {
			for (int i = 0; i < intMemories.Count; ++i) {
				if (intMemories[i].key == key) {
					return intMemories[i].value;
				}
			}
			return 0;
		}
		public string GetStringMemoryValue (string key) {
			for (int i = 0; i < stringMemories.Count; ++i) {
				if (stringMemories[i].key == key) {
					return stringMemories[i].value;
				}
			}
			return "";
		}
		public bool GetBoolMemoryValue (string key) {
			for (int i = 0; i < boolMemories.Count; ++i) {
				if (boolMemories[i].key == key) {
					return boolMemories[i].value;
				}
			}
			return false;
		}

		// Memory Comparators
		public bool CheckMemory (MemoryDestination check) {
			Type memoryCheck = check.GetType();
			if (memoryCheck == Utilities.memoryDestinationInt.GetType()) {
				return CheckIntMemory(check.memoryKey, ((MemoryDestinationInt)check).value, check.checkCode);
			} else if (memoryCheck == Utilities.memoryDestinationString.GetType()) {
				return CheckStringMemory(check.memoryKey, ((MemoryDestinationString)check).value);
			} else if (memoryCheck == Utilities.memoryDestinationBool.GetType()) {
				return CheckBoolMemory(check.memoryKey, ((MemoryDestinationBool)check).value);
			}
			return false;
		}
		public bool CheckIntMemory (string key, int value, string code) {
			if (!this.Contains(key)) return false;
			int memoryValue = GetIntMemoryValue(key);
			if (code == "<=") {
				return (memoryValue <= value);
			} else if (code == ">=") {
				return (memoryValue >= value);
			} else if (code == "=") {
				return (memoryValue == value);
			} else if (code == "<") {
				return (memoryValue < value);
			} else if (code == ">") {
				return (memoryValue > value);
			}
			return false;
		}
		public bool CheckStringMemory (string key, string value) {
			return (value == GetStringMemoryValue(key));
		}
		public bool CheckBoolMemory (string key, bool value) {
			return (value == GetBoolMemoryValue(key));
		}

		public string Save () {
			return JsonUtility.ToJson(this);
		}
		
	}


}