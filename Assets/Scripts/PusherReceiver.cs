using UnityEngine;
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
	public string firstname; 
	public string gender; 
	public string email;

	public bool newData = false;

	void Start () {
		PusherSettings.Verbose = true;
		PusherSettings.AppKey = "3a375f8a13577d092e5897e6ce3a02";
		PusherSettings.HttpAuthUrl = "http://test.flave.world:8080";
		pusherClient = new PusherClient.Pusher ();
		pusherClient.Connected += HandleConnected;
		pusherClient.ConnectionStateChanged += HandleConnectionStateChanged;
		pusherClient.Connect();
	}

	void HandleConnected (object sender) {
		Debug.Log ( "Pusher client connected, now subscribing to private channel" );
		pusherChannel = pusherClient.Subscribe( "core_items" );
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

		//send event to main game controller
		//mainController.newPusherEvent(firstname, lastname, gender, email);

		newData = true;

	}

	void HandleConnectionStateChanged (object sender, PusherClient.ConnectionState state) {
		Debug.Log ( "Pusher connection state changed to: " + state );
	}
}
