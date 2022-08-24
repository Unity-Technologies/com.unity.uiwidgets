using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class IconData : IEquatable<IconData> {
        public IconData(
            int codePoint,
            string fontFamily = null,
            string fontPackage = null,
            bool matchTextDirection = false
        ) {
            this.codePoint = codePoint;
            this.fontFamily = fontFamily;
            this.fontPackage = fontPackage;
            this.matchTextDirection = matchTextDirection;
        }

        public readonly int codePoint;

        public readonly string fontFamily;

        public readonly string fontPackage;

        public readonly bool matchTextDirection;

        public bool Equals(IconData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return codePoint == other.codePoint &&
                   string.Equals(fontFamily, other.fontFamily) && 
                   string.Equals(fontPackage, other.fontPackage) && 
                matchTextDirection == other.matchTextDirection;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((IconData) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = 
                     (codePoint * 397) ^ (fontFamily != null ? fontFamily.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (fontPackage != null ? fontPackage.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ matchTextDirection.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(IconData left, IconData right) {
            return Equals(left, right);
        }
        public static bool operator !=(IconData left, IconData right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            return "IconData(U+" + codePoint.ToString("X5") + ")";
        }
    }
    
    public class IconDataProperty : DiagnosticsProperty<IconData> {
        public IconDataProperty(
            string name,
            IconData value,
            string ifNull = null,
            bool showName = true,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, 
            value,
            showName: showName,
            ifNull: ifNull,
            style: style,
            level: level
        ) {
           
           
        }


        public override Dictionary<string, object> toJsonMap(DiagnosticsSerializationDelegate _delegate) {
            Dictionary<string, object> json = base.toJsonMap(_delegate);
        if (value != null) {
            json["valueProperties"] = new Dictionary<string, object>(){
                {"codePoint", valueT.codePoint},
            };
        }
        return json;
    }
    }
}