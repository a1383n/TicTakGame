using System;
using System.Collections.Generic;
using System.Text;

namespace TicTakGame.Utils
{
    class EnumHelper<E, T> where E : Enum
    {
        public static string[] GetNames() => Enum.GetNames(typeof(E));

        public static Array GetValues() => Enum.GetValues(typeof(E));

        public static string GetNameByIndex(int index) => GetNames()[index];

        public static int GetIndexByName(string name) => Array.IndexOf(GetNames(), name);

        public static E GetEnum(int index) => (E)Enum.Parse(typeof(E), GetNames()[index]);

        public static E GetEnum(string name) => (E)Enum.Parse(typeof(E), name);

        public static T GetValueByEnum(E e) => (T)GetValues().GetValue(GetIndexByName(e.ToString()));

        public static Dictionary<string, T> getEnumMap()
        {
            string[] names = GetNames();
            Array values = GetValues();
            Dictionary<string, T> keyValuePairs = new Dictionary<string, T>(names.Length);

            for (int i = 0; i < names.Length; i++)
            {
                keyValuePairs.Add(names[i], (T)values.GetValue(i));
            }

            return keyValuePairs;
        }
    }
}
