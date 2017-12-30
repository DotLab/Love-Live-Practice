using UnityEngine;

public static class CanvasSizer {
	public static float CanvasHeight, CanvasWidth;

	public static float GetCanvasHeight(float width = 800) {
		return CanvasHeight = width / Camera.main.aspect;
	}

	public static float GetCanvasWidth(float heigth = 600) {
		return CanvasWidth = heigth * Camera.main.aspect;
	}
}
