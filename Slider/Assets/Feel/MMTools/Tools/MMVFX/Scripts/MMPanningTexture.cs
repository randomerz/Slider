using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Use this class on a sprite or mesh to have its texture pan according to the specified speed
	/// You can also force a sorting layer name 
	/// </summary>
	[AddComponentMenu("More Mountains/Tools/VFX/PanningTexture")]
	public class MMPanningTexture : MonoBehaviour
	{
		[MMInformation("This script will let you pan a texture on an attached Renderer.",MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]

		/// whether or not this texture should pan
		public bool TextureShouldPan = true;
		/// the speed at which the texture pans
		public Vector2 Speed = new Vector2(10,10);
		/// the name of the sorting layer to render the texture at
		public string SortingLayerName = "Above";
		/// the property name, for example "_MainTex"
		[Tooltip("the property name, for example _MainTex")]
		public string MaterialPropertyName = "_MainTex_ST";
		/// the index of the material
		[Tooltip("the index of the material")]
		public int MaterialIndex = 0;
        
		protected RawImage _rawImage;
		protected Renderer _renderer;
		protected Vector4 _position = Vector4.one;
		protected Vector4 _speed;
		protected MaterialPropertyBlock _propertyBlock;

		/// <summary>
		/// On start, grabs the renderer and/or raw image
		/// </summary>
		protected virtual void Start()
		{
			_renderer = GetComponent<Renderer>();
			if ((_renderer != null) && (!string.IsNullOrEmpty(SortingLayerName)))
			{
				_renderer.sortingLayerName = SortingLayerName;
				_propertyBlock = new MaterialPropertyBlock();
				_renderer.GetPropertyBlock(_propertyBlock);
			}            
			_position.x = _renderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).x;
			_position.y = _renderer.sharedMaterials[MaterialIndex].GetVector(MaterialPropertyName).y;
			_rawImage = GetComponent<RawImage>();

			_speed = new Vector4(0f, 0f, Speed.x, Speed.y);
		}

		/// <summary>
		/// On update, moves the texture around according to the specified speed
		/// </summary>
		protected virtual void Update()
		{
			if (!TextureShouldPan)
			{
				return;
			}
            
			if ((_rawImage == null) && (_renderer == null))
			{
				return;
			}

			_speed.z = Speed.x;
			_speed.w = Speed.y;
			_position += (_speed / 300) * Time.deltaTime;

			// position reset
			if (_position.z > 1.0f)
			{
				_position.z -= 1.0f;
			}
			if (_position.w > 1.0f)
			{
				_position.w -= 1.0f;
			}
            
			if (_renderer != null)
			{
				_renderer.GetPropertyBlock(_propertyBlock);
				_propertyBlock.SetVector(MaterialPropertyName, _position);
				_renderer.SetPropertyBlock(_propertyBlock, MaterialIndex);
			}
			if (_rawImage != null)
			{
				_rawImage.material.mainTextureOffset = _position;
			}

		}
	}
}