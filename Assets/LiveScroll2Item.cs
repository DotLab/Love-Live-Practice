using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Uif;

public class LiveScroll2Item : InfiniteScrollItem {
	public Texture2D Texture {
		get { return (Texture2D)CoverUiRawImage.texture; }
	}

	public float Width {
		get { return rectTrans.sizeDelta.x; }
		set { rectTrans.sizeDelta = new Vector2(value, rectTrans.sizeDelta.y); }
	}

	public RectTransform CoverUiRawImageRectTrans;
	public RawImage CoverUiRawImage;

	public Text TitleUiText, AuthorUiText, TagUiText, StarsUiText;
	public ColorableSwapable BackgroundColorable, TextColorable;
	public RectClipHidable CoverClipHidable;
}
