﻿using UnityEngine;

namespace Uif {
	public abstract class Swapable <T> : MonoBehaviour, ISwapable<T> {
		public EasingType TransitionEasingType = EasingType.Cubic;
		public EasingPhase TransitionEasingPhase = EasingPhase.InOut;
		public float TransitionDuration = 0.5f;

		public abstract void Swap(T newItem);

		public abstract void ForceSwap(T newItem);
	}

	public abstract class ColorSwapable : Swapable<Color> {

	}

	public abstract class SpriteSwapable : Swapable<Sprite> {

	}
}