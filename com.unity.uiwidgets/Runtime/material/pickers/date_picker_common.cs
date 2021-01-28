using System;

namespace Unity.UIWidgets.material {
    public enum DatePickerEntryMode {
        calendar,

        input,
    }

    public enum DatePickerMode {
        day,

        year,
    }

    public partial class material_ {
        public delegate bool SelectableDayPredicate(DateTime? day);
    }

}