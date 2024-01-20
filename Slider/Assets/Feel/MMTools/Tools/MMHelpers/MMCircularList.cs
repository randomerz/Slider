using System.Collections.Generic;

namespace MoreMountains.Tools
{
	/// <summary>
	/// A improved list that lets you parse it and automatically have it loop to the start or end when you reach the end or start
	/// To use it : set the CurrentIndex to whatever you want, then use IncrementCurrentIndex / DecrementCurrentIndex to move it, get the current element via Current
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class MMCircularList<T> : List<T>
	{
		private int _currentIndex = 0;
		
		/// <summary>
		/// Lets you set the current index, or compute it if you get it
		/// </summary>
		public int CurrentIndex
		{
			get
			{
				return GetCurrentIndex();
			}
			set => _currentIndex = value;
		}

		/// <summary>
		/// Computes the current index
		/// </summary>
		/// <returns></returns>
		protected virtual int GetCurrentIndex()
		{
			if (_currentIndex > Count - 1) { _currentIndex = 0; }
			if (_currentIndex < 0) { _currentIndex = Count - 1; }
			return _currentIndex;
		}

		/// <summary>
		/// Returns the current element
		/// </summary>
		public T Current => this[CurrentIndex];

		/// <summary>
		/// Increments the current index (towards the "right" of the list)
		/// </summary>
		public virtual void IncrementCurrentIndex()
		{
			_currentIndex++;
			GetCurrentIndex();
		}

		/// <summary>
		/// Decrements the current index (towards the "left" of the list)
		/// </summary>
		public virtual void DecrementCurrentIndex()
		{
			_currentIndex--;
			GetCurrentIndex();
		}

		/// <summary>
		/// Returns the previous index in the circular list
		/// </summary>
		public virtual int PreviousIndex => (_currentIndex == 0) ? Count - 1 : _currentIndex - 1;

		/// <summary>
		/// Returns the next index in the circular list
		/// </summary>
		public virtual int NextIndex => (_currentIndex == Count - 1) ? 0 : _currentIndex + 1;
	}
}

