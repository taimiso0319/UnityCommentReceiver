using System;
using System.Collections;
using System.Collections.Generic;
using YouTubeLive.Json;

namespace YouTubeLive {
    public static class ErrorMessageResolver {
        public enum Reason {
            liveChatEnded = 1,
            liveChatNotFound
        }

        public static ErrorDetails FormatError (Json.Error error) {
            return new ErrorDetails (error);
        }

        public static string ExplainReason (string reason) {
            switch (reason) {
                case "liveChatEnded":
                    return "このチャットがすでに終了することを意味します。このチャットが行われている配信は存在しない、もしくは終了しています。";
                case "liveChatNotFound":
                    return "対象のチャットが見つかりません。チャットIDが正しいことを確認してください";
                default:
                    return "UnknownError. " + reason + " このエラーは定義されていません。 https://developers.google.com/youtube/v3/docs/core_errors を参照してください。";
            }
        }
    }

    public class ErrorDetails {
        public string code;
        public string domain;
        public string message;
        public string reason;

        public ErrorDetails () {

        }

        /// <summary>
        /// YouTubeから返されたエラーで初期化します。
        /// ドメインや理由について複数のエラーが出た場合も一つ目だけを利用して初期化します。
        /// (複数出ることがほとんどないため。必要な場合は直接の参照が可能なため。)
        /// </summary>
        /// <param name="error"></param>
        public ErrorDetails (Json.Error error) {
            this.code = error.code;
            this.domain = error.errors[0].domain;
            this.message = error.message;
            this.reason = error.errors[0].reason;
        }
    }
}