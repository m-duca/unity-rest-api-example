using UnityEngine;
using UnityEngine.UI;

namespace APIExample
{
    public static class UtilitiesUI
    {
        public static void SetAlpha(Graphic g, float alpha)
        {
            Color c = g.color;
            c.a = alpha;
            g.color = c;
        }

        public static string CapitalizeFirstLetter(string str)
        {
            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static Texture2D ScaleTexture(Texture2D src, int scale)
        {
            Texture2D result = new Texture2D(src.width * scale, src.height * scale);
            result.filterMode = FilterMode.Point;

            for (int y = 0; y < result.height; y++)
            {
                for (int x = 0; x < result.width; x++)
                {
                    result.SetPixel(x, y, src.GetPixel(x / scale, y / scale));
                }
            }

            result.Apply();
            return result;
        }
    }
}
