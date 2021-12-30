using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

public static class CheckGrid
{
    public static bool contains(string gridString, string regex)
    {

        return Regex.IsMatch(gridString, regex);
    }
    public static bool row(string gridString, string row)
    {
        Debug.Log(gridString + "|" + row);
        return Regex.IsMatch(gridString, "^" + row) || Regex.IsMatch(gridString, "^.{3}" + row) || Regex.IsMatch(gridString, row + "$");
    }
    public static bool column(string gridString, string column)
    {
        return Regex.IsMatch(gridString, "^" + column[0] + ".{2}" + column[1] + ".{2}" + column[2])
            || Regex.IsMatch(gridString, "^." + column[0] + ".{2}" + column[1] + ".{2}" + column[2])
            || Regex.IsMatch(gridString, "^.{2}" + column[0] + ".{2}" + column[1] + ".{2}" + column[2]);
    }

    /// <summary>
    /// subgrid should be 4 characters, in the order top left, top right, bottom left, bottom right
    /// </summary>
    /// <param name="gridString"></param>
    /// <param name="subgrid"></param>
    /// <returns></returns>

    public static bool subgrid(string gridString, string subgrid)
    {
        Debug.Log(gridString + "|" + subgrid);
        return Regex.IsMatch(gridString, "" + subgrid[0] + subgrid[1] + "." + subgrid[2] + subgrid[3]);
    }
    // Start is called before the first frame update

}
