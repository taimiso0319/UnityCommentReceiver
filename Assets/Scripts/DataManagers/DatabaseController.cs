using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SQLite4Unity3d;
using UnityEngine;
using YouTubeLive.Json;

namespace YouTubeLive {
	public class DatabaseController {
		private readonly string dbName = "youtubelive.db";
		private SQLiteConnection dbConnector;
		public DatabaseController () {
			string path = Application.persistentDataPath + "/" + dbName;
			Debug.Log (path);
			dbConnector = new SQLiteConnection (path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
		}

		public void InitializeDatabase () {
			dbConnector.CreateTable<DatabaseTableModel.ListenerData> ();
			dbConnector.CreateTable<DatabaseTableModel.Channel> ();
			dbConnector.CreateTable<DatabaseTableModel.Comment> ();
			dbConnector.CreateTable<DatabaseTableModel.Live> ();
			dbConnector.CreateTable<DatabaseTableModel.SuperChat> ();
		}

		public void DropAllTables () {
			dbConnector.DropTable<DatabaseTableModel.ListenerData> ();
			dbConnector.DropTable<DatabaseTableModel.Channel> ();
			dbConnector.DropTable<DatabaseTableModel.Comment> ();
			dbConnector.DropTable<DatabaseTableModel.Live> ();
			dbConnector.DropTable<DatabaseTableModel.SuperChat> ();
		}

		public void AddData<T> (T data) {
			dbConnector.Insert (data);
		}

		public void AddDatas<T> (T[] datas) {
			dbConnector.InsertAll (datas);
		}

		public void AddChannel (string channelId, string channelTitle, bool updateTitle = false) {
			if (!dbConnector.Table<DatabaseTableModel.Channel> ().Any (x => x.channelId == channelId)) {
				AddData<DatabaseTableModel.Channel> (
					new DatabaseTableModel.Channel {
						channelId = channelId,
							channelTitle = channelTitle,
							updatedAt = DateTime.UtcNow,
							createdAt = DateTime.UtcNow
					});
			} else {
				if (updateTitle) {
					// TODO: 
				}
			}
		}

		public void AddLive (LiveStatus liveStatus) {
			if (!dbConnector.Table<DatabaseTableModel.Live> ().Any (x => x.videoId == liveStatus.videoId)) { }
		}

		public void AddComment (string videoId, CommentStatus commentStatus) {
			string channelId = GetChannelId (videoId);
			AddComment (channelId, videoId, commentStatus);
		}
		public void AddComment (string channelId, string videoId, CommentStatus commentStatus) {
			string message = "";
			bool isSuperChat = false;
			int liveId = GetLiveId (videoId);
			if (commentStatus.type == Json.ChatDetails.Snippet.EventType.superChatEvent.ToString ()) {
				message = commentStatus.userComment;
				isSuperChat = true;
			} else {
				message = commentStatus.displayMessage;
			}
			AddData<DatabaseTableModel.Comment> (
				new DatabaseTableModel.Comment {
					uniqueId = commentStatus.id,
						channelId = channelId,
						liveId = liveId,
						listenerChannelId = commentStatus.channelId,
						isSuperChat = isSuperChat,
						messageText = message,
						publishedAt = DateTime.Parse (Regex.Replace (commentStatus.publishedAt, @"\..*", "Z"), null, System.Globalization.DateTimeStyles.RoundtripKind),
						createdAt = DateTime.UtcNow
				});
			if (isSuperChat) {
				int commentId = GetCommentId (commentStatus.id);
				AddData<DatabaseTableModel.SuperChat> (
					new DatabaseTableModel.SuperChat {
						listenerChannelId = commentStatus.channelId,
							commentId = commentId,
							channelId = channelId,
							liveId = liveId,
							currency = commentStatus.currency,
							amount = commentStatus.amountDisplayString,
							// TODO: convertedAmount~~~~~~~~~~~~
							createdAt = DateTime.UtcNow
					}
				);
			}
		}

		#region Get
		/// <summary>
		/// 大量のデータが返ってくる可能性に留意してください。
		/// </summary>
		public IEnumerable<DatabaseTableModel.ListenerData> GetListenerDatas () { return dbConnector.Table<DatabaseTableModel.ListenerData> (); }
		/// <summary>
		/// 大量のデータが返ってくる可能性に留意してください。
		/// </summary>
		public IEnumerable<DatabaseTableModel.Comment> GetComments () { return dbConnector.Table<DatabaseTableModel.Comment> (); }
		public IEnumerable<DatabaseTableModel.SuperChat> GetSuperChats () { return dbConnector.Table<DatabaseTableModel.SuperChat> (); }
		public IEnumerable<DatabaseTableModel.Live> GetLives () { return dbConnector.Table<DatabaseTableModel.Live> (); }
		public IEnumerable<DatabaseTableModel.Channel> GetChannels () { return dbConnector.Table<DatabaseTableModel.Channel> (); }

		#region  Comment
		public int GetCommentId (string uniqueId) {
			DatabaseTableModel.Comment comment = dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.uniqueId == uniqueId).FirstOrDefault ();
			if (comment == null) { Debug.Log ("[GetCommentId] target comment is not found."); return -1; }
			return comment.id;
		}
		public IEnumerable<DatabaseTableModel.Comment> GetCommentsByListener (string listernerChannelId) { return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.listenerChannelId == listernerChannelId); }
		public IEnumerable<DatabaseTableModel.Comment> GetCommentsByListener (string listernerChannelId, string channelId) { return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.listenerChannelId == listernerChannelId && x.channelId == channelId); }
		public IEnumerable<DatabaseTableModel.Comment> GetComments (int liveId) { return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.liveId == liveId); }
		public IEnumerable<DatabaseTableModel.Comment> GetComments (string videoId) {
			int liveId = GetLiveId (videoId);
			return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.liveId == liveId);
		}
		public IEnumerable<DatabaseTableModel.Comment> GetSuperChatComments () { return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.isSuperChat == true); }
		public IEnumerable<DatabaseTableModel.Comment> GetSuperChatCommentsByVideo (string videoId) {
			int liveId = GetLiveId (videoId);
			return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.isSuperChat == true && x.liveId == liveId);
		}
		public IEnumerable<DatabaseTableModel.Comment> GetSuperChatCommentsByListener (string listernerChannelId) { return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.isSuperChat == true && x.listenerChannelId == listernerChannelId); }
		public IEnumerable<DatabaseTableModel.Comment> GetSuperChatCommentsByListener (string listernerChannelId, string videoId) {
			int liveId = GetLiveId (videoId);
			return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.isSuperChat == true && x.listenerChannelId == listernerChannelId && x.liveId == liveId);
		}
		#endregion Comment

		#region SuperChat
		public IEnumerable<DatabaseTableModel.SuperChat> GetSuperChatsByChannel (string channelId) { return dbConnector.Table<DatabaseTableModel.SuperChat> ().Where (x => x.channelId == channelId); }
		public IEnumerable<DatabaseTableModel.SuperChat> GetSuperChatsByVideo (string videoId) {
			int liveId = GetLiveId (videoId);
			return dbConnector.Table<DatabaseTableModel.SuperChat> ().Where (x => x.liveId == liveId);
		}
		public DatabaseTableModel.SuperChat GetSuperChatByComment (int commentId) { return dbConnector.Table<DatabaseTableModel.SuperChat> ().Where (x => x.commentId == commentId).FirstOrDefault (); }
		/// <summary>
		/// チャンネルの累計スーパーチャット金額を返します。
		/// 為替などの影響があるため、記録上の数値であり正確な数字ではありません。
		/// 正確な数字が欲しい場合は別途APIを利用して取得する必要があります。
		/// </summary>
		public int GetSuperChatsAmountByChannel (string channelId) {
			var superChats = GetSuperChatsByChannel (channelId);
			int amount = 0;
			foreach (var s in superChats) {
				amount += s.convertedAmount;
			}
			return amount;
		}
		/// <summary>
		/// 動画の累計スーパーチャット金額を返します。
		/// 為替などの影響があるため、記録上の数値であり正確な数字ではありません。
		/// 正確な数字が欲しい場合は別途APIを利用して取得する必要があります。
		/// </summary>
		public int GetSuperChatsAmountByVideo (string videoId) {
			var superChats = GetSuperChatsByVideo (videoId);
			int amount = 0;
			foreach (var s in superChats) {
				amount += s.convertedAmount;
			}
			return amount;
		}
		#endregion SuperChat

		#region Live
		public int GetLiveId (string videoId) {
			DatabaseTableModel.Live live = dbConnector.Table<DatabaseTableModel.Live> ().Where (x => x.videoId == videoId).FirstOrDefault ();
			if (live == null) { Debug.Log ("[GetLiveId] target live is not found"); return -1; }
			return live.id;
		}

		public string GetChannelId (string videoId) {
			DatabaseTableModel.Live live = dbConnector.Table<DatabaseTableModel.Live> ().Where (x => x.videoId == videoId).FirstOrDefault ();
			if (live == null) { Debug.Log ("[GetLiveId] target live is not found"); return null; }
			return live.channelId;
		}

		public IEnumerable<DatabaseTableModel.Live> GetLivesByChannel (string channelId) { return dbConnector.Table<DatabaseTableModel.Live> ().Where (x => x.channelId == channelId); }
		#endregion Live

		#endregion Get

		#region Existence Check
		#endregion
	}

	public class DatabaseTableModel {

		public class ListenerData {
			[PrimaryKey, AutoIncrement]
			public int id { get; set; }

			[NotNull]
			public string listenerChannelId { get; set; }

			[NotNull]
			public DateTime createdAt { get; set; }

			[NotNull]
			public DateTime updatedAt { get; set; }

			public override string ToString () {
				return string.Format ("[ListenerData: id={0}, createdAt={1},  updatedAt={2}]",
					id, createdAt, updatedAt);
			}
		}

		public class Comment {
			[PrimaryKey, AutoIncrement]
			public int id { get; set; }

			[NotNull]
			public string uniqueId { get; set; }

			[NotNull]
			public string channelId { get; set; }

			[NotNull]
			public int liveId { get; set; }

			[NotNull]
			public string listenerChannelId { get; set; }
			public bool isSuperChat { get; set; }
			public string messageText { get; set; }
			public DateTime publishedAt { get; set; }

			[NotNull]
			public DateTime createdAt { get; set; }

			public override string ToString () {
				return string.Format ("[Comment: id={0}, channelId={1}, listenerChannelId={2},  isSuperChat={3}, messageText={4}, publishedAt={5}, createdAt={6}]",
					id, channelId, listenerChannelId, isSuperChat, messageText, publishedAt, createdAt);
			}
		}

		public class SuperChat {
			[PrimaryKey, AutoIncrement]
			public int id { get; set; }

			[NotNull]
			public string listenerChannelId { get; set; }

			[NotNull]
			public int commentId { get; set; }

			[NotNull]
			public string channelId { get; set; }

			[NotNull]
			public int liveId { get; set; }
			public string currency { get; set; }
			public string amount { get; set; }
			public int convertedAmount { get; set; }

			[NotNull]
			public DateTime createdAt { get; set; }
			public override string ToString () {
				return string.Format ("[SuperChat: id={0}, listernerId={1}, currency={2},  amount={3}, convertedAmount={4}, createdAt={5}]",
					id, listenerChannelId, currency, amount, convertedAmount, createdAt);
			}
		}

		public class Live {
			[PrimaryKey, AutoIncrement]
			public int id { get; set; }

			[NotNull]
			public string videoId { get; set; }
			public string channelId { get; set; }
			public string liveTitle { get; set; }
			public DateTime publishedAt { get; set; }

			[NotNull]
			public DateTime createdAt { get; set; }

			public override string ToString () {
				return string.Format ("[Live: id={0}, videoId={1}, channelId={2},  publishedAt={3}, createdAt={4}]",
					id, videoId, channelId, publishedAt, createdAt);
			}
		}

		public class Channel {
			[PrimaryKey, AutoIncrement]
			public int id { get; set; }

			[NotNull]
			public string channelId { get; set; }
			public string channelTitle { get; set; }

			[NotNull]
			public DateTime updatedAt { get; set; }

			[NotNull]
			public DateTime createdAt { get; set; }

			public override string ToString () {
				return string.Format ("[Channel: id={0}, channelId={1}, channelTitle={2},  updatedAt={3}, createdAt={4}]",
					id, channelId, channelTitle, updatedAt, createdAt);
			}
		}
	}
}