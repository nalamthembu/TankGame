using System;
using System.Text;
using UnityEngine;

public static class TypeExtensions
{
    #region NUMERICTYPE_FLOAT
    public static string GetFloatStopWatchFormat(this float value)
    {
        TimeSpan t = TimeSpan.FromSeconds(value);

        var sb = new StringBuilder();

        return sb.Append(string.Format
            (
                "{0:00}:{1:00}:{2:000}",
                 t.Minutes,
                 t.Seconds,
                 Mathf.FloorToInt(t.Milliseconds) / 10f
            )).ToString();

    }

    public static string GetFloatMinSecFormat(this float value)
    {
        TimeSpan t = TimeSpan.FromSeconds(value);

        var sb = new StringBuilder();

        return sb.Append(string.Format
            (
                "{0:00}:{1:00}",
                 t.Minutes,
                 t.Seconds
            )).ToString();
    }
    #endregion

    #region NUMERICTYPE_INT
    public static string GetIntStopWatchFormat(this int value)
    {
        TimeSpan t = TimeSpan.FromSeconds(value);

        var sb = new StringBuilder();

        return sb.Append(string.Format
            (
                "{0:00}:{1:00}:{2:000}",
                 t.Minutes,
                 t.Seconds,
                 Mathf.FloorToInt(t.Milliseconds) / 10f
            )).ToString();

    }

    public static string GetIntMinSecFormat(this int value)
    {
        TimeSpan t = TimeSpan.FromSeconds(value);

        var sb = new StringBuilder();

        return sb.Append(string.Format
            (
                "{0:00}:{1:00}",
                 t.Minutes,
                 t.Seconds
            )).ToString();
    }
    #endregion

    #region GAMEOBJECT
    //Checks if the game object is in a specific layer mask.
    public static bool IsInLayerMask(this GameObject gameObject, LayerMask layermask)
    {
        return layermask == (layermask | (1 << gameObject.layer));
    }
    #endregion
}