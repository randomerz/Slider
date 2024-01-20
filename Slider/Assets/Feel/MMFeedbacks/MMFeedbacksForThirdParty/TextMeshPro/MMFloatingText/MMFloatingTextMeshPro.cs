using UnityEngine;
using MoreMountains.Tools;
#if MM_TEXTMESHPRO
using TMPro;
#endif

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// A floating text variant using TextMeshPro instead of regular TextMesh
	/// </summary>
	public class MMFloatingTextMeshPro : MMFloatingText
	{
		#if MM_TEXTMESHPRO
		[Header("TextMeshPro")]
		/// the TextMeshPro object to use to display values
		public TextMeshPro TargetTextMeshPro;
        
		/// <summary>
		/// On init we grab our TMP's color
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_initialTextColor = TargetTextMeshPro.color;
		}
	                
		/// <summary>
		/// Sets the TMP's value
		/// </summary>
		/// <param name="newValue"></param>
		public override void SetText(string newValue)
		{
			TargetTextMeshPro.text = newValue;
		}

		/// <summary>
		/// Sets the color of the target TMP
		/// </summary>
		/// <param name="newColor"></param>
		public override void SetColor(Color newColor)
		{
			TargetTextMeshPro.color = newColor;
		}

		/// <summary>
		/// Sets the opacity of the target TMP
		/// </summary>
		/// <param name="newOpacity"></param>
		public override void SetOpacity(float newOpacity)
		{
			_newColor = TargetTextMeshPro.color;
			_newColor.a = newOpacity;
			TargetTextMeshPro.color = _newColor;
		}
		#endif
	}
}