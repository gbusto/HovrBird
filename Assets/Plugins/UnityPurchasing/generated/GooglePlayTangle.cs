#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("+KvZlXhMBD1aAOiJa/myXrJxUv+4ComquIWOgaIOwA5/hYmJiY2Ii8L4jRpduxyBnx1KJ0qEHTPQUr3qDTIFJt2j4kGoM5wsZAJAy4Jub1+2WxlYDDCeVzqnyAZgX3LIbY8H5AqJh4i4ComCigqJiYgbBb6r9YVyQ1YBXqPQzxczwopSpXZjI44w73+q13MS8rEw2QdG7wFhtIkgVa2/vGijERFfSipRleuxOsRqkWNspihg5tfmhIRTJim33kVpry7ySyKxR+IQAgA113/JUM0GEn/2vTb25dx9SWD+3uSxCo3B/+QdrfvHHGnkJAHz0+TLX2jjxjFONV4adtMFJ1iEXMCuh53/s/i793VIL3E7STkbsk6kVJSv3d1WBVBSgYqLiYiJ");
        private static int[] order = new int[] { 4,4,9,10,10,10,7,7,13,12,13,12,13,13,14 };
        private static int key = 136;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
