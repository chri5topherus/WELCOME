using UnityEngine;
using System.Collections;

public class PusherReceiver : MonoBehaviour {
	// isntance of pusher client
	PusherClient.Pusher pusherClient = null;
	PusherClient.Channel pusherChannel = null;

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
	}

	void HandleConnectionStateChanged (object sender, PusherClient.ConnectionState state) {
		Debug.Log ( "Pusher connection state changed to: " + state );
	}
}
