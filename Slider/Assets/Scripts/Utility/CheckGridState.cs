using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class CheckGrid
{
    public static bool contains(string gridString, string regex)
    {
        // Debug.Log(gridString + "|con" + regex);
        // Debug.Log("bool is " + (Regex.IsMatch(gridString, regex)));

        return Regex.IsMatch(gridString, regex);
    }
    public static bool row(string gridString, string row)
    {
        // Debug.Log(gridString + "|r" + row);
        // Debug.Log("bool is "+ (Regex.IsMatch(gridString, "_" + row) || Regex.IsMatch(gridString, row + "_")));

        return Regex.IsMatch(gridString, "_" + row) || Regex.IsMatch(gridString, row + "_");
    }
    public static bool column(string gridString, string column)
    {
        // Debug.Log(gridString + "|c" + column);
        // Debug.Log("bool is " + (Regex.IsMatch(gridString, "^" + column[0] + ".{3}" + column[1] + ".{3}" + column[2])
            // || Regex.IsMatch(gridString, "^." + column[0] + ".{3}" + column[1] + ".{3}" + column[2])
            // || Regex.IsMatch(gridString, "^.{2}" + column[0] + ".{3}" + column[1] + ".{3}" + column[2])));
        return Regex.IsMatch(gridString, "^" + column[0] + ".{3}" + column[1] + ".{3}" + column[2])
            || Regex.IsMatch(gridString, "^." + column[0] + ".{3}" + column[1] + ".{3}" + column[2])
            || Regex.IsMatch(gridString, "^.{2}" + column[0] + ".{3}" + column[1] + ".{3}" + column[2]);
    }

    /// <summary>
    /// subgrid should be 4 characters, in the order top left, top right, bottom left, bottom right
    /// </summary>
    /// <param name="gridString"></param>
    /// <param name="subgrid"></param>
    /// <returns></returns>

    public static bool subgrid(string gridString, string subgrid)
    {
        // Debug.Log(gridString + "|sg" + subgrid);
        // Debug.Log("bool is " + (Regex.IsMatch(gridString, "" + subgrid[0] + subgrid[1] + "._" + subgrid[2] + subgrid[3])
            // || Regex.IsMatch(gridString, "" + subgrid[0] + subgrid[1] + "_." + subgrid[2] + subgrid[3])));
        return Regex.IsMatch(gridString, "" + subgrid[0] + subgrid[1] + "._" + subgrid[2] + subgrid[3])
            || Regex.IsMatch(gridString, "" + subgrid[0] + subgrid[1] + "_." + subgrid[2] + subgrid[3]);
    }
    // Start is called before the first frame update

}
