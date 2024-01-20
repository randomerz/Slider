using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Generates a grid of the specified size, either entirely full or empty
	/// </summary>
	public class MMGridGeneratorFull : MMGridGenerator 
	{
		/// <summary>
		/// Generates a grid of the specified size, either entirely full or empty
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="full"></param>
		/// <returns></returns>
		public static int[,] Generate(int width, int height, bool full)
		{
			int[,] grid = PrepareGrid(ref width, ref height);
            
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					SetGridCoordinate(grid, i, j, full ? 1 : 0);
				}
			}
			return grid;
		} 
	}
}