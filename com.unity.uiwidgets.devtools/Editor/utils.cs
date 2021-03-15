using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace Unity.UIWidgets.DevTools
{
    public class utils
    {
        public static float safePositiveFloat(float value) {
            if (value.isNaN()) return 0.0f;
            return Mathf.Max(value, 0.0f);
        }
        
        public static float? lerpFloat(float? a, float? b, float t) {
            if (a == b || (a?.isNaN() == true) && (b?.isNaN() == true))
                return a;
            a = a?? 0.0f;
            b = b?? 0.0f;
            D.assert(a.Value.isFinite(), ()=>"Cannot interpolate between finite and non-finite values");
            D.assert(b.Value.isFinite(), ()=>"Cannot interpolate between finite and non-finite values");
            D.assert(t.isFinite(), ()=>"t must be finite when interpolating between values");
            return a * (1.0f - t) + b * t;
        }
        
        
    }
    
    public class JsonUtils {
        public JsonUtils(){}

        public static string getStringMember(Dictionary<string, object> json, string memberName) {
            // TODO(jacobr): should we handle non-string values with a reasonable
            // toString differently?
            return (string)json[memberName];
        }

        public static int getIntMember(Dictionary<string, object> json, string memberName)
        {
            if (!json.ContainsKey(memberName)) return -1;
            return (int)(json.getOrDefault(memberName));
        }

        public static List<string> getValues(Dictionary<string, object> json, string member) {
             List<object> values = json[member] as List<object>;
            if (values == null || values.isEmpty()) {
                return new List<string>();
            }

            return values.Cast<string>().ToList();
        }

        public static bool hasJsonData(string data) {
            return data != null && data.isNotEmpty() && data != "null";
        }
    }
    
}