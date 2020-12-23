using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
//using RSG;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.painting;
using Color = Unity.UIWidgets.ui.Color;
using Brightness = Unity.UIWidgets.ui.Brightness;
namespace Unity.UIWidgets.material {
    public class ThemedataMaterialUtils {
        public static readonly Color _kLightThemeHighlightColor = new Color(0x66BCBCBC); 
        public static readonly Color _kLightThemeSplashColor = new Color(0x66C8C8C8); 
        public static readonly Color _kDarkThemeHighlightColor = new Color(0x40CCCCCC);
        public static readonly Color _kDarkThemeSplashColor = new Color(0x40CCCCCC);
    } 
    public enum MaterialTapTargetSize {

      padded,

      shrinkWrap,
    }

    public class ThemeData : Diagnosticable { 
        public ThemeData(
            Brightness? brightness = null,
            VisualDensity visualDensity = null,
            MaterialColor primarySwatch = null,
            Color primaryColor = null,
            Brightness? primaryColorBrightness = null,
            Color primaryColorLight = null,
            Color primaryColorDark = null,
            Color accentColor = null,
            Brightness? accentColorBrightness = null,
            Color canvasColor = null,
            Color scaffoldBackgroundColor = null,
            Color bottomAppBarColor = null,
            Color cardColor = null,
            Color dividerColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            Color selectedRowColor = null,
            Color unselectedWidgetColor = null,
            Color disabledColor = null,
            Color buttonColor = null,
            ButtonThemeData buttonTheme = null,
            ToggleButtonsThemeData toggleButtonsTheme = null,
            Color secondaryHeaderColor = null,
            Color textSelectionColor = null,
            Color cursorColor = null,
            Color textSelectionHandleColor = null,
            Color backgroundColor = null,
            Color dialogBackgroundColor = null,
            Color indicatorColor = null,
            Color hintColor = null,
            Color errorColor = null,
            Color toggleableActiveColor = null,
            String fontFamily = null,
            TextTheme textTheme = null,
            TextTheme primaryTextTheme = null,
            TextTheme accentTextTheme = null,
            InputDecorationTheme inputDecorationTheme = null,
            IconThemeData iconTheme = null,
            IconThemeData primaryIconTheme = null,
            IconThemeData accentIconTheme = null,
            SliderThemeData sliderTheme = null,
            TabBarTheme tabBarTheme = null,
            TooltipThemeData tooltipTheme = null,
            CardTheme cardTheme = null,
            ChipThemeData chipTheme = null,
            TargetPlatform? platform = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            bool? applyElevationOverlayColor = null,
            PageTransitionsTheme pageTransitionsTheme = null,
            AppBarTheme appBarTheme = null,
            BottomAppBarTheme bottomAppBarTheme = null,
            ColorScheme colorScheme = null,
            DialogTheme dialogTheme = null,
            FloatingActionButtonThemeData floatingActionButtonTheme = null,
            NavigationRailThemeData navigationRailTheme = null,
            Typography typography = null,
            CupertinoThemeData cupertinoOverrideTheme = null,
            SnackBarThemeData snackBarTheme = null,
            BottomSheetThemeData bottomSheetTheme = null,
            PopupMenuThemeData popupMenuTheme = null,
            MaterialBannerThemeData bannerTheme = null,
            DividerThemeData dividerTheme = null,
            ButtonBarThemeData buttonBarTheme = null
            ) {
            
            brightness = brightness ?? Brightness.light;
            bool isDark = brightness == Brightness.dark;
            visualDensity = visualDensity ?? new VisualDensity();
            primarySwatch = primarySwatch ??  Colors.blue;
            primaryColor =  primaryColor ?? (isDark ? Colors.grey[900] : primarySwatch);
            primaryColorBrightness = primaryColorBrightness ?? estimateBrightnessForColor(primaryColor);
            primaryColorLight = primaryColorLight ?? (isDark ? Colors.grey[500] : primarySwatch[100]);
            primaryColorDark = primaryColorDark ?? (isDark ? Colors.black : primarySwatch[700]);
            bool primaryIsDark = primaryColorBrightness == Brightness.dark;
            toggleableActiveColor = toggleableActiveColor ?? (isDark ? Colors.tealAccent[200] : (accentColor ?? primarySwatch[600]));
            accentColor = accentColor ?? (isDark ? Colors.tealAccent[200] : primarySwatch[500]);
            accentColorBrightness = accentColorBrightness ?? estimateBrightnessForColor(accentColor);
            bool accentIsDark = accentColorBrightness == Brightness.dark;
            canvasColor = canvasColor ?? (isDark ? Colors.grey[850] : Colors.grey[50]);
            scaffoldBackgroundColor = scaffoldBackgroundColor ?? canvasColor;
            bottomAppBarColor = bottomAppBarColor ?? (isDark ? Colors.grey[800] : Colors.white);
            cardColor = cardColor ?? (isDark ? Colors.grey[800] : Colors.white);
            dividerColor = dividerColor ?? (isDark ? new Color(0x1FFFFFFF) : new Color(0x1F000000));
            colorScheme = colorScheme ?? ColorScheme.fromSwatch(
              primarySwatch: primarySwatch,
              primaryColorDark: primaryColorDark,
              accentColor: accentColor,
              cardColor: cardColor,
              backgroundColor: backgroundColor,
              errorColor: errorColor,
              brightness: brightness
            );

            splashFactory = splashFactory ?? InkSplash.splashFactory;
            selectedRowColor = selectedRowColor ?? Colors.grey[100];
            unselectedWidgetColor = unselectedWidgetColor ?? (isDark ? Colors.white70 : Colors.black54);
            // Spec doesn't specify a dark theme secondaryHeaderColor, this is a guess.
            secondaryHeaderColor = secondaryHeaderColor ?? (isDark ? Colors.grey[700] : primarySwatch[50]);
            textSelectionColor = textSelectionColor ?? (isDark ? accentColor : primarySwatch[200]);
            // TODO(sandrasandeep): change to color provided by Material Design team
            cursorColor = cursorColor ??  Color.fromRGBO(66, 133, 244, 1.0f);
            textSelectionHandleColor = textSelectionHandleColor ?? (isDark ? Colors.tealAccent[400] : primarySwatch[300]);
            backgroundColor = backgroundColor ?? (isDark ? Colors.grey[700] : primarySwatch[200]);
            dialogBackgroundColor = dialogBackgroundColor ?? (isDark ? Colors.grey[800] : Colors.white);
            indicatorColor = indicatorColor ?? (accentColor == primaryColor ? Colors.white : accentColor);
            hintColor = hintColor ?? (isDark ? new Color(0x80FFFFFF) : new Color(0x8A000000));
            errorColor = errorColor ??  Colors.red[700];
            inputDecorationTheme = inputDecorationTheme ?? new InputDecorationTheme();
            pageTransitionsTheme = pageTransitionsTheme ?? new PageTransitionsTheme();
            primaryIconTheme = primaryIconTheme ?? (primaryIsDark ? new IconThemeData(color: Colors.white) : new IconThemeData(color: Colors.black));
            accentIconTheme = accentIconTheme ?? (accentIsDark ? new IconThemeData(color: Colors.white) : new IconThemeData(color: Colors.black));
            iconTheme = iconTheme ?? (isDark ? new IconThemeData(color: Colors.white) : new IconThemeData(color: Colors.black87));
            platform = platform ?? TargetPlatform.windows; /// ?? todo
            typography = typography ??  Typography.material2014();
            TextTheme defaultTextTheme = isDark ? typography.white : typography.black;
            TextTheme defaultPrimaryTextTheme = primaryIsDark ? typography.white : typography.black;
            TextTheme defaultAccentTextTheme = accentIsDark ? typography.white : typography.black;
            if (fontFamily != null) {
              defaultTextTheme = defaultTextTheme.apply(fontFamily: fontFamily);
              defaultPrimaryTextTheme = defaultPrimaryTextTheme.apply(fontFamily: fontFamily);
              defaultAccentTextTheme = defaultAccentTextTheme.apply(fontFamily: fontFamily);
            }
            textTheme = defaultTextTheme.merge(textTheme);
            primaryTextTheme = defaultPrimaryTextTheme.merge(primaryTextTheme);
            accentTextTheme = defaultAccentTextTheme.merge(accentTextTheme);
            materialTapTargetSize = materialTapTargetSize ??  MaterialTapTargetSize.padded;
            applyElevationOverlayColor = applyElevationOverlayColor ??  false;

            buttonColor = buttonColor ?? (isDark ? primarySwatch[600] : Colors.grey[300]);
            focusColor = focusColor ?? (isDark ? Colors.white.withOpacity(0.12f) : Colors.black.withOpacity(0.12f));
            hoverColor = hoverColor ?? (isDark ? Colors.white.withOpacity(0.04f) : Colors.black.withOpacity(0.04f));
            buttonTheme = buttonTheme ??  new ButtonThemeData(
              colorScheme: colorScheme,
              buttonColor: buttonColor,
              disabledColor: disabledColor,
              focusColor: focusColor,
              hoverColor: hoverColor,
              highlightColor: highlightColor,
              splashColor: splashColor,
              materialTapTargetSize: materialTapTargetSize
            );
            toggleButtonsTheme = toggleButtonsTheme ?? new ToggleButtonsThemeData();
            disabledColor = disabledColor ?? ( isDark ? Colors.white38 : Colors.black38);
            highlightColor = highlightColor ?? (isDark ? ThemedataMaterialUtils._kDarkThemeHighlightColor : ThemedataMaterialUtils._kLightThemeHighlightColor);
            splashColor = splashColor ?? (isDark ? ThemedataMaterialUtils._kDarkThemeSplashColor : ThemedataMaterialUtils._kLightThemeSplashColor);

            sliderTheme = sliderTheme ?? new SliderThemeData();
            tabBarTheme = tabBarTheme ?? new TabBarTheme();
            tooltipTheme = tooltipTheme ?? new TooltipThemeData();
            appBarTheme = appBarTheme ?? new AppBarTheme();
            bottomAppBarTheme = bottomAppBarTheme ?? new BottomAppBarTheme();
            cardTheme = cardTheme ?? new CardTheme();
            chipTheme = chipTheme ?? ChipThemeData.fromDefaults(
              secondaryColor: primaryColor,
              brightness: brightness,
              labelStyle: textTheme.bodyText1
            );
            dialogTheme = dialogTheme ??new DialogTheme();
            floatingActionButtonTheme = floatingActionButtonTheme ?? new FloatingActionButtonThemeData();
            navigationRailTheme = navigationRailTheme ?? new NavigationRailThemeData();
            cupertinoOverrideTheme = cupertinoOverrideTheme?.noDefault();
            snackBarTheme = snackBarTheme ?? new SnackBarThemeData();
            bottomSheetTheme = bottomSheetTheme ?? new BottomSheetThemeData();
            popupMenuTheme =  popupMenuTheme ?? new PopupMenuThemeData();
            bannerTheme = bannerTheme ?? new MaterialBannerThemeData();
            dividerTheme = dividerTheme ?? new DividerThemeData();
            buttonBarTheme = buttonBarTheme ?? new ButtonBarThemeData();
            D.assert(brightness != null);
            D.assert(visualDensity != null);
            D.assert(primaryColor != null);
            D.assert(primaryColorBrightness != null);
            D.assert(primaryColorLight != null);
            D.assert(primaryColorDark != null);
            D.assert(accentColor != null);
            D.assert(accentColorBrightness != null);
            D.assert(canvasColor != null);
            D.assert(scaffoldBackgroundColor != null);
            D.assert(bottomAppBarColor != null);
            D.assert(cardColor != null);
            D.assert(dividerColor != null);
            D.assert(focusColor != null);
            D.assert(hoverColor != null);
            D.assert(highlightColor != null);
            D.assert(splashColor != null);
            D.assert(splashFactory != null);
            D.assert(selectedRowColor != null);
            D.assert(unselectedWidgetColor != null);
            D.assert(disabledColor != null);
            D.assert(toggleableActiveColor != null);
            D.assert(buttonTheme != null);
            D.assert(toggleButtonsTheme != null);
            D.assert(secondaryHeaderColor != null);
            D.assert(textSelectionColor != null);
            D.assert(cursorColor != null);
            D.assert(textSelectionHandleColor != null);
            D.assert(backgroundColor != null);
            D.assert(dialogBackgroundColor != null);
            D.assert(indicatorColor != null);
            D.assert(hintColor != null);
            D.assert(errorColor != null);
            D.assert(textTheme != null);
            D.assert(primaryTextTheme != null);
            D.assert(accentTextTheme != null);
            D.assert(inputDecorationTheme != null);
            D.assert(iconTheme != null);
            D.assert(primaryIconTheme != null);
            D.assert(accentIconTheme != null);
            D.assert(sliderTheme != null);
            D.assert(tabBarTheme != null);
            D.assert(tooltipTheme != null);
            D.assert(cardTheme != null);
            D.assert(chipTheme != null);
            D.assert(platform != null);
            D.assert(materialTapTargetSize != null);
            D.assert(pageTransitionsTheme != null);
            D.assert(appBarTheme != null);
            D.assert(bottomAppBarTheme != null);
            D.assert(colorScheme != null);
            D.assert(dialogTheme != null);
            D.assert(floatingActionButtonTheme != null);
            D.assert(navigationRailTheme != null);
            D.assert(typography != null);
            D.assert(snackBarTheme != null);
            D.assert(bottomSheetTheme != null);
            D.assert(popupMenuTheme != null);
            D.assert(bannerTheme != null);
            D.assert(dividerTheme != null);
            D.assert(buttonBarTheme != null);
            
            this.brightness = brightness;
            this.visualDensity = visualDensity;
            this.primaryColor = primaryColor;
            this.primaryColorBrightness = primaryColorBrightness;
            this.primaryColorLight = primaryColorLight;
            this.primaryColorDark = primaryColorDark;
            this.accentColor = accentColor;
            this.accentColorBrightness = accentColorBrightness;
            this.canvasColor = canvasColor;
            this.scaffoldBackgroundColor = scaffoldBackgroundColor;
            this.bottomAppBarColor = bottomAppBarColor;
            this.cardColor = cardColor;
            this.dividerColor = dividerColor;
            this.focusColor = focusColor;
            this.hoverColor = hoverColor;
            this.highlightColor = highlightColor;
            this.splashColor = splashColor;
            this.splashFactory = splashFactory;
            this.selectedRowColor = selectedRowColor;
            this.unselectedWidgetColor = unselectedWidgetColor;
            this.disabledColor = disabledColor;
            this.buttonTheme = buttonTheme;
            this.buttonColor = buttonColor;
            this.toggleButtonsTheme = toggleButtonsTheme;
            this.toggleableActiveColor = toggleableActiveColor;
            this.secondaryHeaderColor = secondaryHeaderColor;
            this.textSelectionColor = textSelectionColor;
            this.cursorColor = cursorColor;
            this.textSelectionHandleColor = textSelectionHandleColor;
            this.backgroundColor = backgroundColor;
            this.dialogBackgroundColor = dialogBackgroundColor;
            this.indicatorColor = indicatorColor;
            this.hintColor = hintColor;
            this.errorColor = errorColor;
            this.textTheme = textTheme;
            this.primaryTextTheme = primaryTextTheme;
            this.accentTextTheme = accentTextTheme;
            this.inputDecorationTheme = inputDecorationTheme;
            this.iconTheme = iconTheme;
            this.primaryIconTheme = primaryIconTheme;
            this.accentIconTheme = accentIconTheme;
            this.sliderTheme = sliderTheme;
            this.tabBarTheme = tabBarTheme;
            this.tooltipTheme = tooltipTheme;
            this.cardTheme = cardTheme;
            this.chipTheme = chipTheme;
            this.platform = platform;
            this.materialTapTargetSize = materialTapTargetSize;
            this.applyElevationOverlayColor = applyElevationOverlayColor;
            this.pageTransitionsTheme = pageTransitionsTheme;
            this.appBarTheme = appBarTheme;
            this.bottomAppBarTheme = bottomAppBarTheme;
            this.colorScheme = colorScheme;
            this.dialogTheme = dialogTheme;
            this.floatingActionButtonTheme = floatingActionButtonTheme;
            this.navigationRailTheme = navigationRailTheme;
            this.typography = typography;
            this.cupertinoOverrideTheme = cupertinoOverrideTheme;
            this.snackBarTheme = snackBarTheme;
            this.bottomSheetTheme = bottomSheetTheme;
            this.popupMenuTheme = popupMenuTheme;
            this.bannerTheme = bannerTheme;
            this.dividerTheme = dividerTheme;
            this.buttonBarTheme = buttonBarTheme;

        }
        public static ThemeData from(
            ColorScheme colorScheme,
            TextTheme textTheme = null
        ){
            D.assert(colorScheme != null);
            bool isDark = colorScheme.brightness == Brightness.dark;
            Color primarySurfaceColor = isDark ? colorScheme.surface : colorScheme.primary;
            Color onPrimarySurfaceColor = isDark ? colorScheme.onSurface : colorScheme.onPrimary;

            return new ThemeData(
                brightness: colorScheme.brightness,
                primaryColor: primarySurfaceColor,
                primaryColorBrightness: ThemeData.estimateBrightnessForColor(primarySurfaceColor),
                canvasColor: colorScheme.background,
                accentColor: colorScheme.secondary,
                accentColorBrightness: ThemeData.estimateBrightnessForColor(colorScheme.secondary),
                scaffoldBackgroundColor: colorScheme.background,
                bottomAppBarColor: colorScheme.surface,
                cardColor: colorScheme.surface,
                dividerColor: colorScheme.onSurface.withOpacity(0.12f),
                backgroundColor: colorScheme.background,
                dialogBackgroundColor: colorScheme.background,
                errorColor: colorScheme.error,
                textTheme: textTheme,
                indicatorColor: onPrimarySurfaceColor,
                applyElevationOverlayColor: isDark,
                colorScheme: colorScheme
            );
        }

        public static ThemeData light() {
            return new ThemeData(brightness: Brightness.light);
        }

        public ThemeData dark() {
            return new ThemeData(brightness: Brightness.dark);
        }

        public static ThemeData fallback() {
            return ThemeData.light();
        }

        public readonly Brightness? brightness;
        public readonly VisualDensity visualDensity;
        public readonly Color primaryColor;
        public readonly Brightness? primaryColorBrightness;
        public readonly Color primaryColorLight;
        public readonly Color primaryColorDark;
        public readonly Color canvasColor;
        public readonly Color accentColor;
        public readonly Brightness? accentColorBrightness; 
        public readonly Color scaffoldBackgroundColor;
        public readonly Color bottomAppBarColor;
        public readonly Color cardColor;
        public readonly Color dividerColor;
        public readonly Color focusColor;
        public readonly Color hoverColor;
        public readonly Color highlightColor;
        public readonly Color splashColor; 
        public readonly InteractiveInkFeatureFactory splashFactory;
        public readonly Color selectedRowColor;
        public readonly Color unselectedWidgetColor;
        public readonly Color disabledColor;
        public readonly ButtonThemeData buttonTheme; 
        public readonly ToggleButtonsThemeData toggleButtonsTheme;
        public readonly Color buttonColor;
        public readonly Color secondaryHeaderColor;
        public readonly Color textSelectionColor;
        public readonly Color cursorColor;
        public readonly Color textSelectionHandleColor;
        public readonly Color backgroundColor;
        public readonly Color dialogBackgroundColor;
        public readonly Color indicatorColor;
        public readonly Color hintColor;
        public readonly Color errorColor;
        public readonly Color toggleableActiveColor;
        public readonly TextTheme textTheme;
        public readonly TextTheme primaryTextTheme;
        public readonly TextTheme accentTextTheme;
        public readonly InputDecorationTheme inputDecorationTheme;
        public readonly IconThemeData iconTheme;
        public readonly IconThemeData primaryIconTheme;
        public readonly IconThemeData accentIconTheme;
        public readonly SliderThemeData sliderTheme;
        public readonly TabBarTheme tabBarTheme;
        public readonly TooltipThemeData tooltipTheme;
        public readonly CardTheme cardTheme;
        public readonly ChipThemeData chipTheme;
        public readonly TargetPlatform? platform;
        public readonly MaterialTapTargetSize? materialTapTargetSize;
        public readonly bool? applyElevationOverlayColor;
        public readonly PageTransitionsTheme pageTransitionsTheme;
        public readonly AppBarTheme appBarTheme;
        public readonly BottomAppBarTheme bottomAppBarTheme;
        public readonly ColorScheme colorScheme;
        public readonly SnackBarThemeData snackBarTheme;
        public readonly DialogTheme dialogTheme;
        public readonly FloatingActionButtonThemeData floatingActionButtonTheme;
        public readonly NavigationRailThemeData navigationRailTheme;
        public readonly Typography typography;
        public readonly CupertinoThemeData cupertinoOverrideTheme;
        public readonly BottomSheetThemeData bottomSheetTheme;
        public readonly PopupMenuThemeData popupMenuTheme;
        public readonly MaterialBannerThemeData bannerTheme;
        public readonly DividerThemeData dividerTheme;
        public readonly ButtonBarThemeData buttonBarTheme;

  
        public ThemeData copyWith(
            Brightness? brightness = null,
            VisualDensity visualDensity = null,
            MaterialColor primarySwatch = null,
            Color primaryColor = null,
            Brightness? primaryColorBrightness = null,
            Color primaryColorLight = null,
            Color primaryColorDark = null,
            Color accentColor = null,
            Brightness? accentColorBrightness = null,
            Color canvasColor = null,
            Color scaffoldBackgroundColor = null,
            Color bottomAppBarColor = null,
            Color cardColor = null,
            Color dividerColor = null,
            Color focusColor = null,
            Color hoverColor = null,
            Color highlightColor = null,
            Color splashColor = null,
            InteractiveInkFeatureFactory splashFactory = null,
            Color selectedRowColor = null,
            Color unselectedWidgetColor = null,
            Color disabledColor = null,
            Color buttonColor = null,
            ButtonThemeData buttonTheme = null,
            ToggleButtonsThemeData toggleButtonsTheme = null,
            Color secondaryHeaderColor = null,
            Color textSelectionColor = null,
            Color cursorColor = null,
            Color textSelectionHandleColor = null,
            Color backgroundColor = null,
            Color dialogBackgroundColor = null,
            Color indicatorColor = null,
            Color hintColor = null,
            Color errorColor = null,
            Color toggleableActiveColor = null,
            String fontFamily = null,
            TextTheme textTheme = null,
            TextTheme primaryTextTheme = null,
            TextTheme accentTextTheme = null,
            InputDecorationTheme inputDecorationTheme = null,
            IconThemeData iconTheme = null,
            IconThemeData primaryIconTheme = null,
            IconThemeData accentIconTheme = null,
            SliderThemeData sliderTheme = null,
            TabBarTheme tabBarTheme = null,
            TooltipThemeData tooltipTheme = null,
            CardTheme cardTheme = null,
            ChipThemeData chipTheme = null,
            TargetPlatform? platform = null,
            MaterialTapTargetSize? materialTapTargetSize = null,
            bool? applyElevationOverlayColor = null,
            PageTransitionsTheme pageTransitionsTheme = null,
            AppBarTheme appBarTheme = null,
            BottomAppBarTheme bottomAppBarTheme = null,
            ColorScheme colorScheme = null,
            DialogTheme dialogTheme = null,
            FloatingActionButtonThemeData floatingActionButtonTheme = null,
            NavigationRailThemeData navigationRailTheme = null,
            Typography typography = null,
            CupertinoThemeData cupertinoOverrideTheme = null,
            SnackBarThemeData snackBarTheme = null,
            BottomSheetThemeData bottomSheetTheme = null,
            PopupMenuThemeData popupMenuTheme = null,
            MaterialBannerThemeData bannerTheme = null,
            DividerThemeData dividerTheme = null,
            ButtonBarThemeData buttonBarTheme = null
        ) { 
            cupertinoOverrideTheme = cupertinoOverrideTheme?.noDefault();
            return new ThemeData(
              brightness: brightness ?? this.brightness,
              visualDensity: visualDensity ?? this.visualDensity,
              primaryColor: primaryColor ?? this.primaryColor,
              primaryColorBrightness: primaryColorBrightness ?? this.primaryColorBrightness,
              primaryColorLight: primaryColorLight ?? this.primaryColorLight,
              primaryColorDark: primaryColorDark ?? this.primaryColorDark,
              accentColor: accentColor ?? this.accentColor,
              accentColorBrightness: accentColorBrightness ?? this.accentColorBrightness,
              canvasColor: canvasColor ?? this.canvasColor,
              scaffoldBackgroundColor: scaffoldBackgroundColor ?? this.scaffoldBackgroundColor,
              bottomAppBarColor: bottomAppBarColor ?? this.bottomAppBarColor,
              cardColor: cardColor ?? this.cardColor,
              dividerColor: dividerColor ?? this.dividerColor,
              focusColor: focusColor ?? this.focusColor,
              hoverColor: hoverColor ?? this.hoverColor,
              highlightColor: highlightColor ?? this.highlightColor,
              splashColor: splashColor ?? this.splashColor,
              splashFactory: splashFactory ?? this.splashFactory,
              selectedRowColor: selectedRowColor ?? this.selectedRowColor,
              unselectedWidgetColor: unselectedWidgetColor ?? this.unselectedWidgetColor,
              disabledColor: disabledColor ?? this.disabledColor,
              buttonColor: buttonColor ?? this.buttonColor,
              buttonTheme: buttonTheme ?? this.buttonTheme,
              toggleButtonsTheme: toggleButtonsTheme ?? this.toggleButtonsTheme,
              secondaryHeaderColor: secondaryHeaderColor ?? this.secondaryHeaderColor,
              textSelectionColor: textSelectionColor ?? this.textSelectionColor,
              cursorColor: cursorColor ?? this.cursorColor,
              textSelectionHandleColor: textSelectionHandleColor ?? this.textSelectionHandleColor,
              backgroundColor: backgroundColor ?? this.backgroundColor,
              dialogBackgroundColor: dialogBackgroundColor ?? this.dialogBackgroundColor,
              indicatorColor: indicatorColor ?? this.indicatorColor,
              hintColor: hintColor ?? this.hintColor,
              errorColor: errorColor ?? this.errorColor,
              toggleableActiveColor: toggleableActiveColor ?? this.toggleableActiveColor,
              textTheme: textTheme ?? this.textTheme,
              primaryTextTheme: primaryTextTheme ?? this.primaryTextTheme,
              accentTextTheme: accentTextTheme ?? this.accentTextTheme,
              inputDecorationTheme: inputDecorationTheme ?? this.inputDecorationTheme,
              iconTheme: iconTheme ?? this.iconTheme,
              primaryIconTheme: primaryIconTheme ?? this.primaryIconTheme,
              accentIconTheme: accentIconTheme ?? this.accentIconTheme,
              sliderTheme: sliderTheme ?? this.sliderTheme,
              tabBarTheme: tabBarTheme ?? this.tabBarTheme,
              tooltipTheme: tooltipTheme ?? this.tooltipTheme,
              cardTheme: cardTheme ?? this.cardTheme,
              chipTheme: chipTheme ?? this.chipTheme,
              platform: platform ?? this.platform,
              materialTapTargetSize: materialTapTargetSize ?? this.materialTapTargetSize,
              applyElevationOverlayColor: applyElevationOverlayColor ?? this.applyElevationOverlayColor,
              pageTransitionsTheme: pageTransitionsTheme ?? this.pageTransitionsTheme,
              appBarTheme: appBarTheme ?? this.appBarTheme,
              bottomAppBarTheme: bottomAppBarTheme ?? this.bottomAppBarTheme,
              colorScheme: colorScheme ?? this.colorScheme,
              dialogTheme: dialogTheme ?? this.dialogTheme,
              floatingActionButtonTheme: floatingActionButtonTheme ?? this.floatingActionButtonTheme,
              navigationRailTheme: navigationRailTheme ?? this.navigationRailTheme,
              typography: typography ?? this.typography,
              cupertinoOverrideTheme: cupertinoOverrideTheme ?? this.cupertinoOverrideTheme,
              snackBarTheme: snackBarTheme ?? this.snackBarTheme,
              bottomSheetTheme: bottomSheetTheme ?? this.bottomSheetTheme,
              popupMenuTheme: popupMenuTheme ?? this.popupMenuTheme,
              bannerTheme: bannerTheme ?? this.bannerTheme,
              dividerTheme: dividerTheme ?? this.dividerTheme,
              buttonBarTheme: buttonBarTheme ?? this.buttonBarTheme
            );
        }

        public static int _localizedThemeDataCacheSize = 5;

        public static  readonly _FifoCache<_IdentityThemeDataCacheKey, ThemeData> _localizedThemeDataCache = 
            new _FifoCache<_IdentityThemeDataCacheKey, ThemeData>(_localizedThemeDataCacheSize);
        public static ThemeData localize(ThemeData baseTheme, TextTheme localTextGeometry) {
            D.assert(baseTheme != null);
            D.assert(localTextGeometry != null);
            return _localizedThemeDataCache.putIfAbsent(
                new _IdentityThemeDataCacheKey(baseTheme, localTextGeometry),
                ()=>{
                    return baseTheme.copyWith(
                        primaryTextTheme: localTextGeometry.merge(baseTheme.primaryTextTheme),
                        accentTextTheme: localTextGeometry.merge(baseTheme.accentTextTheme),
                        textTheme: localTextGeometry.merge(baseTheme.textTheme)
                    );
                }
            );
        }
        public static Brightness estimateBrightnessForColor(Color color) {
            float relativeLuminance = color.computeLuminance(); 
            float kThreshold = 0.15f;
            if ((relativeLuminance + 0.05f) * (relativeLuminance + 0.05f) > kThreshold)
              return Brightness.light;
            return Brightness.dark;
        }

        public static ThemeData lerp(ThemeData a, ThemeData b, float t) {
             D.assert(a != null);
             D.assert(b != null);
             D.assert(t != null);
             return new ThemeData(
              brightness: t < 0.5f ? a.brightness : b.brightness,
              visualDensity: VisualDensity.lerp(a.visualDensity, b.visualDensity, t),
              primaryColor: Color.lerp(a.primaryColor, b.primaryColor, t),
              primaryColorBrightness: t < 0.5f ? a.primaryColorBrightness : b.primaryColorBrightness,
              primaryColorLight: Color.lerp(a.primaryColorLight, b.primaryColorLight, t),
              primaryColorDark: Color.lerp(a.primaryColorDark, b.primaryColorDark, t),
              canvasColor: Color.lerp(a.canvasColor, b.canvasColor, t),
              accentColor: Color.lerp(a.accentColor, b.accentColor, t),
              accentColorBrightness: t < 0.5 ? a.accentColorBrightness : b.accentColorBrightness,
              scaffoldBackgroundColor: Color.lerp(a.scaffoldBackgroundColor, b.scaffoldBackgroundColor, t),
              bottomAppBarColor: Color.lerp(a.bottomAppBarColor, b.bottomAppBarColor, t),
              cardColor: Color.lerp(a.cardColor, b.cardColor, t),
              dividerColor: Color.lerp(a.dividerColor, b.dividerColor, t),
              focusColor: Color.lerp(a.focusColor, b.focusColor, t),
              hoverColor: Color.lerp(a.hoverColor, b.hoverColor, t),
              highlightColor: Color.lerp(a.highlightColor, b.highlightColor, t),
              splashColor: Color.lerp(a.splashColor, b.splashColor, t),
              splashFactory: t < 0.5f ? a.splashFactory : b.splashFactory,
              selectedRowColor: Color.lerp(a.selectedRowColor, b.selectedRowColor, t),
              unselectedWidgetColor: Color.lerp(a.unselectedWidgetColor, b.unselectedWidgetColor, t),
              disabledColor: Color.lerp(a.disabledColor, b.disabledColor, t),
              buttonTheme: t < 0.5f ? a.buttonTheme : b.buttonTheme,
              toggleButtonsTheme: ToggleButtonsThemeData.lerp(a.toggleButtonsTheme, b.toggleButtonsTheme, t),
              buttonColor: Color.lerp(a.buttonColor, b.buttonColor, t),
              secondaryHeaderColor: Color.lerp(a.secondaryHeaderColor, b.secondaryHeaderColor, t),
              textSelectionColor: Color.lerp(a.textSelectionColor, b.textSelectionColor, t),
              cursorColor: Color.lerp(a.cursorColor, b.cursorColor, t),
              textSelectionHandleColor: Color.lerp(a.textSelectionHandleColor, b.textSelectionHandleColor, t),
              backgroundColor: Color.lerp(a.backgroundColor, b.backgroundColor, t),
              dialogBackgroundColor: Color.lerp(a.dialogBackgroundColor, b.dialogBackgroundColor, t),
              indicatorColor: Color.lerp(a.indicatorColor, b.indicatorColor, t),
              hintColor: Color.lerp(a.hintColor, b.hintColor, t),
              errorColor: Color.lerp(a.errorColor, b.errorColor, t),
              toggleableActiveColor: Color.lerp(a.toggleableActiveColor, b.toggleableActiveColor, t),
              textTheme: TextTheme.lerp(a.textTheme, b.textTheme, t),
              primaryTextTheme: TextTheme.lerp(a.primaryTextTheme, b.primaryTextTheme, t),
              accentTextTheme: TextTheme.lerp(a.accentTextTheme, b.accentTextTheme, t),
              inputDecorationTheme: t < 0.5f ? a.inputDecorationTheme : b.inputDecorationTheme,
              iconTheme: IconThemeData.lerp(a.iconTheme, b.iconTheme, t),
              primaryIconTheme: IconThemeData.lerp(a.primaryIconTheme, b.primaryIconTheme, t),
              accentIconTheme: IconThemeData.lerp(a.accentIconTheme, b.accentIconTheme, t),
              sliderTheme: SliderThemeData.lerp(a.sliderTheme, b.sliderTheme, t),
              tabBarTheme: TabBarTheme.lerp(a.tabBarTheme, b.tabBarTheme, t),
              tooltipTheme: TooltipThemeData.lerp(a.tooltipTheme, b.tooltipTheme, t),
              cardTheme: CardTheme.lerp(a.cardTheme, b.cardTheme, t),
              chipTheme: ChipThemeData.lerp(a.chipTheme, b.chipTheme, t),
              platform: t < 0.5f ? a.platform : b.platform,
              materialTapTargetSize: t < 0.5f ? a.materialTapTargetSize : b.materialTapTargetSize,
              applyElevationOverlayColor: t < 0.5f ? a.applyElevationOverlayColor : b.applyElevationOverlayColor,
              pageTransitionsTheme: t < 0.5f ? a.pageTransitionsTheme : b.pageTransitionsTheme,
              appBarTheme: AppBarTheme.lerp(a.appBarTheme, b.appBarTheme, t),
              bottomAppBarTheme: BottomAppBarTheme.lerp(a.bottomAppBarTheme, b.bottomAppBarTheme, t),
              colorScheme: ColorScheme.lerp(a.colorScheme, b.colorScheme, t),
              dialogTheme: DialogTheme.lerp(a.dialogTheme, b.dialogTheme, t),
              floatingActionButtonTheme: FloatingActionButtonThemeData.lerp(a.floatingActionButtonTheme, b.floatingActionButtonTheme, t),
              navigationRailTheme: NavigationRailThemeData.lerp(a.navigationRailTheme, b.navigationRailTheme, t),
              typography: Typography.lerp(a.typography, b.typography, t),
              cupertinoOverrideTheme: t < 0.5f ? a.cupertinoOverrideTheme : b.cupertinoOverrideTheme,
              snackBarTheme: SnackBarThemeData.lerp(a.snackBarTheme, b.snackBarTheme, t),
              bottomSheetTheme: BottomSheetThemeData.lerp(a.bottomSheetTheme, b.bottomSheetTheme, t),
              popupMenuTheme: PopupMenuThemeData.lerp(a.popupMenuTheme, b.popupMenuTheme, t),
              bannerTheme: MaterialBannerThemeData.lerp(a.bannerTheme, b.bannerTheme, t),
              dividerTheme: DividerThemeData.lerp(a.dividerTheme, b.dividerTheme, t),
              buttonBarTheme: ButtonBarThemeData.lerp(a.buttonBarTheme, b.buttonBarTheme, t)
            );
        }
        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ThemeData defaultData = fallback();
            properties.add( new EnumProperty<TargetPlatform>("platform", platform.Value, defaultValue: TargetPlatform.windows, level: DiagnosticLevel.debug));
            properties.add( new EnumProperty<Brightness>("brightness", brightness.Value, defaultValue: defaultData.brightness, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("primaryColor", primaryColor, defaultValue: defaultData.primaryColor, level: DiagnosticLevel.debug));
            properties.add( new EnumProperty<Brightness>("primaryColorBrightness", primaryColorBrightness.Value, defaultValue: defaultData.primaryColorBrightness, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("accentColor", accentColor, defaultValue: defaultData.accentColor, level: DiagnosticLevel.debug));
            properties.add( new EnumProperty<Brightness>("accentColorBrightness", accentColorBrightness.Value, defaultValue: defaultData.accentColorBrightness, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("canvasColor", canvasColor, defaultValue: defaultData.canvasColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("scaffoldBackgroundColor", scaffoldBackgroundColor, defaultValue: defaultData.scaffoldBackgroundColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("bottomAppBarColor", bottomAppBarColor, defaultValue: defaultData.bottomAppBarColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("cardColor", cardColor, defaultValue: defaultData.cardColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("dividerColor", dividerColor, defaultValue: defaultData.dividerColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("focusColor", focusColor, defaultValue: defaultData.focusColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("hoverColor", hoverColor, defaultValue: defaultData.hoverColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("highlightColor", highlightColor, defaultValue: defaultData.highlightColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("splashColor", splashColor, defaultValue: defaultData.splashColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("selectedRowColor", selectedRowColor, defaultValue: defaultData.selectedRowColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("unselectedWidgetColor", unselectedWidgetColor, defaultValue: defaultData.unselectedWidgetColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("disabledColor", disabledColor, defaultValue: defaultData.disabledColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("buttonColor", buttonColor, defaultValue: defaultData.buttonColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("secondaryHeaderColor", secondaryHeaderColor, defaultValue: defaultData.secondaryHeaderColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("textSelectionColor", textSelectionColor, defaultValue: defaultData.textSelectionColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("cursorColor", cursorColor, defaultValue: defaultData.cursorColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("textSelectionHandleColor", textSelectionHandleColor, defaultValue: defaultData.textSelectionHandleColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("backgroundColor", backgroundColor, defaultValue: defaultData.backgroundColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("dialogBackgroundColor", dialogBackgroundColor, defaultValue: defaultData.dialogBackgroundColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("indicatorColor", indicatorColor, defaultValue: defaultData.indicatorColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("hintColor", hintColor, defaultValue: defaultData.hintColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("errorColor", errorColor, defaultValue: defaultData.errorColor, level: DiagnosticLevel.debug));
            properties.add( new ColorProperty("toggleableActiveColor", toggleableActiveColor, defaultValue: defaultData.toggleableActiveColor, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<ButtonThemeData>("buttonTheme", buttonTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<ToggleButtonsThemeData>("toggleButtonsTheme", toggleButtonsTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<TextTheme>("textTheme", textTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<TextTheme>("primaryTextTheme", primaryTextTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<TextTheme>("accentTextTheme", accentTextTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<InputDecorationTheme>("inputDecorationTheme", inputDecorationTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<IconThemeData>("iconTheme", iconTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<IconThemeData>("primaryIconTheme", primaryIconTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<IconThemeData>("accentIconTheme", accentIconTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<SliderThemeData>("sliderTheme", sliderTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<TabBarTheme>("tabBarTheme", tabBarTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<TooltipThemeData>("tooltipTheme", tooltipTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<CardTheme>("cardTheme", cardTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<ChipThemeData>("chipTheme", chipTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<MaterialTapTargetSize>("materialTapTargetSize", materialTapTargetSize.Value, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<bool>("applyElevationOverlayColor", applyElevationOverlayColor.Value, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<PageTransitionsTheme>("pageTransitionsTheme", pageTransitionsTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<AppBarTheme>("appBarTheme", appBarTheme, defaultValue: defaultData.appBarTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<BottomAppBarTheme>("bottomAppBarTheme", bottomAppBarTheme, defaultValue: defaultData.bottomAppBarTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<ColorScheme>("colorScheme", colorScheme, defaultValue: defaultData.colorScheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<DialogTheme>("dialogTheme", dialogTheme, defaultValue: defaultData.dialogTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<FloatingActionButtonThemeData>("floatingActionButtonThemeData", floatingActionButtonTheme, defaultValue: defaultData.floatingActionButtonTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<NavigationRailThemeData>("navigationRailThemeData", navigationRailTheme, defaultValue: defaultData.navigationRailTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<Typography>("typography", typography, defaultValue: defaultData.typography, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<CupertinoThemeData>("cupertinoOverrideTheme", cupertinoOverrideTheme, defaultValue: defaultData.cupertinoOverrideTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<SnackBarThemeData>("snackBarTheme", snackBarTheme, defaultValue: defaultData.snackBarTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<BottomSheetThemeData>("bottomSheetTheme", bottomSheetTheme, defaultValue: defaultData.bottomSheetTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<PopupMenuThemeData>("popupMenuTheme", popupMenuTheme, defaultValue: defaultData.popupMenuTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<MaterialBannerThemeData>("bannerTheme", bannerTheme, defaultValue: defaultData.bannerTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<DividerThemeData>("dividerTheme", dividerTheme, defaultValue: defaultData.dividerTheme, level: DiagnosticLevel.debug));
            properties.add( new DiagnosticsProperty<ButtonBarThemeData>("buttonBarTheme", buttonBarTheme, defaultValue: defaultData.buttonBarTheme, level: DiagnosticLevel.debug));
        } 
    }

    public class MaterialBasedCupertinoThemeData : CupertinoThemeData {

        public MaterialBasedCupertinoThemeData(
            ThemeData _materialTheme,
            CupertinoThemeData _cupertinoOverrideTheme
        ) : base(
            _cupertinoOverrideTheme.brightness,
            _cupertinoOverrideTheme.primaryColor,
            _cupertinoOverrideTheme.primaryContrastingColor,
            _cupertinoOverrideTheme.textTheme,
            _cupertinoOverrideTheme.barBackgroundColor,
            _cupertinoOverrideTheme.scaffoldBackgroundColor
        ) {
            D.assert(_materialTheme != null);
            D.assert(_cupertinoOverrideTheme != null);
        }

        public static MaterialBasedCupertinoThemeData create(ThemeData materialTheme) {
            D.assert(materialTheme != null);
            CupertinoThemeData _cupertinoOverrideTheme =
                (materialTheme.cupertinoOverrideTheme ?? new CupertinoThemeData()).noDefault();
            return new MaterialBasedCupertinoThemeData(materialTheme, _cupertinoOverrideTheme);
        }

        public readonly ThemeData _materialTheme;
        static readonly CupertinoThemeData _cupertinoOverrideTheme;

        Brightness? brightness {
            get { return _cupertinoOverrideTheme.brightness ?? _materialTheme.brightness; }
        }

        Color primaryColor {
            get { return _cupertinoOverrideTheme.primaryColor ?? _materialTheme.colorScheme.primary; }
        }

        Color primaryContrastingColor {
            get { return  _cupertinoOverrideTheme.primaryContrastingColor ?? _materialTheme.colorScheme.onPrimary; }
        }

        Color scaffoldBackgroundColor {
            get { return _cupertinoOverrideTheme.scaffoldBackgroundColor ?? _materialTheme.scaffoldBackgroundColor;
            }
        }

        public MaterialBasedCupertinoThemeData copyWith(
            Brightness? brightness = null,
            Color primaryColor = null,
            Color primaryContrastingColor = null,
            CupertinoTextThemeData textTheme = null,
            Color barBackgroundColor = null,
            Color scaffoldBackgroundColor = null
            ) {
            return new MaterialBasedCupertinoThemeData(
              _materialTheme,
              _cupertinoOverrideTheme.copyWith(
                brightness: brightness,
                primaryColor: primaryColor,
                primaryContrastingColor: primaryContrastingColor,
                textTheme: textTheme,
                barBackgroundColor: barBackgroundColor,
                scaffoldBackgroundColor: scaffoldBackgroundColor
              )
            );
        }

        public CupertinoThemeData resolveFrom(BuildContext context,  bool nullOk = false ) {
            return new MaterialBasedCupertinoThemeData(
              _materialTheme,
              _cupertinoOverrideTheme.resolveFrom(context, nullOk: nullOk)
            );
        }
    }

    public class _IdentityThemeDataCacheKey: IEquatable<_IdentityThemeDataCacheKey> {
        public _IdentityThemeDataCacheKey(
            ThemeData baseTheme = null,
            TextTheme localTextGeometry=null
            ) {
            this.baseTheme = baseTheme;
            this.localTextGeometry = localTextGeometry;
        }

        public readonly ThemeData baseTheme;
        public readonly TextTheme localTextGeometry;


        public bool Equals(_IdentityThemeDataCacheKey other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(baseTheme, other.baseTheme)
                   && Equals(localTextGeometry, other.localTextGeometry);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((_IdentityThemeDataCacheKey) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((baseTheme != null ? baseTheme.GetHashCode() : 0) * 397) ^ (localTextGeometry != null ? localTextGeometry.GetHashCode() : 0);
            }
        }
    }
    public class _FifoCache<K, V> {
        public _FifoCache(int _maximumSize) {
             D.assert(_maximumSize != null && _maximumSize > 0);
             this._maximumSize = _maximumSize;
        }


        public readonly Dictionary<K, V> _cache = new Dictionary<K, V>();
        public readonly int _maximumSize;
        public V putIfAbsent(K key, Func<ThemeData> loader) { 
            D.assert(key != null);
            D.assert(loader != null);
            V result = _cache[key];
            if (result != null)
                return result;
            if (_cache.Count == _maximumSize)
                _cache.Remove(_cache.Keys.First());
            return _cache[key] = loader;
        }
    }

    public class VisualDensity : Diagnosticable,IEquatable<VisualDensity> { 
        public VisualDensity(
            float horizontal = 0.0f,
            float vertical = 0.0f) {
            D.assert(horizontal != null);
            D.assert(vertical != null);
            D.assert(vertical <= maximumDensity);
            D.assert(vertical >= minimumDensity);
            D.assert(horizontal <= maximumDensity);
            D.assert(horizontal >= minimumDensity);
            this.horizontal = horizontal;
            this.vertical = vertical;
        }
        public readonly float minimumDensity = -4.0f;
        public readonly float maximumDensity = 4.0f;
        public readonly VisualDensity standard = new VisualDensity();
        public readonly VisualDensity comfortable = new VisualDensity(horizontal: -1.0f, vertical: -1.0f);
        public static readonly VisualDensity compact = new VisualDensity(horizontal: -2.0f, vertical: -2.0f);
        
        /*public static VisualDensity  adaptivePlatformDensity {
            get {
                switch (defaultTargetPlatform) {
                    case TargetPlatform.android:
                    case TargetPlatform.iOS:
                    case TargetPlatform.fuchsia:
                        break;
                    case TargetPlatform.linux:
                    case TargetPlatform.macOS:
                    case TargetPlatform.windows:
                        return compact;
                }
                return new VisualDensity();
            }
        }*/
        public VisualDensity copyWith(
            float? horizontal = null,
            float? vertical = null) {
            return new VisualDensity(
              horizontal: horizontal ?? this.horizontal,
              vertical: vertical ?? this.vertical
            );
        }

        public readonly float horizontal;
        public readonly float vertical;
        public Offset baseSizeAdjustment {
            get {  
                float interval = 4.0f;
                return new Offset(horizontal, vertical) * interval;}
        }
        public static VisualDensity lerp(VisualDensity a, VisualDensity b, float t) { 
            return new VisualDensity(
                horizontal:MathUtils.lerpFloat(a.horizontal, b.horizontal, t),
                vertical: MathUtils.lerpFloat(a.horizontal, b.horizontal, t)
            );
        }
        
        /*public BoxConstraints effectiveConstraints(BoxConstraints constraints){ 
            D.assert(constraints != null && constraints.debugAssertIsValid());
            return constraints.copyWith(
                minWidth: (constraints.minWidth + baseSizeAdjustment.dx).clamp(0.0, float.infinity).toDouble(),
                minHeight: (constraints.minHeight + baseSizeAdjustment.dy).clamp(0.0, float.infinity).toDouble(),
            );
        }*/

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add( new FloatProperty("horizontal", horizontal, defaultValue: 0.0));
            properties.add( new FloatProperty("vertical", vertical, defaultValue: 0.0));
        }

        public override string toStringShort() {
            return $"{base.toStringShort()}(h: {debugFormatDouble(horizontal)}, v: {debugFormatDouble(vertical)})";
        }

        public bool Equals(VisualDensity other)
        {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return minimumDensity.Equals(other.minimumDensity) 
                   && maximumDensity.Equals(other.maximumDensity) 
                   && Equals(standard, other.standard)
                   && Equals(comfortable, other.comfortable) 
                   && horizontal.Equals(other.horizontal) 
                   && vertical.Equals(other.vertical);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((VisualDensity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = minimumDensity.GetHashCode();
                hashCode = (hashCode * 397) ^ maximumDensity.GetHashCode();
                hashCode = (hashCode * 397) ^ (standard != null ? standard.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (comfortable != null ? comfortable.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ horizontal.GetHashCode();
                hashCode = (hashCode * 397) ^ vertical.GetHashCode();
                return hashCode;
            }
        }
    }


}