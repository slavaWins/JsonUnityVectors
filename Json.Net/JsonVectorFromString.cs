using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace Json.Net
{
    public static class JsonVectorFromString
    {

        public  static string[] ParseAllShit(string str)
        {
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str = str.Replace(" ", "");
            str = str.Replace(",", "&");

            string sp = NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator;
            str = str.Replace(".", sp); 
            return str.Split('&');
        }

        public static Vector3 StringToVector3(string str)
        {
            string[] list = ParseAllShit(str);
            Vector3 vec = new Vector3();
            vec.x = float.Parse(list[0]);
            vec.y = float.Parse(list[1]);
            vec.z = float.Parse(list[2]);
            return vec;
        }

        public static Vector3 StringToVector2(string str)
        {
            string[] list = ParseAllShit(str);
            Vector3 vec = new Vector3();
            vec.x = float.Parse(list[0]);
            vec.y = float.Parse(list[1]); 
            return vec;
        }

        public static Vector3Int StringToVector3Int(string str)
        {
            Vector3 vec = StringToVector3(str);
            return Vector3Int.CeilToInt(vec); 
        }
        public static Vector2Int StringToVector2Int(string str)
        {
            Vector2 vec = StringToVector2(str);

            return Vector2Int.CeilToInt(vec); 
        }


    }

}