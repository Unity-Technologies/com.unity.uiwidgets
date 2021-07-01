using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.DevTools
{
    public class utils
    {
        public static readonly float pi = 3.1415926535897932f;
        public static float degToRad(float deg) => deg * (pi / 180.0f);
    }
    
    class JsonUtils {
        // JsonUtils._();

        public static string getStringMember(Dictionary<string, object> json, string memberName) {
            // TODO(jacobr): should we handle non-string values with a reasonable
            // toString differently?
            if (json.ContainsKey(memberName))
            {
                return json[memberName] as string;
            }
            Debug.Log("key not found: " + memberName);
            return "key not found: " + memberName;
        }

        public static int getIntMember(Dictionary<string, object> json, string memberName) {
            if (json.ContainsKey(memberName))
            {
                return (int) json[memberName];
            }
            return -1;
        }

        public static List<string> getValues(Dictionary<string, object> json, string member) {
            List<object> values = (List<object>)json[member];
            if (values == null || values.isEmpty()) {
                return new List<string>();
            }

            List<string> res = new List<string>();
            foreach (var value in values)
            {
                res.Add(value.ToString());
            }
            return res;
        }

        static bool hasJsonData(String data) {
            return data != null && data.isNotEmpty() && data != "null";
        }
    }
}