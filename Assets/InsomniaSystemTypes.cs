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
		Has acces to Node data, used for sorting nodes.
	*/
	public class Utilities {

		public static int SortNode (Node a, Node b) {
			return a.id.CompareTo(b.id);
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
		public int id; //ID is used to locate a particular node when it is asked for
		public char type;
		public string body;
		public string speaker;
		public bool player;

		/*
		All data is excepted as a type T so once we identify what type the data is we then 
		store it in a list of that approipate type
		*/
		// Lists of Dialogue Events
		// Store keys that hold parameters to trigger normal unity events
		public List<DialogueEvent> events;
		public List< DialogueEventTemplated<int> > intEvents;
		public List< DialogueEventTemplated<string> > stringEvents;
		public List< DialogueEventTemplated<bool> > boolEvents;
		public int evTotal;
		// Lists of Destinations 
		//Dialogue nodes that are connected to this one
		public List<Destination> destinations;
		public List< MemoryDestination<int> > intDestinations;
		public List< MemoryDestination<string> > stringDestinations;
		public List< MemoryDestination<bool> > boolDestinations;
		public int destTotal;
		// Lists of Memories
		//Stores player data like, items held, amount of money the player has, past dialogue
		//choices any sort of game data that would be referenced to trigger certain dialogue events
		public List< Memory<int> > intMemories;
		public List< Memory<string> > stringMemories;
		public List< Memory<bool> > boolMemories;
		public int memTotal;
		public string spareData;
		// Used for visual file writer.
		public Vector2 position;

		public Node () {
			type = 'e';
			player = false;
			// Set up every single different Dialogue Event, Destination, and Memory type.
			events = new List<DialogueEvent>();
			intEvents = new List< DialogueEventTemplated<int> >();
			stringEvents = new List< DialogueEventTemplated<string> >();
			boolEvents = new List< DialogueEventTemplated<bool> >();
			evTotal = 0;
			destinations = new List<Destination>();
			intDestinations = new List< MemoryDestination<int> >();
			stringDestinations = new List< MemoryDestination<string> >();
			boolDestinations = new List< MemoryDestination<bool> >();
			destTotal = 0;
			intMemories = new List< Memory<int> >();
			stringMemories = new List< Memory<string> >();
			boolMemories = new List< Memory<bool> >();
			memTotal = 0;
		}

		public bool HasDestination (int dest) {
			if (dest == id) return true;
			for (int i = 0; i < destinations.Count; ++i) {
				if (destinations[i].dest == dest) return true;
			}
			for (int i = 0; i < intDestinations.Count; ++i) {
				if (intDestinations[i].dest == dest) return true;
			}
			for (int i = 0; i < stringDestinations.Count; ++i) {
				if (stringDestinations[i].dest == dest) return true;
			}
			for (int i = 0; i < boolDestinations.Count; ++i) {
				if (boolDestinations[i].dest == dest) return true;
			}
			return false;
		}

		// SetType sets the char type variable. Run when producing the JSON dialogue file.
		void SetType () {
			if (destTotal == 0) {
				type = 'e';
			} else if (destTotal == 1) {
				type = 'n';
			} else {
				type = 'b';
			}
		}
		// Returns a JSON formatted string containing the Node information.
		public string SaveNode () {
			SetType();
			return JsonUtility.ToJson(this);
		}
		//Takes Dialogue from the node an outputs in to the TextBox
		public string ToTextBox () {
			string line1 = id.ToString() + " - ";
			line1 += speaker.Substring(0, (int)Mathf.Clamp(11 - line1.Length, 0, speaker.Length));
			string line2;
			if (memTotal > 9) line2 = "9+ Memories";
			else line2 = memTotal.ToString() + " Memories";
			string line3;
			if (evTotal > 9) line3 = "9+ Events";
			else line3 = evTotal.ToString() + " Events";
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
		for other purposes in-game. These take the keys from the DialogueEvent
		lists and trigger them with these event pairs.
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
		Dialogue Events are not actual unity events they store keys that would then be called
		to trigger actual unity events
	*/
	[System.Serializable]
	public class DialogueEvent {
		public string key;
		// The id is used for ordering events in the file writer.
		public int id;

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

        public virtual string GetTemplatedType () {
			return "NONE";
		}

    }
	[System.Serializable]
	public class DialogueEventTemplated<T> : DialogueEvent {
		public T parameter;

		public DialogueEventTemplated (string key_, T param) {
			key = key_;
			parameter = param;
		}

		public DialogueEventTemplated (DialogueEvent c) {
			key = c.key;
			if (c.GetType() == this.GetType()) {
				parameter = ((DialogueEventTemplated<T>)c).parameter;
			}
		}

		public override string GetTemplatedType () {
			// Returns a string that indicates the type. For the base 3 types used in the DS:
			// int = Int3d
			// string = String
			// bool = Boolean
			// base = NONE
			string[] type = this.GetType().GetGenericArguments()[0].ToString().Split('.');
			return type[type.Length - 1];
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
		// The id is used for ordering destinations in the file writer.
		public int id;

		public Destination (int dest_=-1, int id=0) {
			dest = dest_;
		}

		public virtual string GetTemplatedType () {
			return "NONE";
		}

    }
	[System.Serializable]
	public class MemoryDestination<T> : Destination {
		public string memoryKey;
		public T value;
		public string checkCode = "eq";
		public bool forced = false;

		public MemoryDestination (int dest_, int id_, string key="", T val=default(T), string check="=", bool force=false) {
			dest = dest_;
			id = id_;
			memoryKey = key;
			value = val;
			checkCode = check;
			forced = force;
		}

		public MemoryDestination (Destination c) {
			dest = c.dest;
			id = c.id;
			if (c.GetType() == this.GetType()) {
				memoryKey = ((MemoryDestination<T>)c).memoryKey;
				value = ((MemoryDestination<T>)c).value;
				checkCode = ((MemoryDestination<T>)c).checkCode;
				forced = ((MemoryDestination<T>)c).forced;
			}
		}

		public override string GetTemplatedType () {
			// Returns a string that indicates the type. For the base 3 types used in the DS:
			// int = Int3d
			// string = String
			// bool = Boolean
			// base = NONE
			string[] type = this.GetType().GetGenericArguments()[0].ToString().Split('.');
			return type[type.Length - 1];
		}
	}

	/*
	Memories
		Contain information that can be saved and checked against.
		Int, String, and Bool memories are used to save different types of data.
		The type Memory inherits, MemoryBase, allows for the file writer to use one variable for any templated Memory.
	*/
	[System.Serializable]
	public class MemoryBase {
		public string key;
		// Code to change existing memories; can be "set", "+", or "-", depending on templated type.
		public string operation;
		// The id is used for ordering memories in the file writer.
		public int id;

		public virtual string GetTemplatedType () {
			return "NONE";
		}
	}
	[System.Serializable]
	public class Memory<T> : MemoryBase {
		public T value;

		public Memory (string k, T v, string op="set") {
			key = k;
			value = v;
			operation = op;
		}

		public Memory (MemoryBase c) {
			key = c.key;
			operation = "set";
			id = c.id;
		}

		public override string GetTemplatedType () {
			// Returns a string that indicates the type. For the base 3 types used in the DS:
			// int = Int3d
			// string = String
			// bool = Boolean
			// Unlike the other GetTemplatedTypes, this should NEVER return "NONE".
			string[] type = this.GetType().GetGenericArguments()[0].ToString().Split('.');
			return type[type.Length - 1];
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
		public List< Memory<int> > intMemories;
		public List< Memory<string> > stringMemories;
		public List< Memory<bool> > boolMemories;

		// Constructor
		public MemoryDictionary () {
			intMemories = new List< Memory<int> >();
			stringMemories = new List< Memory<string> >();
			boolMemories = new List< Memory<bool> >();
		}

		// Type Based Memory Setters
		public void SetMemoryInt (Memory<int> memory) {
			for (int i = 0; i < intMemories.Count; ++i) {
				if (intMemories[i].key == memory.key) {
					if (memory.operation == "set") {
						intMemories[i].value = memory.value;
					} else if (memory.operation == "+") {
						intMemories[i].value += memory.value;
					} else if (memory.operation == "-") {
						intMemories[i].value -= memory.value;
					}
					return;
				}
			}
			intMemories.Add(memory);
		}
		public void SetMemoryString (Memory<string> memory) {
			for (int i = 0; i < stringMemories.Count; ++i) {
				if (stringMemories[i].key == memory.key) {
					if (memory.operation == "set") {
						stringMemories[i].value = memory.value;
					} else if (memory.operation == "+") {
						stringMemories[i].value += memory.value;
					}
					return;
				}
			}
			stringMemories.Add(memory);
		}
		public void SetMemoryBool (Memory<bool> memory) {
			for (int i = 0; i < boolMemories.Count; ++i) {
				if (boolMemories[i].key == memory.key) {
					// Do not check the memory's operation, as booleans are set only.
					boolMemories[i].value = memory.value;
					return;
				}
			}
			boolMemories.Add(memory);
		}

		// Public Setters; available for other scripts outside of the dialogue system to use.
		public void SetInt (string name, int val, string operation="set") {
			this.SetMemoryInt(new Memory<int>(name, val, operation));
		}
		public void SetString (string name, string val, string operation="set") {
			this.SetMemoryString(new Memory<string>(name, val, operation));
		}
		public void SetBool (string name, bool val, string operation="set") {
			this.SetMemoryBool(new Memory<bool>(name, val, operation));
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
		public int GetMemoryIntValue (string key) {
			for (int i = 0; i < intMemories.Count; ++i) {
				if (intMemories[i].key == key) {
					return intMemories[i].value;
				}
			}
			return 0;
		}
		public string GetMemoryStringValue (string key) {
			for (int i = 0; i < stringMemories.Count; ++i) {
				if (stringMemories[i].key == key) {
					return stringMemories[i].value;
				}
			}
			return "";
		}
		public bool GetMemoryBoolValue (string key) {
			for (int i = 0; i < boolMemories.Count; ++i) {
				if (boolMemories[i].key == key) {
					return boolMemories[i].value;
				}
			}
			return false;
		}

		// Memory Comparators
		public bool CheckMemoryInt (MemoryDestination<int> memory) {
			if (!this.Contains(memory.memoryKey)) return false;
			int memoryValue = GetMemoryIntValue(memory.memoryKey);
			if (memory.checkCode == "<=") {
				return (memoryValue <= memory.value);
			} else if (memory.checkCode == ">=") {
				return (memoryValue >= memory.value);
			} else if (memory.checkCode == "=") {
				return (memoryValue == memory.value);
			} else if (memory.checkCode == "<") {
				return (memoryValue < memory.value);
			} else if (memory.checkCode == ">") {
				return (memoryValue > memory.value);
			}
			return false;
		}
		public bool CheckMemoryString (MemoryDestination<string> memory) {
			return (memory.value == GetMemoryStringValue(memory.memoryKey));
		}
		public bool CheckMemoryBool (MemoryDestination<bool> memory) {
			return (memory.value == GetMemoryBoolValue(memory.memoryKey));
		}

		public string Save () {
			return JsonUtility.ToJson(this);
		}
		
	}


}