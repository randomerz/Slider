using UnityEngine;

namespace MoreMountains.Tools
{
	[System.Serializable]
	public class MMLayer
	{
		[SerializeField]
		protected int _layerIndex = 0;

		public virtual int LayerIndex
		{
			get { return _layerIndex; }
		}

		public virtual void Set(int _layerIndex)
		{
			if (_layerIndex > 0 && _layerIndex < 32)
			{
				this._layerIndex = _layerIndex;
			}
		}

		public virtual int Mask
		{
			get { return 1 << _layerIndex; }
		}
	}
}