using UnityEngine;

namespace Uif {
	public abstract class Swapable <T> : MonoBehaviour, ISwapable<T> {
		public abstract void Swap(T newItem);

		public abstract void ForceSwap(T newItem);
	}
}