using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouTubeLive.UI {
	public class RankElement : MonoBehaviour {
		[SerializeField] private Text name;
		[SerializeField] private Text attendance;
		[SerializeField] private Text totalComment;
		[SerializeField] private Text totalCharge;

		[SerializeField] private Text activenessText;
		[SerializeField] private Image activenessBar;
		private int currentCharge = 0;
		[SerializeField] private Text currentChargeText;
		[SerializeField] private Image currentChargeBar;

		private static readonly int CHARGE_LIMIT = 50000;

		public void SetName (string name) { this.name.text = name + " さん"; }

		public void SetAttendance (string attendance) { this.attendance.text = attendance + "回目"; }

		public void SetAttendance (int attendance) { this.attendance.text = attendance.ToString () + "回目"; }

		public void SetTotalComment (string total) { this.totalComment.text = total; }

		public void SetTotalComment (int total) { this.totalComment.text = total.ToString (); }

		public void SetTotalCharge (string total) { this.totalCharge.text = total; }
		public void SetTotalCharge (int total) { this.totalCharge.text = total.ToString (); }

		public void SetActiveness (int activeness) {
			this.activenessText.text = activeness.ToString ();
			//todo: change fill ratio
		}

		public void AddCurrentCharge (int amount) {
			currentCharge += amount;
			currentChargeBar.fillAmount = currentCharge / CHARGE_LIMIT;
			currentChargeText.text = currentCharge.ToString ();

		}
	}
}