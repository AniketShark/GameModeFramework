using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModules.Utils.Texture
{

[JsonObject]
public class MaskedRect {
	public MaskedRect(int _x, int _y, int _width, int _height) {
		this.x = _x;
		this.y = _y;
		this.w = _width;
		this.h = _height;
	}
	public int x {
		get;
		set;
	}
	public int y {
		get;
		set;
	}
	public int w {
		get;
		set;
	}
	public int h {
		get;
		set;
	}
}

	public static class Texture2DExtensions
	{
		/// <summary>
		/// Overrides pixels defined by MaskedRect in targetTexture with sourceTexture pixels  
		/// </summary>
		/// <param name="targetTexture"></param>
		/// <param name="sourceTexture"></param>
		/// <param name="maskRect"></param>
		public static void OverrideMask(this Texture2D targetTexture, Texture2D sourceTexture, MaskedRect maskRect)
		{
			//loop will only run once if there's just one mask per tex.
			Color[] block;
			//CHARLIE_LOW: The following check can be avoided if a bool parameter is sent to identify whether incoming textures are individual parts or full ones, but doing this here can be more robust.
			block = sourceTexture.GetPixels();  
			targetTexture.SetPixels(maskRect.x, maskRect.y, maskRect.w, maskRect.h, block);
			targetTexture.Apply();
		}

		public static void OverrideMask(this Texture2D targetTexture,Color[] block, MaskedRect maskRect)
		{
			targetTexture.SetPixels(maskRect.x, maskRect.y, maskRect.w, maskRect.h, block);
			targetTexture.Apply();

		}
	}
}
