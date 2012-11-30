using System;

public static class Utility
{
    public static Int32 CombineHashCodes(params Object[] objects)
    {
        int result = 17;
        foreach (Object item in objects)
        {
            result = result * 31 + item.GetHashCode();
        }
        return result;
    }
}
