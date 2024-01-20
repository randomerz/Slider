using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Generates a grid with a path carved by a drunkard walk algorithm
	/// See http://pcg.wikidot.com/pcg-algorithm:drunkard-walk
	/// </summary>
	public class MMGridGeneratorRandomWalk : MMGridGenerator
	{
		/// <summary>
		/// Generates a grid with a path carved by a drunkard walk algorithm
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="seed"></param>
		/// <param name="fillPercentage"></param>
		/// <returns></returns>
		public static int[,] Generate(int width, int height, int seed,  int fillPercentage, Vector2Int startingPoint, int maxIterations)
		{
			int[,] grid = PrepareGrid(ref width, ref height);
			grid = MMGridGeneratorFull.Generate(width, height, true);
			System.Random random = new System.Random(seed);
            
			int requiredFillQuantity = ((width * height) * fillPercentage) / 100;
			int fillCounter = 0;

			int currentX = startingPoint.x;
			int currentY = startingPoint.y;
			grid[currentX, currentY] = 0;
			fillCounter++;
			int iterationsCounter = 0;
            
			while ((fillCounter < requiredFillQuantity) && (iterationsCounter < maxIterations))
			{ 
				int direction = random.Next(4); 

				switch (direction)
				{
					case 0: 
						if ((currentY + 1) < height) 
						{
							currentY++;
							grid = Carve(grid, currentX, currentY, ref fillCounter);
						}
						break;
					case 1: 
						if ((currentY - 1) > 1)
						{ 
							currentY--;
							grid = Carve(grid, currentX, currentY, ref fillCounter);
						}
						break;
					case 2: 
						if ((currentX - 1) > 1)
						{
							currentX--;
							grid = Carve(grid, currentX, currentY, ref fillCounter);
						}
						break;
					case 3: 
						if ((currentX + 1) < width)
						{
							currentX++;
							grid = Carve(grid, currentX, currentY, ref fillCounter);
						}
						break;
				}

				iterationsCounter++;
			}
			return grid; 
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="grid"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="fillCounter"></param>
		/// <returns></returns>
		private static int[,] Carve(int[,] grid, int x, int y, ref int fillCounter)
		{
			if (grid[x, y] == 1) 
			{
				grid[x, y] = 0;
				fillCounter++; 
			}
			return grid;
		}
	}
}