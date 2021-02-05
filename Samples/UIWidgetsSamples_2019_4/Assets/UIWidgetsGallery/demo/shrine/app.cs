using UIWidgetsGallery.demo.shrine;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.shrine
{
    public class ShrineApp : StatefulWidget {
        public override State createState() => new _ShrineAppState();
    }

    public class _ShrineAppState : State<ShrineApp> {//with SingleTickerProviderStateMixin {
  
        AnimationController _controller;
      
        public override void initState() {
            base.initState();
            _controller = new AnimationController(
                vsync: this,
                duration: Durantion(milliseconds: 450),
                value: 1.0
            );
        }

  
        public override Widget build(BuildContext context) {
            return new MaterialApp(
                title: "Shrine",
                home: HomePage(
                    backdrop: Backdrop(
                    frontLayer: ProductPage(),
                    backLayer: CategoryMenuPage(onCategoryTap: () => _controller.forward()),
                    frontTitle: new Text("SHRINE"),
                    backTitle: new Text("MENU"),
                    controller: _controller
                    ),
                expandingBottomSheet: ExpandingBottomSheet(hideController: _controller),
                ),
            initialRoute: "/login",
            onGenerateRoute: _getRoute,
            theme: _kShrineTheme.copyWith(platform: Theme.of(context).platform),
            );
        }
    }

    Route<object> _getRoute(RouteSettings settings) {
      if (settings.name != '/login') {
        return null;
      }

      return new MaterialPageRoute(
        settings: settings,
        builder: (BuildContext context) => LoginPage(),
        fullscreenDialog: true
      );
    }

    public readonly ThemeData _kShrineTheme = _buildShrineTheme();

    IconThemeData _customIconTheme(IconThemeData original) {
        return original.copyWith(color: shrineColorsUtils.kShrineBrown900);
    }

    ThemeData _buildShrineTheme() {
        ThemeData _base = ThemeData.light();
        return _base.copyWith(
            colorScheme: kShrineColorScheme,
            accentColor: shrineColorsUtils.kShrineBrown900,
            primaryColor: shrineColorsUtils.kShrinePink100,
            buttonColor: shrineColorsUtils.kShrinePink100,
            scaffoldBackgroundColor: shrineColorsUtils.kShrineBackgroundWhite,
            cardColor: shrineColorsUtils.kShrineBackgroundWhite,
            textSelectionColor: shrineColorsUtils.kShrinePink100,
            errorColor: shrineColorsUtils.kShrineErrorRed,
            buttonTheme: new ButtonThemeData(
              colorScheme: kShrineColorScheme,
              textTheme: ButtonTextTheme.normal
            ),
            primaryIconTheme: _customIconTheme(_base.iconTheme),
            inputDecorationTheme: new InputDecorationTheme(border: CutCornersBorder()),
            textTheme: _buildShrineTextTheme(_base.textTheme),
            primaryTextTheme: _buildShrineTextTheme(_base.primaryTextTheme),
            accentTextTheme: _buildShrineTextTheme(_base.accentTextTheme),
            iconTheme: _customIconTheme(_base.iconTheme)
        );
    }

    TextTheme _buildShrineTextTheme(TextTheme _base) {
        return _base.copyWith(
            headline5: _base.headline5.copyWith(fontWeight: FontWeight.w500),
            headline6: _base.headline6.copyWith(fontSize: 18.0f),
            caption: _base.caption.copyWith(fontWeight: FontWeight.w400, fontSize: 14.0f),
            bodyText1: _base.bodyText1.copyWith(fontWeight: FontWeight.w500, fontSize: 16.0f),
            button: _base.button.copyWith(fontWeight: FontWeight.w500, fontSize: 14.0f)
        ).apply(
            fontFamily: "Raleway",
            displayColor: shrineColorsUtils.kShrineBrown900,
            bodyColor: shrineColorsUtils.kShrineBrown900
        );
    }

    ColorScheme kShrineColorScheme = new ColorScheme(
        primary: shrineColorsUtils.kShrinePink100,
        primaryVariant: shrineColorsUtils.kShrineBrown900,
        secondary: shrineColorsUtils.kShrinePink50,
        secondaryVariant: shrineColorsUtils.kShrineBrown900,
        surface: shrineColorsUtils.kShrineSurfaceWhite,
        background: shrineColorsUtils.kShrineBackgroundWhite,
        error: shrineColorsUtils.kShrineErrorRed,
        onPrimary: shrineColorsUtils.kShrineBrown900,
        onSecondary: shrineColorsUtils.kShrineBrown900,
        onSurface: shrineColorsUtils.kShrineBrown900,
        onBackground: shrineColorsUtils.kShrineBrown900,
        onError: shrineColorsUtils.kShrineSurfaceWhite,
        brightness: Brightness.light
    );

}