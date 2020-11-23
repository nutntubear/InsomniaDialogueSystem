using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using SFB;

public class SaveLoad : MonoBehaviour
{

	[HideInInspector]
	public static SaveLoad instance;

	void Awake () {
		// Singleton
		if (instance == null) {
			instance = this;
		} else {
			Destroy(this.gameObject);
		}
		// 
		DontDestroyOnLoad(this.gameObject);
	}

	public string[] loadingFile = null;

	public void OpenInsomniaFile () {
		string[] paths = StandaloneFileBrowser.OpenFilePanel("Open Insomnia System File", "", "json", false);
		if (paths.Length == 1) {
			loadingFile = File.ReadAllLines(paths[0]);
			// Load the scene again, resetting all values in the editor, but keeping the loaded file.
			SceneManager.LoadScene("main");
		}
	}

}
