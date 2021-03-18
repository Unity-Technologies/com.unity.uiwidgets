using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.DevTools
{
    public class PreferencesController
    {
        readonly ValueNotifier<bool> _darkModeTheme = new ValueNotifier<bool>(true);
        
        public ValueListenable<bool> darkModeTheme => _darkModeTheme;
        public void init()
        {
            Debug.Log("PreferencesController: storage not initialized");
        }
        
        public void toggleDarkModeTheme(bool useDarkMode) {
            _darkModeTheme.value = useDarkMode;
        }
    }
}