namespace Uif {
	public interface ISwapable <T> {
		void Swap (T newItem);

		void ForceSwap (T newItem);
	}
}