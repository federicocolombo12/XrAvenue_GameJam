using UnityEngine;

namespace Dev.Nicklaj.Butter.Helpers
{
    public static class Logger
    {
        public static void LogInfo(string text)
        {
            Debug.Log($"[{"Butter".Color(UnityEngine.Color.cyan)}] {text}");
        }
        
        public static string ToHex(this Color c) {
            return string.Format("#{0:X2}{1:X2}{2:X2}",ToByte(c.r), ToByte(c.g), ToByte(c.b));

        }

        private static byte ToByte(float f) {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }

        public static string Color(this string text, Color color)
        {
            return string.Format("<color={0}>{1}</color>", color.ToHex(), text);
        }
    }
}
