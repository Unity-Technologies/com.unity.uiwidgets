using Unity.UIWidgets.foundation;

namespace Unity.UIWidgets.widgets {
    public class Spacer : StatelessWidget {
    
        public Spacer(
            Key key = null,
            int? flex = 1)
            : base(key: key) {
            D.assert(flex != null);
            D.assert(flex > 0);
            this.flex = flex.Value;
        }
        public readonly int flex;

        public override Widget build(BuildContext context) {
            return new Expanded(
                flex: flex,
                child: SizedBox.shrink()
                );
            }
        }

    }