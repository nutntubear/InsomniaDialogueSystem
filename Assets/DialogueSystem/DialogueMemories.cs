using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InsomniaSystemTypes;


/*
	The purpose of this is to make it more open to a unity project as a whole rather just being an element of the 
	Dialogue System. It acts as this middle man So now it can be editied and moved form scene to scene without 
	needing to deal with the dialouge system monobehavior.
*/
public class DialogueMemories : MonoBehaviour
{

	public MemoryDictionary memories = new MemoryDictionary();

}
