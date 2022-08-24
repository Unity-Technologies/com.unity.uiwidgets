using System;
using System.Collections.Generic;
using System.Linq;
using uiwidgets;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace Unity.UIWidgets.material {
    public partial class material_ {
        public static readonly Color _kLightThemeHighlightColor = new Color(0x66BCBCBC);

        public static readonly Color _kLightThemeSplashColor = new Color(0x66C8C8C8);

        public static readonly Color _kDarkThemeHighlightColor = new Color(0x40CCCCCC);

        public static readonly Color _kDarkThemeSplashColor = new Color(0x40CCCCCC);
    }

    public enum MaterialTapTargetSize {
        padded,

        shrinkWrap
    }

    public class ThemeData : Diagnosticable, IEquatable<ThemeData> {
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
            string fontFamily = null,
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
            RuntimePlatform? platform = null,
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

            primarySwatch = primarySwatch ?? Colors.blue;
            primaryColor = primaryColor ?? (isDark ? Colors.grey[900] : primarySwatch);
            primaryColorBrightness = primaryColorBrightness ?? estimateBrightnessForColor(primaryColor);
            primaryColorLight = primaryColorLight ?? (isDark ? Colors.grey[500] : primarySwatch[100]);
            primaryColorDark = primaryColorDark ?? (isDark ? Colors.black : primarySwatch[700]);
            bool primaryIsDark = primaryColorBrightness == Brightness.dark;
            toggleableActiveColor = toggleableActiveColor ??
                                    (isDark ? Colors.tealAccent[200] : (accentColor ?? primarySwatch[600]));

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
                brightness: brightness);

            splashFactory = splashFactory ?? InkSplash.splashFactory;
            selectedRowColor = selectedRowColor ?? Colors.grey[100];
            unselectedWidgetColor = unselectedWidgetColor ?? (isDark ? Colors.white70 : Colors.black54);
            secondaryHeaderColor = secondaryHeaderColor ?? (isDark ? Colors.grey[700] : primarySwatch[50]);
            textSelectionColor = textSelectionColor ?? (isDark ? accentColor : primarySwatch[200]);
            cursorColor = cursorColor ?? Color.fromRGBO(66, 133, 244, 1.0f);
            textSelectionHandleColor =
                textSelectionHandleColor ?? (isDark ? Colors.tealAccent[400] : primarySwatch[300]);

            backgroundColor = backgroundColor ?? (isDark ? Colors.grey[700] : primarySwatch[200]);
            dialogBackgroundColor = dialogBackgroundColor ?? (isDark ? Colors.grey[800] : Colors.white);
            indicatorColor = indicatorColor ?? (accentColor == primaryColor ? Colors.white : accentColor);
            hintColor = hintColor ?? (isDark ? new Color(0x80FFFFFF) : new Color(0x8A000000));
            errorColor = errorColor ?? Colors.red[700];
            inputDecorationTheme = inputDecorationTheme ?? new InputDecorationTheme();
            pageTransitionsTheme = pageTransitionsTheme ?? new PageTransitionsTheme();
            appBarTheme = appBarTheme ?? new AppBarTheme();
            bottomAppBarTheme = bottomAppBarTheme ?? new BottomAppBarTheme();
            primaryIconTheme = primaryIconTheme ??
                               (primaryIsDark
                                   ? new IconThemeData(color: Colors.white)
                                   : new IconThemeData(color: Colors.black));
            accentIconTheme = accentIconTheme ??
                              (accentIsDark
                                  ? new IconThemeData(color: Colors.white)
                                  : new IconThemeData(color: Colors.black));
            iconTheme = iconTheme ??
                        (isDark ? new IconThemeData(color: Colors.white) : new IconThemeData(color: Colors.black87));
            platform = platform ?? Application.platform;
            typography = typography ?? Typography.material2014();
            TextTheme defaultTextTheme = isDark ? typography.white : typography.black;
            textTheme = defaultTextTheme.merge(textTheme);
            TextTheme defaultPrimaryTextTheme = primaryIsDark ? typography.white : typography.black;
            primaryTextTheme = defaultPrimaryTextTheme.merge(primaryTextTheme);
            TextTheme defaultAccentTextTheme = accentIsDark ? typography.white : typography.black;
            accentTextTheme = defaultAccentTextTheme.merge(accentTextTheme);
            materialTapTargetSize = materialTapTargetSize ?? MaterialTapTargetSize.padded;
            applyElevationOverlayColor = applyElevationOverlayColor ?? false;
            
            if (fontFamily != null) {
                textTheme = textTheme.apply(fontFamily: fontFamily);
                primaryTextTheme = primaryTextTheme.apply(fontFamily: fontFamily);
                accentTextTheme = accentTextTheme.apply(fontFamily: fontFamily);
            }

            buttonColor = buttonColor ?? (isDark ? primarySwatch[600] : Colors.grey[300]);
            focusColor = focusColor ??(isDark ? Colors.white.withOpacity(0.12f) : Colors.black.withOpacity(0.12f));
            hoverColor = hoverColor ??(isDark ? Colors.white.withOpacity(0.04f) : Colors.black.withOpacity(0.04f));
            buttonTheme = buttonTheme ?? new ButtonThemeData(
                colorScheme: colorScheme,
                buttonColor: buttonColor,
                disabledColor: disabledColor,
                focusColor: focusColor,
                hoverColor: hoverColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                materialTapTargetSize: materialTapTargetSize);
            toggleButtonsTheme = toggleButtonsTheme?? new ToggleButtonsThemeData();
            disabledColor = disabledColor ?? (isDark ? Colors.white38 : Colors.black38);
            highlightColor = highlightColor ??
                             (isDark
                                 ? material_._kDarkThemeHighlightColor
                                 : material_._kLightThemeHighlightColor);
            splashColor = splashColor ??
                          (isDark
                              ? material_._kDarkThemeSplashColor
                              : material_._kLightThemeSplashColor);

            sliderTheme = sliderTheme ?? SliderThemeData.fromPrimaryColors(
                primaryColor: primaryColor,
                primaryColorLight: primaryColorLight,
                primaryColorDark: primaryColorDark,
                valueIndicatorTextStyle: accentTextTheme.body2);

            tabBarTheme = tabBarTheme ?? new TabBarTheme();
            tooltipTheme = tooltipTheme ?? new TooltipThemeData();
            cardTheme = cardTheme ?? new CardTheme();
            chipTheme = chipTheme ?? ChipThemeData.fromDefaults(
                secondaryColor: primaryColor,
                brightness: brightness,
                labelStyle: textTheme.bodyText1
            );
            dialogTheme = dialogTheme ?? new DialogTheme();
            floatingActionButtonTheme = floatingActionButtonTheme ?? new FloatingActionButtonThemeData();
            navigationRailTheme = navigationRailTheme ?? new NavigationRailThemeData();
            snackBarTheme = snackBarTheme ?? new SnackBarThemeData();
            bottomSheetTheme = bottomSheetTheme ?? new BottomSheetThemeData();
            popupMenuTheme = popupMenuTheme ?? new PopupMenuThemeData();
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
            D.assert(highlightColor != null);
            D.assert(splashColor != null);
            D.assert(splashFactory != null);
            D.assert(selectedRowColor != null);
            D.assert(unselectedWidgetColor != null);
            D.assert(disabledColor != null);
            D.assert(toggleableActiveColor != null);
            D.assert(buttonTheme != null);
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
            D.assert(sliderTheme != null);
            D.assert(iconTheme != null);
            D.assert(primaryIconTheme != null);
            D.assert(accentIconTheme != null);
            D.assert(materialTapTargetSize != null);
            D.assert(applyElevationOverlayColor != null);
            D.assert(pageTransitionsTheme != null);
            D.assert(appBarTheme != null);
            D.assert(bottomAppBarTheme != null);
            D.assert(colorScheme != null);
            D.assert(typography != null);
            D.assert(buttonColor != null);
            D.assert(tabBarTheme != null);
            D.assert(cardTheme != null);
            D.assert(chipTheme != null);
            D.assert(dialogTheme != null);
            D.assert(floatingActionButtonTheme != null);
            D.assert(snackBarTheme != null);
            D.assert(bottomSheetTheme != null);
            D.assert(popupMenuTheme != null);
            D.assert(bannerTheme != null);
            D.assert(dividerTheme != null);
            D.assert(buttonBarTheme != null);

            this.brightness = brightness ?? Brightness.light;
            this.visualDensity = visualDensity;
            this.primaryColor = primaryColor;
            this.primaryColorBrightness = primaryColorBrightness ?? Brightness.light;
            this.primaryColorLight = primaryColorLight;
            this.primaryColorDark = primaryColorDark;
            this.canvasColor = canvasColor;
            this.accentColor = accentColor;
            this.accentColorBrightness = accentColorBrightness ?? Brightness.light;
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
            this.toggleButtonsTheme = toggleButtonsTheme;
            this.buttonColor = buttonColor;
            this.secondaryHeaderColor = secondaryHeaderColor;
            this.textSelectionColor = textSelectionColor;
            this.cursorColor = cursorColor;
            this.textSelectionHandleColor = textSelectionHandleColor;
            this.backgroundColor = backgroundColor;
            this.dialogBackgroundColor = dialogBackgroundColor;
            this.indicatorColor = indicatorColor;
            this.hintColor = hintColor;
            this.errorColor = errorColor;
            this.toggleableActiveColor = toggleableActiveColor;
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
            this.platform = platform.Value;
            this.materialTapTargetSize = materialTapTargetSize ?? MaterialTapTargetSize.padded;
            this.applyElevationOverlayColor = applyElevationOverlayColor.Value;
            this.pageTransitionsTheme = pageTransitionsTheme;
            this.appBarTheme = appBarTheme;
            this.bottomAppBarTheme = bottomAppBarTheme;
            this.colorScheme = colorScheme;
            this.dialogTheme = dialogTheme;
            this.floatingActionButtonTheme = floatingActionButtonTheme;
            this.navigationRailTheme = navigationRailTheme;
            this.typography = typography;
            this.snackBarTheme = snackBarTheme;
            this.bottomSheetTheme = bottomSheetTheme;
            this.popupMenuTheme = popupMenuTheme;
            this.bannerTheme = bannerTheme;
            this.dividerTheme = dividerTheme;
            this.buttonBarTheme = buttonBarTheme;
        }

        public static ThemeData raw(
            Brightness? brightness = null,
            VisualDensity visualDensity = null,
            Color primaryColor = null,
            Brightness? primaryColorBrightness = null,
            Color primaryColorLight = null,
            Color primaryColorDark = null,
            Color canvasColor = null,
            Color accentColor = null,
            Brightness? accentColorBrightness = null,
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
            ButtonThemeData buttonTheme = null,
            Color buttonColor = null,
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
            TextTheme textTheme = null,
            TextTheme primaryTextTheme = null,
            TextTheme accentTextTheme = null,
            SliderThemeData sliderTheme = null,
            InputDecorationTheme inputDecorationTheme = null,
            IconThemeData iconTheme = null,
            IconThemeData primaryIconTheme = null,
            IconThemeData accentIconTheme = null,
            TabBarTheme tabBarTheme = null,
            TooltipThemeData tooltipTheme = null,
            CardTheme cardTheme = null,
            ChipThemeData chipTheme = null,
            RuntimePlatform? platform = null,
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
            SnackBarThemeData snackBarTheme = null,
            BottomSheetThemeData bottomSheetTheme = null,
            PopupMenuThemeData popupMenuTheme = null,
            MaterialBannerThemeData bannerTheme = null,
            DividerThemeData dividerTheme = null,
            ButtonBarThemeData buttonBarTheme = null
        ) {
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
            D.assert(sliderTheme != null);
            D.assert(inputDecorationTheme != null);
            D.assert(iconTheme != null);
            D.assert(primaryIconTheme != null);
            D.assert(accentIconTheme != null);
            D.assert(platform != null);
            D.assert(materialTapTargetSize != null);
            D.assert(applyElevationOverlayColor != null);
            D.assert(pageTransitionsTheme != null);
            D.assert(appBarTheme != null);
            D.assert(bottomAppBarTheme != null);
            D.assert(colorScheme != null);
            D.assert(typography != null);
            D.assert(buttonColor != null);
            D.assert(tabBarTheme != null);
            D.assert(tooltipTheme != null);
            D.assert(cardTheme != null);
            D.assert(chipTheme != null);
            D.assert(dialogTheme != null);
            D.assert(floatingActionButtonTheme != null);
            D.assert(navigationRailTheme != null);
            D.assert(snackBarTheme != null);
            D.assert(bottomSheetTheme != null);
            D.assert(popupMenuTheme != null);
            D.assert(bannerTheme != null);
            D.assert(dividerTheme != null);
            D.assert(buttonBarTheme != null);

            return new ThemeData(
                brightness: brightness,
                visualDensity: visualDensity,
                primaryColor: primaryColor,
                primaryColorBrightness: primaryColorBrightness,
                primaryColorLight: primaryColorLight,
                primaryColorDark: primaryColorDark,
                accentColor: accentColor,
                accentColorBrightness: accentColorBrightness,
                canvasColor: canvasColor,
                scaffoldBackgroundColor: scaffoldBackgroundColor,
                bottomAppBarColor: bottomAppBarColor,
                cardColor: cardColor,
                dividerColor: dividerColor,
                focusColor: focusColor,
                hoverColor: hoverColor,
                highlightColor: highlightColor,
                splashColor: splashColor,
                splashFactory: splashFactory,
                selectedRowColor: selectedRowColor,
                unselectedWidgetColor: unselectedWidgetColor,
                disabledColor: disabledColor,
                buttonTheme: buttonTheme,
                buttonColor: buttonColor,
                toggleButtonsTheme: toggleButtonsTheme,
                toggleableActiveColor: toggleableActiveColor,
                secondaryHeaderColor: secondaryHeaderColor,
                textSelectionColor: textSelectionColor,
                cursorColor: cursorColor,
                textSelectionHandleColor: textSelectionHandleColor,
                backgroundColor: backgroundColor,
                dialogBackgroundColor: dialogBackgroundColor,
                indicatorColor: indicatorColor,
                hintColor: hintColor,
                errorColor: errorColor,
                textTheme: textTheme,
                primaryTextTheme: primaryTextTheme,
                accentTextTheme: accentTextTheme,
                inputDecorationTheme: inputDecorationTheme,
                iconTheme: iconTheme,
                primaryIconTheme: primaryIconTheme,
                accentIconTheme: accentIconTheme,
                sliderTheme: sliderTheme,
                tabBarTheme: tabBarTheme,
                tooltipTheme:tooltipTheme,
                cardTheme: cardTheme,
                chipTheme: chipTheme,
                platform: platform,
                materialTapTargetSize: materialTapTargetSize,
                applyElevationOverlayColor: applyElevationOverlayColor,
                pageTransitionsTheme: pageTransitionsTheme,
                appBarTheme: appBarTheme,
                bottomAppBarTheme: bottomAppBarTheme,
                colorScheme: colorScheme,
                dialogTheme: dialogTheme,
                floatingActionButtonTheme: floatingActionButtonTheme,
                navigationRailTheme: navigationRailTheme,
                typography: typography,
                snackBarTheme: snackBarTheme,
                bottomSheetTheme: bottomSheetTheme,
                popupMenuTheme: popupMenuTheme,
                bannerTheme: bannerTheme,
                dividerTheme: dividerTheme,
                buttonBarTheme: buttonBarTheme
                );
        }

        public static ThemeData from(
            ColorScheme colorScheme,
            TextTheme textTheme = null
            ) {
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

        public static ThemeData dark() {
            return new ThemeData(brightness: Brightness.dark);
        }

        public static ThemeData fallback() {
            return light();
        }


        public readonly Brightness brightness;
        
        public readonly  VisualDensity visualDensity;

        public readonly Color primaryColor;

        public readonly Brightness primaryColorBrightness;

        public readonly Color primaryColorLight;

        public readonly Color primaryColorDark;

        public readonly Color canvasColor;

        public readonly Color accentColor;

        public readonly Brightness accentColorBrightness;

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

        public readonly SliderThemeData sliderTheme;

        public readonly InputDecorationTheme inputDecorationTheme;

        public readonly IconThemeData iconTheme;

        public readonly IconThemeData primaryIconTheme;

        public readonly IconThemeData accentIconTheme;

        public readonly TabBarTheme tabBarTheme;
        
        public readonly TooltipThemeData tooltipTheme;

        public readonly CardTheme cardTheme;
        public readonly ChipThemeData chipTheme;

        public readonly RuntimePlatform platform;

        public readonly MaterialTapTargetSize materialTapTargetSize;

        public readonly bool applyElevationOverlayColor;

        public readonly PageTransitionsTheme pageTransitionsTheme;

        public readonly AppBarTheme appBarTheme;

        public readonly BottomAppBarTheme bottomAppBarTheme;

        public readonly ColorScheme colorScheme;

        public readonly SnackBarThemeData snackBarTheme;

        public readonly DialogTheme dialogTheme;

        public readonly FloatingActionButtonThemeData floatingActionButtonTheme;

        public readonly NavigationRailThemeData navigationRailTheme;
        
        public readonly Typography typography;

        public readonly BottomSheetThemeData bottomSheetTheme;
        
        public readonly CupertinoThemeData cupertinoOverrideTheme;

        /// A theme for customizing the color, shape, elevation, and text style of
        /// popup menus.
        public readonly PopupMenuThemeData popupMenuTheme;

        /// A theme for customizing the color and text style of a [MaterialBanner].
        public readonly MaterialBannerThemeData bannerTheme;

        /// A theme for customizing the color, thickness, and indents of [Divider]s,
        /// [VerticalDivider]s, etc.
        public readonly DividerThemeData dividerTheme;

        
        public readonly ButtonBarThemeData buttonBarTheme;

        public ThemeData copyWith(
            Brightness? brightness = null,
            VisualDensity visualDensity = null,
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
            ButtonThemeData buttonTheme = null,
            ToggleButtonsThemeData toggleButtonsTheme = null,
            Color buttonColor = null,
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
            TextTheme textTheme = null,
            TextTheme primaryTextTheme = null,
            TextTheme accentTextTheme = null,
            SliderThemeData sliderTheme = null,
            InputDecorationTheme inputDecorationTheme = null,
            IconThemeData iconTheme = null,
            IconThemeData primaryIconTheme = null,
            IconThemeData accentIconTheme = null,
            TabBarTheme tabBarTheme = null,
            TooltipThemeData tooltipTheme = null,
            CardTheme cardTheme = null,
            ChipThemeData chipTheme = null,
            RuntimePlatform? platform = null,
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
            SnackBarThemeData snackBarTheme = null,
            BottomSheetThemeData bottomSheetTheme = null,
            PopupMenuThemeData popupMenuTheme = null,
            MaterialBannerThemeData bannerTheme = null,
            DividerThemeData dividerTheme = null,
            ButtonBarThemeData buttonBarTheme = null
        ) {
            return raw(
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
                buttonTheme: buttonTheme ?? this.buttonTheme,
                toggleButtonsTheme: toggleButtonsTheme ?? this.toggleButtonsTheme,
                buttonColor: buttonColor ?? this.buttonColor,
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
                sliderTheme: sliderTheme ?? this.sliderTheme,
                inputDecorationTheme: this.inputDecorationTheme ?? this.inputDecorationTheme,
                iconTheme: iconTheme ?? this.iconTheme,
                primaryIconTheme: primaryIconTheme ?? this.primaryIconTheme,
                accentIconTheme: accentIconTheme ?? this.accentIconTheme,
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
                snackBarTheme: snackBarTheme ?? this.snackBarTheme,
                bottomSheetTheme: bottomSheetTheme ?? this.bottomSheetTheme,
                popupMenuTheme: popupMenuTheme ?? this.popupMenuTheme,
                bannerTheme: bannerTheme ?? this.bannerTheme,
                dividerTheme: dividerTheme ?? this.dividerTheme,
                buttonBarTheme: buttonBarTheme ?? this.buttonBarTheme
            );
        }

        const int _localizedThemeDataCacheSize = 5;

        static readonly _FifoCache<_IdentityThemeDataCacheKey, ThemeData> _localizedThemeDataCache =
            new _FifoCache<_IdentityThemeDataCacheKey, ThemeData>(_localizedThemeDataCacheSize);


        public static ThemeData localize(ThemeData baseTheme, TextTheme localTextGeometry) {
            D.assert(baseTheme != null);
            D.assert(localTextGeometry != null);

            return _localizedThemeDataCache.putIfAbsent(
                new _IdentityThemeDataCacheKey(baseTheme, localTextGeometry),
                () => {
                    return baseTheme.copyWith(
                        primaryTextTheme: localTextGeometry.merge(baseTheme.primaryTextTheme),
                        accentTextTheme: localTextGeometry.merge(baseTheme.accentTextTheme),
                        textTheme: localTextGeometry.merge(baseTheme.textTheme)
                    );
                });
        }

        public static Brightness estimateBrightnessForColor(Color color) {
            float relativeLuminance = color.computeLuminance();
            float kThreshold = 0.15f;
            if ((relativeLuminance + 0.05f) * (relativeLuminance + 0.05f) > kThreshold) {
                return Brightness.light;
            }

            return Brightness.dark;
        }

        public static ThemeData lerp(ThemeData a, ThemeData b, float t) {
            D.assert(a != null);
            D.assert(b != null);
            return raw(
                brightness: t < 0.5 ? a.brightness : b.brightness,
                visualDensity: VisualDensity.lerp(a.visualDensity, b.visualDensity, t),
                primaryColor: Color.lerp(a.primaryColor, b.primaryColor, t),
                primaryColorBrightness: t < 0.5 ? a.primaryColorBrightness : b.primaryColorBrightness,
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
                splashFactory: t < 0.5 ? a.splashFactory : b.splashFactory,
                selectedRowColor: Color.lerp(a.selectedRowColor, b.selectedRowColor, t),
                unselectedWidgetColor: Color.lerp(a.unselectedWidgetColor, b.unselectedWidgetColor, t),
                disabledColor: Color.lerp(a.disabledColor, b.disabledColor, t),
                buttonTheme: t < 0.5 ? a.buttonTheme : b.buttonTheme,
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
                platform: t < 0.5 ? a.platform : b.platform,
                materialTapTargetSize: t < 0.5 ? a.materialTapTargetSize : b.materialTapTargetSize,
                applyElevationOverlayColor: t < 0.5 ? a.applyElevationOverlayColor : b.applyElevationOverlayColor,
                pageTransitionsTheme: t < 0.5 ? a.pageTransitionsTheme : b.pageTransitionsTheme,
                appBarTheme: AppBarTheme.lerp(a.appBarTheme, b.appBarTheme, t),
                bottomAppBarTheme: BottomAppBarTheme.lerp(a.bottomAppBarTheme, b.bottomAppBarTheme, t),
                colorScheme: ColorScheme.lerp(a.colorScheme, b.colorScheme, t),
                dialogTheme: DialogTheme.lerp(a.dialogTheme, b.dialogTheme, t),
                floatingActionButtonTheme: FloatingActionButtonThemeData.lerp(a.floatingActionButtonTheme,
                    b.floatingActionButtonTheme, t),
                navigationRailTheme: NavigationRailThemeData.lerp(a.navigationRailTheme, b.navigationRailTheme, t),
                typography: Typography.lerp(a.typography, b.typography, t),
                snackBarTheme: SnackBarThemeData.lerp(a.snackBarTheme, b.snackBarTheme, t),
                bottomSheetTheme: BottomSheetThemeData.lerp(a.bottomSheetTheme, b.bottomSheetTheme, t),
                popupMenuTheme: PopupMenuThemeData.lerp(a.popupMenuTheme, b.popupMenuTheme, t),
                bannerTheme: MaterialBannerThemeData.lerp(a.bannerTheme, b.bannerTheme, t),
                dividerTheme: DividerThemeData.lerp(a.dividerTheme, b.dividerTheme, t),
                buttonBarTheme: ButtonBarThemeData.lerp(a.buttonBarTheme, b.buttonBarTheme, t)
            );
        }

        public bool Equals(ThemeData other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return other.brightness == brightness &&
                   other.visualDensity == visualDensity && 
                   other.primaryColor == primaryColor &&
                   other.primaryColorBrightness == primaryColorBrightness &&
                   other.primaryColorLight == primaryColorLight &&
                   other.primaryColorDark == primaryColorDark &&
                   other.accentColor == accentColor &&
                   other.accentColorBrightness == accentColorBrightness &&
                   other.canvasColor == canvasColor &&
                   other.scaffoldBackgroundColor == scaffoldBackgroundColor &&
                   other.bottomAppBarColor == bottomAppBarColor &&
                   other.cardColor == cardColor &&
                   other.dividerColor == dividerColor &&
                   other.focusColor == focusColor &&
                   other.hoverColor == hoverColor &&
                   other.highlightColor == highlightColor &&
                   other.splashColor == splashColor &&
                   other.splashFactory == splashFactory &&
                   other.selectedRowColor == selectedRowColor &&
                   other.unselectedWidgetColor == unselectedWidgetColor &&
                   other.disabledColor == disabledColor &&
                   other.buttonTheme == buttonTheme &&
                   other.toggleButtonsTheme == toggleButtonsTheme &&
                   other.buttonColor == buttonColor &&
                   other.secondaryHeaderColor == secondaryHeaderColor &&
                   other.textSelectionColor == textSelectionColor &&
                   other.cursorColor == cursorColor &&
                   other.textSelectionHandleColor == textSelectionHandleColor &&
                   other.backgroundColor == backgroundColor &&
                   other.dialogBackgroundColor == dialogBackgroundColor &&
                   other.indicatorColor == indicatorColor &&
                   other.hintColor == hintColor &&
                   other.errorColor == errorColor &&
                   other.textTheme == textTheme &&
                   other.primaryTextTheme == primaryTextTheme &&
                   other.accentTextTheme == accentTextTheme &&
                   other.sliderTheme == sliderTheme &&
                   other.inputDecorationTheme == inputDecorationTheme &&
                   other.toggleableActiveColor == toggleableActiveColor &&
                   other.iconTheme == iconTheme &&
                   other.primaryIconTheme == primaryIconTheme &&
                   other.accentIconTheme == accentIconTheme &&
                   other.tabBarTheme == tabBarTheme &&
                   other.tooltipTheme == tooltipTheme &&
                   other.cardTheme == cardTheme &&
                   other.chipTheme == chipTheme &&
                   other.platform == platform &&
                   other.materialTapTargetSize == materialTapTargetSize &&
                   other.applyElevationOverlayColor == applyElevationOverlayColor &&
                   other.pageTransitionsTheme == pageTransitionsTheme &&
                   other.appBarTheme == appBarTheme &&
                   other.bottomAppBarTheme == bottomAppBarTheme &&
                   other.colorScheme == colorScheme &&
                   other.dialogTheme == dialogTheme &&
                   other.floatingActionButtonTheme == floatingActionButtonTheme &&
                   other.navigationRailTheme == navigationRailTheme &&
                   other.typography == typography &&
                   other.snackBarTheme == snackBarTheme &&
                   other.bottomSheetTheme == bottomSheetTheme && 
                   other.popupMenuTheme == popupMenuTheme &&
                   other.bannerTheme == bannerTheme &&
                   other.dividerTheme == dividerTheme &&
                   other.buttonBarTheme == buttonBarTheme;
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

            return Equals((ThemeData) obj);
        }

        public static bool operator ==(ThemeData left, ThemeData right) {
            return Equals(left, right);
        }

        public static bool operator !=(ThemeData left, ThemeData right) {
            return !Equals(left, right);
        }

        int? _cachedHashCode = null;

        public override int GetHashCode() {
            if (_cachedHashCode != null) {
                return _cachedHashCode.Value;
            }

            unchecked {
                var hashCode = brightness.GetHashCode();
                hashCode = (hashCode * 397) ^ visualDensity.GetHashCode();
                hashCode = (hashCode * 397) ^ primaryColor.GetHashCode();
                hashCode = (hashCode * 397) ^ primaryColorBrightness.GetHashCode();
                hashCode = (hashCode * 397) ^ primaryColorLight.GetHashCode();
                hashCode = (hashCode * 397) ^ primaryColorDark.GetHashCode();
                hashCode = (hashCode * 397) ^ canvasColor.GetHashCode();
                hashCode = (hashCode * 397) ^ accentColor.GetHashCode();
                hashCode = (hashCode * 397) ^ accentColorBrightness.GetHashCode();
                hashCode = (hashCode * 397) ^ scaffoldBackgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ bottomAppBarColor.GetHashCode();
                hashCode = (hashCode * 397) ^ cardColor.GetHashCode();
                hashCode = (hashCode * 397) ^ dividerColor.GetHashCode();
                hashCode = (hashCode * 397) ^ focusColor.GetHashCode();
                hashCode = (hashCode * 397) ^ hoverColor.GetHashCode();
                hashCode = (hashCode * 397) ^ highlightColor.GetHashCode();
                hashCode = (hashCode * 397) ^ splashColor.GetHashCode();
                hashCode = (hashCode * 397) ^ splashFactory.GetHashCode();
                hashCode = (hashCode * 397) ^ selectedRowColor.GetHashCode();
                hashCode = (hashCode * 397) ^ unselectedWidgetColor.GetHashCode();
                hashCode = (hashCode * 397) ^ disabledColor.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ toggleButtonsTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonColor.GetHashCode();
                hashCode = (hashCode * 397) ^ secondaryHeaderColor.GetHashCode();
                hashCode = (hashCode * 397) ^ textSelectionColor.GetHashCode();
                hashCode = (hashCode * 397) ^ cursorColor.GetHashCode();
                hashCode = (hashCode * 397) ^ textSelectionHandleColor.GetHashCode();
                hashCode = (hashCode * 397) ^ backgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ dialogBackgroundColor.GetHashCode();
                hashCode = (hashCode * 397) ^ indicatorColor.GetHashCode();
                hashCode = (hashCode * 397) ^ hintColor.GetHashCode();
                hashCode = (hashCode * 397) ^ errorColor.GetHashCode();
                hashCode = (hashCode * 397) ^ toggleableActiveColor.GetHashCode();
                hashCode = (hashCode * 397) ^ textTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ primaryTextTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ accentTextTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ inputDecorationTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ iconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ primaryIconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ accentIconTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ sliderTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ tabBarTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ tooltipTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ cardTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ chipTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ platform.GetHashCode();
                hashCode = (hashCode * 397) ^ materialTapTargetSize.GetHashCode();
                hashCode = (hashCode * 397) ^ applyElevationOverlayColor.GetHashCode();
                hashCode = (hashCode * 397) ^ pageTransitionsTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ appBarTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ bottomAppBarTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ colorScheme.GetHashCode();
                hashCode = (hashCode * 397) ^ dialogTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ floatingActionButtonTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ navigationRailTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ typography.GetHashCode();
                hashCode = (hashCode * 397) ^ snackBarTheme.GetHashCode(); 
                hashCode = (hashCode * 397) ^ bottomSheetTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ popupMenuTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ bannerTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ dividerTheme.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonBarTheme.GetHashCode();

                _cachedHashCode = hashCode;
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            ThemeData defaultData = fallback();
            properties.add(new EnumProperty<RuntimePlatform>("platform", platform,
                defaultValue: Application.platform));
            properties.add(new EnumProperty<Brightness>("brightness", brightness,
                defaultValue: defaultData.brightness));
            properties.add(new DiagnosticsProperty<Color>("primaryColor", primaryColor,
                defaultValue: defaultData.primaryColor));
            properties.add(new EnumProperty<Brightness>("primaryColorBrightness", primaryColorBrightness,
                defaultValue: defaultData.primaryColorBrightness));
            properties.add(new DiagnosticsProperty<Color>("accentColor", accentColor,
                defaultValue: defaultData.accentColor));
            properties.add(new EnumProperty<Brightness>("accentColorBrightness", accentColorBrightness,
                defaultValue: defaultData.accentColorBrightness));
            properties.add(new DiagnosticsProperty<Color>("canvasColor", canvasColor,
                defaultValue: defaultData.canvasColor));
            properties.add(new DiagnosticsProperty<Color>("scaffoldBackgroundColor", scaffoldBackgroundColor,
                defaultValue: defaultData.scaffoldBackgroundColor));
            properties.add(new DiagnosticsProperty<Color>("bottomAppBarColor", bottomAppBarColor,
                defaultValue: defaultData.bottomAppBarColor));
            properties.add(new DiagnosticsProperty<Color>("cardColor", cardColor,
                defaultValue: defaultData.cardColor));
            properties.add(new DiagnosticsProperty<Color>("dividerColor", dividerColor,
                defaultValue: defaultData.dividerColor));
            properties.add(new ColorProperty("focusColor", focusColor, defaultValue: defaultData.focusColor, level: DiagnosticLevel.debug));
            properties.add(new ColorProperty("hoverColor", hoverColor, defaultValue: defaultData.hoverColor, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("highlightColor", highlightColor,
                defaultValue: defaultData.highlightColor));
            properties.add(new DiagnosticsProperty<Color>("splashColor", splashColor,
                defaultValue: defaultData.splashColor));
            properties.add(new DiagnosticsProperty<Color>("selectedRowColor", selectedRowColor,
                defaultValue: defaultData.selectedRowColor));
            properties.add(new DiagnosticsProperty<Color>("unselectedWidgetColor", unselectedWidgetColor,
                defaultValue: defaultData.unselectedWidgetColor));
            properties.add(new DiagnosticsProperty<Color>("disabledColor", disabledColor,
                defaultValue: defaultData.disabledColor));
            properties.add(new DiagnosticsProperty<ButtonThemeData>("buttonTheme", buttonTheme));
            properties.add(new DiagnosticsProperty<ToggleButtonsThemeData>("toggleButtonsTheme", toggleButtonsTheme, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Color>("buttonColor", buttonColor,
                defaultValue: defaultData.buttonColor));
            properties.add(new DiagnosticsProperty<Color>("secondaryHeaderColor", secondaryHeaderColor,
                defaultValue: defaultData.secondaryHeaderColor));
            properties.add(new DiagnosticsProperty<Color>("textSelectionColor", textSelectionColor,
                defaultValue: defaultData.textSelectionColor));
            properties.add(new DiagnosticsProperty<Color>("cursorColor", cursorColor,
                defaultValue: defaultData.cursorColor));
            properties.add(new DiagnosticsProperty<Color>("textSelectionHandleColor", textSelectionHandleColor,
                defaultValue: defaultData.textSelectionHandleColor));
            properties.add(new DiagnosticsProperty<Color>("backgroundColor", backgroundColor,
                defaultValue: defaultData.backgroundColor));
            properties.add(new DiagnosticsProperty<Color>("dialogBackgroundColor", dialogBackgroundColor,
                defaultValue: defaultData.dialogBackgroundColor));
            properties.add(new DiagnosticsProperty<Color>("indicatorColor", indicatorColor,
                defaultValue: defaultData.indicatorColor));
            properties.add(new DiagnosticsProperty<Color>("hintColor", hintColor,
                defaultValue: defaultData.hintColor));
            properties.add(new DiagnosticsProperty<Color>("errorColor", errorColor,
                defaultValue: defaultData.errorColor));
            properties.add(new DiagnosticsProperty<TextTheme>("textTheme", textTheme));
            properties.add(new DiagnosticsProperty<TextTheme>("primaryTextTheme", primaryTextTheme));
            properties.add(new DiagnosticsProperty<TextTheme>("accentTextTheme", accentTextTheme));
            properties.add(
                new DiagnosticsProperty<InputDecorationTheme>("inputDecorationTheme", inputDecorationTheme));
            properties.add(new DiagnosticsProperty<Color>("toggleableActiveColor", toggleableActiveColor,
                defaultValue: defaultData.toggleableActiveColor));
            properties.add(new DiagnosticsProperty<IconThemeData>("iconTheme", iconTheme));
            properties.add(new DiagnosticsProperty<IconThemeData>("primaryIconTheme", primaryIconTheme));
            properties.add(new DiagnosticsProperty<IconThemeData>("accentIconTheme", accentIconTheme));
            properties.add(new DiagnosticsProperty<SliderThemeData>("sliderTheme", sliderTheme));
            properties.add(new DiagnosticsProperty<TabBarTheme>("tabBarTheme", tabBarTheme));
            properties.add(new DiagnosticsProperty<TooltipThemeData>("tooltipTheme", tooltipTheme, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<CardTheme>("cardTheme", cardTheme));
            properties.add(new DiagnosticsProperty<ChipThemeData>("chipTheme", chipTheme));
            properties.add(
                new DiagnosticsProperty<MaterialTapTargetSize>("materialTapTargetSize", materialTapTargetSize));
            properties.add(new DiagnosticsProperty<bool>("applyElevationOverlayColor", applyElevationOverlayColor, level: DiagnosticLevel.debug));
            properties.add(
                new DiagnosticsProperty<PageTransitionsTheme>("pageTransitionsTheme", pageTransitionsTheme));
            properties.add(new DiagnosticsProperty<AppBarTheme>("appBarTheme", appBarTheme));
            properties.add(new DiagnosticsProperty<BottomAppBarTheme>("bottomAppBarTheme", bottomAppBarTheme));
            properties.add(new DiagnosticsProperty<ColorScheme>("colorScheme", colorScheme,
                defaultValue: defaultData.colorScheme));
            properties.add(new DiagnosticsProperty<DialogTheme>("dialogTheme", dialogTheme,
                defaultValue: defaultData.dialogTheme));
            properties.add(new DiagnosticsProperty<FloatingActionButtonThemeData>("floatingActionButtonTheme",
                floatingActionButtonTheme, defaultValue: defaultData.floatingActionButtonTheme));
            properties.add(new DiagnosticsProperty<NavigationRailThemeData>("navigationRailThemeData", navigationRailTheme, defaultValue: defaultData.navigationRailTheme, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<Typography>("typography", typography,
                defaultValue: defaultData.typography));
            properties.add(new DiagnosticsProperty<SnackBarThemeData>("snackBarTheme", snackBarTheme, defaultValue: defaultData.snackBarTheme, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<BottomSheetThemeData>("bottomSheetTheme", bottomSheetTheme, defaultValue: defaultData.bottomSheetTheme, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<PopupMenuThemeData>("popupMenuTheme", popupMenuTheme, defaultValue: defaultData.popupMenuTheme, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<MaterialBannerThemeData>("bannerTheme", bannerTheme, defaultValue: defaultData.bannerTheme, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<DividerThemeData>("dividerTheme", dividerTheme, defaultValue: defaultData.dividerTheme, level: DiagnosticLevel.debug));
            properties.add(new DiagnosticsProperty<ButtonBarThemeData>("buttonBarTheme", buttonBarTheme, defaultValue: defaultData.buttonBarTheme, level: DiagnosticLevel.debug));

        }
    }


    class _IdentityThemeDataCacheKey : IEquatable<_IdentityThemeDataCacheKey> {
        public _IdentityThemeDataCacheKey(
            ThemeData baseTheme,
            TextTheme localTextGeometry) {
            this.baseTheme = baseTheme;
            this.localTextGeometry = localTextGeometry;
        }

        public readonly ThemeData baseTheme;

        public readonly TextTheme localTextGeometry;

        public bool Equals(_IdentityThemeDataCacheKey other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return ReferenceEquals(baseTheme, other.baseTheme) &&
                   ReferenceEquals(localTextGeometry, other.localTextGeometry);
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

            return Equals((_IdentityThemeDataCacheKey) obj);
        }

        public static bool operator ==(_IdentityThemeDataCacheKey left, _IdentityThemeDataCacheKey right) {
            return Equals(left, right);
        }

        public static bool operator !=(_IdentityThemeDataCacheKey left, _IdentityThemeDataCacheKey right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            var hashCode = baseTheme.GetHashCode();
            hashCode = (hashCode * 397) ^ localTextGeometry.GetHashCode();
            return hashCode;
        }
    }

    class _FifoCache<K, V> {
        public _FifoCache(int maximumSize) {
            D.assert(maximumSize > 0);
            _maximumSize = maximumSize;
        }

        readonly Dictionary<K, V> _cache = new Dictionary<K, V>();

        readonly int _maximumSize;

        public V putIfAbsent(K key, Func<V> value) {
            D.assert(key != null);
            D.assert(value != null);

            V get_value;
            if (_cache.TryGetValue(key, out get_value)) {
                return get_value;
            }

            if (_cache.Count == _maximumSize) {
                _cache.Remove(_cache.Keys.First());
            }

            _cache[key] = value();
            return _cache[key];
        }
    }

    public class VisualDensity : Diagnosticable, IEquatable<VisualDensity> {
        public VisualDensity(
            float? horizontal = 0.0f,
            float? vertical = 0.0f
        ) {
            D.assert(horizontal != null);
            D.assert(vertical != null);
            D.assert(vertical <= maximumDensity);
            D.assert(vertical >= minimumDensity);
            D.assert(horizontal <= maximumDensity);
            D.assert(horizontal >= minimumDensity);
            this.horizontal = horizontal.Value;
            this.vertical = vertical.Value;
        }

        public static readonly float minimumDensity = -4.0f;

        public static readonly float maximumDensity = 4.0f;

        public static readonly VisualDensity standard = new VisualDensity();

        public static readonly VisualDensity comfortable = new VisualDensity(horizontal: -1.0f, vertical: -1.0f);

        public static readonly VisualDensity compact = new VisualDensity(horizontal: -2.0f, vertical: -2.0f);

        public static VisualDensity adaptivePlatformDensity {
            get {
                // switch (defaultTargetPlatform) {
                //     case TargetPlatform.android:
                //     case TargetPlatform.iOS:
                //     case TargetPlatform.fuchsia:
                //         break;
                //     case TargetPlatform.linux:
                //     case TargetPlatform.macOS:
                //     case TargetPlatform.windows:
                //         return compact;
                // }

                return new VisualDensity();
            }
        }

        public VisualDensity copyWith(
            float? horizontal,
            float? vertical
        ) {
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

                return new Offset(horizontal, vertical) * interval;
            }
        }

        internal static VisualDensity lerp(VisualDensity a, VisualDensity b, float t) {
            return new VisualDensity(
                horizontal: MathUtils.lerpNullableFloat(a.horizontal, b.horizontal, t),
                vertical: MathUtils.lerpNullableFloat(a.horizontal, b.horizontal, t)
            );
        }

        public BoxConstraints effectiveConstraints(BoxConstraints constraints) {
            D.assert(constraints != null && constraints.debugAssertIsValid());
            return constraints.copyWith(
                minWidth: (constraints.minWidth + baseSizeAdjustment.dx).clamp(0.0f, float.PositiveInfinity),
                minHeight: (constraints.minHeight + baseSizeAdjustment.dy).clamp(0.0f, float.PositiveInfinity)
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            properties.add(new FloatProperty("horizontal", horizontal, defaultValue: 0.0f));
            properties.add(new FloatProperty("vertical", vertical, defaultValue: 0.0f));
        }

        public override string ToString() {
            return $"{base.toStringShort()}(h: {D.debugFormatFloat(horizontal)}, v: {D.debugFormatFloat(vertical)})";
        }

        public static bool operator ==(VisualDensity self, object other) {
            return Equals(self, other);
        }

        public static bool operator !=(VisualDensity self, object other) {
            return !Equals(self, other);
        }

        public bool Equals(VisualDensity other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return horizontal.Equals(other.horizontal) && vertical.Equals(other.vertical);
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

            return Equals((VisualDensity) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (horizontal.GetHashCode() * 397) ^ vertical.GetHashCode();
            }
        }
    }
}