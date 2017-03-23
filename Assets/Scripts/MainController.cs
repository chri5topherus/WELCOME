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
	private string smokeColorString;
	private Color smokeColor;
	private int fontSize;
	private int delay;
	private int nameMode;
	private int animationMode;
	private int animationEase;
	private int animationDuration;
	private int autoFadeOut;
	private int smoke;

	//MAIN TEXT OBJECT
	private Text mainText;
	public Text mainTextFont01;
	public Text mainTextFont02;
	public Text mainTextFont03;
	public Text mainTextFont04;

	public List<Text> listWithMainTextFontElements = new List<Text>();

	private iTween.EaseType ease;

	private Queue<GameObject> queue;
	private List<GameObject> allTextGameObjects = new List<GameObject> ();
	private int deleteValue = -1;

	public PusherReceiver pusherReceiver;

	//TESTING
	private String[,] names = new String[,] { { "Chris", "Boesch" }, { "Michael", "Mair" }, { "Andrea", "Hagen" }, { "Tim", "Turbo"}, { "Alex", "Fuerst"}, { "Manfred", "Hofer"}, { "Klaus", "Igel"} };


	// Use this for initialization
	void Start () {

		queue = new Queue<GameObject> ();

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
				absolutePath = absolutePathOfApp.Substring(0,absolutePathOfApp.Length-Application.productName.Length+2) + "/settings";
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
		font = (Int32.Parse (ini.ReadValue ("Welcome", "font", "1"))) - 1;
		fontSize = Int32.Parse(ini.ReadValue ("Welcome", "fontSize", "100"));
		fontColorString = ini.ReadValue ("Welcome", "fontColor", "1");
		smokeColorString = ini.ReadValue ("Welcome", "smokeColor", "1");
		nameMode = Int32.Parse(ini.ReadValue ("Welcome", "nameMode", "1"));
		animationMode = Int32.Parse(ini.ReadValue ("Welcome", "animationMode", "1"));
		animationDuration = Int32.Parse(ini.ReadValue ("Welcome", "animationDuration", "1"));
		delay = Int32.Parse(ini.ReadValue ("Welcome", "delay", "1"));
		autoFadeOut = Int32.Parse(ini.ReadValue ("Welcome", "autoFadeOut", "1"));
		smoke = Int32.Parse (ini.ReadValue ("Welcome", "smoke", "0"));
		animationEase = Int32.Parse (ini.ReadValue ("Welcome", "animationEase", "1"));


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
	
		ColorUtility.TryParseHtmlString (smokeColorString, out smokeColor);
		mainText.transform.Find ("WhiteSmoke").gameObject.GetComponent<ParticleSystem> ().startColor = smokeColor;



		//----------------------------
		//----------- EASE -----------
		//----------------------------
		setEase ();

		//----------------------------
		//----------- SIZE -----------
		//----------------------------
		mainText.fontSize = fontSize;
		RectTransform rectTransform = mainText.GetComponent (typeof (RectTransform)) as RectTransform;
		rectTransform.sizeDelta = new Vector2 (Screen.width, Screen.height);

		//----------------------------
		//------- CLEAR SCREEN -------
		//----------------------------
		//Debug.Log(animationMode);
		switch (animationMode) {
		case 1: 
			mainText.transform.localPosition = new Vector3(0F,Screen.height, 0F);
			break; 
		case 2: 
			mainText.transform.localPosition = new Vector3(0F,-Screen.height, 0F);
			break; 
		case 3: 
			mainText.transform.localPosition = new Vector3(0F,Screen.height, 0F);
			break;

		}

		StartCoroutine (loopWithDelay ());
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) { 
			newPusherEvent (names [allTextGameObjects.Count,0], names[allTextGameObjects.Count,1], "", "");
			//generateNewName ();
		}

		if (pusherReceiver.newData) { 
			pusherReceiver.newData = false;
			newPusherEvent (pusherReceiver.firstname, pusherReceiver.lastname, pusherReceiver.gender, pusherReceiver.email);
		}

	}

	private void setEase() {
		switch (animationEase) { 
		case 1: 
			ease = iTween.EaseType.easeOutSine;
			break; 
		case 2: 
			ease = iTween.EaseType.easeOutQuart;
			break; 
		case 3: 
			ease = iTween.EaseType.easeInOutBounce;
			break;
		}
	}




	private void setFont() {

		foreach (Text txt in listWithMainTextFontElements) {
			txt.gameObject.SetActive (false);
		}

		mainText = listWithMainTextFontElements[font];
		mainText.gameObject.SetActive (true);



		if (smoke == 0)
			mainText.transform.Find ("WhiteSmoke").gameObject.SetActive (false);
		else if (smoke == 1) 
			mainText.transform.Find ("WhiteSmoke").gameObject.SetActive (true);

		

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


	public void newPusherEvent(string firstname, string lastname, string gender, string email) {
		generateNewName (firstname, lastname);

	}

	private String getNameString(string firstname, string lastname) { 
		String createdName = "";
		switch (nameMode) {
		case 1:
			createdName = firstname + " " + lastname;
			break;
		case 2: 
			createdName = firstname.Substring (0, 1) + ". " + lastname;
			break; 
		case 3:
			createdName = firstname + " " + lastname.Substring (0, 1) + ".";
			break;
		}
		return createdName;
	}


	private IEnumerator loopWithDelay() {
		yield return new WaitForSeconds(animationDuration * animationMode);
		if (queue.Count > 0) {
			if (listWithCreatedNamed.Count > nameCount) { 
				deleteValue++;
			}
			StartCoroutine (IN (queue.Dequeue ()));
		}
		StartCoroutine (loopWithDelay ());

	}

	private void generateNewName(string firstname, string lastname) {

		bool duplicate = false;

		//check for duplicate
		foreach (GameObject go in allTextGameObjects) {
			if (go.GetComponent<Text> ().text == getNameString (firstname, lastname)) { 
				duplicate = true;
			}
		}


		//TODO remove this line: 
		duplicate = false;


		if (allTextGameObjects.Count == 0) {
			duplicate = false;
		}

		if (!duplicate) {
			GameObject newName = Instantiate (mainText.gameObject, textCanvas.transform);
			newName.GetComponent<Text> ().text = getNameString (firstname, lastname);
			queue.Enqueue (newName);
			allTextGameObjects.Add (newName);

		}



	}


	private IEnumerator IN(GameObject go) {
		yield return new WaitForSeconds (delay); 

		listWithCreatedNamed.Add (go);
	
		if (autoFadeOut == 0) {
			for (int i = 0; i < listWithCreatedNamed.Count - 1; i++) {
				StartCoroutine( OUT (listWithCreatedNamed [i], 0F));
			}
		} else { 
			for (int i = 0; i < listWithCreatedNamed.Count; i++) {
				StartCoroutine( OUT (listWithCreatedNamed [i], autoFadeOut + animationDuration));
			}
		}




		switch (animationMode) {
		case 1: case 2:

			iTween.MoveTo (go, iTween.Hash (
					"position", new Vector3 (0F, 0F, 0F),
					"easetype", ease,
					"time", animationDuration));

			if(animationEase == 3)
				yield return new WaitForSeconds (animationDuration*0.75F); 
			else if(animationEase == 2) 
				yield return new WaitForSeconds (animationDuration*0.4F); 
			else if(animationEase == 1) 
				yield return new WaitForSeconds (animationDuration*0.75F); 
			stopSmoke (go);
			break; 

		case 3: 
			yield return new WaitForSeconds (animationDuration*0.75F); 
			go.transform.localScale = new Vector3 (0.6F, 0.6F, 0.6F);
			iTween.ScaleTo (go, iTween.Hash ("" +
				"scale", new Vector3 (1F, 1F, 1F),
				"easetype", iTween.EaseType.easeOutExpo,
				"time", animationDuration));
			go.GetComponent<Text> ().CrossFadeAlpha (0F, 0F, false);
			go.transform.localPosition = new Vector3(0F,0F, 0F);
			go.GetComponent<Text> ().CrossFadeAlpha (1F, animationDuration, false);
			break; 
		}


	}


	private IEnumerator OUT(GameObject go, float delayOut) {
		yield return new WaitForSeconds (delayOut); 

		float translateValue = 0F;


		switch (animationMode) {
		case 1:
			if (go.GetComponent<RectTransform> ().localScale.x < 1F) {
				translateValue = go.transform.localPosition.y - (fontSize / 3F);
			} else {
				translateValue = go.transform.localPosition.y - fontSize;
				iTween.ScaleTo (go, iTween.Hash ("" +
				"scale", new Vector3 (0.25F, 0.25F, 0.25F),
					"easetype", iTween.EaseType.easeOutExpo,
					"time", animationDuration));
			}
				
			iTween.MoveTo (go, iTween.Hash (
				"position", new Vector3 (0F, translateValue, 0F),
				"easetype", iTween.EaseType.easeOutQuart,
				"time", animationDuration));

			//delete value
			if (deleteValue > -1) {
				allTextGameObjects [deleteValue].GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration / 2, false);
				StartCoroutine (deleteObjects (allTextGameObjects [deleteValue], animationDuration));
			}

			break; 
		case 2: 
			if (go.GetComponent<RectTransform> ().localScale.x < 1F) {
				translateValue = go.transform.localPosition.y + (fontSize / 3F);
			} else {
				translateValue = go.transform.localPosition.y + fontSize;
				iTween.ScaleTo (go, iTween.Hash ("" +
				"scale", new Vector3 (0.25F, 0.25F, 0.25F),
					"easetype", iTween.EaseType.easeOutExpo,
					"time", animationDuration));
			}

			iTween.MoveTo (go, iTween.Hash (
				"position", new Vector3 (0F, translateValue, 0F),
				"easetype", iTween.EaseType.easeOutQuart,
				"time", animationDuration));

			//delete value
			if (deleteValue > -1) {
				allTextGameObjects [deleteValue].GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration / 2, false);
				StartCoroutine (deleteObjects (allTextGameObjects [deleteValue], animationDuration));
			}

			break;
		case 3:
			
			if (go.GetComponent<RectTransform> ().localScale.x < 1F) {
				translateValue = go.transform.localPosition.y - (fontSize / 3F);
			} else {
				translateValue = go.transform.localPosition.y - fontSize;
				go.GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration, false);
			}
				
			yield return new WaitForSeconds (animationDuration); 
			go.transform.localScale = new Vector3 (0.25F, 0.25F, 0.25F);
			go.transform.localPosition = new Vector3 (0F, translateValue, 0F);

			go.GetComponent<Text> ().CrossFadeAlpha (1F, animationDuration, false);

			//deleteValue
			if (deleteValue > -1)
				StartCoroutine (deleteObjects (allTextGameObjects [deleteValue], 0F));
			
			break;
		}
	}

	private IEnumerator deleteObjects(GameObject go, float delay) {
		yield return new WaitForSeconds (delay); 
		go.SetActive (false);
	}

	private IEnumerator waitWithFade(float delayOut, GameObject go, float translateValue) {
		yield return new WaitForSeconds (delayOut); 
		go.transform.localScale = new Vector3 (0.25F, 0.25F, 0.25F);
		iTween.MoveTo(go,iTween.Hash(
			"position",new Vector3(0F, translateValue, 0F),
			"easetype",ease,
			"time",animationDuration));
		go.GetComponent<Text> ().CrossFadeAlpha (1F, animationDuration, false);
	}

		
	private void stopSmoke(GameObject go) {
		ParticleSystem ps = go.transform.Find("WhiteSmoke").gameObject.GetComponent<ParticleSystem>();
		ParticleSystem.EmissionModule em = ps.emission;
		em.enabled = false;
	}










}
