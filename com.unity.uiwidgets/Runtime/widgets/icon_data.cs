using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class IconData : IEquatable<IconData> {
        public IconData(
            int codePoint,
            string fontFamily = null
        ) {
            this.codePoint = codePoint;
            this.fontFamily = fontFamily;
        }

        public readonly int codePoint;

        public readonly string fontFamily;

        public bool Equals(IconData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return codePoint == other.codePoint &&
                   string.Equals(fontFamily, other.fontFamily);
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
                return (codePoint * 397) ^ (fontFamily != null ? fontFamily.GetHashCode() : 0);
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
    
    /// [DiagnosticsProperty] that has an [IconData] as value.
    public class IconDataProperty : DiagnosticsProperty<IconData> {
        /// Create a diagnostics property for [IconData].
        ///
        /// The [showName], [style], and [level] arguments must not be null.
        public IconDataProperty(
            String name,
            IconData value,
            String ifNull = null,
            bool showName = true,
            DiagnosticsTreeStyle style = DiagnosticsTreeStyle.singleLine,
            DiagnosticLevel level = DiagnosticLevel.info
        ) : base(name, value,
            showName: showName,
            ifNull: ifNull,
            style: style,
            level: level
        ) {
            D.assert(showName != null);
            D.assert(style != null);
            D.assert(level != null);
        }


        public override Dictionary<String, Object> toJsonMap(DiagnosticsSerializationDelegate _delegate) {
            Dictionary<String, Object> json = base.toJsonMap(_delegate);
        if (value != null) {
            json["valueProperties"] = new Dictionary<String, Object>(){
                {"codePoint", value.codePoint},
            };
        }
        return json;
    }
    }
}