using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckInversions
{
    public static int GetInvCount(int[,] arr)
    {
        int inv_count = 0;
        for (int i = 0; i < 3 - 1; i++)
            for (int j = i + 1; j < 3; j++)

                // Value 0 is used for empty space
                if (arr[j, i] > 0 && arr[j, i] > arr[i, j])
                    inv_count++;
        return inv_count;
    }

    // This function returns true
    // if given 8 puzzle is solvable.
    public static bool IsSolvable(int[,] puzzle)
    {
        // Count inversions in given 8 puzzle
        int invCount = GetInvCount(puzzle);

        // return true if inversion count is even.
        return (invCount % 2 == 0);
    }
}
