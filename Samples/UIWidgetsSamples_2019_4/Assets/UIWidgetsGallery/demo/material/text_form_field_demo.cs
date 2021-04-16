using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using uiwidgets;
using UIWidgets.Runtime.material;
using UIWidgetsGallery.gallery;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.material
{
    internal class TextFormFieldDemo : StatefulWidget
    {
        public TextFormFieldDemo(Key key = null) : base(key: key)
        {
        }

        public static readonly string routeName = "/material/text-form-field";

        public override State createState()
        {
            return new TextFormFieldDemoState();
        }
    }

    internal class PersonData
    {
        public string name = "";
        public string phoneNumber = "";
        public string email = "";
        public string password = "";
    }

    internal class PasswordField : StatefulWidget
    {
        public PasswordField(
            Key fieldKey = null,
            string hintText = null,
            string labelText = null,
            string helperText = null,
            FormFieldSetter<string> onSaved = null,
            FormFieldValidator<string> validator = null,
            ValueChanged<string> onFieldSubmitted = null
        )
        {
            this.fieldKey = fieldKey;
            this.hintText = hintText;
            this.labelText = labelText;
            this.helperText = helperText;
            this.onSaved = onSaved;
            this.validator = validator;
            this.onFieldSubmitted = onFieldSubmitted;
        }

        public readonly Key fieldKey;
        public readonly string hintText;
        public readonly string labelText;
        public readonly string helperText;
        public readonly FormFieldSetter<string> onSaved;
        public readonly FormFieldValidator<string> validator;
        public readonly ValueChanged<string> onFieldSubmitted;


        public override State createState()
        {
            return new _PasswordFieldState();
        }
    }

    internal class _PasswordFieldState : State<PasswordField>
    {
        private bool _obscureText = true;

        public override Widget build(BuildContext context)
        {
            return new TextFormField(
                key: widget.fieldKey,
                obscureText: _obscureText,
                maxLength: 8,
                onSaved: widget.onSaved,
                validator: widget.validator,
                onFieldSubmitted: widget.onFieldSubmitted,
                decoration: new InputDecoration(
                    border: new UnderlineInputBorder(),
                    filled: true,
                    hintText: widget.hintText,
                    labelText: widget.labelText,
                    helperText: widget.helperText,
                    suffixIcon: new GestureDetector(
                        dragStartBehavior: DragStartBehavior.down,
                        onTap: () => { setState(() => { _obscureText = !_obscureText; }); },
                        child: new Icon(
                            _obscureText ? Icons.visibility : Icons.visibility_off
                        )
                    )
                )
            );
        }
    }

    internal class TextFormFieldDemoState : State<TextFormFieldDemo>
    {
        private readonly GlobalKey<ScaffoldState> _scaffoldKey = GlobalKey<ScaffoldState>.key();

        private PersonData person = new PersonData();

        private void showInSnackBar(string value)
        {
            _scaffoldKey.currentState.showSnackBar(new SnackBar(
                content: new Text(value)
            ));
        }

        private bool _autovalidate = false;
        private bool _formWasEdited = false;

        private readonly GlobalKey<FormState> _formKey = GlobalKey<FormState>.key();
        private readonly GlobalKey<FormFieldState<string>> _passwordFieldKey = GlobalKey<FormFieldState<string>>.key();
        private readonly _UsNumberTextInputFormatter _phoneNumberFormatter = new _UsNumberTextInputFormatter();

        private void _handleSubmitted()
        {
            FormState form = _formKey.currentState;
            if (!form.validate())
            {
                _autovalidate = true; // Start validating on every change"
                showInSnackBar("Please fix the errors in red before submitting.");
            }
            else
            {
                form.save();
                showInSnackBar($"{person.name}'s phone number is ${person.phoneNumber}");
            }
        }

        private string _validateName(string value)
        {
            _formWasEdited = true;
            if (value.isEmpty())
                return "Name is required.";
            if (!Regex.IsMatch(value, @"^[a-zA-Z]+$"))
                return "Please enter only alphabetical characters.";
            return null;
        }

        private string _validatePhoneNumber(string value)
        {
            _formWasEdited = true;
            if (!Regex.IsMatch(value, @"^\(\d\d\d\) \d\d\d\-\d\d\d\d$"))
                return "(###) ###-#### - Enter a US phone number.";
            return null;
        }

        private string _validatePassword(string value)
        {
            _formWasEdited = true;
            FormFieldState<string> passwordField = _passwordFieldKey.currentState;
            if (passwordField.value == null || passwordField.value.isEmpty())
                return "Please enter a password.";
            if (passwordField.value != value)
                return "The passwords don't match";
            return null;
        }

        private Future<bool> _warnUserAboutInvalidData()
        {
            FormState form = _formKey.currentState;
            if (form == null || !_formWasEdited || form.validate())
                return Future.value(true).to<bool>();

            return material_.showDialog<bool>(
                context: context,
                builder: (BuildContext context) =>
                {
                    return new AlertDialog(
                        title: new Text("This form has errors"),
                        content: new Text("Really leave this form?"),
                        actions: new List<Widget>
                        {
                            new FlatButton(
                                child: new Text("YES"),
                                onPressed: () => { Navigator.of(context).pop(true); }
                            ),
                            new FlatButton(
                                child: new Text("NO"),
                                onPressed: () => { Navigator.of(context).pop(false); }
                            )
                        }
                    );
                }
            )?? Future.value(false).to<bool>();
        }


        public override Widget build(BuildContext context)
        {
            return new Scaffold(
                drawerDragStartBehavior: DragStartBehavior.down,
                key: _scaffoldKey,
                appBar: new AppBar(
                    title: new Text("Text fields"),
                    actions: new List<Widget> {new MaterialDemoDocumentationButton(TextFormFieldDemo.routeName)}
                ),
                body: new SafeArea(
                    top: false,
                    bottom: false,
                    child: new Form(
                        key: _formKey,
                        autovalidate: _autovalidate,
                        onWillPop: _warnUserAboutInvalidData,
                        child: new Scrollbar(
                            child: new SingleChildScrollView(
                                dragStartBehavior: DragStartBehavior.down,
                                padding: EdgeInsets.symmetric(horizontal: 16.0f),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.stretch,
                                    children: new List<Widget>
                                    {
                                        new SizedBox(height: 24.0f),
                                        new TextFormField(
                                            textCapitalization: TextCapitalization.words,
                                            decoration: new InputDecoration(
                                                border: new UnderlineInputBorder(),
                                                filled: true,
                                                icon: new Icon(Icons.person),
                                                hintText: "What do people call you?",
                                                labelText: "Name *"
                                            ),
                                            onSaved: (string value) => { person.name = value; },
                                            validator: _validateName
                                        ),
                                        new SizedBox(height: 24.0f),
                                        new TextFormField(
                                            decoration: new InputDecoration(
                                                border: new UnderlineInputBorder(),
                                                filled: true,
                                                icon: new Icon(Icons.phone),
                                                hintText: "Where can we reach you?",
                                                labelText: "Phone Number *",
                                                prefixText: "+1"
                                            ),
                                            keyboardType: TextInputType.phone,
                                            onSaved: (string value) => { person.phoneNumber = value; },
                                            validator: _validatePhoneNumber,
                                            // TextInputFormatters are applied in sequence.
                                            inputFormatters: new List<TextInputFormatter>
                                            {
                                                WhitelistingTextInputFormatter.digitsOnly,
                                                // Fit the validating format.
                                                _phoneNumberFormatter
                                            }
                                        ),
                                        new SizedBox(height: 24.0f),
                                        new TextFormField(
                                            decoration: new InputDecoration(
                                                border: new UnderlineInputBorder(),
                                                filled: true,
                                                icon: new Icon(Icons.email),
                                                hintText: "Your email address",
                                                labelText: "E-mail"
                                            ),
                                            keyboardType: TextInputType.emailAddress,
                                            onSaved: (string value) => { person.email = value; }
                                        ),
                                        new SizedBox(height: 24.0f),
                                        new TextFormField(
                                            decoration: new InputDecoration(
                                                border: new OutlineInputBorder(),
                                                hintText:
                                                "Tell us about yourself (e.g., write down what you do or what hobbies you have)",
                                                helperText: "Keep it short, this is just a demo.",
                                                labelText: "Life story"
                                            ),
                                            maxLines: 3
                                        ),
                                        new SizedBox(height: 24.0f),
                                        new TextFormField(
                                            keyboardType: TextInputType.number,
                                            decoration: new InputDecoration(
                                                border: new OutlineInputBorder(),
                                                labelText: "Salary",
                                                prefixText: "$",
                                                suffixText: "USD",
                                                suffixStyle: new TextStyle(color: Colors.green)
                                            ),
                                            maxLines: 1
                                        ),
                                        new SizedBox(height: 24.0f),
                                        new PasswordField(
                                            fieldKey: _passwordFieldKey,
                                            helperText: "No more than 8 characters.",
                                            labelText: "Password *",
                                            onFieldSubmitted: (string value) =>
                                            {
                                                setState(() => { person.password = value; });
                                            }
                                        ),
                                        new SizedBox(height: 24.0f),
                                        new TextFormField(
                                            enabled: person.password != null && person.password.isNotEmpty(),
                                            decoration: new InputDecoration(
                                                border: new UnderlineInputBorder(),
                                                filled: true,
                                                labelText: "Re-type password"
                                            ),
                                            maxLength: 8,
                                            obscureText: true,
                                            validator: _validatePassword
                                        ),
                                        new SizedBox(height: 24.0f),
                                        new Center(
                                            child: new RaisedButton(
                                                child: new Text("SUBMIT"),
                                                onPressed: _handleSubmitted
                                            )
                                        ),
                                        new SizedBox(height: 24.0f),
                                        new Text(
                                            "* indicates required field",
                                            style: Theme.of(context).textTheme.caption
                                        ),
                                        new SizedBox(height: 24.0f)
                                    }
                                )
                            )
                        )
                    )
                )
            );
        }
    }

    internal class _UsNumberTextInputFormatter : TextInputFormatter
    {
        public override TextEditingValue formatEditUpdate(
            TextEditingValue oldValue,
            TextEditingValue newValue
        )
        {
            int newTextLength = newValue.text.Length;
            int selectionIndex = newValue.selection.end;
            int usedSubstringIndex = 0;
            StringBuilder newText = new StringBuilder();
            if (newTextLength >= 1)
            {
                newText.Append("(");
                if (newValue.selection.end >= 1)
                    selectionIndex++;
            }

            if (newTextLength >= 4)
            {
                newText.Append(newValue.text.Substring(0, usedSubstringIndex = 3) + ") ");
                if (newValue.selection.end >= 3)
                    selectionIndex += 2;
            }

            if (newTextLength >= 7)
            {
                newText.Append(newValue.text.Substring(3, usedSubstringIndex = 6) + "-");
                if (newValue.selection.end >= 6)
                    selectionIndex++;
            }

            if (newTextLength >= 11)
            {
                newText.Append(newValue.text.Substring(6, usedSubstringIndex = 10) + " ");
                if (newValue.selection.end >= 10)
                    selectionIndex++;
            }

            // Dump the rest.
            if (newTextLength >= usedSubstringIndex)
                newText.Append(newValue.text.Substring(usedSubstringIndex));
            return new TextEditingValue(
                text: newText.ToString(),
                selection: TextSelection.collapsed(offset: selectionIndex)
            );
        }
    }
}