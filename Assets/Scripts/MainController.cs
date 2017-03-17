using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

public class MainController : MonoBehaviour {


	//DEBUG TEXT
	public Text debugText;

	//BACKDROP
	public Image backdropImg;

	//INI parser
	INIParser ini = new INIParser();

	//TEXT CANVAS
	public Canvas textCanvas;
	public List<GameObject> listWithCreatedNamed = new List<GameObject> ();
	private Rect screenSizeRect;

	//SETTINGS
	private int nameCount;
	private int font;
	private string fontColorString;
	private Color fontColor;
	private int fontSize;
	private int delay;
	private int nameMode;
	private int animationMode;
	private int animationDuration;

	//MAIN TEXT OBJECT
	private Text mainText;
	public Text mainTextOpenSansBold;
	public Text mainTextOpenSansLight;
	public Text mainTextPressStart2P;
	public Text mainTextLewis;




	//TESTING
	private String[,] names = new String[,] { { "Chris", "Boesch" }, { "Michael", "Mair" }, { "Andrea", "Hagen" }, { "Tim", "Turbo"} };
	private int currentNameCounter = 0;



	// Use this for initialization
	void Start () {

		string absolutePathOfApp = Application.dataPath;
		string absolutePath = "";

		if (Application.isEditor) {
			absolutePath = absolutePathOfApp + "/Resources/Settings";
		} else {
			#if UNITY_STANDALONE_OSX
			absolutePath = absolutePathOfApp.Substring (0, absolutePathOfApp.Length - Application.productName.Length-6) + "settings";
			//debugTxt.text = "path:" + absolutePath;
			//absolutePath = absolutePathOfApp.Substring(0,absolutePathOfApp.Length-21) + "watching";
			//absolutePathTarget = absolutePathOfApp.Substring(0,absolutePathOfApp.Length-21) + "loaded";
			//absolutePath = ini.ReadValue("Path","osx","test");
			//absolutePathTarget = ini.ReadValue("Path","osxtarget","test");
			#endif

			#if UNITY_STANDALONE_WIN
				absolutePath = absolutePathOfApp.Substring(0,absolutePathOfApp.Length-Application.productName.Length+3) + "settings";
				//debugTxt.text = absolutePath;
				//absolutePathTarget = absolutePathOfApp.Substring(0,absolutePathOfApp.Length-13) + "loaded";
				//absolutePath = ini.ReadValue("Path","win","test");
				//absolutePathTarget = ini.ReadValue("Path","wintarget","test");
			#endif

		}

		debugText.text = absolutePath;


		//----------------------------
		//------------ INI -----------
		//----------------------------
		ini.Open(absolutePath + "/settings.txt");
		nameCount = Int32.Parse(ini.ReadValue("Welcome","nameCount", "1"));
		font = Int32.Parse(ini.ReadValue("Welcome","font", "1"));
		fontSize = Int32.Parse(ini.ReadValue ("Welcome", "fontSize", "100"));
		fontColorString = ini.ReadValue ("Welcome", "fontColor", "1");
		nameMode = Int32.Parse(ini.ReadValue ("Welcome", "nameMode", "1"));
		animationMode = Int32.Parse(ini.ReadValue ("Welcome", "animationMode", "1"));
		animationDuration = Int32.Parse(ini.ReadValue ("Welcome", "animationDuration", "1"));
		delay = Int32.Parse(ini.ReadValue ("Welcome", "delay", "1"));
		ini.Close ();

		//----------------------------
		//--------- BACKDROP ---------
		//----------------------------
		setBackdrop (absolutePath);

		//----------------------------
		//----------- FONT -----------
		//----------------------------
		setFont ();

		//----------------------------
		//----------- COLOR ----------
		//----------------------------
		ColorUtility.TryParseHtmlString (fontColorString, out fontColor);
		mainText.color = fontColor;

		//----------------------------
		//----------- SIZE -----------
		//----------------------------
		mainText.fontSize = fontSize;
		RectTransform rectTransform = mainText.GetComponent (typeof (RectTransform)) as RectTransform;
		rectTransform.sizeDelta = new Vector2 (Screen.width, Screen.height);

		//----------------------------
		//------- CLEAR SCREEN -------
		//----------------------------
		mainText.transform.localPosition = new Vector3(0F,Screen.height, 0F);

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) { 
			generateNewName ();
		}
	}

	private void setFont() {
		switch (font) {
		case 1:
			mainText = mainTextOpenSansBold;
			mainTextOpenSansLight.gameObject.SetActive (false);
			mainTextPressStart2P.gameObject.SetActive (false);
			mainTextLewis.gameObject.SetActive (false);
			break; 
		case 2: 
			mainText = mainTextOpenSansLight;
			mainTextOpenSansBold.gameObject.SetActive (false);
			mainTextPressStart2P.gameObject.SetActive (false);
			mainTextLewis.gameObject.SetActive (false);
			break; 
		case 3: 
			mainText = mainTextPressStart2P;
			mainTextOpenSansLight.gameObject.SetActive (false);
			mainTextOpenSansBold.gameObject.SetActive (false);
			mainTextLewis.gameObject.SetActive (false);
			break; 
		case 4: 
			mainText = mainTextLewis;
			mainTextOpenSansLight.gameObject.SetActive (false);
			mainTextOpenSansBold.gameObject.SetActive (false);
			mainTextPressStart2P.gameObject.SetActive (false);
			break; 

		}

	}

	private void setBackdrop(string absolutePath) { 
		DirectoryInfo info = new DirectoryInfo (absolutePath);
		FileInfo[] files = info.GetFiles ();

		foreach (FileInfo f in files) {
			if (validFileType (f.Name)) {
				WWW www = new WWW ("file://" + f.FullName);
				Texture2D textureTmp = www.texture;
				Sprite spriteTmp = Sprite.Create (textureTmp, new Rect (0, 0, textureTmp.width, textureTmp.height), new Vector2 (0, 0));
				backdropImg.sprite = spriteTmp;
			}
		}
	}

	bool validFileType(string filename) {
		if (filename.IndexOf ("meta") < 0 && filename.IndexOf ("~") < 0 && (filename.IndexOf ("jpg") > 0 || filename.IndexOf ("png") > 0))
			return true;
		return false;
	}


	private String getNameString(int currentName) { 
		String createdName = "";
		switch (nameMode) {
		case 1:
			createdName = names[currentName,0] + " " + names[currentName,1];
			break;
		case 2: 
			createdName = names[currentName,0].Substring (0, 1) + ". " + names[currentName,1];
			break; 
		case 3:
			createdName = names[currentName,0] + " " + names[currentName,1].Substring (0, 1) + ".";
			break;
		}
		return createdName;
	}


	private void generateNewName() {
		GameObject newName = Instantiate (mainText.gameObject,textCanvas.transform);
		listWithCreatedNamed.Add (newName);
		newName.GetComponent<Text> ().text = getNameString (currentNameCounter);
		StartCoroutine (IN (newName));
		currentNameCounter++;
	}


	private IEnumerator IN(GameObject go) {
		yield return new WaitForSeconds (delay); 

		switch (animationMode) {
		case 1:
			iTween.MoveTo(go,iTween.Hash(
				"position",new Vector3(0F,0F,0F),
				"easetype",iTween.EaseType.easeOutQuart,
				"time",animationDuration));
			break; 
		case 2:
			go.transform.localScale = new Vector3 (0.6F, 0.6F, 0.6F);
			iTween.ScaleTo (go, iTween.Hash ("" +
				"scale", new Vector3 (1F, 1F, 1F),
				"easetype", iTween.EaseType.easeOutExpo,
				"time", animationDuration));
			go.GetComponent<Text> ().CrossFadeAlpha (0F, 0F, false);
			go.transform.localPosition = new Vector3(0F,0F, 0F);
			go.GetComponent<Text> ().CrossFadeAlpha (1F, animationDuration, false);
			break; 


		case 3: 
			break;

		}



	

	}

	private IEnumerator STAY() {
		yield return new WaitForSeconds (0F); 

	}



	private IEnumerator OUT() {
		yield return new WaitForSeconds (0F); 

	}












}
