namespace Uif {
	public interface IHidable {
		bool Shown ();

		bool Hided ();

		void Show ();

		void Hide ();
	}

	public enum HidableState {
		None,
		Shown,
		Hided
	}

	public enum HidableAction {
		None,
		Show,
		Hide
	}
}