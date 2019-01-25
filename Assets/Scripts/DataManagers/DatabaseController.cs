using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SQLite4Unity3d;
using UniRx;
using UniRx.Async;
using UnityEngine;
using YouTubeLive.Json;

namespace YouTubeLive {
	public class DatabaseController {
		private readonly string dbName = "youtubelive.db";
		private SQLiteConnection dbConnector;
		public DatabaseController () {
			if (!Directory.Exists (FilePath)) { Directory.CreateDirectory (FilePath); }
			dbConnector = new SQLiteConnection (Path.Combine (FilePath, dbName), SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
			InitializeDatabase (); //なければつくる。
		}

		private static string FilePath {
			get {
#if !UNITY_STANDALONE_WIN && !UNITY_EDITOR
				return Application.persistentDataPath;
#else
				return "./Database/";
#endif
			}
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

		public void DropTable<T> () {
			dbConnector.DropTable<T> ();
		}

		async Task AddData<T> (T data) {
			await Task.Run (() => dbConnector.Insert (data));
		}

		async Task AddDatas<T> (T[] datas) {
			await Task.Run (() => dbConnector.InsertAll (datas));
		}

		#region Update
		public void UpdateChannelTitle (LiveStatus liveStatus, bool orInsert = false) {
			UpdateChannelTitle (liveStatus.channelId, liveStatus.channelTitle, orInsert);
		}

		public void UpdateChannelTitle (string channelUniqueId, string channelTitle, bool orInsert = false) {
			if (!IsChannelExists (channelUniqueId)) {
				DatabaseTableModel.Channel channel = GetChannel (channelUniqueId);
				channel.channelTitle = channelTitle;
				dbConnector.Update (channel);
				return;
			}
			if (orInsert) {
				AddChannel (channelUniqueId, channelTitle);
			}
		}
		public void UpdateLiveTitle (string videoId, string liveTitle) {
			if (!IsLiveExists (videoId)) {
				DatabaseTableModel.Live live = GetLive (videoId);
				live.liveTitle = liveTitle;
				dbConnector.Update (live);
			}
		}

		#endregion Update

		#region Insert
		public void AddListenerData (string listenerId) {
			if (!IsListenerDataExists (listenerId)) {
				Task.Run (() => AddData<DatabaseTableModel.ListenerData> (
					new DatabaseTableModel.ListenerData {
						listenerChannelId = listenerId,
							createdAt = DateTime.UtcNow,
							updatedAt = DateTime.UtcNow
					}));
			}
		}
		public void AddChannel (string channelUniqueId, string channelTitle, bool updateTitle = false) {
			if (!IsChannelExists (channelUniqueId)) {
				Task.Run (() => AddData<DatabaseTableModel.Channel> (
					new DatabaseTableModel.Channel {
						uniqueId = channelUniqueId,
							channelTitle = channelTitle,
							updatedAt = DateTime.UtcNow,
							createdAt = DateTime.UtcNow
					}));
				return;
			}

			if (updateTitle) {
				UpdateChannelTitle (channelUniqueId, channelTitle);
			}
		}

		public void AddChannel (LiveStatus liveStatus, bool updateTitle = false) {
			AddChannel (liveStatus.channelId, liveStatus.channelTitle, updateTitle);
		}

		public void AddLive (LiveStatus liveStatus, bool updateTitle = false) {
			if (!IsChannelExists (liveStatus.channelId)) {
				AddChannel (liveStatus);
			}
			if (!IsLiveExists (liveStatus.videoId)) {
				Task.Run (() => AddData<DatabaseTableModel.Live> (
					new DatabaseTableModel.Live {
						videoId = liveStatus.videoId,
							channelUniqueId = liveStatus.channelId,
							liveTitle = liveStatus.liveTitle,
							publishedAt = DateTime.Parse (Regex.Replace (liveStatus.livePublishedAt, @"\..*", "Z"), null, System.Globalization.DateTimeStyles.RoundtripKind),
							createdAt = DateTime.UtcNow
					}));
			}

			if (updateTitle) {
				UpdateLiveTitle (liveStatus.videoId, liveStatus.liveTitle);
			}
		}

		public void AddComment (string videoId, CommentStatus[] commentStatus) {
			string channelId = GetLiveChannelId (videoId);
			if (channelId == null) {
				return;
			}
			AddComment (channelId, videoId, commentStatus);
		}

		public void AddComment (string channelId, string videoId, CommentStatus[] commentStatus) {
			string message = "";
			bool isSuperChat = false;
			int liveId = GetLiveId (videoId);
			DatabaseTableModel.Comment[] data = new DatabaseTableModel.Comment[commentStatus.Length];
			for (int i = 0; i < commentStatus.Length; i++) {
				if (commentStatus[i].type == Json.ChatDetails.Snippet.EventType.superChatEvent.ToString ()) {
					message = commentStatus[i].userComment;
					isSuperChat = true;
				} else {
					message = commentStatus[i].displayMessage;
				}
				data[i] =
					new DatabaseTableModel.Comment {
						uniqueId = commentStatus[i].id,
							channelId = channelId,
							liveId = liveId,
							listenerChannelId = commentStatus[i].channelId,
							isSuperChat = isSuperChat,
							messageText = message,
							publishedAt = DateTime.Parse (Regex.Replace (commentStatus[i].publishedAt, @"\..*", "Z"), null, System.Globalization.DateTimeStyles.RoundtripKind),
							createdAt = DateTime.UtcNow
					};
				if (isSuperChat) {
					int commentId = GetCommentId (commentStatus[i].id);
					AddSuperChat (channelId, liveId, commentStatus[i]);
				}
			}
			Task.Run (() => AddDatas<DatabaseTableModel.Comment> (data));

		}

		public void AddSuperChat (string channelId, int liveId, CommentStatus commentStatus) {
			int commentId = GetCommentId (commentStatus.id);
			Task.Run (() => AddData<DatabaseTableModel.SuperChat> (
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
			));
		}
		#endregion Insert

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

		#region Comment
		public int GetCommentId (string uniqueId) {
			DatabaseTableModel.Comment comment = dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.uniqueId == uniqueId).FirstOrDefault ();
			if (comment == null) { Debug.Log ("[GetCommentId] target comment is not found."); return -1; }
			return comment.id;
		}
		public IEnumerable<DatabaseTableModel.Comment> GetCommentsByListener (string listenerChannelId) { return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.listenerChannelId == listenerChannelId); }
		public IEnumerable<DatabaseTableModel.Comment> GetCommentsByListener (string listenerChannelId, string channelId) { return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.listenerChannelId == listenerChannelId && x.channelId == channelId); }
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
		public IEnumerable<DatabaseTableModel.Comment> GetSuperChatCommentsByListener (string listenerChannelId) { return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.isSuperChat == true && x.listenerChannelId == listenerChannelId); }
		public IEnumerable<DatabaseTableModel.Comment> GetSuperChatCommentsByListener (string listenerChannelId, string videoId) {
			int liveId = GetLiveId (videoId);
			return dbConnector.Table<DatabaseTableModel.Comment> ().Where (x => x.isSuperChat == true && x.listenerChannelId == listenerChannelId && x.liveId == liveId);
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

		public string GetLiveChannelId (string videoId) {
			DatabaseTableModel.Live live = GetLive (videoId);
			if (live == null) { Debug.Log ("[GetLiveChannelId] target live is not found"); return null; }
			return live.channelUniqueId;
		}
		public DatabaseTableModel.Live GetLive (string videoId) {
			return dbConnector.Table<DatabaseTableModel.Live> ().Where (x => x.videoId == videoId).FirstOrDefault ();
		}
		public IEnumerable<DatabaseTableModel.Live> GetLivesByChannel (string channelId) { return dbConnector.Table<DatabaseTableModel.Live> ().Where (x => x.channelUniqueId == channelId); }
		#endregion Live

		#region Channel
		public int GetChannelId (string channelUniqueId) {
			DatabaseTableModel.Channel channel = GetChannel (channelUniqueId);
			if (channel == null) { Debug.Log ("[GetChannelTitle] the target channel is not found"); return -1; }
			return channel.id;
		}

		public DatabaseTableModel.Channel GetChannel (string channelUniqueId) { return dbConnector.Table<DatabaseTableModel.Channel> ().Where (x => x.uniqueId == channelUniqueId).FirstOrDefault (); }

		public string GetChannelTitle (string channelUniqueId) {
			DatabaseTableModel.Channel channel = GetChannel (channelUniqueId);
			if (channel == null) { Debug.Log ("[GetChannelTitle] the target channel is not found"); return null; }
			return channel.channelTitle;
		}

		#endregion Channel

		#endregion Get

		#region CheckExistence
		private bool IsChannelExists (string channelId) {
			if (dbConnector.Query<DatabaseTableModel.Channel> ("SELECT * FROM Channel WHERE uniqueId='" + channelId + "';").FirstOrDefault () == null) { return false; }
			return true;
		}

		private bool IsLiveExists (string videoId) {
			if (dbConnector.Query<DatabaseTableModel.Live> ("SELECT * FROM Live WHERE videoId='" + videoId + "';").FirstOrDefault () == null) { return false; }
			return true;
		}

		private bool IsListenerDataExists (string listenerChannelId) {
			if (dbConnector.Query<DatabaseTableModel.ListenerData> ("SELECT * FROM ListenerData WHERE listenerChannelId='" + listenerChannelId + "';").FirstOrDefault () == null) { return false; }
			return true;
		}

		private bool IsCommentExists (string uniqueId) {
			if (dbConnector.Query<DatabaseTableModel.Comment> ("SELECT * FROM Comment WHERE uniqueId='" + uniqueId + "';").FirstOrDefault () == null) { return false; }
			return true;
		}
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
				return string.Format ("[ListenerData: id={0}, listenerChannelId={1}, createdAt={2},  updatedAt={3}]",
					id, listenerChannelId, createdAt, updatedAt);
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
				return string.Format ("[Comment: id={0},uniqueId={1}, channelId={2}, liveId={3}, listenerChannelId={4},  isSuperChat={5}, messageText={6}, publishedAt={7}, createdAt={8}]",
					id, uniqueId, channelId, liveId, listenerChannelId, isSuperChat, messageText, publishedAt, createdAt);
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
			public string channelUniqueId { get; set; }
			public string liveTitle { get; set; }
			public DateTime publishedAt { get; set; }

			[NotNull]
			public DateTime createdAt { get; set; }

			public override string ToString () {
				return string.Format ("[Live: id={0}, videoId={1}, channelId={2}, liveTitle={3}, publishedAt={4}, createdAt={5}]",
					id, videoId, channelUniqueId, liveTitle, publishedAt, createdAt);
			}
		}

		public class Channel {
			[PrimaryKey, AutoIncrement]
			public int id { get; set; }

			[NotNull]
			public string uniqueId { get; set; }
			public string channelTitle { get; set; }

			[NotNull]
			public DateTime updatedAt { get; set; }

			[NotNull]
			public DateTime createdAt { get; set; }

			public override string ToString () {
				return string.Format ("[Channel: id={0}, uniqueId={1}, channelTitle={2},  updatedAt={3}, createdAt={4}]",
					id, uniqueId, channelTitle, updatedAt, createdAt);
			}
		}
	}
}