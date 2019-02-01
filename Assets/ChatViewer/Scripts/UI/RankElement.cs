using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouTubeLive.UI {
	public class RankElement : Element {
		private string _listenerId;
		public string listenerId { get { return _listenerId; } }

		[SerializeField] private Text attendance;
		public int totalComment = 0;
		[SerializeField] private Text totalCommentText;
		public int totalCharge = 0;
		[SerializeField] private Text totalChargeText;

		[SerializeField] private Text activenessText;
		[SerializeField] private Image activenessBar;
		public int currentCharge = 0;
		[SerializeField] private Text currentChargeText;
		[SerializeField] private Image currentChargeBar;

		private static readonly int CHARGE_LIMIT = 50000;

		public void SetListenerId (string id) { _listenerId = id; }

		public void SetAttendance (string attendance) { this.attendance.text = attendance + "回目"; }
		public void SetAttendance (int attendance) { SetAttendance (attendance.ToString ()); }

		public void SetTotalComment (int total) {
			totalComment = total;
			UpdateText ();
		}

		public void SetTotalCharge (int total) {
			totalCharge = total;
			UpdateText ();
		}

		public void SetActiveness (int activeness) {
			activenessText.text = activeness.ToString ();
			//todo: change fill ratio
		}

		public void AddCurrentCharge (int amount) {
			currentCharge += amount;
			currentCharge = currentCharge > CHARGE_LIMIT? CHARGE_LIMIT : currentCharge;
			currentChargeBar.fillAmount = (float) currentCharge / CHARGE_LIMIT;
			AddTotalCharge (amount);
		}

		public void AddTotalCharge (int amount) {
			totalCharge += amount;
			UpdateText ();

		}

		private void UpdateText () {
			currentChargeText.text = "￥" + currentCharge.ToString ();
			totalChargeText.text = "￥" + totalCharge.ToString ();
			totalCommentText.text = totalComment.ToString ();
		}
	}
}