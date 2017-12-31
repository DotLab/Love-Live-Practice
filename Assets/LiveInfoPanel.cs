using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

using LoveLivePractice.Api;

public class LiveInfoPanel : MonoBehaviour {
	public RawImage bgUiRawImage;
	public AspectRatioFitter bgFitter;
	public EasedHidable bgHidable;

	public TextSwapable titleText, uploaderText, songInfoText, mapInfoText, playerInfoText;

	public void ChangeLive(Texture2D texture, LiveListItem liveListItem) {
		StartCoroutine(ChangeLiveHandler(texture, liveListItem));
	}

	IEnumerator ChangeLiveHandler(Texture2D texture, LiveListItem liveListItem) {
		bgHidable.Hide();

		yield return new WaitForSeconds(bgHidable.TransitionDuration + 0.1f);

		titleText.Swap(liveListItem.artist + " - " + liveListItem.live_name + " [LEVEL" + liveListItem.level.ToString("N0") + "]");
		uploaderText.Swap("Mapped by " + liveListItem.upload_user.username);

		bgUiRawImage.texture = texture;
		bgFitter.aspectRatio = (float)texture.width / texture.height;
	
		bgHidable.Show();
	}
}
