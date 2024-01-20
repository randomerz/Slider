using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Generates a simple grid filled with random points 
	/// </summary>
	public class MMGridGeneratorRandom : MMGridGenerator
	{
		/// <summary>
		/// Generates a simple grid filled with random points 
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="seed"></param>
		/// <param name="fillPercentage"></param>
		/// <returns></returns>
		public static int[,] Generate(int width, int height, int seed, int fillPercentage)
		{
			int[,] grid = PrepareGrid(ref width, ref height);
            
			grid = MMGridGeneratorFull.Generate(width, height, true);
			System.Random random = new System.Random(seed);

			for (int i = 0; i <= width; i ++) 
			{
				for (int j = 0; j <= height; j ++) 
				{
					int value = (random.Next(0,100) < fillPercentage)? 1: 0;
					SetGridCoordinate(grid, i, j, value);
				}
			}

			return grid;
		}
	}

}