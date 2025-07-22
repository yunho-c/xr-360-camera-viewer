
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.Networking;
using TMPro;


[System.Serializable]
public class OrientationState
{
    public float yaw;
    public float pitch;
    public float roll;
    public float fov_x = 90.0f;
}


public class WebRTCReprojectionClient : MonoBehaviour
{
    [Header("Signaling Server")]
    [Tooltip("URL of the signaling server")]
    public string serverUrl = "http://localhost:8080/offer";

    [Header("VR Camera")]
    [Tooltip("The VR camera to track")]
    public Camera vrCamera;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text statusText;

    private RTCPeerConnection pc;
    private RTCDataChannel controlChannel;

    void Start()
    {
        statusText.text = "Starting WebRTC...";
        StartCoroutine(StartWebRTC());
    }

    void Update()
    {
        if (controlChannel != null && controlChannel.ReadyState == RTCDataChannelState.Open)
        {
            SendOrientation();
        }
    }

    private IEnumerator StartWebRTC()
    {
        CreatePeerConnection();

        // Create data channel
        controlChannel = pc.CreateDataChannel("control");
        SetupDataChannelEvents();

        // Create offer
        var offer = pc.CreateOffer();
        yield return offer;

        if (offer.IsError)
        {
            Debug.LogError("Error creating offer: " + offer.Error.message);
            yield break;
        }

        var desc = offer.Desc;
        var localDescOp = pc.SetLocalDescription(ref desc);
        yield return localDescOp;

        if (localDescOp.IsError)
        {
            Debug.LogError("Error setting local description: " + localDescOp.Error.message);
            yield break;
        }

        // Send offer to server
        statusText.text = "Sending offer...";
        SignalingMessage offerMessage = new SignalingMessage { type = "offer", sdp = desc.sdp };
        string jsonOffer = JsonUtility.ToJson(offerMessage);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonOffer);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error sending offer: " + www.error);
                statusText.text = "Error sending offer.";
                yield break;
            }

            statusText.text = "Offer sent, waiting for answer...";
            string jsonAnswer = www.downloadHandler.text;
            SignalingMessage answerMessage = JsonUtility.FromJson<SignalingMessage>(jsonAnswer);
            StartCoroutine(OnGotAnswer(answerMessage.sdp));
        }
    }

    private void CreatePeerConnection()
    {
        var configuration = GetSelectedSdpSemantics();
        pc = new RTCPeerConnection(ref configuration);
        Debug.Log("Peer Connection created.");

        pc.OnConnectionStateChange = state =>
        {
            Debug.Log("Connection state changed to: " + state);
            if (state == RTCPeerConnectionState.Connected)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    statusText.text = "Peers connected!";
                });
            }
        };

        pc.OnDataChannel = channel =>
        {
            Debug.Log("Data Channel received!");
            controlChannel = channel;
            SetupDataChannelEvents();
        };

        // The client receives the video stream
        pc.OnTrack = (RTCTrackEvent e) =>
        {
            if (e.Track.Kind == TrackKind.Video)
            {
                // Here you would handle the received video track,
                // for example by assigning it to a texture.
            }
        };
    }

    private IEnumerator OnGotAnswer(string sdp)
    {
        var remoteDesc = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = sdp };
        var remoteDescOp = pc.SetRemoteDescription(ref remoteDesc);
        yield return remoteDescOp;

        if (remoteDescOp.IsError)
        {
            Debug.LogError("Error setting remote description on answer: " + remoteDescOp.Error.message);
        }
    }

    private void SetupDataChannelEvents()
    {
        controlChannel.OnOpen = () =>
        {
            Debug.Log("Control Channel is open!");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                statusText.text = "Control channel open. Streaming orientation.";
            });
        };

        controlChannel.OnClose = () =>
        {
            Debug.Log("Control Channel is closed!");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                statusText.text = "Control channel closed.";
            });
        };
    }

    private void SendOrientation()
    {
        if (vrCamera != null)
        {
            OrientationState state = new OrientationState
            {
                yaw = vrCamera.transform.eulerAngles.y,
                pitch = -vrCamera.transform.eulerAngles.x, // Invert pitch for correct mapping
                roll = vrCamera.transform.eulerAngles.z
            };
            string jsonState = JsonUtility.ToJson(state);
            controlChannel.Send(jsonState);
        }
    }

    private void OnApplicationQuit()
    {
        if (controlChannel != null) controlChannel.Close();
        if (pc != null) pc.Close();
    }

    private static RTCConfiguration GetSelectedSdpSemantics()
    {
        return new RTCConfiguration
        {
            iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } }
        };
    }
}
