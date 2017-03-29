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
	private int creditStart;
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
	private int secondRow;


	//CLOUD 
	private List<Vector2> listWithPositionValues = new List<Vector2>();
	private int currentCloudCounter = 0;
	private static System.Random rng = new System.Random();

	//MAIN TEXT OBJECT
	private Text mainText;

	public List<Text> listWithMainTextFontElements = new List<Text>();

	private iTween.EaseType ease;

	private Queue<GameObject> queue;
	public List<GameObject> allTextGameObjects = new List<GameObject> ();
	private int deleteValue = -1;

	public PusherReceiver pusherReceiver;
	public float yRotation = 0.0F;
	private int delayCounter = 0;
	private bool mouseReady = true;
	private float animationDelay;

	//CONNECTED
	public Image connectedImage;


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
		secondRow = Int32.Parse (ini.ReadValue ("Welcome", "secondRow", "0"));
		creditStart = Int32.Parse (ini.ReadValue ("Welcome", "creditstart", "0"));

		ini.Close ();

		/*

		nameCount = 2;
		font = 2;
		fontColorString = "#ffffff";
		smokeColorString="#06ff00";
		fontSize=100;
		nameMode = 1;
		animationMode = 1;
		animationEase = 2;
		animationDuration = 2;
		delay = 0;
		autoFadeOut = 0;
		smoke = 1;
*/



		//----------------------------
		//--------- BACKDROP ---------
		//----------------------------
		setBackdrop (absolutePath);


		//----------------------------
		//----------- FONT -----------
		//----------------------------
		if(animationMode == 3 || animationEase == 3 || animationMode == 4) 
			smoke = 0;
		setFont ();


		//----------------------------
		//----------- COLOR ----------
		//----------------------------
		ColorUtility.TryParseHtmlString (fontColorString, out fontColor);
		mainText.color = fontColor;

		ColorUtility.TryParseHtmlString (smokeColorString, out smokeColor);
		mainText.transform.Find ("WhiteSmoke").gameObject.GetComponent<ParticleSystem> ().startColor = smokeColor;


		//----------------------------
		//----------- CLOUD -----------
		//----------------------------
		generateCloudArray ();




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
			animationDelay = animationDuration;
			mainText.transform.localPosition = new Vector3(0F,Screen.height, 0F);
			break; 
		case 2: 
			animationDelay = animationDuration;
			mainText.transform.localPosition = new Vector3(0F,-Screen.height, 0F);
			break; 
		case 3: 
			animationDelay = animationDuration*2F;
			smoke = 0;
			mainText.transform.localPosition = new Vector3(0F,Screen.height, 0F);
			break;
		case 4: 
			nameCount = nameCount-1;
			if(nameCount < 0) 
				nameCount = 0;
			animationDelay = animationDuration;
			iTween.ScaleTo (mainText.gameObject, iTween.Hash ("" +
				"scale", new Vector3 (0.5F, 0.5F, 0.5F),
				"easetype", iTween.EaseType.linear,
				"time", 1));
			mainText.transform.localPosition = new Vector3(0F,Screen.height, 0F);
			break; 

		case 5:
			nameCount = nameCount-1;
			if(nameCount < 0) 
				nameCount = 0;
			mainText.transform.localPosition = new Vector3(0F,Screen.height, 0F);
			break; 
		}







		//hide mouse 
		Cursor.visible = false;

		StartCoroutine (loopWithDelay ());
		StartCoroutine (setConnected ()); 

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Space)) { 

			newPusherEvent (names [allTextGameObjects.Count % (names.Length/2),0], names[allTextGameObjects.Count % (names.Length/2),1], "", "", "Dr. Andreas Hagen");
			//generateNewName ();
		}

		if(Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0){
			if(mouseReady)
				StartCoroutine (onMouseMovement ());
		}

		if (pusherReceiver.newData) { 
			pusherReceiver.newData = false;
			newPusherEvent (pusherReceiver.firstname, pusherReceiver.lastname, pusherReceiver.gender, pusherReceiver.email, pusherReceiver.nameWithTitle);
		}

		//debugText.text = pusherReceiver.msg;

	}

	private void generateCloudArray() {

		for (int i = -2; i < 3; i++) {
			for (int j = -2; j < 3; j++) {
				listWithPositionValues.Add(new Vector2 
					(0 + i*Screen.width/5F + UnityEngine.Random.Range(-Screen.width/40,Screen.width/40), 
					0 + j*Screen.height/5F +UnityEngine.Random.Range(-Screen.width/30,Screen.width/30)));
			} 
		}

		randomize (listWithPositionValues);
	}

	private IEnumerator setConnected() {
		yield return new WaitForSeconds (0.15F);

		//check if connected
		if (pusherReceiver.connectedStatus) { 
			yRotation += 45F;
			connectedImage.transform.eulerAngles = new Vector3 (0, 0, yRotation);
		} else {
			
		}

		StartCoroutine (setConnected ()); 

	}

	private IEnumerator onMouseMovement() { 
		if (mouseReady == true) {
			mouseReady = false;
			connectedImage.CrossFadeAlpha (1F, 1F, false);

			//Debug.Log ("MOUSE");

			yield return new WaitForSeconds (4F);
			connectedImage.CrossFadeAlpha (0F, 1F, false);
			mouseReady = true;

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

		listWithMainTextFontElements [1].transform.Find ("WhiteSmoke").gameObject.transform.SetParent (mainText.transform);

		if (smoke == 0) {
			mainText.transform.Find ("WhiteSmoke").gameObject.SetActive (false);
		} else if (smoke == 1) {
			mainText.transform.Find ("WhiteSmoke").gameObject.SetActive (true);
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


	public void newPusherEvent(string firstname, string lastname, string gender, string email, string nameWithTitle) {
		
		generateNewName (firstname, lastname, nameWithTitle);

	}

	private String getNameString(string firstname, string lastname, string nameWithTitle) { 
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
		case 4: 
			createdName = nameWithTitle;
			break;
		}
		return createdName;
	}


	private IEnumerator loopWithDelay() {
		yield return new WaitForSeconds(animationDelay);

		if (queue.Count > 0) {
			if (listWithCreatedNamed.Count > nameCount) { 
				deleteValue++;
			}
			StartCoroutine (IN (queue.Dequeue ()));
		}
		StartCoroutine (loopWithDelay ());

	}

	private void generateNewName(string firstname, string lastname, string nameWithTitle) {

		bool duplicate = false;

		//check for duplicate
		foreach (GameObject go in allTextGameObjects) {
			if (go.GetComponent<Text> ().text == getNameString (firstname, lastname, nameWithTitle)) { 
				duplicate = true;
			}
		}


		//TODO remove this line if duplicate elemtes should be ignored
		duplicate = false;


		if (allTextGameObjects.Count == 0) {
			duplicate = false;
		}

		if (!duplicate) {
			GameObject newName = Instantiate (mainText.gameObject, textCanvas.transform);
			newName.GetComponent<Text> ().text = getNameString (firstname, lastname, nameWithTitle);

			//generate second row
			if (secondRow == 1) {
				GameObject newNameSub = Instantiate (mainText.gameObject, textCanvas.transform);
				newNameSub.GetComponent<Text> ().text = "hahaha";
				newNameSub.GetComponent<Text> ().fontSize =  (int) (fontSize * 0.5F);
				newNameSub.transform.SetParent (newName.transform);
				newNameSub.transform.localPosition = new Vector3 (0F, -fontSize * 0.75F, 0F);
				foreach(Transform child in newNameSub.transform) {
					Destroy(child.gameObject);
				}
			} 

			allTextGameObjects.Add (newName);

			if(allTextGameObjects.Count > creditStart)
				queue.Enqueue (newName);
		}
			

	}


	private IEnumerator IN(GameObject go) {
		yield return new WaitForSeconds (delay); 

		listWithCreatedNamed.Add (go);


		int translateSecondRow = 0; 
		if (secondRow == 1) { 
			translateSecondRow = fontSize/3; 
		}
	
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
				"easetype", iTween.EaseType.easeInOutExpo,
				"time", animationDuration));
			go.GetComponent<Text> ().CrossFadeAlpha (0F, 0F, false);
			go.transform.localPosition = new Vector3(0F,0F, 0F);
			go.GetComponent<Text> ().CrossFadeAlpha (1F, animationDuration, false);
			break; 

		//credit style
		case 4: 

			float newPosition = 0F;

			newPosition = fontSize * 0.75F * ((nameCount + 1F) / 2F) + translateSecondRow;	
			if (secondRow != 1)
				newPosition = newPosition - fontSize* 0.75F/2;

			Debug.Log (newPosition);

			iTween.MoveTo (go, iTween.Hash (
				"position", new Vector3 (0F, newPosition, 0F),
				"easetype", iTween.EaseType.easeInOutExpo,
				"time", animationDuration));

			break; 

		//cloud
		case 5: 
			go.transform.localScale = new Vector3 (0.6F, 0.6F, 0.6F);
			iTween.ScaleTo (go, iTween.Hash ("" +
			"scale", new Vector3 (1F, 1F, 1F),
				"easetype", iTween.EaseType.easeInOutBack,
				"time", animationDuration/2));
			go.GetComponent<Text> ().CrossFadeAlpha (0F, 0F, false);
			go.transform.localPosition = new Vector3 (listWithPositionValues [currentCloudCounter].x, listWithPositionValues [currentCloudCounter].y, 0F);
			go.GetComponent<Text> ().CrossFadeAlpha (1F, animationDuration, false);
			currentCloudCounter++; 
			if (currentCloudCounter > 24)
				currentCloudCounter = 0;
			break;
		}



	}


	private IEnumerator OUT(GameObject go, float delayOut) {
		yield return new WaitForSeconds (delayOut); 

		float translateValue = 0F;

		int translateSecondRow = 0; 

		if (go.GetComponent<RectTransform> ().localScale.x < 1F) { 


		}
		if (secondRow == 1) { 
			translateSecondRow = fontSize/3; 
		}


		switch (animationMode) {
		case 1:
			if (go.GetComponent<RectTransform> ().localScale.x < 1F) {
				translateValue = go.transform.localPosition.y - (fontSize / 3F) - translateSecondRow;
			} else {
				translateValue = go.transform.localPosition.y - fontSize - translateSecondRow;
				iTween.ScaleTo (go, iTween.Hash ("" +
				"scale", new Vector3 (0.25F, 0.25F, 0.25F),
					"easetype", iTween.EaseType.easeInOutQuad,
					"time", animationDuration));
			}
				
			iTween.MoveTo (go, iTween.Hash (
				"position", new Vector3 (0F, translateValue, 0F),
				"easetype", iTween.EaseType.easeOutQuart,
				"time", animationDuration));

			//delete value
			if (deleteValue > -1) {
				allTextGameObjects [deleteValue].GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration / 2, false);
				if (secondRow == 1) {
					allTextGameObjects [deleteValue].GetComponentsInChildren<Text> ()[1].CrossFadeAlpha (0F, animationDuration / 2, false);
				}

				StartCoroutine (deleteObjects (allTextGameObjects [deleteValue], animationDuration));
			}

			break; 
		case 2: 
			if (go.GetComponent<RectTransform> ().localScale.x < 1F) {
				translateValue = go.transform.localPosition.y + (fontSize / 3F) + translateSecondRow;
			} else {
				translateValue = go.transform.localPosition.y + fontSize + translateSecondRow;
				iTween.ScaleTo (go, iTween.Hash ("" +
				"scale", new Vector3 (0.25F, 0.25F, 0.25F),
					"easetype", iTween.EaseType.easeInOutQuad,
					"time", animationDuration));
			}

			iTween.MoveTo (go, iTween.Hash (
				"position", new Vector3 (0F, translateValue, 0F),
				"easetype", iTween.EaseType.easeOutQuart,
				"time", animationDuration));

			//delete value
			if (deleteValue > -1) {
				allTextGameObjects [deleteValue].GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration / 2, false);
				if (secondRow == 1) {
					allTextGameObjects [deleteValue].GetComponentsInChildren<Text> ()[1].CrossFadeAlpha (0F, animationDuration / 2, false);
				}

				StartCoroutine (deleteObjects (allTextGameObjects [deleteValue], animationDuration));
			}

			break;
		case 3:
			
			if (go.GetComponent<RectTransform> ().localScale.x < 1F) {
				translateValue = go.transform.localPosition.y - (fontSize / 3F) - translateSecondRow;
			} else {
				translateValue = go.transform.localPosition.y - fontSize - translateSecondRow;
				go.GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration, false);
			}
				
			yield return new WaitForSeconds (animationDuration); 
			go.transform.localScale = new Vector3 (0.25F, 0.25F, 0.25F);

			iTween.MoveTo (go, iTween.Hash (
				"position", new Vector3 (0F, translateValue, 0F),
				"easetype", iTween.EaseType.easeOutQuart,
				"time", animationDuration));

			//go.transform.localPosition = new Vector3 (0F, translateValue, 0F);

			go.GetComponent<Text> ().CrossFadeAlpha (1F, animationDuration, false);

			//deleteValue
			if (deleteValue > -1)
				//allTextGameObjects [deleteValue].GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration / 2, false);
				StartCoroutine (deleteObjects (allTextGameObjects [deleteValue], 0F));
			break;


			//credit style
		case 4: 

			translateValue = go.transform.localPosition.y - (fontSize * 0.75F) - translateSecondRow;

			iTween.MoveTo (go, iTween.Hash (
				"position", new Vector3 (0F, translateValue, 0F),
				"easetype", iTween.EaseType.easeOutQuart,
				"time", animationDuration));

			//delete value
			if (deleteValue > -1) {
				allTextGameObjects [deleteValue+creditStart].GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration / 2, false);
				if (secondRow == 1) {
					allTextGameObjects [deleteValue+creditStart].GetComponentsInChildren<Text> ()[1].CrossFadeAlpha (0F, animationDuration / 2, false);
				}

				StartCoroutine (deleteObjects (allTextGameObjects [deleteValue+creditStart], animationDuration));
			}

			break; 

		case 5:


			if (deleteValue > -1) {
				iTween.ScaleTo (allTextGameObjects [deleteValue+creditStart].gameObject, iTween.Hash ("" +
					"scale", new Vector3 (0.6F, 0.6F, 0.6F),
					"easetype", iTween.EaseType.easeInOutQuad,
					"time", animationDuration));
				allTextGameObjects [deleteValue+creditStart].GetComponent<Text> ().CrossFadeAlpha (0F, animationDuration * 0.75F, false);
				if (secondRow == 1) {
					allTextGameObjects [deleteValue+creditStart].GetComponentsInChildren<Text> ()[1].CrossFadeAlpha (0F, animationDuration / 2, false);
				}

				StartCoroutine (deleteObjects (allTextGameObjects [deleteValue+creditStart], animationDuration));
			}




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

	private void randomize (List<Vector2> list)
	{  
		int n = list.Count;  
		while (n > 1) {  
			n--;  
			int k = rng.Next (n + 1);  
			Vector2 value = list [k];  
			list [k] = list [n];  
			list [n] = value;  
		}  
	}


}
