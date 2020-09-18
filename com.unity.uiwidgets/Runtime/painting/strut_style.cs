using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    public class StrutStyle : Diagnosticable {
        public StrutStyle(
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            float? fontSize = null,
            float? height = null,
            float? leading = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            bool forceStrutHeight = false,
            string debugLabel = null
        ) {
            D.assert(fontSize == null || fontSize > 0);
            D.assert(leading == null || leading >= 0);
            this.fontFamily = fontFamily;
            _fontFamilyFallback = fontFamilyFallback;
            this.fontSize = fontSize;
            this.height = height;
            this.fontWeight = fontWeight;
            this.fontStyle = fontStyle;
            this.leading = leading;
            this.forceStrutHeight = forceStrutHeight;
            this.debugLabel = debugLabel;
        }

        public static StrutStyle fromTextStyle(
            TextStyle textStyle,
            string fontFamily = null,
            List<string> fontFamilyFallback = null,
            float? fontSize = null,
            float? height = null,
            float? leading = null,
            FontWeight fontWeight = null,
            FontStyle? fontStyle = null,
            bool forceStrutHeight = false,
            string debugLabel = null
        ) {
            D.assert(textStyle != null);
            D.assert(fontSize == null || fontSize > 0);
            D.assert(leading == null || leading >= 0);
            return new StrutStyle(
                fontFamily: fontFamily ?? textStyle.fontFamily,
                fontFamilyFallback: fontFamilyFallback ?? textStyle.fontFamilyFallback,
                height: height ?? textStyle.height,
                fontSize: fontSize ?? textStyle.fontSize,
                fontWeight: fontWeight ?? textStyle.fontWeight,
                fontStyle: fontStyle ?? textStyle.fontStyle,
                debugLabel: debugLabel ?? textStyle.debugLabel
            );
        }

        public static readonly StrutStyle disabled = new StrutStyle(
            height: 0.0f,
            leading: 0.0f
        );

        public readonly string fontFamily;

        public List<string> fontFamilyFallback {
            get { return _fontFamilyFallback; }
        }

        readonly List<string> _fontFamilyFallback;

        public readonly float? fontSize;
        public readonly float? height;
        public readonly FontWeight fontWeight;
        public readonly FontStyle? fontStyle;
        public readonly float? leading;
        public readonly bool forceStrutHeight;
        public readonly string debugLabel;

        public RenderComparison compareTo(StrutStyle other) {
            if (ReferenceEquals(this, other)) {
                return RenderComparison.identical;
            }

            if (other == null) {
                return RenderComparison.layout;
            }

            if (fontFamily != other.fontFamily ||
                fontSize != other.fontSize ||
                fontWeight != other.fontWeight ||
                fontStyle != other.fontStyle ||
                height != other.height ||
                leading != other.leading ||
                forceStrutHeight != other.forceStrutHeight ||
                !CollectionUtils.equalsList(fontFamilyFallback, other.fontFamilyFallback)) {
                return RenderComparison.layout;
            }

            return RenderComparison.identical;
        }

        public StrutStyle inheritFromTextStyle(TextStyle other) {
            if (other == null) {
                return this;
            }

            return new StrutStyle(
                fontFamily: fontFamily ?? other.fontFamily,
                fontFamilyFallback: fontFamilyFallback ?? other.fontFamilyFallback,
                height: height ?? other.height,
                leading: leading,
                fontSize: fontSize ?? other.fontSize,
                fontWeight: fontWeight ?? other.fontWeight,
                fontStyle: fontStyle ?? other.fontStyle,
                forceStrutHeight: forceStrutHeight,
                debugLabel: debugLabel ?? other.debugLabel
            );
        }

        public bool Equals(StrutStyle other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return fontFamily == other.fontFamily &&
                   fontSize == other.fontSize &&
                   fontWeight == other.fontWeight &&
                   fontStyle == other.fontStyle &&
                   height == other.height &&
                   leading == other.leading &&
                   forceStrutHeight == other.forceStrutHeight;
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

            return Equals((StrutStyle) obj);
        }

        public static bool operator ==(StrutStyle left, StrutStyle right) {
            return Equals(left, right);
        }

        public static bool operator !=(StrutStyle left, StrutStyle right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = fontFamily?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (fontSize?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (fontWeight?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (fontStyle?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (height?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (leading?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ forceStrutHeight.GetHashCode();
                return hashCode;
            }
        }

        public override string toStringShort() {
            return $"{GetType()}";
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            if (debugLabel != null) {
                properties.add(new MessageProperty("debugLabel", debugLabel));
            }

            List<DiagnosticsNode> styles = new List<DiagnosticsNode>();
            styles.Add(new StringProperty("family", fontFamily, defaultValue: foundation_.kNullDefaultValue,
                quoted: false));
            styles.Add(new EnumerableProperty<string>("familyFallback", fontFamilyFallback));
            styles.Add(new DiagnosticsProperty<float?>("size", fontSize,
                defaultValue: foundation_.kNullDefaultValue));
            string weightDescription = "";
            if (fontWeight != null) {
                weightDescription = fontWeight.weightValue.ToString();
            }

            styles.Add(new DiagnosticsProperty<FontWeight>(
                "weight", fontWeight,
                description: weightDescription,
                defaultValue: foundation_.kNullDefaultValue
            ));
            styles.Add(new EnumProperty<FontStyle?>("style", fontStyle,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new DiagnosticsProperty<float?>("height", height,
                defaultValue: foundation_.kNullDefaultValue));
            styles.Add(new FlagProperty("forceStrutHeight", value: forceStrutHeight,
                defaultValue: foundation_.kNullDefaultValue));

            bool styleSpecified = styles.Any((DiagnosticsNode n) => !n.isFiltered(DiagnosticLevel.info));
            foreach (var style in styles) {
                properties.add(style);
            }

            if (!styleSpecified) {
                properties.add(new FlagProperty("forceStrutHeight", value: forceStrutHeight,
                    ifTrue: "<strut height forced>",
                    ifFalse: "<strut height normal>"));
            }
        }
    }
}