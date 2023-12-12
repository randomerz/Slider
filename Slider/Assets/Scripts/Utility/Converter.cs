using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Converter
{
    /// <summary>
    /// Converts integers >= 10 to characters 10 = A, 11 = B, etc.
    /// Integers 0-9 are left untouched (note that they are now characters)
    /// </summary>
    /// <returns>The corresponding char to the input num</returns>
    public static char IntToChar(int num)
    {
        return (num > 9) ? (char)('A' - 10 + num) : (char)('0' + num);
    }

    public static int CharToInt(char c)
    {
        return (c > '9') ? (c - 'A' + 10) : (c - '0');
    }
}
