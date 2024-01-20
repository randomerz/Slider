using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Generates a grid with a path in the specified direction
	/// </summary>
	public class MMGridGeneratorPath : MMGridGenerator 
	{
		public enum Directions { TopToBottom, BottomToTop, LeftToRight, RightToLeft }
        
		/// <summary>
		/// Generates a grid with a path in the specified direction
		/// </summary>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="seed"></param>
		/// <param name="direction"></param>
		/// <param name="startPosition"></param>
		/// <param name="pathMinWidth"></param>
		/// <param name="pathMaxWidth"></param>
		/// <param name="directionChangeDistance"></param>
		/// <param name="widthChangePercentage"></param>
		/// <param name="directionChangePercentage"></param>
		/// <returns></returns>
		public static int[,] Generate(int width, int height, int seed, Directions direction, Vector2Int startPosition, int pathMinWidth, int pathMaxWidth, int directionChangeDistance, int widthChangePercentage, int directionChangePercentage)
		{
			int[,] grid = PrepareGrid(ref width, ref height);
			grid = MMGridGeneratorFull.Generate(width, height, true);
			System.Random random = new System.Random(seed);
			Random.InitState(seed);
            
			int pathWidth = 1;
			int initialX = startPosition.x;
			int initialY = startPosition.y;
            
			SetGridCoordinate(grid, initialX, initialY, 0);

			switch (direction)
			{
				case Directions.TopToBottom:
					int x1 = initialX;
					for (int i = -pathWidth; i <= pathWidth; i++) 
					{
						SetGridCoordinate(grid, x1 + i, initialY, 0);
					}
					for (int y = initialY; y > 0; y--) 
					{
						pathWidth = ComputeWidth(random, widthChangePercentage, pathMinWidth, pathMaxWidth, pathWidth);
						x1 = DetermineNextStep(random, x1, directionChangeDistance, directionChangePercentage, pathMaxWidth, width);
						for (int i = -pathWidth; i <= pathWidth; i++) 
						{
							SetGridCoordinate(grid, x1 + i, y, 0);
						}
					}
					break;
				case Directions.BottomToTop:
					int x2 = initialX;
					for (int i = -pathWidth; i <= pathWidth; i++) 
					{
						SetGridCoordinate(grid, x2 + i, initialY, 0);
					}
					for (int y = initialY; y < height; y++) 
					{
						pathWidth = ComputeWidth(random, widthChangePercentage, pathMinWidth, pathMaxWidth, pathWidth);
						x2 = DetermineNextStep(random, x2, directionChangeDistance, directionChangePercentage, pathMaxWidth, width);
						for (int i = -pathWidth; i <= pathWidth; i++) 
						{
							SetGridCoordinate(grid, x2 + i, y, 0);
						}
					}
					break;
				case Directions.LeftToRight:
					int y1 = initialY;
					for (int i = -pathWidth; i <= pathWidth; i++) 
					{
						SetGridCoordinate(grid, initialX, y1 + i, 0);
					}
					for (int x = initialX; x < width; x++) 
					{
						pathWidth = ComputeWidth(random, widthChangePercentage, pathMinWidth, pathMaxWidth, pathWidth);
						y1 = DetermineNextStep(random, y1, directionChangeDistance, directionChangePercentage, pathMaxWidth, width);
						for (int i = -pathWidth; i <= pathWidth; i++) 
						{
							SetGridCoordinate(grid, x, y1 + i, 0);
						}
					}
					break;
				case Directions.RightToLeft:
					int y2 = initialY;
					for (int i = -pathWidth; i <= pathWidth; i++) 
					{
						SetGridCoordinate(grid, initialX, y2 + i, 0);
					}
					for (int x = initialX; x > 0; x--) 
					{
						pathWidth = ComputeWidth(random, widthChangePercentage, pathMinWidth, pathMaxWidth, pathWidth);
						y2 = DetermineNextStep(random, y2, directionChangeDistance, directionChangePercentage, pathMaxWidth, width);
						for (int i = -pathWidth; i <= pathWidth; i++) 
						{
							SetGridCoordinate(grid, x, y2 + i, 0);
						}
					}
					break;
			}
			return grid;
		}

		/// <summary>
		/// Determines the new width of the path
		/// </summary>
		/// <param name="random"></param>
		/// <param name="widthChangePercentage"></param>
		/// <param name="pathMinWidth"></param>
		/// <param name="pathMaxWidth"></param>
		/// <param name="pathWidth"></param>
		/// <returns></returns>
		private static int ComputeWidth(System.Random random, int widthChangePercentage, int pathMinWidth, int pathMaxWidth, int pathWidth)
		{
			if (random.Next(0, 100) > widthChangePercentage) 
			{
				int widthChange = Random.Range(-pathMaxWidth, pathMaxWidth);
				pathWidth += widthChange;
				if (pathWidth < pathMinWidth) 
				{
					pathWidth = pathMinWidth;
				}
				if (pathWidth > pathMaxWidth) 
				{
					pathWidth = pathMaxWidth;
				}
			}

			return pathWidth;
		}

		/// <summary>
		/// Determines in what direction to move the path
		/// </summary>
		/// <param name="random"></param>
		/// <param name="x"></param>
		/// <param name="directionChangeDistance"></param>
		/// <param name="directionChangePercentage"></param>
		/// <param name="pathMaxWidth"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		private static int DetermineNextStep(System.Random random, int x, int directionChangeDistance, int directionChangePercentage, int pathMaxWidth, int width)
		{
			if (random.Next(0, 100) > directionChangePercentage) 
			{
				int xChange = Random.Range(-directionChangeDistance, directionChangeDistance); 
				x += xChange;
				if (x < pathMaxWidth) 
				{
					x = pathMaxWidth;
				}
				if (x > (width - pathMaxWidth)) 
				{
					x = width - pathMaxWidth;
				}
			}

			return x;
		}
	}
}