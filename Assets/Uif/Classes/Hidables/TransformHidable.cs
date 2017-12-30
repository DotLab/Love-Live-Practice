using UnityEngine;

namespace Uif {
	[AddComponentMenu("Uif/Hidable/Transform Hidable")]
	public class TransformHidable : EasedHidable {
		[System.Serializable]
		public struct Translation {
			public Vector3 Position;
			public Vector3 Rotation;
			public Vector3 Scale;

			public Quaternion RotationQ {
				get { return Quaternion.Euler(Rotation); }
			}
		}

		public Transform Trans;

		[Space]
		public bool TransPosition;
		public bool TransRotation;
		public bool TransScale;

		[Space]
		public Translation ShowTranslation;
		public Translation HideTranslation;

		[ContextMenu("Record Show Translation")]
		public void RecordShowTranslation () {
			ShowTranslation.Position = Trans.localPosition;
			ShowTranslation.Rotation = Trans.localRotation.eulerAngles;
			ShowTranslation.Scale = Trans.localScale;
		}

		[ContextMenu("Record Hide Translation")]
		public void RecordHideTranslation () {
			HideTranslation.Position = Trans.localPosition;
			HideTranslation.Rotation = Trans.localRotation.eulerAngles;
			HideTranslation.Scale = Trans.localScale;
		}

		void OnValidate () {
			Trans = GetComponent<Transform>();
		}

		public override bool Shown () {
			return 	(TransPosition && Trans.localPosition == ShowTranslation.Position)
			|| (TransRotation && Trans.localRotation == ShowTranslation.RotationQ)
			|| (TransScale && Trans.localScale == ShowTranslation.Scale);
		}

		public override bool Hided () {
			return 	(TransPosition && Trans.localPosition == HideTranslation.Position)
			|| (TransRotation && Trans.localRotation == HideTranslation.RotationQ)
			|| (TransScale && Trans.localScale == HideTranslation.Scale);
		}

		public override void ForceShow () {
			if (TransPosition)
				Trans.localPosition = ShowTranslation.Position;
			if (TransRotation)
				Trans.localRotation = ShowTranslation.RotationQ;
			if (TransScale)
				Trans.localScale = ShowTranslation.Scale;
		}

		public override void ForceHide () {
			if (TransPosition)
				Trans.localPosition = HideTranslation.Position;
			if (TransRotation)
				Trans.localRotation = HideTranslation.RotationQ;
			if (TransScale)
				Trans.localScale = HideTranslation.Scale;
		}

		Vector3 startPosition, endPosition, startScale, endScale;
		Quaternion startRotation, endRotation;

		public override void PrepareShow () {
			startPosition = Trans.localPosition;
			startRotation = Trans.localRotation;
			startScale = Trans.localScale;

			endPosition = ShowTranslation.Position;
			endRotation = ShowTranslation.RotationQ;
			endScale = ShowTranslation.Scale;
		}

		public override void PrepareHide () {
			startPosition = Trans.localPosition;
			startRotation = Trans.localRotation;
			startScale = Trans.localScale;

			endPosition = HideTranslation.Position;
			endRotation = HideTranslation.RotationQ;
			endScale = HideTranslation.Scale;
		}

		public override void ApplyTransition (float step) {
			if (TransPosition)
				Trans.localPosition = Vector3.Lerp(startPosition, endPosition, step);
			if (TransRotation)
				Trans.localRotation = Quaternion.Lerp(startRotation, endRotation, step);
			if (TransScale)
				Trans.localScale = Vector3.Lerp(startScale, endScale, step);
		}
	}
}