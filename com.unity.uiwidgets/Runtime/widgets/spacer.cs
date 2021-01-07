using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using UnityEngine;
using Brightness = Unity.UIWidgets.ui.Brightness;

namespace Unity.UIWidgets.widgets {
    public class Spacer : StatelessWidget {
    
        public Spacer(
            Key key = null,
            int flex = 1)
            : base(key: key) {
            this.flex = flex;
            D.assert(flex != null);
            D.assert(flex > 0);
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