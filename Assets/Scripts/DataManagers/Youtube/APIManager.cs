using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using YouTubeLive.Json;
namespace YouTubeLive {
    [RequireComponent (typeof (LiveStatus))]
    public class APIManager : SingletonMonoBehaviour<APIManager> {
        private bool receiveChat = true;
        private bool isFirstTry = true;
        private int pollingIntervalMillis;
        [SerializeField, Tooltip ("初期化や取得ができなかった時、時間が短すぎた時に用います。3000以上が望ましいです。")] private int defaultPollingIntervalMillis = 5000;
        private int noItemsRespondCount = 0;
        [SerializeField, Tooltip ("ある一定期間コメントが取得できない場合、Liveが行われているか再度チェックするインターバルです。")] private int noItemsRespondLimit = 5;
        public bool receiveChannelDetails = true;
        public int reciveChannelDetailsInterval = 20;
        private ListenersData _listenersData;
        public ListenersData listenersData {
            get { return _listenersData; }
        }
        private CommentData _commentData;
        public CommentData commentData {
            get { return _commentData; }
        }

        [SerializeField] private LiveStatus _liveStatus;
        public LiveStatus liveStatus {
            get { return _liveStatus; }
        }

        private AtlasManager _atlasManager;
        public AtlasManager atlasManager {
            get { return _atlasManager; }
        }

        [SerializeField] private bool showDebugLog = false;

        // Coroutineを個別に止めるのは、動画のチャットなどとは無関係の画像のキャッシングが止まらないようにするため。
        private Coroutine currentChatCoroutine = null;
        private Coroutine currentChannelCoroutine = null;
        private Coroutine currentCheckCoroutine = null;

        private void Start () {
            if (_listenersData == null) {
                _listenersData = new ListenersData ();
            }
            if (_commentData == null) {
                _commentData = new CommentData ();
            }
            if (_atlasManager == null) {
                _atlasManager = new AtlasManager ();
            }
            if (_liveStatus == null) {
                _liveStatus = this.gameObject.GetComponent<LiveStatus> ();
            }
        }

        public bool StartReceiveComments () {
            if (APIData.videoId.Equals ("") && APIData.channelId.Equals ("")) {
                return false;
            }
            if (!APIData.chatId.Equals ("")) {
                GetChatComments ();
            } else if (APIData.videoId.Equals ("")) {
                currentChatCoroutine = StartCoroutine (GetVideoId ());
            } else {
                currentChatCoroutine = StartCoroutine (GetVideoDetails ());
            }
            receiveChat = true;
            return true;
        }

        /// <summary>
        /// チェンネルか対象の動画のurlをもとに開始します。
        /// すでにコメント取得が動作している場合、止めてから取得開始します。
        /// 同じライブでもコメントはすべて最初から取得されます。
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool StartReceiveComments (string url) {
            if (!url.Contains ("youtu")) {
                Debug.LogError ("urlが正しくない恐れがあります。");
                return false;
            }
            ClearSettings ();
            if (url.Contains ("channel")) {
                SetChannelId (Regex.Replace (url, "(https?:\\/\\/)?(www\\.)?youtu((\\.be)|(be\\..{2,5}))\\/((user)|(channel))\\/", ""));
            } else {
                SetVideoId (Regex.Split (url, "(youtu(?:\\.be|be\\.com)\\/(?:.*v(?:\\/|=)|(?:.*\\/)?)([\\w'-]+))") [2]);
            }
            return StartReceiveComments ();
        }

        public void StopReceiveComments () {
            receiveChat = false;
        }

        public void StopReceiveProgress () {
            StopReceiveComments ();
            if (currentChatCoroutine != null) { StopCoroutine (currentChatCoroutine); currentChatCoroutine = null; }
            if (currentChannelCoroutine != null) { StopCoroutine (currentChannelCoroutine); currentChannelCoroutine = null; }
            if (currentCheckCoroutine != null) { StopCoroutine (currentCheckCoroutine); currentCheckCoroutine = null; }
        }

        /// <summary>
        /// コメント取得を停止し、チャンネルIDやビデオIDを消します。APIKeyは保持されます。
        /// </summary>
        public void ClearSettings () {
            StopReceiveProgress ();
            APIData.InitializeData ();
            isFirstTry = true;
        }

        public void GetChatComments () {
            if (APIData.chatId.Equals ("")) {
                StartCoroutine (GetChatId ());
            }
            receiveChat = true;
            currentChatCoroutine = StartCoroutine (GetChat ());
        }

        private IEnumerator GetChatId () {
            string searchChatURI = APIData.SearchChatURI ();

            if (Debug.isDebugBuild) {
#if UNITY_EDITOR
                if (showDebugLog) { Debug.Log (searchChatURI); }
#elif UNITY_STANDALONE
                Debug.Log (searchChatURI);
#endif
            }
            UnityWebRequest webRequest = UnityWebRequest.Get (searchChatURI);
            yield return webRequest.SendWebRequest ();

            if (webRequest.isHttpError || webRequest.isNetworkError) {
                Debug.Log (webRequest.error);
            } else {
                string jsonText = webRequest.downloadHandler.text;
                Json.LiveStreamingDetails.SerializedItems serializedItems = JsonUtility.FromJson<Json.LiveStreamingDetails.SerializedItems> (jsonText);
                if (serializedItems.items[0].liveStreamingDetails.activeLiveChatId == null) {
                    Debug.LogError ("this broadcast is not started yet or already closed.");
                } else {
                    SetLiveStreamingDetails (serializedItems);
                    SetChatId (serializedItems);
                    currentChatCoroutine = StartCoroutine (GetChat ());
                }
            }
            webRequest.Dispose ();
        }

        private IEnumerator GetChat () {
            if (APIData.chatId.Equals ("")) {
                yield break;
            }
            if (!receiveChat) {
                Debug.Log ("receive chat stopped");
                yield break;
            }
            float waitSeconds = (float) TimeSpan.FromMilliseconds (pollingIntervalMillis).TotalSeconds;
            yield return new WaitForSeconds (waitSeconds);
            pollingIntervalMillis = 0;
            string chatURI = APIData.ChatURI ();
            if (Debug.isDebugBuild) {
#if UNITY_EDITOR
                if (showDebugLog) { Debug.Log (chatURI); }
#elif UNITY_STANDALONE
                Debug.Log (chatURI);
#endif
            }
            UnityWebRequest webRequest = UnityWebRequest.Get (chatURI);
            yield return webRequest.SendWebRequest ();
            string jsonText = webRequest.downloadHandler.text;
            Json.ChatDetails.SerializedItems serializedItems = JsonUtility.FromJson<Json.ChatDetails.SerializedItems> (jsonText);
            if (serializedItems.items != null) {
                if (serializedItems.items.Length == 0) {
                    noItemsRespondCount++;
                    if (noItemsRespondCount >= noItemsRespondLimit) {
                        noItemsRespondCount = 0;
                        currentCheckCoroutine = StartCoroutine (GetVideoDetails ());
                    }
                } else {
                    SetNextPageToken (serializedItems);
                    pollingIntervalMillis = serializedItems.pollingIntervalMillis;
                    AddComment (serializedItems);
                }
            }
            /* else if (serializedItems.error != null) {
                           ErrorDetails details = ErrorMessageResolver.FormatError (serializedItems.error);
                           if (details.reason.Equals (ErrorMessageResolver.Reason.liveChatEnded.ToString ())) {
                               Debug.Log ("LiveChatが終了したため、コメントの取得を停止しました。");
                               ClearSettings ();
                               yield break;
                           }
                       } */
            if (serializedItems.pollingIntervalMillis < defaultPollingIntervalMillis) {
                pollingIntervalMillis = defaultPollingIntervalMillis;
            }
            webRequest.Dispose ();
            if (liveStatus.liveBroadcastContent.Equals ("none")) {
                StopReceiveProgress ();
                Debug.Log ("Liveが終了しているため、コメントの取得を停止しました。");
                yield break;
            }
            currentChatCoroutine = StartCoroutine (WaitForReceiveChat ());
        }

        private IEnumerator WaitForReceiveChat () {
            if (isFirstTry) {
                isFirstTry = false;
                yield return new WaitForSeconds (4.0f);
            } else {
                yield return new WaitForSeconds (1.0f);
            }
            currentChatCoroutine = StartCoroutine (GetChat ());
        }

        private IEnumerator WaitForReceiveChannelDetails () {
            if (!receiveChannelDetails) { yield break; }
            yield return new WaitForSeconds (reciveChannelDetailsInterval);
            currentChannelCoroutine = StartCoroutine (GetChannelDetails ());
        }

        private IEnumerator GetVideoId () {
            string videoSearchURI = APIData.SearchVideoURI ();
            if (Debug.isDebugBuild) {
#if UNITY_EDITOR
                if (showDebugLog) { Debug.Log (videoSearchURI); }
#elif UNITY_STANDALONE
                Debug.Log (videoSearchURI);
#endif
            }
            UnityWebRequest webRequest = UnityWebRequest.Get (videoSearchURI);
            yield return webRequest.SendWebRequest ();

            if (webRequest.isHttpError || webRequest.isNetworkError) {
                Debug.LogError (webRequest.error);
            } else {
                string jsonText = webRequest.downloadHandler.text;
                Json.LiveStreamingDetails.SerializedItems serializedItems = JsonUtility.FromJson<Json.LiveStreamingDetails.SerializedItems> (jsonText);
                if (serializedItems.items.Length != 0) {
                    SetVideoId (serializedItems);
                    StartCoroutine (GetVideoDetails ());
                } else {
                    Debug.LogWarning ("there is no any active broadcast");
                }
            }
            webRequest.Dispose ();
        }

        private IEnumerator GetVideoDetails () {
            string videoDetailsURI = APIData.VideoDetailsURI ();
            if (Debug.isDebugBuild) {
#if UNITY_EDITOR
                if (showDebugLog) { Debug.Log (videoDetailsURI); }
#elif UNITY_STANDALONE
                Debug.Log (videoDetailsURI);
#endif
            }
            UnityWebRequest webRequest = UnityWebRequest.Get (videoDetailsURI);
            yield return webRequest.SendWebRequest ();
            if (webRequest.isHttpError || webRequest.isNetworkError) {
                Debug.LogError (webRequest.error);
            } else {
                string jsonText = webRequest.downloadHandler.text;
                Json.LiveStreamingDetails.SerializedItems serializedItems = JsonUtility.FromJson<Json.LiveStreamingDetails.SerializedItems> (jsonText);
                SetLiveStreamingStatus (serializedItems);
                if (isFirstTry) {
                    SetChannelId (serializedItems);
                    currentChannelCoroutine = StartCoroutine (GetChannelDetails ());
                }
            }
            webRequest.Dispose ();
        }

        private IEnumerator GetChannelDetails () {
            string channelDetailsURI = APIData.ChannelDetailsURI ();
            if (Debug.isDebugBuild) {
#if UNITY_EDITOR
                if (showDebugLog) { Debug.Log (channelDetailsURI); }
#elif UNITY_STANDALONE
                Debug.Log (channelDetailsURI);
#endif
            }
            UnityWebRequest webRequest = UnityWebRequest.Get (channelDetailsURI);
            yield return webRequest.SendWebRequest ();
            if (webRequest.isHttpError || webRequest.isNetworkError) {
                Debug.LogError (webRequest.error);
            } else {
                string jsonText = webRequest.downloadHandler.text;
                Json.ChannelDetails.SerializedItems serializedItems = JsonUtility.FromJson<Json.ChannelDetails.SerializedItems> (jsonText);
                SetChannelDetails (serializedItems);
                if (isFirstTry) {
                    StartCoroutine (GetChatId ());
                }
                currentChannelCoroutine = StartCoroutine (WaitForReceiveChannelDetails ());
            }
            webRequest.Dispose ();
        }

        private void AddComment (Json.ChatDetails.SerializedItems serializedItems) {
            string channelId;
            string imageUrl;
            if (serializedItems.items == null) {
                return;
            }
            for (int i = 0; i < serializedItems.items.Length; i++) {
                channelId = serializedItems.items[i].authorDetails.channelId;
                imageUrl = serializedItems.items[i].authorDetails.profileImageUrl;
                if (!_listenersData.IsListenerDataExists (channelId)) {
                    _listenersData.AddListenerData (new ListenerData (channelId, imageUrl, new AtlasManager.AtlasInfo ()));
                    StartCoroutine (CacheListenerProfileImage (channelId, imageUrl));

                } else if (!_listenersData.IsListenerDataExists (channelId, imageUrl)) {
                    StartCoroutine (CacheListenerProfileImage (channelId, imageUrl));

                }
                _commentData.EnqueueComment (serializedItems.items[i]);
            }
        }

        public void ReCacheListenerProfileImage (string channelId) {
            string imageUrl = _listenersData.GetImageUrl (channelId);
            if (imageUrl != null) {
                StartCoroutine (CacheListenerProfileImage (channelId, imageUrl));
            }
        }

        private IEnumerator CacheListenerProfileImage (string channelId, string imageUrl) {
            UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture (imageUrl);
            yield return webRequest.SendWebRequest ();
            if (webRequest.isHttpError || webRequest.isNetworkError) {
                Debug.LogError (webRequest.error);
            } else {
                AtlasManager.AtlasInfo atlasInfo = atlasManager.AddIconTextureToAtlas (DownloadHandlerTexture.GetContent (webRequest));
                _listenersData.UpdateProfileImage (channelId, imageUrl, atlasInfo);
            }
            webRequest.Dispose ();
        }

        private void SetLiveStreamingStatus (Json.LiveStreamingDetails.SerializedItems serializedItems) { _liveStatus.SetLiveStreamingStatus (serializedItems); }

        private void SetLiveStreamingDetails (Json.LiveStreamingDetails.SerializedItems serializedItems) { _liveStatus.SetLiveStreamingDetails (serializedItems); }

        private void SetChannelDetails (Json.ChannelDetails.SerializedItems serializedItems) { _liveStatus.SetChannelDetails (serializedItems); }

        private void SetChatId (Json.LiveStreamingDetails.SerializedItems serializedItems) { APIData.chatId = serializedItems.items[0].liveStreamingDetails.activeLiveChatId; }

        private void SetChannelId (Json.LiveStreamingDetails.SerializedItems serializedItems) { APIData.channelId = serializedItems.items[0].snippet.channelId; }

        private void SetChannelId (string channelId) { APIData.channelId = channelId; }

        private void SetVideoId (Json.LiveStreamingDetails.SerializedItems serializedItems) { APIData.videoId = serializedItems.items[0].id.videoId; }

        private void SetVideoId (string videoId) { APIData.videoId = videoId; }

        private void SetNextPageToken (Json.ChatDetails.SerializedItems serializedItems) { APIData.nextPageToken = serializedItems.nextPageToken; }
    }

}