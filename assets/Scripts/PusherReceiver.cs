using UnityEngine;
using System.Text;
using UnityEngine.UI;
using System.Collections;

public class PusherReceiver : MonoBehaviour {
	// isntance of pusher client
	PusherClient.Pusher pusherClient = null;
	PusherClient.Channel pusherChannel = null;
	//public GameObject mainGameController;
	//public MainController mainController;
	private string pusherString;

	//data values
	public string lastname; 
	public string nameWithTitle; 
	public string firstname; 
	public string gender; 
	public string email;

	public bool connectedStatus = false;

	public string msg = "";

	public bool newData = false;
	private string channel;

	void Start () {

	}


	public void StartPusher(string url, string channel) {

		this.channel = channel;

		PusherSettings.Verbose = true;
		PusherSettings.AppKey = "3a375f8a13577d092e5897e6ce3a02";
		PusherSettings.HttpAuthUrl = url;
		pusherClient = new PusherClient.Pusher ();
		pusherClient.Connected += HandleConnected;
		pusherClient.ConnectionStateChanged += HandleConnectionStateChanged;
		pusherClient.Connect();

		StartCoroutine (testConnection ());

	}


	void HandleConnected (object sender) {
		Debug.Log ( "Pusher client connected, now subscribing to private channel" );	
		pusherChannel = pusherClient.Subscribe( channel );
		pusherChannel.BindAll( HandleChannelEvent );
	}

	void OnDestroy() {
		if( pusherClient != null )
			pusherClient.Disconnect();
	}

	void HandleChannelEvent( string eventName, object evData ) {
		Debug.Log ( "Received event on channel, event name: " + eventName + ", data: " + JsonHelper.Serialize(evData) );
		newEvent (JsonHelper.Serialize (evData));
	}

	private void newEvent(string stringTMP) {

		var	N = SimpleJSON.JSON.Parse (stringTMP);
		firstname = N ["tag_owner"]["firstname"];
		lastname = N ["tag_owner"]["lastname"]; 
		gender = N ["tag_owner"]["gender"]; 
		email = N ["tag_owner"] ["email"];
		nameWithTitle = N ["tag_owner"] ["name_with_title"];

		//send event to main game controller
		//mainController.newPusherEvent(firstname, lastname, gender, email);

		newData = true;

	}

	void HandleConnectionStateChanged (object sender, PusherClient.ConnectionState state) {
		//msg = state.ToString();
		Debug.Log ( "Pusher connection state changed to: " + state );
	}


	private IEnumerator testConnection() {
		yield return new WaitForSeconds (5F);
		WWW www = new WWW("http://google.com");
		yield return www;
		if (www.error != null) {
			connectedStatus = false;
		} else {
			connectedStatus = true;
		}
		StartCoroutine (testConnection ());
	}

}
