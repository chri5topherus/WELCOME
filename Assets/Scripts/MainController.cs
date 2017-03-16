using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


	//debug text
	public Text debugText;

	//path of backdrop
	private string absolutePath;
	//private string absolutePathTarget;
	private string absolutePathOfApp;
	private FileInfo[] files;
	private DirectoryInfo info;
	public Image backdropImg;

	//INI parser
	INIParser ini = new INIParser();

	private int nameCount;
	private int font;
	private string fontColorString;
	private Color fontColor;
	private int fontSize;
	private int delay;

	public Text mainText;

	private int nameMode;

	private String testFirstname = "Chris";
	private String testLastname = "Bösch";
	private String testName = "";


	// Use this for initialization
	void Start () {

		absolutePathOfApp = Application.dataPath;

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

		ini.Open(absolutePath + "/settings.txt");
		nameCount = Int32.Parse(ini.ReadValue("Welcome","nameCount", "1"));
		font = Int32.Parse(ini.ReadValue("Welcome","font", "1"));
		fontSize = Int32.Parse(ini.ReadValue ("Welcome", "fontSize", "24"));
		fontColorString = ini.ReadValue ("Welcome", "fontColor", "1");
		nameMode = Int32.Parse(ini.ReadValue ("Welcome", "nameMode", "1"));
		delay = Int32.Parse(ini.ReadValue ("Welcome", "delay", "1"));

		ini.Close ();
		setBackdrop ();

		setNameMode (nameMode);

		ColorUtility.TryParseHtmlString (fontColorString, out fontColor);
		mainText.color = fontColor;

		mainText.fontSize = fontSize;

		mainText.transform.localPosition = new Vector3(0F,Screen.height/2+100, 0F);
		StartCoroutine(startAnimation ());

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void setBackdrop() { 
		info = new DirectoryInfo (absolutePath);
		files = info.GetFiles ();

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

		
	private IEnumerator startAnimation() {
		yield return new WaitForSeconds (delay);

		iTween.MoveTo(mainText.gameObject,iTween.Hash(
			"position",new Vector3(0F,0F,0F),
			"easetype",iTween.EaseType.easeOutQuart,
			"time",3F));
	}

	private void setNameMode(int nameMode) { 

		switch (nameMode) {
		case 1:
			testName = testFirstname + " " + testLastname;
			break;
		case 2: 
			testName = testFirstname.Substring (0, 1) + ". " + testLastname;
			break; 
		case 3:
			testName = testFirstname + " " + testLastname.Substring (0, 1) + ".";
			break;
		}
			mainText.text = testName;
	}












}
