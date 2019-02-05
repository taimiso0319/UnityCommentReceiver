using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YouTubeLive.UI {
	public class RankElement : Element {
		private string _listenerId;
		public string listenerId { get { return _listenerId; } }

		[SerializeField] private Text attendance;
		public int commentTotal = 0;
		[SerializeField] private Text commentTotalText;
		public int chargeTotal = 0;
		[SerializeField] private Text chargeTotalText;

		[SerializeField] private Text activenessText;
		[SerializeField] private Image activenessBar;
		public int currentCharge = 0;
		[SerializeField] private Text currentChargeText;
		[SerializeField] private Image currentChargeBar;

		private static readonly int CHARGE_LIMIT = 50000;

		public void SetListenerId (string id) { _listenerId = id; }

		public void SetAttendance (string attendance) { this.attendance.text = attendance + "回目"; }
		public void SetAttendance (int attendance) { SetAttendance (attendance.ToString ()); }

		public void SetCommentTotal (int total) {
			commentTotal = total;
			UpdateText ();
		}

		public void AddCommentTotal (int i) {
			commentTotal += i;
		}

		public void SetChargeTotal (int total) {
			chargeTotal = total;
			UpdateText ();
		}

		public void SetActiveness (int activeness) {
			activenessText.text = activeness.ToString ();
			//todo: change fill ratio
		}

		public void AddCharge (int amount, bool updateTotal = false) {
			currentCharge += amount;
			currentCharge = currentCharge > CHARGE_LIMIT? CHARGE_LIMIT : currentCharge;
			currentChargeBar.fillAmount = (float) currentCharge / CHARGE_LIMIT;
			if (updateTotal) {
				AddChargeTotal (amount);
			} else {
				UpdateText ();
			}
		}

		public void AddChargeTotal (int amount) {
			chargeTotal += amount;
			UpdateText ();

		}

		private void UpdateText () {
			currentChargeText.text = "￥" + currentCharge.ToString ();
			chargeTotalText.text = "￥" + chargeTotal.ToString ();
			commentTotalText.text = commentTotal.ToString ();
		}
	}
}