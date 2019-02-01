using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
namespace YouTubeLive.Util {
    /// <summary>
    /// 外貨の日本円換算を行うクラス
    /// </summary>
    public class CurrencyExchanger {
        /// <summary>
        /// 変換可能な外貨一覧
        /// 末尾にJPYとついているものはhttps://ja.1forge.com/から取得
        /// それ以外の通貨（コメントに※がついているもの）に関しては手動で入力
        /// </summary>
        public enum CurrencyType {
            /// <summary>米ドル</summary>
            USDJPY,
            /// <summary>ユーロ</summary>
            EURJPY,
            /// <summary>英ポンド</summary>
            GBPJPY,
            /// <summary>スイスフラン</summary>
            CHFJPY,
            /// <summary>カナダドル</summary>
            CADJPY,
            /// <summary>オーストラリアドル</summary>
            AUDJPY,
            /// <summary>ニュージーランドドル</summary>
            NZDJPY,
            /// <summary>ノルウェークローネ</summary>
            NOKJPY,
            /// <summary>シンガポールドル</summary>
            SGDJPY,
            /// <summary>ロシアルーブル</summary>
            RUBJPY,
            /// <summary>スウェーデンクローナ</summary>
            SEKJPY,
            /// <summary>トルコリラ</summary>
            TRYJPY,
            /// <summary>南アフリカランド</summary>
            ZARJPY,
            /// <summary>香港ドル</summary>
            HKDJPY,
            /// <summary>人民元</summary>
            CNHJPY,
            /// <summary>デンマーククローネ</summary>
            DKKJPY,
            /// <summary>メキシコペソ</summary>
            MXNJPY,
            /// <summary>ポーランドズロチ</summary>
            PLNJPY,
            /// <summary>韓国ウォン※</summary>
            KRW,
            /// <summary>ニュー台湾ドル※</summary>
            TWD,
        }
        //private Dictionary<string, CurrencyType> ISO4217CurrencyTypeDictionary = new Dictionary<string, CurrencyType> ();
        /// <summary>
        /// 変換に使用するレートデータ及び、レート取得用のAPIデータなどを保持するクラス
        /// Jsonに変換されて保存される
        /// </summary>
        [Serializable]
        public class CurrencyData {
            public List<CurrencyRate> rateList;
            public string updatedAt;
        }

        [Serializable]
        public class JsonRawData {
            [Serializable]
            public class JsonStatus {
                public bool error;
                public string errorMessage;
            }

            [Serializable]
            public class JsonCurrencyRateData {
                public string CurrencyType;
                public float Rate;
                public string UpdatedAt;
            }
            public JsonStatus Status;
            public JsonCurrencyRateData[] CurrencyRateData;
        }
        /// <summary>
        /// 通貨タイプと日本円換算のレート、その他情報を保持するクラス
        /// </summary>
        [Serializable]
        public class CurrencyRate : IEquatable<CurrencyRate> {
            public CurrencyType Type;
            public float rate;
            public string updatedAt;
            public override bool Equals (object obj) {
                if (obj == null) return false;
                CurrencyRate objAsPart = obj as CurrencyRate;
                if (objAsPart == null) return false;
                else return Equals (objAsPart);
            }
            public override int GetHashCode () {
                return (int) Type ^ rate.GetHashCode () ^ (updatedAt.GetHashCode () * 397);
            }
            public bool Equals (CurrencyRate other) {
                if (other == null) return false;
                return (this.Type.Equals (other.Type));
            }
        }

        private CurrencyData currencyData;
        private JsonRawData jsonRawData;
        private static readonly string APIURL = "https://script.google.com/macros/s/AKfycbwR--J1UCvVPdW2LNpyWnXoiof_NbjgUnmfNfgRDUpU7pCQ9Zs/exec";
        private bool isConvertable = false;
        // Use this for initialization
        public CurrencyExchanger () {
            //ローカルからデータをロード、なければ空で生成する
            currencyData = new CurrencyData ();
            //読み込んだデータがレート変換として既に必要な個数そろっていれば、この時点で使用可能にする
            if (currencyData.rateList != null) {
                isConvertable = (currencyData.rateList.Count == Enum.GetNames (typeof (CurrencyType)).Length);
            }
            //自作APIからJsonを取得して新しい情報を書き込む
            Observable.FromCoroutine (_ => DownloadCurrencyRate ()).Subscribe (_ => SetCurrencyData ());
        }
        private IEnumerator DownloadCurrencyRate () {
            UnityWebRequest request = UnityWebRequest.Get (APIURL);
            yield return request.SendWebRequest ();
            if (request.isHttpError || request.isNetworkError) {
                //4.エラー確認
                Debug.Log (request.error);
            } else {
                jsonRawData = JsonUtility.FromJson<JsonRawData> (request.downloadHandler.text);
            }
        }
        private void SetCurrencyData () {
            if (jsonRawData == null) {
                return;
            }
            //stringのCurrectTypeとEnumのCurrectTypeを紐づけるためのDictionaryを作成
            var stringEnumCheckDictionary = new Dictionary<string, CurrencyType> ();
            foreach (CurrencyType type in Enum.GetValues (typeof (CurrencyType))) {
                stringEnumCheckDictionary.Add (type.ToString (), type);
            }
            //データをチェックしてListを更新
            currencyData.rateList = new List<CurrencyRate> ();
            foreach (JsonRawData.JsonCurrencyRateData data in jsonRawData.CurrencyRateData) {
                if (stringEnumCheckDictionary.ContainsKey (data.CurrencyType) && data.Rate > 0) {
                    var currencyRate = new CurrencyRate ();
                    currencyRate.Type = stringEnumCheckDictionary[data.CurrencyType];
                    currencyRate.rate = data.Rate;
                    currencyRate.updatedAt = data.UpdatedAt;
                    currencyData.rateList.Add (currencyRate);
                }
            }

            //(APIの不備などで）重複があった場合は削除
            var beforeCount = currencyData.rateList.Count;
            currencyData.rateList.Distinct ();
            if (currencyData.rateList.Count != beforeCount) {
                Debug.Log ("【Warning】 通貨レートの変換データを更新時に重複したデータがありました。APIかクライアント実装に問題がある可能性があります");
            }

            if (currencyData.rateList.Count == Enum.GetNames (typeof (CurrencyType)).Length) {
                isConvertable = true;
            } else {
                isConvertable = false;
                Debug.Log ("【Error】通貨レート変換データに不備があったため、データが使用できません");
            }
        }
        /// <summary>
        /// 外貨を指定して日本円に換算した金額を取得する
        /// 変換できなかった場合は-1を返す
        /// </summary>
        /// <param name="type">外貨のタイプ</param>
        /// <param name="foreignCurrency">変換する金額</param>
        /// <returns>日本円(JPY)　or -1(失敗)</returns>
        public float ToJPY (CurrencyType type, float foreignCurrency) {
            if (!isConvertable) {
                return -1;
            }
            var check = new CurrencyRate ();
            check.Type = type;
            var index = currencyData.rateList.IndexOf (check);
            if (index == -1) {
                Debug.Log ("【Warning】 通貨レートの変換において存在しないTypeが指定されました。データの取得に問題がある可能性があります\n Type :" + type.ToString () + ", ForeignCurrency : " + foreignCurrency);
                return -1;
            } else {
                return foreignCurrency * currencyData.rateList[index].rate;
            }
        }
        /// <summary>
        /// ISO4217で定義されたalphabeticCodeを元に変換可能であればCurrencyTypeを返します
        /// https://docs.google.com/spreadsheets/d/1mG3PyewTfHb2luI50Yrlfvb2SEvhF6XNeD3xAagXCNM/edit?usp=sharing
        /// 渡された文字列が3文字でないか、変換不可能である場合はNullを返します
        /// </summary>
        /// <param name="alphabeticCode">AlphabeticCode(ISO4217)</param>
        ///         /// <returns>CurrencyType or Null</returns>
        public static CurrencyType? RecognizeType (string alphabeticCode) {
            if (alphabeticCode.Length != 3) {
                return null;
            }
            //CurrencyTypeがalphabeticCodeそのままか、末尾にJPYを付けたものであることを前提にしてます
            //APIルールが変わったら壊れる可能性があるので注意してください
            foreach (CurrencyType t in Enum.GetValues (typeof (CurrencyType))) {
                //オンショアとオフショアに関する特殊処理。
                if (alphabeticCode == "CNY") {
                    alphabeticCode = "CNH";
                }
                if (alphabeticCode == t.ToString () || alphabeticCode + "JPY" == t.ToString ()) {
                    return t;
                }
            }
            return null;
        }
    }
}