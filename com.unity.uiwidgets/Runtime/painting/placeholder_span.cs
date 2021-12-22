using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.ui;

namespace Unity.UIWidgets.painting {
    abstract class PlaceholderSpan : InlineSpan {
        public PlaceholderSpan(
            TextBaseline? baseline = null,
            TextStyle style = null,
            PlaceholderAlignment alignment = PlaceholderAlignment.bottom
            
        ) : base(style: style) {
            this.baseline = baseline;
            this.alignment = alignment;
        }

        public PlaceholderAlignment alignment;

        public TextBaseline? baseline;

        public override void computeToPlainText(
            StringBuilder buffer,
            bool includeSemanticsLabels = true,
            bool includePlaceholders = true
        ) {
            if (includePlaceholders) {
                buffer.Append('\uFFFC');
            }
        }

        public override void computeSemanticsInformation(List<InlineSpanSemanticsInformation> collector) {
            collector.Add(InlineSpanSemanticsInformation.placeholder);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);

            properties.add(new EnumProperty<PlaceholderAlignment>("alignment", alignment, defaultValue: null));
            properties.add(new EnumProperty<TextBaseline>("baseline", (TextBaseline)baseline, defaultValue: null));
        }
    }
}