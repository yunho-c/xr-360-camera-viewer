```
 │    126     private void CreatePeerConnection()                                                                                              │
 │    127     {                                                                                                                                │
 │    128         var configuration = GetSelectedSdpSemantics();                                                                               │
 │    129         pc = new RTCPeerConnection(ref configuration);                                                                               │
 │    130         Debug.Log("Peer Connection created.");                                                                                       │
 │    131                                                                                                                                      │
 │    132         pc.OnIceCandidate = candidate =>                                                                                             │
 │    133         {                                                                                                                            │
 │    134             Debug.Log("Generated ICE Candidate: " + candidate.Candidate);                                                            │
 │    135             var candidateInit = new RTCIceCandidateInit { candidate = candidate.Candidate, sdpMid = candidate.SdpMid,                │
 │        sdpMLineIndex = candidate.SdpMLineIndex };                                                                                           │
 │    136             SignalingMessageReprojection msg = new SignalingMessageReprojection { type = "candidate", sdp =                          │
 │        JsonUtility.ToJson(candidateInit) };                                                                                                 │
 │    137             websocket.SendText(JsonUtility.ToJson(msg));                                                                             │
 │    138         };                                                                                                                           │
 │    139                                                                                                                                      │
 │    140         pc.OnConnectionStateChange = state =>                                                                                        │
 │    141         {                                                                                                                            │
 │    142             Debug.Log("Connection state changed to: " + state);                                                                      │
 │    143             if (state == RTCPeerConnectionState.Connected)                                                                           │
 │    144             {                                                                                                                        │
 │    145                 UnityMainThreadDispatcher.Instance().Enqueue(() =>                                                                   │
 │    146                 {                                                                                                                    │
 │    147                     statusText.text = "Peers connected!";                                                                            │
 │    148                 });                                                                                                                  │
 │    149             }                                                                                                                        │
 │    150         };                                                                                                                           │
 │    151                                                                                                                                      │
 │    152         pc.OnDataChannel = channel =>                                                                                                │
 │    153         {                                                                                                                            │
 │    154             Debug.Log("Data Channel received!");                                                                                     │
 │    155             controlChannel = channel;                                                                                                │
 │    156             SetupDataChannelEvents();                                                                                                │
 │    157         };                                                                                                                           │
 │    158                                                                                                                                      │
 │    159         // The client receives the video stream                                                                                      │
 │    160         pc.OnTrack = (RTCTrackEvent e) =>                                                                                            │
 │    161         {                                                                                                                            │
 │    162             if (e.Track.Kind == TrackKind.Video)                                                                                     │
 │    163             {                                                                                                                        │
 │    164                 // Here you would handle the received video track,                                                                   │
 │    165                 // for example by assigning it to a texture.                                                                         │
 │    166             }                                                                                                                        │
 │    167         };                                                                                                                           │
 │    168     }                                                                                                                                │
 │    169                                                                                                                                      │
 │    170     private IEnumerator OnGotOffer(string sdp)                                                                                       │
 │    171     {                                                                                                                                │
 │    172         if (pc == null)                                                                                                              │
 │    173         {                                                                                                                            │
 │    174             CreatePeerConnection();                                                                                                  │
 │    175         }                                                                                                                            │
 │    176                                                                                                                                      │
 │    177         var remoteDesc = new RTCSessionDescription { type = RTCSdpType.Offer, sdp = sdp };                                           │
 │    178         var remoteDescOp = pc.SetRemoteDescription(ref remoteDesc);                                                                  │
 │    179         yield return remoteDescOp;                                                                                                   │
 │    180                                                                                                                                      │
 │    181         if (remoteDescOp.IsError)                                                                                                    │
 │    182         {                                                                                                                            │
 │    183             Debug.LogError("Error setting remote description: " + remoteDescOp.Error.message);                                       │
 │    184             yield break;                                                                                                             │
 │    185         }                                                                                                                            │
 │    186                                                                                                                                      │
 │    187         var answer = pc.CreateAnswer();                                                                                              │
 │    188         yield return answer;                                                                                                         │
 │    189                                                                                                                                      │
 │    190         if (answer.IsError)                                                                                                          │
 │    191         {                                                                                                                            │
 │    192             Debug.LogError("Error creating answer: " + answer.Error.message);                                                        │
 │    193             yield break;                                                                                                             │
 │    194         }                                                                                                                            │
 │    195                                                                                                                                      │
 │    196         var desc = answer.Desc;                                                                                                      │
 │    197         var localDescOp = pc.SetLocalDescription(ref desc);                                                                          │
 │    198         yield return localDescOp;                                                                                                    │
 │    199                                                                                                                                      │
 │    200         if (localDescOp.IsError)                                                                                                     │
 │    201         {                                                                                                                            │
 │    202             Debug.LogError("Error setting local description for answer: " + localDescOp.Error.message);                              │
 │    203             yield break;                                                                                                             │
 │    204         }                                                                                                                            │
 │    205                                                                                                                                      │
 │    206         Debug.Log("Sending answer..");                                                                                               │
 │    207         SignalingMessageReprojection msg = new SignalingMessageReprojection { type = "answer", sdp = desc.sdp };                     │
 │    208         websocket.SendText(JsonUtility.ToJson(msg));                                                                                 │
 │    209     }                                                                                                                                │
 │    210                                                                                                                                      │
 │    211     private IEnumerator OnGotAnswer(string sdp)                                                                                      │
 │    212     {                                                                                                                                │
 │    213         var remoteDesc = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = sdp };                                          │
 │    214         var remoteDescOp = pc.SetRemoteDescription(ref remoteDesc);                                                                  │
 │    215         yield return remoteDescOp;                                                                                                   │
 │    216                                                                                                                                      │
 │    217         if (remoteDescOp.IsError)                                                                                                    │
 │    218         {                                                                                                                            │
 │    219             Debug.LogError("Error setting remote description on answer: " + remoteDescOp.Error.message);                             │
 │    220         }                                                                                                                            │
 │    221     }                                                                                                                                │
 │    222                                                                                                                                      │
 │    223     private void SetupDataChannelEvents()                                                                                            │
 │    224     {                                                                                                                                │
 │    225         controlChannel.OnOpen = () =>                                                                                                │
 │    226         {                                                                                                                            │
 │    227             Debug.Log("Control Channel is open!");                                                                                   │
 │    228             UnityMainThreadDispatcher.Instance().Enqueue(() =>                                                                       │
 │    229             {                                                                                                                        │
 │    230                 statusText.text = "Control channel open. Streaming orientation.";                                                    │
 │    231             });                                                                                                                      │
 │    232         };                                                                                                                           │
 │    233                                                                                                                                      │
 │    234         controlChannel.OnClose = () =>                                                                                               │
 │    235         {                                                                                                                            │
 │    236             Debug.Log("Control Channel is closed!");                                                                                 │
 │    237             UnityMainThreadDispatcher.Instance().Enqueue(() =>                                                                       │
 │    238             {                                                                                                                        │
 │    239                 statusText.text = "Control channel closed.";                                                                         │
 │    240             });                                                                                                                      │
 │    241         };                                                                                                                           │
 │    242     }                                                                                                                                │
 │    243                                                                                                                                      │
 │    244     private void SendOrientation()                                                                                                   │
 │    245     {                                                                                                                                │
 │    246         if (vrCamera != null)                                                                                                        │
 │    247         {                                                                                                                            │
 │    248             OrientationState state = new OrientationState                                                                            │
 │    249             {                                                                                                                        │
 │    250                 yaw = vrCamera.transform.eulerAngles.y,                                                                              │
 │    251                 pitch = -vrCamera.transform.eulerAngles.x, // Invert pitch for correct mapping                                       │
 │    252                 roll = vrCamera.transform.eulerAngles.z                                                                              │
 │    253             };                                                                                                                       │
 │    254             string jsonState = JsonUtility.ToJson(state);                                                                            │
 │    255             controlChannel.Send(jsonState);                                                                                          │
 │    256         }                                                                                                                            │
 │    257     }                                                                                                                                │
 │    258                                                                                                                                      │
 │    259     private async void OnApplicationQuit()                                                                                           │
 │    260     {                                                                                                                                │
 │    261         if (controlChannel != null) controlChannel.Close();                                                                          │
 │    262         if (pc != null) pc.Close();                                                                                                  │
 │    263         if (websocket != null) await websocket.Close();                                                                              │
 │    264     }                                                                                                                                │
 │    265                                                                                                                                      │
 │    266     private static RTCConfiguration GetSelectedSdpSemantics()                                                                        │
 │    267     {                                                                                                                                │
 │    268         return new RTCConfiguration                                                                                                  │
 │    269         {                                                                                                                            │
 │    270             iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } }                              │
 │    271         };                                                                                                                           │
 │    272     }                                                                                                                                │
 │    273 }                                                                                                                                    │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ I've created the WebRTCReprojectionClient.cs script. Now, I'll create a new Unity scene to use this script. I'll start by reading the
  contents of an existing scene to use as a template.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  ReadFile ...ssets\Scenes\Static360ImageScene.unity                                                                                       │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ I have the content of the Static360ImageScene.unity file. Now I will create a new scene named WebRTCReprojectionScene.unity and add the
  necessary GameObjects and components. This will include:

   1. A WebRTCReprojectionClient GameObject with the script attached.
   2. A UI Canvas with a TextMeshPro element to display the connection status.
   3. A Camera Rig, similar to the one in the existing scene, to represent the VR headset.

  I'll write the new scene file now.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  WriteFile Writing to ...Assets\\WebRTCReprojectionScene.unity                                                                            │
 │                                                                                                                                             │
 │    ... first 268 lines hidden ...                                                                                                           │
 │    269   m_AnchorMax: {x: 1, y: 1}                                                                                                          │
 │    270   m_AnchoredPosition: {x: 0, y: -100}                                                                                                │
 │    271   m_SizeDelta: {x: 0, y: 50}                                                                                                         │
 │    272   m_Pivot: {x: 0.5, y: 0.5}                                                                                                          │
 │    273                                                                                                                                      │
 │    274 # --- Camera Rig from Static360ImageScene --- #                                                                                      │
 │    275                                                                                                                                      │
 │    276 --- !u!1 &609325532                                                                                                                  │
 │    277 GameObject:                                                                                                                          │
 │    278   m_ObjectHideFlags: 0                                                                                                               │
 │    279   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    280   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    281   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    282   serializedVersion: 6                                                                                                               │
 │    283   m_Component:                                                                                                                       │
 │    284   - component: {fileID: 609325537}                                                                                                   │
 │    285   - component: {fileID: 609325536}                                                                                                   │
 │    286   - component: {fileID: 609325535}                                                                                                   │
 │    287   - component: {fileID: 609325534}                                                                                                   │
 │    288   - component: {fileID: 609325533}                                                                                                   │
 │    289   m_Layer: 0                                                                                                                         │
 │    290   m_Name: Camera Rig                                                                                                                 │
 │    291   m_TagString: Untagged                                                                                                              │
 │    292   m_Icon: {fileID: 0}                                                                                                                │
 │    293   m_NavMeshLayer: 0                                                                                                                  │
 │    294   m_StaticEditorFlags: 0                                                                                                             │
 │    295   m_IsActive: 1                                                                                                                      │
 │    296 --- !u!114 &609325533                                                                                                                │
 │    297 MonoBehaviour:                                                                                                                       │
 │    298   m_ObjectHideFlags: 0                                                                                                               │
 │    299   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    300   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    301   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    302   m_GameObject: {fileID: 609325532}                                                                                                  │
 │    303   m_Enabled: 1                                                                                                                       │
 │    304   m_EditorHideFlags: 0                                                                                                               │
 │    305   m_Script: {fileID: 11500000, guid: 5178bc8574ce2bf4388e787a2e2af326, type: 3}                                                      │
 │    306   m_Name:                                                                                                                            │
 │    307   m_EditorClassIdentifier:                                                                                                           │
 │    308   opMode: 1                                                                                                                          │
 │    309   resetHmdPoseOnRelease: 1                                                                                                           │
 │    310   resetHmdPoseByMiddleMouseButton: 1                                                                                                 │
 │    311   activateKeys: 3201000031010000                                                                                                     │
 │    312   activateKeyBindings:                                                                                                               │
 │    313   - <Keyboard>/leftCtrl                                                                                                              │
 │    314   - <Keyboard>/rightCtrl                                                                                                             │
 │    315   - <Keyboard>/f1                                                                                                                    │
 │    316   pitchKeys: 3401000033010000                                                                                                        │
 │    317   pitchKeyBindings:                                                                                                                  │
 │    318   - <Keyboard>/leftAlt                                                                                                               │
 │    319   - <Keyboard>/rightAlt                                                                                                              │
 │    320   - <Keyboard>/f2                                                                                                                    │
 │    321 --- !u!114 &609325534                                                                                                                │
 │    322 MonoBehaviour:                                                                                                                       │
 │    323   m_ObjectHideFlags: 0                                                                                                               │
 │    324   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    325   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    326   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    327   m_GameObject: {fileID: 609325532}                                                                                                  │
 │    328   m_Enabled: 1                                                                                                                       │
 │    329   m_EditorHideFlags: 0                                                                                                               │
 │    330   m_Script: {fileID: 11500000, guid: 7e933e81d3c20c74ea6fdc708a67e3a5, type: 3}                                                      │
 │    331   m_Name:                                                                                                                            │
 │    332   m_EditorClassIdentifier:                                                                                                           │
 │    333   _monoscopic: 0                                                                                                                     │
 │    334   _sharpenType: 0                                                                                                                    │
 │    335   _enableDynamicResolution: 0                                                                                                        │
 │    336   minDynamicResolutionScale: 1                                                                                                       │
 │    337   maxDynamicResolutionScale: 1                                                                                                       │
 │    338   quest2MinDynamicResolutionScale: 0.7                                                                                               │
 │    339   quest2MaxDynamicResolutionScale: 1.3                                                                                               │
 │    340   quest3MinDynamicResolutionScale: 0.7                                                                                               │
 │    341   quest3MaxDynamicResolutionScale: 1.6                                                                                               │
 │    342   minRenderScale: 0.7                                                                                                                │
 │    343   maxRenderScale: 1                                                                                                                  │
 │    344   _headPoseRelativeOffsetRotation: {x: 0, y: 0, z: 0}                                                                                │
 │    345   _headPoseRelativeOffsetTranslation: {x: 0, y: 0, z: 0}                                                                             │
 │    346   profilerTcpPort: 32419                                                                                                             │
 │    347   expandMixedRealityCapturePropertySheet: 0                                                                                          │
 │    348   enableMixedReality: 0                                                                                                              │
 │    349   compositionMethod: 0                                                                                                               │
 │    350   extraHiddenLayers:                                                                                                                 │
 │    351     serializedVersion: 2                                                                                                             │
 │    352     m_Bits: 0                                                                                                                        │
 │    353   extraVisibleLayers:                                                                                                                │
 │    354     serializedVersion: 2                                                                                                             │
 │    355     m_Bits: 0                                                                                                                        │
 │    356   dynamicCullingMask: 1                                                                                                              │
 │    357   externalCompositionBackdropColorRift: {r: 0, g: 1, b: 0, a: 1}                                                                     │
 │    358   externalCompositionBackdropColorQuest: {r: 0, g: 0, b: 0, a: 0}                                                                    │
 │    359   capturingCameraDevice: 0                                                                                                           │
 │    360   flipCameraFrameHorizontally: 0                                                                                                     │
 │    361   flipCameraFrameVertically: 0                                                                                                       │
 │    362   handPoseStateLatency: 0                                                                                                            │
 │    363   sandwichCompositionRenderLatency: 0                                                                                                │
 │    364   sandwichCompositionBufferedFrames: 8                                                                                               │
 │    365   chromaKeyColor: {r: 0, g: 1, b: 0, a: 1}                                                                                           │
 │    366   chromaKeySimilarity: 0.6                                                                                                           │
 │    367   chromaKeySmoothRange: 0.03                                                                                                         │
 │    368   chromaKeySpillRange: 0.06                                                                                                          │
 │    369   useDynamicLighting: 0                                                                                                              │
 │    370   depthQuality: 1                                                                                                                    │
 │    371   dynamicLightingSmoothFactor: 8                                                                                                     │
 │    372   dynamicLightingDepthVariationClampingValue: 0.001                                                                                  │
 │    373   virtualGreenScreenType: 0                                                                                                          │
 │    374   virtualGreenScreenTopY: 10                                                                                                         │
 │    375   virtualGreenScreenBottomY: -10                                                                                                     │
 │    376   virtualGreenScreenApplyDepthCulling: 0                                                                                             │
 │    377   virtualGreenScreenDepthTolerance: 0.2                                                                                              │
 │    378   mrcActivationMode: 0                                                                                                               │
 │    379   launchSimultaneousHandsControllersOnStartup: 0                                                                                     │
 │    380   isInsightPassthroughEnabled: 0                                                                                                     │
 │    381   shouldBoundaryVisibilityBeSuppressed: 0                                                                                            │
 │    382   requestBodyTrackingPermissionOnStartup: 0                                                                                          │
 │    383   requestFaceTrackingPermissionOnStartup: 0                                                                                          │
 │    384   requestEyeTrackingPermissionOnStartup: 0                                                                                           │
 │    385   requestScenePermissionOnStartup: 0                                                                                                 │
 │    386   requestRecordAudioPermissionOnStartup: 0                                                                                           │
 │    387   _localDimming: 1                                                                                                                   │
 │    388   _trackingOriginType: 1                                                                                                             │
 │    389   usePositionTracking: 1                                                                                                             │
 │    390   useRotationTracking: 1                                                                                                             │
 │    391   useIPDInPositionTracking: 1                                                                                                        │
 │    392   resetTrackerOnLoad: 0                                                                                                              │
 │    393   AllowRecenter: 1                                                                                                                   │
 │    394   LateControllerUpdate: 1                                                                                                            │
 │    395   LateLatching: 0                                                                                                                    │
 │    396   controllerDrivenHandPosesType: 0                                                                                                   │
 │    397   SimultaneousHandsAndControllersEnabled: 0                                                                                          │
 │    398   _readOnlyWideMotionModeHandPosesEnabled: 0                                                                                         │
 │    399   wideMotionModeHandPosesEnabled: 0                                                                                                  │
 │    400   dynamicResolutionVersion: 1                                                                                                        │
 │    401 --- !u!114 &609325535                                                                                                                │
 │    402 MonoBehaviour:                                                                                                                       │
 │    403   m_ObjectHideFlags: 0                                                                                                               │
 │    404   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    405   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    406   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    407   m_GameObject: {fileID: 609325532}                                                                                                  │
 │    408   m_Enabled: 1                                                                                                                       │
 │    409   m_EditorHideFlags: 0                                                                                                               │
 │    410   m_Script: {fileID: 11500000, guid: df9f338034892c44ebb62d97894772f1, type: 3}                                                      │
 │    411   m_Name:                                                                                                                            │
 │    412   m_EditorClassIdentifier:                                                                                                           │
 │    413   usePerEyeCameras: 0                                                                                                                │
 │    414   useFixedUpdateForTracking: 0                                                                                                       │
 │    415   disableEyeAnchorCameras: 0                                                                                                         │
 │    416 --- !u!114 &609325536                                                                                                                │
 │    417 MonoBehaviour:                                                                                                                       │
 │    418   m_ObjectHideFlags: 0                                                                                                               │
 │    419   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    420   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    421   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    422   m_GameObject: {fileID: 609325532}                                                                                                  │
 │    423   m_Enabled: 1                                                                                                                       │
 │    424   m_EditorHideFlags: 0                                                                                                               │
 │    425   m_Script: {fileID: 11500000, guid: 5b023183069df224080314a560e6e1ff, type: 3}                                                      │
 │    426   m_Name:                                                                                                                            │
 │    427   m_EditorClassIdentifier:                                                                                                           │
 │    428   blockId: e47682b9-c270-40b1-b16d-90b627a5ce1b                                                                                      │
 │    429   instanceId: 8026a110-5011-460e-a013-eba1e23dfdf8                                                                                   │
 │    430   version: 1                                                                                                                         │
 │    431   installationRoutineCheckpoint:                                                                                                     │
 │    432     _installationRoutineId:                                                                                                          │
 │    433     _installationVariants: []                                                                                                        │
 │    434 --- !u!4 &609325537                                                                                                                  │
 │    435 Transform:                                                                                                                           │
 │    436   m_ObjectHideFlags: 0                                                                                                               │
 │    437   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    438   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    439   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    440   m_GameObject: {fileID: 609325532}                                                                                                  │
 │    441   serializedVersion: 2                                                                                                               │
 │    442   m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}                                                                                          │
 │    443   m_LocalPosition: {x: 0, y: 0, z: 0}                                                                                                │
 │    444   m_LocalScale: {x: 1, y: 1, z: 1}                                                                                                   │
 │    445   m_ConstrainProportionsScale: 0                                                                                                     │
 │    446   m_Children:                                                                                                                        │
 │    447   - {fileID: 1420924372}                                                                                                             │
 │    448   m_Father: {fileID: 0}                                                                                                              │
 │    449   m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}                                                                                         │
 │    450 --- !u!1 &1071890478                                                                                                                 │
 │    451 GameObject:                                                                                                                          │
 │    452   m_ObjectHideFlags: 0                                                                                                               │
 │    453   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    454   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    455   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    456   serializedVersion: 6                                                                                                               │
 │    457   m_Component:                                                                                                                       │
 │    458   - component: {fileID: 1071890479}                                                                                                  │
 │    459   - component: {fileID: 1071890481}                                                                                                  │
 │    460   - component: {fileID: 1071890480}                                                                                                  │
 │    461   - component: {fileID: 1071890482}                                                                                                  │
 │    462   m_Layer: 0                                                                                                                         │
 │    463   m_Name: CenterEyeAnchor                                                                                                            │
 │    464   m_TagString: MainCamera                                                                                                            │
 │    465   m_Icon: {fileID: 0}                                                                                                                │
 │    466   m_NavMeshLayer: 0                                                                                                                  │
 │    467   m_StaticEditorFlags: 0                                                                                                             │
 │    468   m_IsActive: 1                                                                                                                      │
 │    469 --- !u!4 &1071890479                                                                                                                 │
 │    470 Transform:                                                                                                                           │
 │    471   m_ObjectHideFlags: 0                                                                                                               │
 │    472   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    473   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    474   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    475   m_GameObject: {fileID: 1071890478}                                                                                                 │
 │    476   serializedVersion: 2                                                                                                               │
 │    477   m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}                                                                                          │
 │    478   m_LocalPosition: {x: 0, y: 0, z: 0}                                                                                                │
 │    479   m_LocalScale: {x: 1, y: 1, z: 1}                                                                                                   │
 │    480   m_ConstrainProportionsScale: 0                                                                                                     │
 │    481   m_Children: []                                                                                                                     │
 │    482   m_Father: {fileID: 1420924372}                                                                                                     │
 │    483   m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}                                                                                         │
 │    484 --- !u!20 &1071890480                                                                                                                │
 │    485 Camera:                                                                                                                              │
 │    486   m_ObjectHideFlags: 0                                                                                                               │
 │    487   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    488   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    489   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    490   m_GameObject: {fileID: 1071890478}                                                                                                 │
 │    491   m_Enabled: 1                                                                                                                       │
 │    492   serializedVersion: 2                                                                                                               │
 │    493   m_ClearFlags: 1                                                                                                                    │
 │    494   m_BackGroundColor: {r: 0.19215687, g: 0.3019608, b: 0.4745098, a: 0.019607844}                                                     │
 │    495   m_projectionMatrixMode: 1                                                                                                          │
 │    496   m_GateFitMode: 2                                                                                                                   │
 │    497   m_FOVAxisMode: 0                                                                                                                   │
 │    498   m_Iso: 200                                                                                                                         │
 │    499   m_ShutterSpeed: 0.005                                                                                                              │
 │    500   m_Aperture: 16                                                                                                                     │
 │    501   m_FocusDistance: 10                                                                                                                │
 │    502   m_FocalLength: 50                                                                                                                  │
 │    503   m_BladeCount: 5                                                                                                                    │
 │    504   m_Curvature: {x: 2, y: 11}                                                                                                         │
 │    505   m_BarrelClipping: 0.25                                                                                                             │
 │    506   m_Anamorphism: 0                                                                                                                   │
 │    507   m_SensorSize: {x: 36, y: 24}                                                                                                       │
 │    508   m_LensShift: {x: 0, y: 0}                                                                                                          │
 │    509   m_NormalizedViewPortRect:                                                                                                          │
 │    510     serializedVersion: 2                                                                                                             │
 │    511     x: 0                                                                                                                             │
 │    512     y: 0                                                                                                                             │
 │    513     width: 1                                                                                                                         │
 │    514     height: 1                                                                                                                        │
 │    515   near clip plane: 0.1                                                                                                               │
 │    516   far clip plane: 1000                                                                                                               │
 │    517   field of view: 90                                                                                                                  │
 │    518   orthographic: 0                                                                                                                    │
 │    519   orthographic size: 5                                                                                                               │
 │    520   m_Depth: 0                                                                                                                         │
 │    521   m_CullingMask:                                                                                                                     │
 │    522     serializedVersion: 2                                                                                                             │
 │    523     m_Bits: 4294967295                                                                                                               │
 │    524   m_RenderingPath: -1                                                                                                                │
 │    525   m_TargetTexture: {fileID: 0}                                                                                                       │
 │    526   m_TargetDisplay: 0                                                                                                                 │
 │    527   m_TargetEye: 3                                                                                                                     │
 │    528   m_HDR: 0                                                                                                                           │
 │    529   m_AllowMSAA: 1                                                                                                                     │
 │    530   m_AllowDynamicResolution: 0                                                                                                        │
 │    531   m_ForceIntoRT: 0                                                                                                                   │
 │    532   m_OcclusionCulling: 1                                                                                                              │
 │    533   m_StereoConvergence: 10                                                                                                            │
 │    534   m_StereoSeparation: 0.022                                                                                                          │
 │    535 --- !u!81 &1071890481                                                                                                                │
 │    536 AudioListener:                                                                                                                       │
 │    537   m_ObjectHideFlags: 0                                                                                                               │
 │    538   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    539   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    540   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    541   m_GameObject: {fileID: 1071890478}                                                                                                 │
 │    542   m_Enabled: 1                                                                                                                       │
 │    543 --- !u!114 &1071890482                                                                                                               │
 │    544 MonoBehaviour:                                                                                                                       │
 │    545   m_ObjectHideFlags: 0                                                                                                               │
 │    546   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    547   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    548   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    549   m_GameObject: {fileID: 1071890478}                                                                                                 │
 │    550   m_Enabled: 1                                                                                                                       │
 │    551   m_EditorHideFlags: 0                                                                                                               │
 │    552   m_Script: {fileID: 11500000, guid: a79441f348de89743a2939f4d699eac1, type: 3}                                                      │
 │    553   m_Name:                                                                                                                            │
 │    554   m_EditorClassIdentifier:                                                                                                           │
 │    555   m_RenderShadows: 1                                                                                                                 │
 │    556   m_RequiresDepthTextureOption: 2                                                                                                    │
 │    557   m_RequiresOpaqueTextureOption: 2                                                                                                   │
 │    558   m_CameraType: 0                                                                                                                    │
 │    559   m_Cameras: []                                                                                                                      │
 │    560   m_RendererIndex: -1                                                                                                                │
 │    561   m_VolumeLayerMask:                                                                                                                 │
 │    562     serializedVersion: 2                                                                                                             │
 │    563     m_Bits: 1                                                                                                                        │
 │    564   m_VolumeTrigger: {fileID: 0}                                                                                                       │
 │    565   m_VolumeFrameworkUpdateModeOption: 2                                                                                               │
 │    566   m_RenderPostProcessing: 0                                                                                                          │
 │    567   m_Antialiasing: 0                                                                                                                  │
 │    568   m_AntialiasingQuality: 2                                                                                                           │
 │    569   m_StopNaN: 0                                                                                                                       │
 │    570   m_Dithering: 0                                                                                                                     │
 │    571   m_ClearDepth: 1                                                                                                                    │
 │    572   m_AllowXRRendering: 1                                                                                                              │
 │    573   m_AllowHDROutput: 1                                                                                                                │
 │    574   m_UseScreenCoordOverride: 0                                                                                                        │
 │    575   m_ScreenSizeOverride: {x: 0, y: 0, z: 0, w: 0}                                                                                     │
 │    576   m_ScreenCoordScaleBias: {x: 0, y: 0, z: 0, w: 0}                                                                                   │
 │    577   m_RequiresDepthTexture: 0                                                                                                          │
 │    578   m_RequiresColorTexture: 0                                                                                                          │
 │    579   m_Version: 2                                                                                                                       │
 │    580   m_TaaSettings:                                                                                                                     │
 │    581     m_Quality: 3                                                                                                                     │
 │    582     m_FrameInfluence: 0.1                                                                                                            │
 │    583     m_JitterScale: 1                                                                                                                 │
 │    584     m_MipBias: 0                                                                                                                     │
 │    585     m_VarianceClampScale: 0.9                                                                                                        │
 │    586     m_ContrastAdaptiveSharpening: 0                                                                                                  │
 │    587 --- !u!1 &1420924371                                                                                                                 │
 │    588 GameObject:                                                                                                                          │
 │    589   m_ObjectHideFlags: 0                                                                                                               │
 │    590   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    591   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    592   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    593   serializedVersion: 6                                                                                                               │
 │    594   m_Component:                                                                                                                       │
 │    595   - component: {fileID: 1420924372}                                                                                                  │
 │    596   m_Layer: 0                                                                                                                         │
 │    597   m_Name: TrackingSpace                                                                                                              │
 │    598   m_TagString: Untagged                                                                                                              │
 │    599   m_Icon: {fileID: 0}                                                                                                                │
 │    600   m_NavMeshLayer: 0                                                                                                                  │
 │    601   m_StaticEditorFlags: 0                                                                                                             │
 │    602   m_IsActive: 1                                                                                                                      │
 │    603 --- !u!4 &1420924372                                                                                                                 │
 │    604 Transform:                                                                                                                           │
 │    605   m_ObjectHideFlags: 0                                                                                                               │
 │    606   m_CorrespondingSourceObject: {fileID: 0}                                                                                           │
 │    607   m_PrefabInstance: {fileID: 0}                                                                                                      │
 │    608   m_PrefabAsset: {fileID: 0}                                                                                                         │
 │    609   m_GameObject: {fileID: 1420924371}                                                                                                 │
 │    610   serializedVersion: 2                                                                                                               │
 │    611   m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}                                                                                          │
 │    612   m_LocalPosition: {x: 0, y: 0, z: 0}                                                                                                │
 │    613   m_LocalScale: {x: 1, y: 1, z: 1}                                                                                                   │
 │    614   m_ConstrainProportionsScale: 0                                                                                                     │
 │    615   m_Children:                                                                                                                        │
 │    616   - {fileID: 1071890479}                                                                                                             │
 │    617   m_Father: {fileID: 609325537}                                                                                                      │
 │    618   m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}                                                                                         │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ I've created the new scene. However, I've left placeholders for the script and font GUIDs. I'll now find the correct GUIDs and replace the
  placeholders in the scene file. First, I'll find the GUID for the WebRTCReprojectionClient.cs script.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  FindFiles '**/WebRTCReprojectionClient.cs.meta'                                                                                          │
 │                                                                                                                                             │
 │    Found 1 matching file(s)                                                                                                                 │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ Now that I have the path to the meta file, I can read it to get the GUID.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  ReadFile ...sets\\WebRTCReprojectionClient.cs.meta                                                                                       │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ I have the GUID for the script: 4b5ddaf321c60394996557c585360f1d. Now I need the GUID for the font. I'll look for the LiberationSans font
  that's already in the project.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  FindFiles '**/LiberationSans.ttf.meta'                                                                                                   │
 │                                                                                                                                             │
 │    Found 1 matching file(s)                                                                                                                 │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ Now I'll read the font's meta file to get its GUID.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  ReadFile ...Assets\Fonts\LiberationSans.ttf.meta                                                                                         │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ I have the font GUID: e3265ab4bf004d28a9537516768c1c75. Now I can replace the placeholders in the WebRTCReprojectionScene.unity file.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  Edit ...Assets\\WebRTCReprojectionScene.unity: [YOUR_SCRIPT_GUID] => 4b5ddaf321c60394996557c585360f...                                   │
 │                                                                                                                                             │
 │    157    m_GameObject: {fileID: 1001}                                                                                                      │
 │    158    m_Enabled: 1                                                                                                                      │
 │    159    m_EditorHideFlags: 0                                                                                                              │
 │    160  - m_Script: {fileID: 11500000, guid: [YOUR_SCRIPT_GUID], type: 3} # Will be replaced with the actual GUID                           │
 │    160  + m_Script: {fileID: 11500000, guid: 4b5ddaf321c60394996557c585360f1d, type: 3} # Will be replaced with the actual GUID             │
 │    161    m_Name:                                                                                                                           │
 │    162    m_EditorClassIdentifier:                                                                                                          │
 │    163    serverUrl: ws://localhost:8080/ws                                                                                                 │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  Edit ...Assets\\WebRTCReprojectionScene.unity: [YOUR_FONT_GUID] => e3265ab4bf004d28a9537516768c1c...                                     │
 │                                                                                                                                             │
 │    213      m_Name: StatusText                                                                                                              │
 │    214      m_EditorClassIdentifier:                                                                                                        │
 │    215      m_text: Connecting...                                                                                                           │
 │    216  -   m_fontAsset: {fileID: 11400000, guid: [YOUR_FONT_GUID], type: 3} # Will be replaced with actual GUID                            │
 │    216  +   m_fontAsset: {fileID: 11400000, guid: e3265ab4bf004d28a9537516768c1c75, type: 3} # Will be replaced with actual GUID            │
 │    217      m_fontSize: 24                                                                                                                  │
 │    218      m_alignment: 4                                                                                                                  │
 │    219    --- !u!224 &2004                                                                                                                  │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ Now that the Unity scene is set up, I need to modify the Python server to handle the orientation data from the Unity client. I will modify
  360_server_reprojection.py to receive the orientation data from the "control" data channel and use it to update the AppState. I will also
  change the WebRTCServer to be a WebSocketServer to handle the signaling.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ -  Edit ...References\\360_server_reprojection.py: from xr_360_camera_streamer.st... => from xr_360_camera_streamer.st...                   │
 │                                                                                                                                             │
 │    10     from fastapi.responses import FileResponse                                                                                        │
 │    11                                                                                                                                       │
 │    12     from xr_360_camera_streamer.sources import FFmpegFileSource, OpenCVFileSource                                                     │
 │    13   - from xr_360_camera_streamer.streaming import WebRTCServer                                                                         │
 │    13   + from xr_360_camera_streamer.streaming import WebSocketServer                                                                      │
 │    14     from xr_360_camera_streamer.transforms import EquilibEqui2Pers                                                                    │
 │    15                                                                                                                                       │
 │    16     # Params                                                                                                                          │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯

╭───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
│  > Please take a look at @References/xr-360-camera-streamer/python/src/xr_360_camera_streamer/streaming/webrtc_server.py before you do that! │
╰───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯

 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  ReadManyFiles Will attempt to read and concatenate files using patterns: `References/xr-360-camera-streamer/python/src/xr_360_camera_s…  │
 │                                                                                                                                             │
 │    ### ReadManyFiles Result (Target Dir: `C:\Users\G14\GitHub\xr-360-camera-viewer\Unity`)                                                  │
 │                                                                                                                                             │
 │    Successfully read and concatenated content from **1 file(s)**.                                                                           │
 │                                                                                                                                             │
 │    **Processed Files:**                                                                                                                     │
 │    - `References/xr-360-camera-streamer/python/src/xr_360_camera_streamer/streaming/webrtc_server.py`                                       │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ You are right to point that out. I was about to make a mistake. The WebRTCServer in the Python package uses HTTP for signaling, not
  WebSockets. My newly created WebRTCReprojectionClient.cs is using WebSockets, which is inconsistent with the server I'm trying to connect to.

  I need to modify WebRTCReprojectionClient.cs to use HTTP for signaling. This will involve removing the WebSocket code and using Unity's
  UnityWebRequest to send the offer and receive the answer. I will also remove the ICE candidate signaling, as the Python server seems to
  handle that within the SDP exchange.

  First, I'll read the WebRTCReprojectionClient.cs file again.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  ReadFile Assets\\WebRTCReprojectionClient.cs                                                                                             │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ I will now replace the WebSocket signaling with HTTP-based signaling in WebRTCReprojectionClient.cs. This involves removing the
  NativeWebSocket dependency and using UnityEngine.Networking.UnityWebRequest to send the offer to the Python server. I will also remove the
  OnIceCandidate handler, as the WebRTCServer handles ICE candidates within the SDP. Finally, I will change the serverUrl to be an HTTP URL.
 ╭─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  Edit Assets\\WebRTCReprojectionClient.cs: ... => ...                                                                                     │
 │                                                                                                                                             │
 │    3      using System.Collections.Generic;                                                                                                 │
 │    4      using UnityEngine;                                                                                                                │
 │    5      using Unity.WebRTC;                                                                                                               │
 │    6    - using NativeWebSocket;                                                                                                            │
 │    6    + using UnityEngine.Networking;                                                                                                     │
 │    7      using TMPro;                                                                                                                      │
 │    8                                                                                                                                        │
 │    9      [System.Serializable]                                                                                                             │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    26     {                                                                                                                                 │
 │    27         [Header("Signaling Server")]                                                                                                  │
 │    28         [Tooltip("URL of the signaling server")]                                                                                      │
 │    29   -     public string serverUrl = "ws://localhost:8080/ws";                                                                           │
 │    29   +     public string serverUrl = "http://localhost:8080/offer";                                                                      │
 │    30                                                                                                                                       │
 │    31         [Header("VR Camera")]                                                                                                         │
 │    32         [Tooltip("The VR camera to track")]                                                                                           │
 │    35         [Header("UI Elements")]                                                                                                       │
 │    36         [SerializeField] private TMP_Text statusText;                                                                                 │
 │    37                                                                                                                                       │
 │    38   -     private WebSocket websocket;                                                                                                  │
 │    38         private RTCPeerConnection pc;                                                                                                 │
 │    39         private RTCDataChannel controlChannel;                                                                                        │
 │    40                                                                                                                                       │
 │    42   -     async void Start()                                                                                                            │
 │    41   +     void Start()                                                                                                                  │
 │    42         {                                                                                                                             │
 │    44   -         statusText.text = "Connecting to signaling server...";                                                                    │
 │    43   +         statusText.text = "Starting WebRTC...";                                                                                   │
 │    44   +         StartCoroutine(StartWebRTC());                                                                                            │
 │    45   +     }                                                                                                                             │
 │    46                                                                                                                                       │
 │    46   -         websocket = new WebSocket(serverUrl);                                                                                     │
 │    47   -                                                                                                                                   │
 │    48   -         websocket.OnOpen += () =>                                                                                                 │
 │    47   +     void Update()                                                                                                                 │
 │    48         {                                                                                                                             │
 │    50   -             Debug.Log("WebSocket connection opened");                                                                             │
 │    51   -             UnityMainThreadDispatcher.Instance().Enqueue(() =>                                                                    │
 │    49   +         if (controlChannel != null && controlChannel.ReadyState == RTCDataChannelState.Open)                                      │
 │    50             {                                                                                                                         │
 │    53   -                 statusText.text = "Connected to server. Waiting for offer...";                                                    │
 │    54   -             });                                                                                                                   │
 │    55   -         };                                                                                                                        │
 │    51   +             SendOrientation();                                                                                                    │
 │    52   +         }                                                                                                                         │
 │    53   +     }                                                                                                                             │
 │    54                                                                                                                                       │
 │    57   -         websocket.OnError += (e) =>                                                                                               │
 │    55   +     private IEnumerator StartWebRTC()                                                                                             │
 │    56         {                                                                                                                             │
 │    59   -             Debug.LogError("WebSocket Error: " + e);                                                                              │
 │    60   -             UnityMainThreadDispatcher.Instance().Enqueue(() =>                                                                    │
 │    61   -             {                                                                                                                     │
 │    62   -                 statusText.text = "Error connecting to server.";                                                                  │
 │    63   -             });                                                                                                                   │
 │    64   -         };                                                                                                                        │
 │    57   +         CreatePeerConnection();                                                                                                   │
 │    58                                                                                                                                       │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    66   -         websocket.OnClose += (e) =>                                                                                               │
 │    67   -         {                                                                                                                         │
 │    68   -             Debug.Log("WebSocket connection closed!");                                                                            │
 │    69   -             UnityMainThreadDispatcher.Instance().Enqueue(() =>                                                                    │
 │    70   -             {                                                                                                                     │
 │    71   -                 statusText.text = "Connection closed.";                                                                           │
 │    72   -             });                                                                                                                   │
 │    73   -         };                                                                                                                        │
 │    59   +         // Create data channel                                                                                                    │
 │    60   +         controlChannel = pc.CreateDataChannel("control");                                                                         │
 │    61   +         SetupDataChannelEvents();                                                                                                 │
 │    62                                                                                                                                       │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    75   -         websocket.OnMessage += (bytes) =>                                                                                         │
 │    76   -         {                                                                                                                         │
 │    77   -             var messageStr = System.Text.Encoding.UTF8.GetString(bytes);                                                          │
 │    78   -             Debug.Log("Received signaling message: " + messageStr);                                                               │
 │    79   -             SignalingMessageReprojection message = JsonUtility.FromJson<SignalingMessageReprojection>(messageStr);                │
 │    63   +         // Create offer                                                                                                           │
 │    64   +         var offer = pc.CreateOffer();                                                                                             │
 │    65   +         yield return offer;                                                                                                       │
 │    66                                                                                                                                       │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    81   -             UnityMainThreadDispatcher.Instance().Enqueue(() =>                                                                    │
 │    67   +         if (offer.IsError)                                                                                                        │
 │    68             {                                                                                                                         │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    83   -                 HandleSignalingMessage(message);                                                                                  │
 │    84   -             });                                                                                                                   │
 │    85   -         };                                                                                                                        │
 │    86   -                                                                                                                                   │
 │    87   -         await websocket.Connect();                                                                                                │
 │    69   +             Debug.LogError("Error creating offer: " + offer.Error.message);                                                       │
 │    70   +             yield break;                                                                                                          │
 │    71             }                                                                                                                         │
 │    72                                                                                                                                       │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    90   -     void Update()                                                                                                                 │
 │    73   +         var desc = offer.Desc;                                                                                                    │
 │    74   +         var localDescOp = pc.SetLocalDescription(ref desc);                                                                       │
 │    75   +         yield return localDescOp;                                                                                                 │
 │    76   +                                                                                                                                   │
 │    77   +         if (localDescOp.IsError)                                                                                                  │
 │    78             {                                                                                                                         │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    92   - #if !UNITY_WEBGL || UNITY_EDITOR                                                                                                  │
 │    93   -         if (websocket != null)                                                                                                    │
 │    94   -         {                                                                                                                         │
 │    95   -             websocket.DispatchMessageQueue();                                                                                     │
 │    79   +             Debug.LogError("Error setting local description: " + localDescOp.Error.message);                                      │
 │    80   +             yield break;                                                                                                          │
 │    81             }                                                                                                                         │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    97   - #endif                                                                                                                            │
 │    82                                                                                                                                       │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    99   -         if (controlChannel != null && controlChannel.ReadyState == RTCDataChannelState.Open)                                      │
 │    83   +         // Send offer to server                                                                                                   │
 │    84   +         statusText.text = "Sending offer...";                                                                                     │
 │    85   +         SignalingMessageReprojection offerMessage = new SignalingMessageReprojection { type = "offer", sdp = desc.sdp             │
 │           };                                                                                                                                │
 │    86   +         string jsonOffer = JsonUtility.ToJson(offerMessage);                                                                      │
 │    87   +                                                                                                                                   │
 │    88   +         using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))                                                      │
 │    89             {                                                                                                                         │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    101  -             SendOrientation();                                                                                                    │
 │    102  -         }                                                                                                                         │
 │    103  -     }                                                                                                                             │
 │    90   +             byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonOffer);                                                       │
 │    91   +             www.uploadHandler = new UploadHandlerRaw(bodyRaw);                                                                    │
 │    92   +             www.downloadHandler = new DownloadHandlerBuffer();                                                                    │
 │    93   +             www.SetRequestHeader("Content-Type", "application/json");                                                             │
 │    94                                                                                                                                       │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    105  -     private void HandleSignalingMessage(SignalingMessageReprojection message)                                                     │
 │    95   +             yield return www.SendWebRequest();                                                                                    │
 │    96   +                                                                                                                                   │
 │    97   +             if (www.result != UnityWebRequest.Result.Success)                                                                     │
 │    98                 {                                                                                                                     │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    107  -         switch (message.type)                                                                                                     │
 │    108  -         {                                                                                                                         │
 │    109  -             case "offer":                                                                                                         │
 │    110  -                 Debug.Log("Received offer, creating answer...");                                                                  │
 │    111  -                 StartCoroutine(OnGotOffer(message.sdp));                                                                          │
 │    112  -                 break;                                                                                                            │
 │    113  -             case "answer":                                                                                                        │
 │    114  -                 Debug.Log("Received answer.");                                                                                    │
 │    115  -                 StartCoroutine(OnGotAnswer(message.sdp));                                                                         │
 │    116  -                 break;                                                                                                            │
 │    117  -             case "candidate":                                                                                                     │
 │    118  -                 Debug.Log("Received ICE candidate.");                                                                             │
 │    119  -                 RTCIceCandidateInit candidateInit = JsonUtility.FromJson<RTCIceCandidateInit>(message.sdp);                       │
 │    120  -                 RTCIceCandidate candidate = new RTCIceCandidate(candidateInit);                                                   │
 │    121  -                 pc.AddIceCandidate(candidate);                                                                                    │
 │    122  -                 break;                                                                                                            │
 │    99   +                 Debug.LogError("Error sending offer: " + www.error);                                                              │
 │    100  +                 statusText.text = "Error sending offer.";                                                                         │
 │    101  +                 yield break;                                                                                                      │
 │    102                }                                                                                                                     │
 │    103  +                                                                                                                                   │
 │    104  +             statusText.text = "Offer sent, waiting for answer...";                                                                │
 │    105  +             string jsonAnswer = www.downloadHandler.text;                                                                         │
 │    106  +             SignalingMessageReprojection answerMessage =                                                                          │
 │           JsonUtility.FromJson<SignalingMessageReprojection>(jsonAnswer);                                                                   │
 │    107  +             StartCoroutine(OnGotAnswer(answerMessage.sdp));                                                                       │
 │    108            }                                                                                                                         │
 │    109  +     }                                                                                                                             │
 │    110                                                                                                                                      │
 │    111        private void CreatePeerConnection()                                                                                           │
 │    112        {                                                                                                                             │
 │    114            pc = new RTCPeerConnection(ref configuration);                                                                            │
 │    115            Debug.Log("Peer Connection created.");                                                                                    │
 │    116                                                                                                                                      │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    132  -         pc.OnIceCandidate = candidate =>                                                                                          │
 │    133  -         {                                                                                                                         │
 │    134  -             Debug.Log("Generated ICE Candidate: " + candidate.Candidate);                                                         │
 │    135  -             var candidateInit = new RTCIceCandidateInit { candidate = candidate.Candidate, sdpMid = candidate.SdpMid,             │
 │           sdpMLineIndex = candidate.SdpMLineIndex };                                                                                        │
 │    136  -             SignalingMessageReprojection msg = new SignalingMessageReprojection { type = "candidate", sdp =                       │
 │           JsonUtility.ToJson(candidateInit) };                                                                                              │
 │    137  -             websocket.SendText(JsonUtility.ToJson(msg));                                                                          │
 │    138  -         };                                                                                                                        │
 │    139  -                                                                                                                                   │
 │    117            pc.OnConnectionStateChange = state =>                                                                                     │
 │    118            {                                                                                                                         │
 │    119                Debug.Log("Connection state changed to: " + state);                                                                   │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    144            };                                                                                                                        │
 │    145        }                                                                                                                             │
 │    146                                                                                                                                      │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    170  -     private IEnumerator OnGotOffer(string sdp)                                                                                    │
 │    171  -     {                                                                                                                             │
 │    172  -         if (pc == null)                                                                                                           │
 │    173  -         {                                                                                                                         │
 │    174  -             CreatePeerConnection();                                                                                               │
 │    175  -         }                                                                                                                         │
 │    176  -                                                                                                                                   │
 │    177  -         var remoteDesc = new RTCSessionDescription { type = RTCSdpType.Offer, sdp = sdp };                                        │
 │    178  -         var remoteDescOp = pc.SetRemoteDescription(ref remoteDesc);                                                               │
 │    179  -         yield return remoteDescOp;                                                                                                │
 │    180  -                                                                                                                                   │
 │    181  -         if (remoteDescOp.IsError)                                                                                                 │
 │    182  -         {                                                                                                                         │
 │    183  -             Debug.LogError("Error setting remote description: " + remoteDescOp.Error.message);                                    │
 │    184  -             yield break;                                                                                                          │
 │    185  -         }                                                                                                                         │
 │    186  -                                                                                                                                   │
 │    187  -         var answer = pc.CreateAnswer();                                                                                           │
 │    188  -         yield return answer;                                                                                                      │
 │    189  -                                                                                                                                   │
 │    190  -         if (answer.IsError)                                                                                                       │
 │    191  -         {                                                                                                                         │
 │    192  -             Debug.LogError("Error creating answer: " + answer.Error.message);                                                     │
 │    193  -             yield break;                                                                                                          │
 │    194  -         }                                                                                                                         │
 │    195  -                                                                                                                                   │
 │    196  -         var desc = answer.Desc;                                                                                                   │
 │    197  -         var localDescOp = pc.SetLocalDescription(ref desc);                                                                       │
 │    198  -         yield return localDescOp;                                                                                                 │
 │    199  -                                                                                                                                   │
 │    200  -         if (localDescOp.IsError)                                                                                                  │
 │    201  -         {                                                                                                                         │
 │    202  -             Debug.LogError("Error setting local description for answer: " + localDescOp.Error.message);                           │
 │    203  -             yield break;                                                                                                          │
 │    204  -         }                                                                                                                         │
 │    205  -                                                                                                                                   │
 │    206  -         Debug.Log("Sending answer..");                                                                                            │
 │    207  -         SignalingMessageReprojection msg = new SignalingMessageReprojection { type = "answer", sdp = desc.sdp };                  │
 │    208  -         websocket.SendText(JsonUtility.ToJson(msg));                                                                              │
 │    209  -     }                                                                                                                             │
 │    210  -                                                                                                                                   │
 │    147        private IEnumerator OnGotAnswer(string sdp)                                                                                   │
 │    148        {                                                                                                                             │
 │    149            var remoteDesc = new RTCSessionDescription { type = RTCSdpType.Answer, sdp = sdp };                                       │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    192            }                                                                                                                         │
 │    193        }                                                                                                                             │
 │    194                                                                                                                                      │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    259  -     private async void OnApplicationQuit()                                                                                        │
 │    195  +     private void OnApplicationQuit()                                                                                              │
 │    196        {                                                                                                                             │
 │    197            if (controlChannel != null) controlChannel.Close();                                                                       │
 │    198            if (pc != null) pc.Close();                                                                                               │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════            │
 │    263  -         if (websocket != null) await websocket.Close();                                                                           │
 │    199        }                                                                                                                             │
 │    200                                                                                                                                      │
 │    201        private static RTCConfiguration GetSelectedSdpSemantics()                                                                     │
 ╰─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯


✦ Now that the WebRTCReprojectionClient is updated to use HTTP for signaling, the final step is to modify the Python server to correctly
  handle the incoming data. I will modify 360_server_reprojection.py to use the WebRTCServer and to correctly parse the JSON from the Unity
  client. I will also change the on_control_message function to handle the OrientationState object sent from Unity.
 ╭───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ -  Edit ...References\\360_server_reprojection.py: import asyncio... => import asyncio...                                         │
 │                                                                                                                                   │
 │    49         async def recv(self):                                                                                               │
 │    50             equi_frame_rgb = next(self.source)  # ALT                                                                       │
 │    51                                                                                                                             │
 │    52   -         # ORIG                                                                                                          │
 │    53   -         # try:                                                                                                          │
 │    54   -         #     # Get the next equirectangular frame from the source                                                      │
 │    55   -         #     equi_frame_rgb = next(self.source)                                                                        │
 │    56   -         # except StopIteration:                                                                                         │
 │    57   -         #     # Loop the video (or handle end-of-stream)                                                                │
 │    58   -         #     print("Restarting video source...")                                                                       │
 │    59   -         #     # This is a simple way to loop. A more robust solution might be needed.                                   │
 │    60   -         #     self.source.release()                                                                                     │
 │    61   -         #     self.source = VIDEO_SOURCE(self.source.filepath)                                                          │
 │    62   -         #     equi_frame_rgb = next(self.source)                                                                        │
 │    63   -                                                                                                                         │
 │    52             # Get current orientation from the shared state                                                                 │
 │    53             rot = self.state.get_rot()                                                                                      │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════  │
 │    66   -         # fov_x = self.state.fov_x                                                                                      │
 │    54                                                                                                                             │
 │    55             # # Apply the equirectangular-to-perspective transform                                                          │
 │    56             perspective_frame = self.transform.transform(frame=equi_frame_rgb, rot=rot)                                     │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════  │
 │    70   -         # perspective_frame = equi_frame_rgb  # DEBUG                                                                   │
 │    57                                                                                                                             │
 │    58             # Create a VideoFrame for aiortc                                                                                │
 │    59             frame = VideoFrame.from_ndarray(perspective_frame, format="rgb24")                                              │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════  │
 │    72         try:                                                                                                                │
 │    73             data = json.loads(message)                                                                                      │
 │    74             print(f"Received control data: {data}")                                                                         │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════  │
 │    89   -         state.pitch = np.deg2rad(float(data.get("pitch", state.pitch)))                                                 │
 │    90   -         state.yaw = np.deg2rad(float(data.get("yaw", state.yaw)))                                                       │
 │    91   -         state.roll = np.deg2rad(float(data.get("roll", state.roll)))                                                    │
 │    75   +         state.pitch = np.deg2rad(float(data.get("pitch", np.rad2deg(state.pitch))))                                     │
 │    76   +         state.yaw = np.deg2rad(float(data.get("yaw", np.rad2deg(state.yaw))))                                           │
 │    77   +         state.roll = np.deg2rad(float(data.get("roll", np.rad2deg(state.roll))))                                        │
 │    78             state.fov_x = float(data.get("fov_x", state.fov_x))                                                             │
 │    79         except (json.JSONDecodeError, TypeError, ValueError) as e:                                                          │
 │    80             print(f"Could not process control command: {e}")                                                                │
 │    82                                                                                                                             │
 │    83     # Factory for creating the video track                                                                                  │
 │    84     def create_video_track(state: AppState):                                                                                │
 │    ═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════════  │
 │    99   -     # NOTE: Update this path to your 360 video file.                                                                    │
 │    100  -     # The asset directory is expected to be at the root of the repository.                                              │
 │    85         video_path = os.path.join(                                                                                          │
 │    86             Path(__file__).parents[2],                                                                                      │
 │    87             "xr-360-streamer-assets",                                                                                       │
 ╰───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯

╭───────────────────────────────────────────────────────────────────────────────────────────╮
│  > Could you create a new file instead of modifying 360_server_reprojection.py in-place?  │
╰───────────────────────────────────────────────────────────────────────────────────────────╯

✦ Of course. I will create a new file named 360_server_unity.py and adapt it for the Unity client. This new script will not serve the HTML
  file.

  First, I'll read the original 360_server_reprojection.py file to get the base content.
 ╭────────────────────────────────────────────────────────╮
 │ ✔  ReadFile ...References\\360_server_reprojection.py  │
 ╰────────────────────────────────────────────────────────╯
✦ Now I will create the 360_server_unity.py file. This script will be very similar to the original, but with the following changes:

   1. The on_control_message function will be updated to correctly parse the JSON from the Unity client.
   2. The FastAPI endpoint for serving the HTML file will be removed.
   3. The WebRTCServer will be used instead of the WebSocketServer.
 ╭───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
 │ ✔  WriteFile Writing to ...eferences\examples\360_server_unity.py                                                                 │
 │                                                                                                                                   │
 │      1 import asyncio                                                                                                             │
 │      2 import json                                                                                                                │
 │      3 import os                                                                                                                  │
 │      4 import time                                                                                                                │
 │      5 from pathlib import Path                                                                                                   │
 │      6                                                                                                                            │
 │      7 import numpy as np                                                                                                         │
 │      8 from aiortc import MediaStreamTrack                                                                                        │
 │      9 from av import VideoFrame                                                                                                  │
 │     10                                                                                                                            │
 │     11 from xr_360_camera_streamer.sources import FFmpegFileSource, OpenCVFileSource                                              │
 │     12 from xr_360_camera_streamer.streaming import WebRTCServer                                                                  │
 │     13 from xr_360_camera_streamer.transforms import EquilibEqui2Pers                                                             │
 │     14                                                                                                                            │
 │     15 # Params                                                                                                                   │
 │     16 VIDEO_SOURCE = FFmpegFileSource                                                                                            │
 │     17 # VIDEO_SOURCE = OpenCVFileSource                                                                                          │
 │     18                                                                                                                            │
 │     19                                                                                                                            │
 │     20 # Define a state object for orientation                                                                                    │
 │     21 class AppState:                                                                                                            │
 │     22     def __init__(self):                                                                                                    │
 │     23         self.pitch = 0.0                                                                                                   │
 │     24         self.yaw = 0.0                                                                                                     │
 │     25         self.roll = 0.0                                                                                                    │
 │     26         self.fov_x = 90.0  # Horizontal FOV in degrees                                                                     │
 │     27                                                                                                                            │
 │     28     def __repr__(self):                                                                                                    │
 │     29         return (                                                                                                           │
 │     30             f"<AppState pitch={self.pitch}, yaw={self.yaw}, roll={self.roll}, fov_x={self.fov_x}>"                         │
 │     31         )                                                                                                                  │
 │     32                                                                                                                            │
 │     33     def get_rot(self) -> dict[str, float]:                                                                                 │
 │     34         return {"pitch": self.pitch, "yaw": self.yaw, "roll": self.roll}                                                   │
 │     35                                                                                                                            │
 │     36                                                                                                                            │
 │     37 # Define a custom video track that applies reprojection                                                                    │
 │     38 class ReprojectionTrack(MediaStreamTrack):                                                                                 │
 │     39     kind = "video"                                                                                                         │
 │     40                                                                                                                            │
 │     41     def __init__(self, state: AppState, source: VIDEO_SOURCE, transform: EquilibEqui2Pers):                                │
 │     42         super().__init__()                                                                                                 │
 │     43         self.state = state                                                                                                 │
 │     44         self.source = source                                                                                               │
 │     45         self.transform = transform                                                                                         │
 │     46         self._timestamp = 0                                                                                                │
 │     47                                                                                                                            │
 │     48     async def recv(self):                                                                                                  │
 │     49         equi_frame_rgb = next(self.source)  # ALT                                                                          │
 │     50                                                                                                                            │
 │     51         # Get current orientation from the shared state                                                                    │
 │     52         rot = self.state.get_rot()                                                                                         │
 │     53                                                                                                                            │
 │     54         # # Apply the equirectangular-to-perspective transform                                                             │
 │     55         perspective_frame = self.transform.transform(frame=equi_frame_rgb, rot=rot)                                        │
 │     56                                                                                                                            │
 │     57         # Create a VideoFrame for aiortc                                                                                   │
 │     58         frame = VideoFrame.from_ndarray(perspective_frame, format="rgb24")                                                 │
 │     59                                                                                                                            │
 │     60         # Set timestamp                                                                                                    │
 │     61         time_base = 90000                                                                                                  │
 │     62         frame.pts = self._timestamp                                                                                        │
 │     63         frame.time_base = time_base                                                                                        │
 │     64         self._timestamp += int(time_base / self.source.fps)                                                                │
 │     65                                                                                                                            │
 │     66         return frame                                                                                                       │
 │     67                                                                                                                            │
 │     68                                                                                                                            │
 │     69 # Data channel handler to update orientation state                                                                         │
 │     70 def on_control_message(message: str, state: AppState):                                                                     │
 │     71     try:                                                                                                                   │
 │     72         data = json.loads(message)                                                                                         │
 │     73         print(f"Received control data: {data}")                                                                            │
 │     74         state.pitch = np.deg2rad(float(data.get("pitch", np.rad2deg(state.pitch))))                                        │
 │     75         state.yaw = np.deg2rad(float(data.get("yaw", np.rad2deg(state.yaw))))                                              │
 │     76         state.roll = np.deg2rad(float(data.get("roll", np.rad2deg(state.roll))))                                           │
 │     77         state.fov_x = float(data.get("fov_x", state.fov_x))                                                                │
 │     78     except (json.JSONDecodeError, TypeError, ValueError) as e:                                                             │
 │     79         print(f"Could not process control command: {e}")                                                                   │
 │     80                                                                                                                            │
 │     81                                                                                                                            │
 │     82 # Factory for creating the video track                                                                                     │
 │     83 def create_video_track(state: AppState):                                                                                   │
 │     84     video_path = os.path.join(                                                                                             │
 │     85         Path(__file__).parents[2],                                                                                         │
 │     86         "xr-360-streamer-assets",                                                                                          │
 │     87         "videos",                                                                                                          │
 │     88         "test_video.mp4",                                                                                                  │
 │     89     )                                                                                                                      │
 │     90                                                                                                                            │
 │     91     if not os.path.exists(video_path):                                                                                     │
 │     92         raise FileNotFoundError(                                                                                           │
 │     93             f"Video asset not found at {video_path}. "                                                                     │
 │     94             "Please download the assets from the repository "                                                              │
 │     95             "and place them in `xr-360-streamer-assets` at the project root."                                              │
 │     96         )                                                                                                                  │
 │     97                                                                                                                            │
 │     98     # Initialize the video source and transform                                                                            │
 │     99     video_source = VIDEO_SOURCE(video_path)                                                                                │
 │    100     video_transform = EquilibEqui2Pers(output_width=1280, output_height=720, fov_x=state.fov_x)                            │
 │    101                                                                                                                            │
 │    102     return ReprojectionTrack(state, video_source, video_transform)                                                         │
 │    103                                                                                                                            │
 │    104                                                                                                                            │
 │    105 # Start server                                                                                                             │
 │    106 if __name__ == "__main__":                                                                                                 │
 │    107     data_handlers = {                                                                                                      │
 │    108         "control": on_control_message,                                                                                     │
 │    109     }                                                                                                                      │
 │    110                                                                                                                            │
 │    111     server = WebRTCServer(                                                                                                 │
 │    112         video_track_factory=create_video_track,                                                                            │
 │    113         datachannel_handlers=data_handlers,                                                                                │
 │    114         state_factory=AppState,                                                                                            │
 │    115     )                                                                                                                      │
 │    116                                                                                                                            │
 │    117     server.run()                                                                                                           │
 ╰───────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
✦ I have created the 360_server_unity.py file. You should now be able to run this Python script and the Unity scene to establish a WebRTC
  connection, send the VR headset's orientation to the server, and receive a reprojected video stream.

  To run the system:

   1. Run the Python server:

   1     python C:/Users/G14/GitHub/xr-360-camera-viewer/Unity/References/xr-360-camera-streamer/python/examples/360_server_unity.py
   2. Open the Unity project.
   3. Open the `WebRTCReprojectionScene` scene.
   4. Enter Play Mode in the Unity Editor.

  The Unity application will then connect to the Python server, and you should see the status text in the Unity scene change to "Peers
  connected!". The server will then start streaming the reprojected video to the Unity client. The orientation of the VR camera in Unity
  will control the reprojection on the server.

  Is there anything else I can help you with?
```


```
╭────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
│  > @References/xr-360-camera-streamer/python/examples/360_server_reprojection.py works fine when using web browser client                                     │
│    (@References/xr-360-camera-streamer/python/examples/360_server_reprojection.html), but doesn't work when using the Unity client                            │
│    (@Unity/Assets/Scripts/WebRTCReprojectionClient.cs). The `pc.setLocalDescription()` method in line 172 of                                                  │
│    @References/xr-360-camera-streamer/python/src/xr_360_camera_streamer/streaming/webrtc_server.py fails. I ruled out potential errors (like the source video │
│     not being present), and made a notable observation that, when it works correctly for the browser, the `answer` object contains two media channels (a      │
│    video and an application), but for Unity, there's only one media channel (just the application). I will provide you the answers (parsed into               │
│    sdp.SessionDescription now): \nUnity (doesn't work):\ndescription                                                                                          │
│    <aiortc.sdp.SessionDescription object at 0x14af37b10>                                                                                                      │
│    special variables                                                                                                                                          │
│    function variables                                                                                                                                         │
│    group = [GroupDescription(semantic='BUNDLE', items=['0'])]                                                                                                 │
│    host = None                                                                                                                                                │
│    media = [<aiortc.sdp.MediaDescription object at 0x14af37c50>]                                                                                              │
│    special variables                                                                                                                                          │
│    function variables                                                                                                                                         │
│    0 = <aiortc.sdp.MediaDescription object at 0x14af37c50>                                                                                                    │
│    special variables                                                                                                                                          │
│    direction = None                                                                                                                                           │
│    dtls = RTCDtlsParameters(fingerprints=[RTCDtlsFingerprint(algorithm='sha-256',                                                                             │
│    value='53:27:FB:49:82:A6:59:0C:BF:7F:DB:97:4C:9D:0C:5A:BF:2A:E5:65:E7:AA:FB:D5:AE:15:3D:B4:51:B8:81:12'), RTCDtlsFingerprint(algorithm='sha-384',          │
│    value='F8:3F:3B:43:75:23:27:C2:DF:4B:44:D1:04:F6:B8:7A:81:7C:F0:42:28:4F:76:A5:EE:F7:A4:76:72:27:08:F2:4B:3E:7C:07:C9:57:67:11:72:B0:67:17:26:C8:B8:11'),  │
│    RTCDtlsFingerprint(algorithm='sha-512', value='82:BE:49:7B:22:56:F7:1E:28:6C:5B:B0:F4:03:FF:B7:6E:68:41:7D:AA:9E:9C:21:82:C6:60:E7:BE:13:39:6E:D3:06:14:C1 │
│    :05:FF:A7:61:A2:7F:B9:A2:D6:DB:2C:A8:95:F1:F3:87:1D:33:46:B1:CB:6D:8E:9C:26:C2:A3:75')], role='client')                                                    │
│    fmt = ['webrtc-datachannel']                                                                                                                               │
│    host = '0.0.0.0'                                                                                                                                           │
│    ice = RTCIceParameters(usernameFragment='emIA', password='Jrd4RSrYbL9ryjaNvtCf68', iceLite=False)                                                          │
│    ice_candidates = []                                                                                                                                        │
│    ice_candidates_complete = False                                                                                                                            │
│    ice_options = None                                                                                                                                         │
│    kind = 'application'                                                                                                                                       │
│    msid = None                                                                                                                                                │
│    port = 9                                                                                                                                                   │
│    profile = 'UDP/DTLS/SCTP'                                                                                                                                  │
│    rtcp_host = None                                                                                                                                           │
│    rtcp_mux = False                                                                                                                                           │
│    rtcp_port = None                                                                                                                                           │
│    rtp = RTCRtpParameters(codecs=[], headerExtensions=[], muxId='0', rtcp=RTCRtcpParameters(cname=None, mux=False, ssrc=None))                                │
│    sctpCapabilities = RTCSctpCapabilities(maxMessageSize=65536)                                                                                               │
│    sctp_port = 5000                                                                                                                                           │
│    sctpmap = {}                                                                                                                                               │
│    ssrc = []                                                                                                                                                  │
│    ssrc_group = []                                                                                                                                            │
│    len() = 1                                                                                                                                                  │
│    msid_semantic = [GroupDescription(semantic='WMS', items=['*'])]                                                                                            │
│    name = '-'                                                                                                                                                 │
│    origin = '- 3962190583 3962190583 IN IP4 0.0.0.0'                                                                                                          │
│    time = '0 0'                                                                                                                                               │
│    type = None                                                                                                                                                │
│    version = 0\nBrowser (works):\ndescription                                                                                                                 │
│    <aiortc.sdp.SessionDescription object at 0x16c52d310>                                                                                                      │
│    special variables                                                                                                                                          │
│    function variables                                                                                                                                         │
│    group = [GroupDescription(semantic='BUNDLE', items=['0', '1'])]                                                                                            │
│    special variables                                                                                                                                          │
│    function variables                                                                                                                                         │
│    0 = GroupDescription(semantic='BUNDLE', items=['0', '1'])                                                                                                  │
│    len() = 1                                                                                                                                                  │
│    host = None                                                                                                                                                │
│    media = [<aiortc.sdp.MediaDescription object at 0x16c5350f0>, <aiortc.sdp.MediaDescription object at 0x16c317e30>]                                         │
│    special variables                                                                                                                                          │
│    function variables                                                                                                                                         │
│    0 = <aiortc.sdp.MediaDescription object at 0x16c5350f0>                                                                                                    │
│    special variables                                                                                                                                          │
│    direction = 'sendonly'                                                                                                                                     │
│    dtls = RTCDtlsParameters(fingerprints=[RTCDtlsFingerprint(algorithm='sha-256',                                                                             │
│    value='0B:C0:95:94:CC:B7:E4:87:C4:8D:0A:FF:8A:D0:13:C1:6E:A2:92:14:90:2A:95:26:6C:53:8E:B9:47:9F:A7:06'), RTCDtlsFingerprint(algorithm='sha-384',          │
│    value='A7:39:45:6C:30:7A:FC:53:66:B0:47:D3:4C:89:1D:A2:7C:E5:1F:E0:52:09:5F:2E:49:B1:56:2B:B9:38:AE:85:13:CB:81:DC:28:21:04:A9:42:62:51:48:41:C8:F1:97'),  │
│    RTCDtlsFingerprint(algorithm='sha-512', value='AF:58:D1:0F:67:B5:EB:42:88:95:68:11:12:EF:97:06:A7:01:12:0C:EC:74:AE:7F:5F:D8:BF:D5:A4:A9:B1:53:D9:FC:CF:48 │
│    :74:58:2A:7F:6F:12:33:2A:3B:E8:CC:FB:8C:B2:1F:75:AC:84:22:42:65:07:DC:9E:67:8A:F9:85')], role='client')                                                    │
│    fmt = [120, 124, 126, 127, 105, 106]                                                                                                                       │
│    host = '0.0.0.0'                                                                                                                                           │
│    ice = RTCIceParameters(usernameFragment='BwEN', password='yJHWIpRr2qhLgWj8IXWXIF', iceLite=False)                                                          │
│    ice_candidates = []                                                                                                                                        │
│    ice_candidates_complete = False                                                                                                                            │
│    ice_options = None                                                                                                                                         │
│    kind = 'video'                                                                                                                                             │
│    msid = '692882a2-c437-4041-97ff-46a5a735e2e8 cc36010c-deed-4738-9e18-a7b04029aa03'                                                                         │
│    port = 9                                                                                                                                                   │
│    profile = 'UDP/TLS/RTP/SAVPF'                                                                                                                              │
│    rtcp_host = '0.0.0.0'                                                                                                                                      │
│    rtcp_mux = True                                                                                                                                            │
│    rtcp_port = 9                                                                                                                                              │
│    rtp = RTCRtpParameters(codecs=[RTCRtpCodecParameters(mimeType='video/VP8', clockRate=90000, channels=None, payloadType=120,                                │
│    rtcpFeedback=[RTCRtcpFeedback(type='nack', parameter=None), RTCRtcpFeedback(type='nack', parameter='pli'), RTCRtcpFeedback(type='goog-remb',               │
│    parameter=None)], parameters={}), RTCRtpCodecParameters(mimeType='video/rtx', clockRate=90000, channels=None, payloadType=124, rtcpFeedback=[],            │
│    parameters={'apt': 120}), RTCRtpCodecParameters(mimeType='video/H264', clockRate=90000, channels=None, payloadType=126,                                    │
│    rtcpFeedback=[RTCRtcpFeedback(type='nack', parameter=None), RTCRtcpFeedback(type='nack', parameter='pli'), RTCRtcpFeedback(type='goog-remb',               │
│    parameter=None)], parameters={'level-asymmetry-allowed': '1', 'packetization-mode': '1', 'profile-level-id': '42e01f'}),                                   │
│    RTCRtpCodecParameters(mimeType='video/rtx', clockRate=90000, channels=None, payloadType=127, rtcpFeedback=[], parameters={'apt': 126}),                    │
│    RTCRtpCodecParameters(mimeType='video/H264', clockRate=90000, channels=None, payloadType=105, rtcpFeedb...                                                 │
│    sctpCapabilities = None                                                                                                                                    │
│    sctp_port = None                                                                                                                                           │
│    sctpmap = {}                                                                                                                                               │
│    ssrc = [SsrcDescription(ssrc=2600950611, cname='f0d01b80-a318-46c1-8d76-f1f1ffde0772', msid=None, mslabel=None, label=None),                               │
│    SsrcDescription(ssrc=2505926521, cname='f0d01b80-a318-46c1-8d76-f1f1ffde0772', msid=None, mslabel=None, label=None)]                                       │
│    ssrc_group = [GroupDescription(semantic='FID', items=[2600950611, 2505926521])]                                                                            │
│    1 = <aiortc.sdp.MediaDescription object at 0x16c317e30>                                                                                                    │
│    special variables                                                                                                                                          │
│    direction = None                                                                                                                                           │
│    dtls = RTCDtlsParameters(fingerprints=[RTCDtlsFingerprint(algorithm='sha-256',                                                                             │
│    value='0B:C0:95:94:CC:B7:E4:87:C4:8D:0A:FF:8A:D0:13:C1:6E:A2:92:14:90:2A:95:26:6C:53:8E:B9:47:9F:A7:06'), RTCDtlsFingerprint(algorithm='sha-384',          │
│    value='A7:39:45:6C:30:7A:FC:53:66:B0:47:D3:4C:89:1D:A2:7C:E5:1F:E0:52:09:5F:2E:49:B1:56:2B:B9:38:AE:85:13:CB:81:DC:28:21:04:A9:42:62:51:48:41:C8:F1:97'),  │
│    RTCDtlsFingerprint(algorithm='sha-512', value='AF:58:D1:0F:67:B5:EB:42:88:95:68:11:12:EF:97:06:A7:01:12:0C:EC:74:AE:7F:5F:D8:BF:D5:A4:A9:B1:53:D9:FC:CF:48 │
│    :74:58:2A:7F:6F:12:33:2A:3B:E8:CC:FB:8C:B2:1F:75:AC:84:22:42:65:07:DC:9E:67:8A:F9:85')], role='client')                                                    │
│    fmt = ['webrtc-datachannel']                                                                                                                               │
│    host = '0.0.0.0'                                                                                                                                           │
│    ice = RTCIceParameters(usernameFragment='BwEN', password='yJHWIpRr2qhLgWj8IXWXIF', iceLite=False)                                                          │
│    ice_candidates = []                                                                                                                                        │
│    ice_candidates_complete = False                                                                                                                            │
│    ice_options = None                                                                                                                                         │
│    kind = 'application'                                                                                                                                       │
│    msid = None                                                                                                                                                │
│    port = 9                                                                                                                                                   │
│    profile = 'UDP/DTLS/SCTP'                                                                                                                                  │
│    rtcp_host = None                                                                                                                                           │
│    rtcp_mux = False                                                                                                                                           │
│    rtcp_port = None                                                                                                                                           │
│    rtp = RTCRtpParameters(codecs=[], headerExtensions=[], muxId='1', rtcp=RTCRtcpParameters(cname=None, mux=False, ssrc=None))                                │
│    sctpCapabilities = RTCSctpCapabilities(maxMessageSize=65536)                                                                                               │
│    sctp_port = 5000                                                                                                                                           │
│    sctpmap = {}                                                                                                                                               │
│    ssrc = []                                                                                                                                                  │
│    ssrc_group = []                                                                                                                                            │
│    len() = 2                                                                                                                                                  │
│    msid_semantic = [GroupDescription(semantic='WMS', items=['*'])]                                                                                            │
│    name = '-'                                                                                                                                                 │
│    origin = '- 3962189471 3962189471 IN IP4 0.0.0.0'                                                                                                          │
│    time = '0 0'                                                                                                                                               │
│    type = None                                                                                                                                                │
│    version = 0                                                                                                                                                │
╰────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
```

```
✦ Based on your detailed analysis, the issue stems from the Unity client not requesting a video stream in its SDP offer, while the server unconditionally adds a video track. This leads to an
  inconsistent state in aiortc that causes pc.setLocalDescription(answer) to fail.

  To resolve this, I will modify References/xr-360-camera-streamer/python/src/xr_360_camera_streamer/streaming/webrtc_server.py to conditionally add the video track only if the client's offer
  indicates it wants to receive video.

  Here is the plan:
   1. Read the content of webrtc_server.py.
   2. In the _create_offer_handler method, I will move the video track creation logic.
   3. The new logic will first set the remote description from the client's offer.
   4. Then, it will parse the offer to check for a video media section.
   5. The video track will only be added if the client has requested video.
   6. Finally, it will create and set the local description as before.

  This will make the server robust to clients that do not wish to receive video, resolving the issue with the Unity client. I will now proceed with reading the file.
```
