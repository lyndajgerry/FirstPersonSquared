using Byn.Common;
using Byn.Media;
using Byn.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap.Unity;
using LeapInternal;
//using MessagePack;
//using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Apple.TV;
using UnityEngine.UI;
using UnityEngine.XR.WSA.WebCam;

public class VideoChatService : MonoBehaviour
{

    [Header("Camera Settings, string=name, int=index")] 
    public string CameraNameOrIndex = "0";
    
    
    [Header("Raw Image References")]
    public RawImage LocalRawImage;
    public RawImage RemoteRawImage;

    [Header("Hand Hold References")] 
    public HandHold LocalHandHold;
    public HandHold RemoteHandHold;
    
    
    [Header("WebRtc Settings")]

    public string SecretPassword;


    
    
    /// <summary>
    /// This is a test server. Don't use in production! The server code is in a zip file in WebRtcNetwork
    /// </summary>
    public string uSignalingUrl = "ws://signaling.because-why-not.com/callapp";

    /// <summary>
    /// By default the secure version is currently only used in WebGL builds as
    /// some browsers require. Unity old mono version comes with a SSL implementation
    /// that can be quite slow and hangs sometimes
    /// </summary>
    public string uSecureSignalingUrl = "wss://signaling.because-why-not.com/callapp";

    /// <summary>
    /// If set to true only the secure signaling url will be used.
    /// </summary>
    public bool uForceSecureSignaling = false;


    /// <summary>
    /// Ice server is either a stun or a turn server used to get trough
    /// the firewall.
    /// Warning: make sure the url is in a valid format and
    /// starts with stun: or turn:
    /// 
    /// WebRTC will try many different ways to connect the peers so if
    /// this server is not available it might still be able
    /// to establish a direct connection or use the second ice server.
    /// 
    /// If you need more than two servers change the CreateNetworkConfig
    /// method.
    /// </summary>
    public string uIceServer = "stun:stun.because-why-not.com:443";

    //
    public string uIceServerUser = "";
    public string uIceServerPassword = "";

    /// <summary>
    /// Second ice server. As I can't guarantee my one is always online.
    /// If you need more than two servers or username / password then
    /// change the CreateNetworkConfig method.
    /// </summary>
    public string uIceServer2 = "stun:stun.l.google.com:19302";

    /// <summary>
    /// Set true to use send the WebRTC log + wrapper log output to the unity log.
    /// </summary>
    public bool uLog = false;

    /// <summary>
    /// Debug console to be able to see the unity log on every platform
    /// </summary>
    public bool uDebugConsole = false;

    /// <summary>
    /// Do not change. This length is enforced on the server side to avoid abuse.
    /// </summary>
    public const int MAX_CODE_LENGTH = 256;

    
    
    [Header("Debug, do not change")]
    public double LatestDataSent = double.MinValue;
    //public double LatestRightDataSent = double.MinValue;
    
    
    
    /// <summary>
    /// Call class handling all the functionality
    /// </summary>
    protected ICall mCall;


    private static bool sLogSet = false;

//    protected CallAppUi mUi;

    //Configuration used for the next call after SetupCall
    protected MediaConfig mMediaConfig;

    //Configuration for the currently active call
    /// <summary>
    /// Set to true after Join is called.
    /// Set to false after either Join failed or the call
    /// ended / network failed / user exit
    /// 
    /// </summary>
    private bool mCallActive = false;
    private string mUseAddress = null;
    protected MediaConfig mMediaConfigInUse;
    private ConnectionId mRemoteUserId = ConnectionId.INVALID;


    private bool mAutoRejoin = true;
    private float mRejoinTime = 2;

    private bool mLocalFrameEvents = true;

    /// <summary>
    /// Used to backup the original sleep timeout value.
    /// Will be restored after a call has ended
    /// </summary>
    private int mSleepTimeoutBackup;

    /// <summary>
    /// For customization. Set to false to allow devices to sleep
    /// even if a call is active. 
    /// </summary>
    private bool mBlockSleep = true;


    private Texture2D _localTexture;
    private Texture2D _remoteTexture;
    
    #region Calls from unity
    //
    protected virtual void Awake()
    {

        LocalHandHold.IsLocalHands = true;
        RemoteHandHold.IsLocalHands = false;
//        mUi = GetComponent<CallAppUi>();

    }

    IEnumerator Start()
    {
        
        //(Jonathon)
        //Other things have to initialize as well, so  wait a frame
        
        yield return null;
        
                
        Init();
        mMediaConfig = CreateMediaConfig();
        mMediaConfigInUse = mMediaConfig;
        SetupCall();
        Join(SecretPassword);
    }


    private void OnDestroy()
    {
        CleanupCall();
    }
    private void OnGUI()
    {
        DebugHelper.DrawConsole();
    }
    /// <summary>
    /// The call object needs to be updated regularly to sync data received via webrtc with
    /// unity. All events will be triggered during the update method in the unity main thread
    /// to avoid multi threading errors
    /// </summary>
    protected virtual void Update()
    {
        if (mCall != null)
        {

            //figure out hand datas we need to send and send them
            List<HandData> leftOrderedDatas = HandMath.GetOrderedList(LocalHandHold.HandDatas);
            foreach (HandData data in leftOrderedDatas)
            {
                if (data.NetworkTimeStamp > LatestDataSent)
                {
                    string message = JsonUtility.ToJson(data);
//                    string message = JsonConvert.SerializeObject(data,Formatting.Indented, 
//                        new JsonSerializerSettings { 
//                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
//                        });
                    
                    Send(message);
                }
            }

            if (leftOrderedDatas.Count>0) LatestDataSent = leftOrderedDatas[leftOrderedDatas.Count - 1].NetworkTimeStamp;
            
//            List<HandData> rightOrderedDatas = HandMath.GetOrderedList(LocalHandHold.RightHandDatas);
//            foreach (HandData data in rightOrderedDatas)
//            {
//                if (data.NetworkTimeStamp > LatestRightDataSent)
//                {
//                    string message = JsonUtility.ToJson(data);
//
////                    string message = JsonConvert.SerializeObject(data,Formatting.Indented, 
////                        new JsonSerializerSettings { 
////                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
////                            
////                        });
//
//                    Send(message);
//                }
//            }
//
//            if (rightOrderedDatas.Count>0) LatestRightDataSent = rightOrderedDatas[rightOrderedDatas.Count - 1].NetworkTimeStamp;
            
            //update the call object. This will trigger all buffered events to be fired
            //to ensure it is done in the unity thread at a convenient time.
            mCall.Update();
        }
    }
    #endregion

    protected virtual void Init()
    {

        if (uDebugConsole)
            DebugHelper.ActivateConsole();
        if (uLog)
        {
            if (sLogSet == false)
            {
                SLog.SetLogger(OnLog);
                sLogSet = true;
                SLog.L("Log active");
            }
        }

        //This can be used to get the native webrtc log but causes a huge slowdown
        //only use if not webgl
        bool nativeWebrtcLog = true;

        if (nativeWebrtcLog)
        {
#if UNITY_ANDROID
            //uncomment for debug log via log cat
            //Byn.Net.Native.NativeWebRtcNetworkFactory.SetNativeLogLevel(WebRtcCSharp.LoggingSeverity.LS_INFO);
#elif UNITY_IOS
			//uncomment for log output via xcode
			//Byn.Net.Native.NativeWebRtcNetworkFactory.SetNativeLogLevel(WebRtcCSharp.LoggingSeverity.LS_INFO);
#elif (!UNITY_WEBGL || UNITY_EDITOR)
            Byn.Net.Native.NativeWebRtcNetworkFactory.SetNativeLogLevel(WebRtcCSharp.LoggingSeverity.LS_INFO);
            //Byn.Net.Native.NativeWebRtcNetworkFactory.SetNativeLogToSLog(WebRtcCSharp.LoggingSeverity.LS_INFO);
#else
            //webgl. logging isn't supported here and has to be done via the browser.
            Debug.LogWarning("Platform doesn't support native webrtc logging.");
#endif
            
        }

        if (UnityCallFactory.Instance == null)
        {
            Debug.LogError("UnityCallFactory failed to initialize");
        }

        //not yet implemented
        //CustomUnityVideo.Instance.Register();

    }


    private static void OnLog(object msg, string[] tags)
    {
        StringBuilder builder = new StringBuilder();
        bool warning = false;
        bool error = false;
        builder.Append("TAGS:[");
        foreach (var v in tags)
        {
            builder.Append(v);
            builder.Append(",");
            if (v == SLog.TAG_ERROR || v == SLog.TAG_EXCEPTION)
            {
                error = true;
            }
            else if (v == SLog.TAG_WARNING)
            {
                warning = true;
            }
        }
        builder.Append("]");
        builder.Append(msg);
        if (error)
        {
            LogError(builder.ToString());
        }
        else if (warning)
        {
            LogWarning(builder.ToString());
        }
        else
        {
            Log(builder.ToString());
        }
    }

    private static void Log(string s)
    {
        if (s.Length > 2048 && Application.platform != RuntimePlatform.Android)
        {
            foreach (string splitMsg in SplitLongMsgs(s))
            {
                Debug.Log(splitMsg);
            }
        }
        else
        {
            Debug.Log(s);
        }
    }
    private static void LogWarning(string s)
    {
        if (s.Length > 2048 && Application.platform != RuntimePlatform.Android)
        {
            foreach (string splitMsg in SplitLongMsgs(s))
            {
                Debug.LogWarning(splitMsg);
            }
        }
        else
        {
            Debug.LogWarning(s);
        }
    }
    private static void LogError(string s)
    {
        if (s.Length > 2048 && Application.platform != RuntimePlatform.Android)
        {
            foreach (string splitMsg in SplitLongMsgs(s))
            {
                Debug.LogError(splitMsg);
            }
        }
        else
        {
            Debug.LogError(s);
        }
    }

    private static string[] SplitLongMsgs(string s)
    {
        const int maxLength = 2048;
        int count = s.Length / maxLength + 1;
        string[] messages = new string[count];
        for (int i = 0; i < count; i++)
        {
            int start = i * maxLength;
            int length = s.Length - start;
            if (length > maxLength)
                length = maxLength;
            messages[i] = "[" + (i + 1) + "/" + count + "]" + s.Substring(start, length);

        }
        return messages;
    }


    protected virtual NetworkConfig CreateNetworkConfig()
    {
        NetworkConfig netConfig = new NetworkConfig();
        if (string.IsNullOrEmpty(uIceServer) == false)
            netConfig.IceServers.Add(new IceServer(uIceServer, uIceServerUser, uIceServerPassword));
        if (string.IsNullOrEmpty(uIceServer2) == false)
            netConfig.IceServers.Add(new IceServer(uIceServer2));

        if (Application.platform == RuntimePlatform.WebGLPlayer || uForceSecureSignaling)
        {
            netConfig.SignalingUrl = uSecureSignalingUrl;
        }
        else
        {
            netConfig.SignalingUrl = uSignalingUrl;
        }

        if (string.IsNullOrEmpty(netConfig.SignalingUrl))
        {
            throw new InvalidOperationException("set signaling url is null or empty");
        }
        return netConfig;
    }

    /// <summary>
    /// Creates the call object and uses the configure method to activate the 
    /// video / audio support if the values are set to true.
    /// </summary>
    /// generating new frames after this call so the user can see himself before
    /// the call is connected.</param>
    public virtual void SetupCall()
    {
        Append("Setting up ...");

        //hacks to turn off certain connection types. If both set to true only
        //turn servers are used. This helps simulating a NAT that doesn't support
        //opening ports.
        //hack to turn off direct connections
        //Byn.Net.Native.AWebRtcPeer.sDebugIgnoreTypHost = true;
        //hack to turn off connections via stun servers
        //Byn.Net.Native.WebRtcDataPeer.sDebugIgnoreTypSrflx = true;

        NetworkConfig netConfig = CreateNetworkConfig();


        Debug.Log("Creating call using NetworkConfig:" + netConfig);
        //setup the server
        mCall = UnityCallFactory.Instance.Create(netConfig);
        if (mCall == null)
        {
            Append("Failed to create the call");
            return;
        }
        mCall.LocalFrameEvents = mLocalFrameEvents;
        string[] devices = UnityCallFactory.Instance.GetVideoDevices();
        if (devices == null || devices.Length == 0)
        {
            Debug.Log("no device found or no device information available");
        }
        else
        {
            foreach (string s in devices)
                Debug.Log("device found: " + s);
        }
        Append("Call created!");
        mCall.CallEvent += Call_CallEvent;

        //this happens in awake now to allow an ui or other external app
        //to change media config before calling SetupCall
        //mMediaConfig = CreateMediaConfig();

        //make a deep clone to avoid confusion if settings are changed
        //at runtime. 
        mMediaConfigInUse = mMediaConfig.DeepClone();
        Debug.Log("Configure call using MediaConfig: " + mMediaConfigInUse);
        mCall.Configure(mMediaConfigInUse);
//        mUi.SetGuiState(false);

        if(mBlockSleep)
        {
            //backup sleep timeout and set it to never sleep
            mSleepTimeoutBackup = Screen.sleepTimeout;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }



    /// <summary>
    /// Handler of call events.
    /// 
    /// Can be customized in via subclasses.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void Call_CallEvent(object sender, CallEventArgs e)
    {
        switch (e.Type)
        {
            case CallEventType.CallAccepted:
                //Outgoing call was successful or an incoming call arrived
                Append("Connection established");
                mRemoteUserId = ((CallAcceptedEventArgs)e).ConnectionId;
                Debug.Log("New connection with id: " + mRemoteUserId
                    + " audio:" + mCall.HasAudioTrack(mRemoteUserId)
                    + " video:" + mCall.HasVideoTrack(mRemoteUserId));
                break;
            case CallEventType.CallEnded:
                //Call was ended / one of the users hung up -> reset the app
                Append("Call ended");
                InternalResetCall();
                break;
            case CallEventType.ListeningFailed:
                //listening for incoming connections failed
                //this usually means a user is using the string / room name already to wait for incoming calls
                //try to connect to this user
                //(note might also mean the server is down or the name is invalid in which case call will fail as well)
                mCall.Call(mUseAddress);
                break;

            case CallEventType.ConnectionFailed:
                {
                    Byn.Media.ErrorEventArgs args = e as Byn.Media.ErrorEventArgs;
                    Append("Connection failed error: " + args.ErrorMessage);
                    InternalResetCall();
                }
                break;
            case CallEventType.ConfigurationFailed:
                {
                    Byn.Media.ErrorEventArgs args = e as Byn.Media.ErrorEventArgs;
                    Append("Configuration failed error: " + args.ErrorMessage);
                    InternalResetCall();
                }
                break;

            case CallEventType.FrameUpdate:
                {

                    //new frame received from webrtc (either from local camera or network)
                    if (e is FrameUpdateEventArgs)
                    {
                        UpdateFrame((FrameUpdateEventArgs)e);
                    }
                    break;
                }

            case CallEventType.Message:
                {
                    //text message received
                    MessageEventArgs args = e as MessageEventArgs;
                    //Append(args.Content);
                    //Debug.Log("Recieved: " + args.Content);
                    //HandData data = JsonConvert.DeserializeObject<HandData>(args.Content);
                    HandData data = JsonUtility.FromJson<HandData>(args.Content);
                    //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(args.Content);
                    //HandData data = MessagePackSerializer.Deserialize<HandData>(buffer);
//                    if (data.IsRightHand) RemoteHandHold.RightHandDatas.Add(data);
                    RemoteHandHold.HandDatas.Add(data);
                    break;
                }
            case CallEventType.WaitForIncomingCall:
                {
                    //the chat app will wait for another app to connect via the same string
                    WaitForIncomingCallEventArgs args = e as WaitForIncomingCallEventArgs;
                    Append("Waiting for incoming call address: " + args.Address);
                    break;
                }
        }

    }

    /// <summary>
    /// Destroys the call. Used if unity destroys the object or if a call
    /// ended / failed due to an error.
    /// 
    /// </summary>
    protected virtual void CleanupCall()
    {
        if (mCall != null)
        {
            mCallActive = false;
            mRemoteUserId = ConnectionId.INVALID;
            Debug.Log("Destroying call!");
            mCall.CallEvent -= Call_CallEvent;
            mCall.Dispose();
            mCall = null;
            //call the garbage collector. This isn't needed but helps discovering
            //memory bugs early on.
            Debug.Log("Triggering garbage collection");
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Debug.Log("Call destroyed");

            if (mBlockSleep)
            {
                //revert to the original value
                Screen.sleepTimeout = mSleepTimeoutBackup;
            }
        }
    }


    /// <summary>
    /// Create the default configuration for this CallApp instance.
    /// This can be overwritten in a subclass allowing the creation custom apps that
    /// use a slightly different configuration.
    /// </summary>
    /// <returns></returns>
    public virtual MediaConfig CreateMediaConfig()
    {
        MediaConfig mediaConfig = new MediaConfig();
        //testing echo cancellation (native only)
        bool useEchoCancellation = false;
        if(useEchoCancellation)
        {
#if !UNITY_WEBGL
            var nativeConfig = new Byn.Media.Native.NativeMediaConfig();
            nativeConfig.AudioOptions.echo_cancellation = true;
            nativeConfig.AudioOptions.extended_filter_aec = true;
            nativeConfig.AudioOptions.delay_agnostic_aec = true;

            mediaConfig = nativeConfig;
#endif 
        }



        //use video and audio by default (the UI is toggled on by default as well it will change on click )
        mediaConfig.Audio = true;
        mediaConfig.Video = true;
//        mediaConfig.VideoDeviceName = CameraName;
//        if (mediaConfig.VideoDeviceName == "")
//            mediaConfig.VideoDeviceName = null;
        
        int n = -1;
        bool isNumeric = int.TryParse(CameraNameOrIndex, out n);

        if (isNumeric)
        {
            mediaConfig.VideoDeviceName = UnityCallFactory.Instance.GetVideoDevices()[n];
        }
        else
        {
            mediaConfig.VideoDeviceName = CameraNameOrIndex;
        }
        

        //keep the resolution low.
        //This helps avoiding problems with very weak CPU's and very high resolution cameras
        //(apparently a problem with win10 tablets)
        mediaConfig.MinWidth = 160;
        mediaConfig.MinHeight = 120;
        mediaConfig.MaxWidth = 1920;
        mediaConfig.MaxHeight = 1080;
        mediaConfig.IdealWidth = 640;
        mediaConfig.IdealHeight = 480;
        mediaConfig.IdealFrameRate = 30;
        return mediaConfig;
    }

    /// <summary>
    /// Destroys the call object and shows the setup screen again.
    /// Called after a call ends or an error occurred.
    /// </summary>
    public virtual void ResetCall()
    {
        //outside quits. don't rejoin automatically
        mAutoRejoin = false;
        InternalResetCall();
    }

    private void InternalResetCall()
    {
        CleanupCall();
//        mUi.SetGuiState(true);
        if (mAutoRejoin)
            StartCoroutine(CoroutineRejoin());
    }

    public virtual void SetRemoteVolume(float volume)
    {
        if (mCall == null)
            return;
        if(mRemoteUserId == ConnectionId.INVALID)
        {
            return;
        }
        mCall.SetVolume(volume, mRemoteUserId);
    }


    /// <summary>
    /// Returns a list of video devices for the UI to show.
    /// This is used to avoid having the UI directly access the UnityCallFactory.
    /// </summary>
    /// <returns></returns>
    public string[] GetVideoDevices()
    {
        if (CanSelectVideoDevice())
        {
            List<string> devices = new List<string>();
            string[] videoDevices = UnityCallFactory.Instance.GetVideoDevices();
            devices.Add("Any");
            devices.AddRange(videoDevices);
            return devices.ToArray();
        }
        else
        {
            return new string[] { "Default" };
        };
    }

    /// <summary>
    /// Used by the UI
    /// </summary>
    /// <returns></returns>
    public bool CanSelectVideoDevice()
    {
        return UnityCallFactory.Instance.CanSelectVideoDevice();
    }

    /// <summary>
    /// Called by UI when the join buttin is pressed.
    /// </summary>
    /// <param name="address"></param>
    public virtual void Join(string address)
    {
        if (address.Length > MAX_CODE_LENGTH)
            throw new ArgumentException("Address can't be longer than " + MAX_CODE_LENGTH);
        mUseAddress = address;
        InternalJoin();
    }
    private void InternalJoin()
    {
        if (mCallActive)
        {
            Debug.LogError("Join call failed. Call is already/still active");
            return;
        }
        Debug.Log("Try listing on address: " + mUseAddress);
        mCallActive = true;
        this.mCall.Listen(mUseAddress);
    }

    private IEnumerator CoroutineRejoin()
    {
        yield return new WaitForSecondsRealtime(mRejoinTime);
        SetupCall();
        InternalJoin();
    }

    /// <summary>
    /// Called by ui to send a message.
    /// </summary>
    /// <param name="msg"></param>
    public virtual void Send(string msg)
    {
        this.mCall.Send(msg);
    }


    public void SetAudio(bool value)
    {
        mMediaConfig.Audio = value;
    }
    public void SetVideo(bool value)
    {
        mMediaConfig.Video = value;
    }
    public void SetVideoDevice(string deviceName)
    {
        mMediaConfig.VideoDeviceName = deviceName;
    }

    public void SetIdealResolution(int width, int height)
    {
        mMediaConfig.IdealWidth = width;
        mMediaConfig.IdealHeight = height;
    }

    public void SetIdealFps(int fps)
    {
        mMediaConfig.IdealFrameRate = fps;
    }


    public void SetShowLocalVideo(bool showLocalVideo)
    {
        mLocalFrameEvents = showLocalVideo;
    }
    
    public void SetAutoRejoin(bool rejoin, float rejoinTime = 2)
    {
        mAutoRejoin = rejoin;
        mRejoinTime = rejoinTime;
    }

    public bool GetLoudspeakerStatus()
    {
        //check if call is created to ensure this isn't called before initialization
        if(mCall != null)
        {
            return UnityCallFactory.Instance.GetLoudspeakerStatus();
        }
        return false;
    }

    public void SetLoudspeakerStatus(bool state)
    {
        //check if call is created to ensure this isn't called before initialization
        if (mCall != null)
        {
            UnityCallFactory.Instance.SetLoudspeakerStatus(state);
        }
    }

    protected virtual void UpdateFrame(FrameUpdateEventArgs frameUpdateEventArgs)
    {
        //the avoid wasting CPU time the library uses the format returned by the browser -> ABGR little endian thus
        //the bytes are in order R G B A
        //Unity seem to use this byte order but also flips the image horizontally (reading the last row first?)
        //this is reversed using UI to avoid wasting CPU time

        //Debug.Log("frame update remote: " + frameUpdateEventArgs.IsRemote);

        if (frameUpdateEventArgs.IsRemote == false)
        {
    //        mUi.UpdateLocalTexture(frameUpdateEventArgs.Frame, frameUpdateEventArgs.Format);
            UpdateImage(LocalRawImage,ref _localTexture,frameUpdateEventArgs.Frame,frameUpdateEventArgs.Format);
        }
        else
        {
    //        mUi.UpdateRemoteTexture(frameUpdateEventArgs.Frame, frameUpdateEventArgs.Format);
            UpdateImage(RemoteRawImage,ref _remoteTexture,frameUpdateEventArgs.Frame,frameUpdateEventArgs.Format);
        }
    }
    
    /// <summary>
    /// Updates the local video. If the frame is null it will hide the video image
    /// </summary>
    /// <param name="frame"></param>
    protected virtual void UpdateImage(RawImage image, ref Texture2D tex, RawFrame frame, FramePixelFormat format)
    {
        
        if (image != null)
        {
            if (frame != null)
            {
                UpdateTexture(ref tex, frame, format);
                image.texture = tex;
                if (image.gameObject.activeSelf == false)
                {
                    image.gameObject.SetActive(true);
                }
                //apply rotation
                //watch out uLocalVideoImage should be scaled -1 X to make the local camera appear mirrored
                //it should also be scaled -1 Y because Unity reads the image from bottom to top
                image.transform.rotation = Quaternion.Euler(0, 0, frame.Rotation);

//                mHasLocalVideo = true;
//                mLocalFrameCounter++;
//                mLocalVideoWidth = frame.Width;
//                mLocalVideoHeight = frame.Height;
//                mLocalVideoFormat = format;
            }
            else
            {
                //app shutdown. reset values
//                mHasLocalVideo = false;
                image.texture = null;
                image.transform.rotation = Quaternion.Euler(0, 0, 0);
                image.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Updates the texture based on the given frame update.
    /// 
    /// Returns true if a complete new texture was created
    /// </summary>
    /// <param name="tex"></param>
    /// <param name="frame"></param>
    protected bool UpdateTexture(ref Texture2D tex, RawFrame frame, FramePixelFormat format)
    {
        bool newTextureCreated = false;
        //texture exists but has the wrong height /width? -> destroy it and set the value to null
        if (tex != null && (tex.width != frame.Width || tex.height != frame.Height))
        {
            Texture2D.Destroy(tex);
            tex = null;
        }

        //no texture? create a new one first
        if (tex == null)
        {
            newTextureCreated = true;
            Debug.Log("Creating new texture with resolution " + frame.Width + "x" + frame.Height + " Format:" + format);

            //so far only ABGR is really supported. this will change later
            if (format == FramePixelFormat.ABGR)
            {
                tex = new Texture2D(frame.Width, frame.Height, TextureFormat.RGBA32, false);
            }
            else
            {
                Debug.LogWarning("YUY2 texture is set. This is only for testing");
                tex = new Texture2D(frame.Width, frame.Height, TextureFormat.YUY2, false);
            }

            tex.wrapMode = TextureWrapMode.Clamp;
        }
        //copy image data into the texture and apply
        //Watch out the RawImage has the top pixels in the top row but
        //unity has the top pixels in the bottom row. Result is an image that is
        //flipped. Fixing this here would waste a lot of CPU power thus
        //the UI will simply set scale.Y of the UI element to -1 to reverse this.
        tex.LoadRawTextureData(frame.Buffer);
        tex.Apply();
        return newTextureCreated;
    }


    private void Append(string txt)
    {
  //      mUi.Append(txt);
    }
}
