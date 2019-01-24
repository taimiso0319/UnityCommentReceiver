using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YouTubeLive.Json;

namespace YouTubeLive {

	public class CommentData {

		public CommentData () {
			_commentQueue = new Queue<CommentStatus> ();
		}
		private Queue<CommentStatus> _commentQueue;
		public Queue<CommentStatus> commentQueue {
			get { return _commentQueue; }
		}

		public CommentStatus EnqueueComment (Json.ChatDetails.Items items) {
			CommentStatus commentStatus = new CommentStatus (items);
			_commentQueue.Enqueue (commentStatus);
			return commentStatus;
		}

		public CommentStatus DequeueComment () {
			return _commentQueue.Dequeue ();
		}

		public CommentStatus PeekComment () {
			return _commentQueue.Peek ();
		}

		public void ClearQueue () {
			_commentQueue.Clear ();
		}

	}

	public class CommentStatus : IEquatable<CommentStatus> {
		public string type;
		public string id;
		public string channelId;
		public string displayName;
		public string displayMessage;
		public string messageText;
		public string currency;
		public string amountDisplayString;
		public string userComment;
		public int tier;
		public string publishedAt;
		public bool hasDisplayContent;
		public bool isVerified;
		public bool isChatOwner;
		public bool isChatSponsor;
		public bool isChatModerator;

		public CommentStatus (Json.ChatDetails.Items items) {
			type = items.snippet.type;
			id = items.id;
			channelId = items.authorDetails.channelId;
			displayName = items.authorDetails.displayName;
			displayMessage = items.snippet.displayMessage;
			messageText = items.snippet.textMessageDetails.messageText;
			currency = items.snippet.superChatDetails.currency;
			amountDisplayString = items.snippet.superChatDetails.amountDisplayString;
			userComment = items.snippet.superChatDetails.userComment;
			tier = items.snippet.superChatDetails.tier;
			publishedAt = items.snippet.publishedAt;
			hasDisplayContent = items.snippet.hasDisplayContent;
			isVerified = items.authorDetails.isVerified;
			isChatOwner = items.authorDetails.isChatOwner;
			isChatSponsor = items.authorDetails.isChatSponsor;
			isChatModerator = items.authorDetails.isChatModerator;
		}

		public override bool Equals (object other) {
			if (!(other is CommentStatus)) return false;
			return Equals ((CommentStatus) other);
		}

		public bool Equals (CommentStatus other) {
			return type.Equals (other.type) &&
				id.Equals (other.id) &&
				channelId.Equals (other.channelId) &&
				displayName.Equals (other.displayName) &&
				displayMessage.Equals (other.displayMessage) &&
				messageText.Equals (other.messageText) &&
				currency.Equals (other.currency) &&
				amountDisplayString.Equals (other.amountDisplayString) &&
				userComment.Equals (other.userComment) &&
				tier.Equals (other.tier) &&
				publishedAt.Equals (other.publishedAt) &&
				hasDisplayContent.Equals (other.hasDisplayContent) &&
				isVerified.Equals (other.isVerified) &&
				isChatOwner.Equals (other.isChatOwner) &&
				isChatSponsor.Equals (other.isChatSponsor) &&
				isChatModerator.Equals (other.isChatModerator);
		}

		public override int GetHashCode () {
			unchecked {
				int hashCode = type.GetHashCode ();
				hashCode = (hashCode * 397) ^ id.GetHashCode ();
				hashCode = (hashCode * 397) ^ channelId.GetHashCode ();
				hashCode = (hashCode * 397) ^ displayName.GetHashCode ();
				if (!string.IsNullOrEmpty (displayMessage)) { hashCode = (hashCode * 397) ^ displayMessage.GetHashCode (); }
				if (!string.IsNullOrEmpty (messageText)) { hashCode = (hashCode * 397) ^ messageText.GetHashCode (); }
				if (!string.IsNullOrEmpty (currency)) { hashCode = (hashCode * 397) ^ currency.GetHashCode (); }
				if (!string.IsNullOrEmpty (amountDisplayString)) { hashCode = (hashCode * 397) ^ amountDisplayString.GetHashCode (); }
				if (!string.IsNullOrEmpty (userComment)) { hashCode = (hashCode * 397) ^ userComment.GetHashCode (); }
				hashCode = (hashCode * 397) ^ publishedAt.GetHashCode ();
				return hashCode ^ tier.GetHashCode () ^ (hasDisplayContent.GetHashCode () << 1) ^ (isVerified.GetHashCode () << 2) ^ (isChatOwner.GetHashCode () << 3) ^ (isChatSponsor.GetHashCode () << 4) ^ (isChatModerator.GetHashCode () << 5);
			}
		}
	}
}