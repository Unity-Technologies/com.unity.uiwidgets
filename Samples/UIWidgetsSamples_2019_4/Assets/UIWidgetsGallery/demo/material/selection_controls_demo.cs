using System.Collections.Generic;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal static class SelectionControlDemoUtils
    {
        public static readonly string _checkboxText =
            "Checkboxes allow the user to select multiple options from a set. " +
            "A normal checkbox\"s value is true or false and a tristate checkbox\"s " +
            "value can also be null.";

        public static readonly string _checkboxCode = "selectioncontrols_checkbox";

        public static readonly string _radioText =
            "Radio buttons allow the user to select one option from a set. Use radio " +
            "buttons for exclusive selection if you think that the user needs to see " +
            "all available options side-by-side.";

        public static readonly string _radioCode = "selectioncontrols_radio";

        public static readonly string _switchText =
            "On/off switches toggle the state of a single settings option. The option " +
            "that the switch controls, as well as the state it’s in, should be made " +
            "clear from the corresponding inline label.";

        public static readonly string _switchCode = "selectioncontrols_switch";
    }

    internal class SelectionControlsDemo : StatefulWidget
    {
        public static readonly string routeName = "/material/selection-controls";

        public override State createState()
        {
            return new _SelectionControlsDemoState();
        }
    }

    internal class _SelectionControlsDemoState : State<SelectionControlsDemo>
    {
        public override Widget build(BuildContext context)
        {
            List<ComponentDemoTabData> demos = new List<ComponentDemoTabData>
            {
                new ComponentDemoTabData(
                    tabName: "CHECKBOX",
                    description: SelectionControlDemoUtils._checkboxText,
                    demoWidget: buildCheckbox(),
                    exampleCodeTag: SelectionControlDemoUtils._checkboxCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/Checkbox-class.html"
                ),
                new ComponentDemoTabData(
                    tabName: "RADIO",
                    description: SelectionControlDemoUtils._radioText,
                    demoWidget: buildRadio(),
                    exampleCodeTag: SelectionControlDemoUtils._radioCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/Radio-class.html"
                ),
                new ComponentDemoTabData(
                    tabName: "SWITCH",
                    description: SelectionControlDemoUtils._switchText,
                    demoWidget: buildSwitch(),
                    exampleCodeTag: SelectionControlDemoUtils._switchCode,
                    documentationUrl: "https://docs.flutter.io/flutter/material/Switch-class.html"
                )
            };

            return new TabbedComponentDemoScaffold(
                title: "Selection controls",
                demos: demos
            );
        }

        private bool checkboxValueA = true;
        private bool checkboxValueB = false;
        private bool? checkboxValueC;
        private int radioValue = 0;
        private bool switchValue = false;

        private void handleRadioValueChanged(int value)
        {
            setState(() => { radioValue = value; });
        }

        private Widget buildCheckbox()
        {
            return new Align(
                alignment: new Alignment(0.0f, -0.2f),
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>
                    {
                        new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new Checkbox(
                                    value: checkboxValueA,
                                    onChanged: (bool? value) => { setState(() => { checkboxValueA = value.Value; }); }
                                ),
                                new Checkbox(
                                    value: checkboxValueB,
                                    onChanged: (bool? value) => { setState(() => { checkboxValueB = value.Value; }); }
                                ),
                                new Checkbox(
                                    value: checkboxValueC,
                                    tristate: true,
                                    onChanged: (bool? value) => { setState(() => { checkboxValueC = value; }); }
                                )
                            }
                        ),
                        new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                // Disabled checkboxes
                                new Checkbox(value: true, onChanged: null),
                                new Checkbox(value: false, onChanged: null),
                                new Checkbox(value: null, tristate: true, onChanged: null),
                            }
                        )
                    }
                )
            );
        }

        private Widget buildRadio()
        {
            return new Align(
                alignment: new Alignment(0.0f, -0.2f),
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>
                    {
                        new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new Radio<int>(
                                    value: 0,
                                    groupValue: radioValue,
                                    onChanged: handleRadioValueChanged
                                ),
                                new Radio<int>(
                                    value: 1,
                                    groupValue: radioValue,
                                    onChanged: handleRadioValueChanged
                                ),
                                new Radio<int>(
                                    value: 2,
                                    groupValue: radioValue,
                                    onChanged: handleRadioValueChanged
                                )
                            }
                        ),
                        // Disabled radio buttons
                        new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget>
                            {
                                new Radio<int>(
                                    value: 0,
                                    groupValue: 0,
                                    onChanged: null
                                ),
                                new Radio<int>(
                                    value: 1,
                                    groupValue: 0,
                                    onChanged: null
                                ),
                                new Radio<int>(
                                    value: 2,
                                    groupValue: 0,
                                    onChanged: null
                                )
                            }
                        )
                    }
                )
            );
        }

        private Widget buildSwitch()
        {
            return new Align(
                alignment: new Alignment(0.0f, -0.2f),
                child: new Row(
                    mainAxisSize: MainAxisSize.min,
                    children: new List<Widget>
                    {
                        Switch.adaptive(
                            value: switchValue,
                            onChanged: (bool? value) => { setState(() => { switchValue = value.Value; }); }
                        ),
                        // Disabled switches
                        Switch.adaptive(value: true, onChanged: null),
                        Switch.adaptive(value: false, onChanged: null)
                    }
                )
            );
        }
    }
}