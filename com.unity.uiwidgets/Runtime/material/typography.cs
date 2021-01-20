using System.Collections.Generic;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.material {
    public enum ScriptCategory {
        englishLike,

        dense,

        tall
    }


    public class Typography : Diagnosticable {
        Typography(
            TextTheme black = null,
            TextTheme white = null,
            TextTheme englishLike = null,
            TextTheme dense = null,
            TextTheme tall = null
        ) {
            D.assert(black != null);
            D.assert(white != null);
            D.assert(englishLike != null);
            D.assert(dense != null);
            D.assert(tall != null);

            this.black = black;
            this.white = white;
            this.englishLike = englishLike;
            this.dense = dense;
            this.tall = tall;
        }

        public static Typography create(
            TextTheme black = null,
            TextTheme white = null,
            TextTheme englishLike = null,
            TextTheme dense = null,
            TextTheme tall = null
        ) {
            return material2014(black, white, englishLike, dense, tall);
        }

        public static Typography material2014(
            TextTheme black = null,
            TextTheme white = null,
            TextTheme englishLike = null,
            TextTheme dense = null,
            TextTheme tall = null
        ) {
            return Typography._withPlatform(
                black, white,
                englishLike ?? englishLike2014,
                dense ?? dense2014,
                tall ?? tall2014
            );
        }
        
        public static Typography material2018(
            TextTheme black = null,
            TextTheme white = null,
            TextTheme englishLike = null,
            TextTheme dense = null,
            TextTheme tall = null
        ) {
            return Typography._withPlatform(
                black, white,
                englishLike ?? englishLike2018,
                dense ?? dense2018,
                tall ?? tall2018
            );
        }

        internal static Typography _withPlatform(
            TextTheme black = null,
            TextTheme white = null,
            TextTheme englishLike = null,
            TextTheme dense = null,
            TextTheme tall = null
            ) {
            D.assert(englishLike != null);
            D.assert(dense != null);
            D.assert(tall != null);
            
            //force to use Android color
            black = black ?? blackMountainView;
            white = white ?? whiteMountainView;
            
            return new Typography(
                black, white, englishLike, dense, tall);
        }
        
        public readonly TextTheme black;

        public readonly TextTheme white;

        public readonly TextTheme englishLike;

        public readonly TextTheme dense;

        public readonly TextTheme tall;

        public TextTheme geometryThemeFor(ScriptCategory category) {
            switch (category) {
                case ScriptCategory.englishLike:
                    return englishLike;
                case ScriptCategory.dense:
                    return dense;
                case ScriptCategory.tall:
                    return tall;
            }

            return null;
        }


        public Typography copyWith(
            TextTheme black = null,
            TextTheme white = null,
            TextTheme englishLike = null,
            TextTheme dense = null,
            TextTheme tall = null) {
            return new Typography(
                black: black ?? this.black,
                white: white ?? this.white,
                englishLike: englishLike ?? this.englishLike,
                dense: dense ?? this.dense,
                tall: tall ?? this.tall);
        }

        public static Typography lerp(Typography a, Typography b, float t) {
            return new Typography(
                black: TextTheme.lerp(a.black, b.black, t),
                white: TextTheme.lerp(a.white, b.white, t),
                englishLike: TextTheme.lerp(a.englishLike, b.englishLike, t),
                dense: TextTheme.lerp(a.dense, b.dense, t),
                tall: TextTheme.lerp(a.tall, b.tall, t)
            );
        }


        public bool Equals(Typography other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return black == other.black
                   && white == other.white
                   && englishLike == other.englishLike
                   && dense == other.dense
                   && tall == other.tall;
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

            return Equals((Typography) obj);
        }

        public static bool operator ==(Typography left, Typography right) {
            return Equals(left, right);
        }

        public static bool operator !=(Typography left, Typography right) {
            return !Equals(left, right);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = black.GetHashCode();
                hashCode = (hashCode * 397) ^ white.GetHashCode();
                hashCode = (hashCode * 397) ^ englishLike.GetHashCode();
                hashCode = (hashCode * 397) ^ dense.GetHashCode();
                hashCode = (hashCode * 397) ^ tall.GetHashCode();
                return hashCode;
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
            base.debugFillProperties(properties);
            Typography defaultTypography = Typography.material2014();
            properties.add(
                new DiagnosticsProperty<TextTheme>("black", black, defaultValue: defaultTypography.black));
            properties.add(
                new DiagnosticsProperty<TextTheme>("white", white, defaultValue: defaultTypography.white));
            properties.add(new DiagnosticsProperty<TextTheme>("englishLike", englishLike,
                defaultValue: defaultTypography.englishLike));
            properties.add(
                new DiagnosticsProperty<TextTheme>("dense", dense, defaultValue: defaultTypography.dense));
            properties.add(new DiagnosticsProperty<TextTheme>("tall", tall, defaultValue: defaultTypography.tall));
        }

        public static readonly TextTheme blackMountainView = new TextTheme(
                headline1 : new TextStyle(debugLabel: "blackMountainView headline1", fontFamily: "Roboto", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                headline2 : new TextStyle(debugLabel: "blackMountainView headline2", fontFamily: "Roboto", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                headline3 : new TextStyle(debugLabel: "blackMountainView headline3", fontFamily: "Roboto", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                headline4 : new TextStyle(debugLabel: "blackMountainView headline4", fontFamily: "Roboto", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                headline5 : new TextStyle(debugLabel: "blackMountainView headline5", fontFamily: "Roboto", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                headline6 : new TextStyle(debugLabel: "blackMountainView headline6", fontFamily: "Roboto", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                bodyText1 : new TextStyle(debugLabel: "blackMountainView bodyText1", fontFamily: "Roboto", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                bodyText2 : new TextStyle(debugLabel: "blackMountainView bodyText2", fontFamily: "Roboto", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                subtitle1 : new TextStyle(debugLabel: "blackMountainView subtitle1", fontFamily: "Roboto", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                subtitle2 : new TextStyle(debugLabel: "blackMountainView subtitle2", fontFamily: "Roboto", inherit: true, color: Colors.black,   decoration: TextDecoration.none),
                caption   : new TextStyle(debugLabel: "blackMountainView caption",   fontFamily: "Roboto", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                button    : new TextStyle(debugLabel: "blackMountainView button",    fontFamily: "Roboto", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                overline  : new TextStyle(debugLabel: "blackMountainView overline",  fontFamily: "Roboto", inherit: true, color: Colors.black,   decoration: TextDecoration.none)
        );

        public static readonly TextTheme whiteMountainView = new TextTheme(
                headline1 : new TextStyle(debugLabel: "whiteMountainView headline1", fontFamily: "Roboto", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                headline2 : new TextStyle(debugLabel: "whiteMountainView headline2", fontFamily: "Roboto", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                headline3 : new TextStyle(debugLabel: "whiteMountainView headline3", fontFamily: "Roboto", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                headline4 : new TextStyle(debugLabel: "whiteMountainView headline4", fontFamily: "Roboto", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                headline5 : new TextStyle(debugLabel: "whiteMountainView headline5", fontFamily: "Roboto", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                headline6 : new TextStyle(debugLabel: "whiteMountainView headline6", fontFamily: "Roboto", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                bodyText1 : new TextStyle(debugLabel: "whiteMountainView bodyText1", fontFamily: "Roboto", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                bodyText2 : new TextStyle(debugLabel: "whiteMountainView bodyText2", fontFamily: "Roboto", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                subtitle1 : new TextStyle(debugLabel: "whiteMountainView subtitle1", fontFamily: "Roboto", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                subtitle2 : new TextStyle(debugLabel: "whiteMountainView subtitle2", fontFamily: "Roboto", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                caption   : new TextStyle(debugLabel: "whiteMountainView caption",   fontFamily: "Roboto", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                button    : new TextStyle(debugLabel: "whiteMountainView button",    fontFamily: "Roboto", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                overline  : new TextStyle(debugLabel: "whiteMountainView overline",  fontFamily: "Roboto", inherit: true, color: Colors.white,   decoration: TextDecoration.none)
        );

        public static readonly TextTheme blackRedmond = new TextTheme(
                headline1 : new TextStyle(debugLabel: "blackRedmond headline1", fontFamily: "Segoe UI", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                headline2 : new TextStyle(debugLabel: "blackRedmond headline2", fontFamily: "Segoe UI", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                headline3 : new TextStyle(debugLabel: "blackRedmond headline3", fontFamily: "Segoe UI", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                headline4 : new TextStyle(debugLabel: "blackRedmond headline4", fontFamily: "Segoe UI", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                headline5 : new TextStyle(debugLabel: "blackRedmond headline5", fontFamily: "Segoe UI", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                headline6 : new TextStyle(debugLabel: "blackRedmond headline6", fontFamily: "Segoe UI", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                bodyText1 : new TextStyle(debugLabel: "blackRedmond bodyText1", fontFamily: "Segoe UI", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                bodyText2 : new TextStyle(debugLabel: "blackRedmond bodyText2", fontFamily: "Segoe UI", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                subtitle1 : new TextStyle(debugLabel: "blackRedmond subtitle1", fontFamily: "Segoe UI", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                subtitle2 : new TextStyle(debugLabel: "blackRedmond subtitle2", fontFamily: "Segoe UI", inherit: true, color: Colors.black,   decoration: TextDecoration.none),
                caption   : new TextStyle(debugLabel: "blackRedmond caption",   fontFamily: "Segoe UI", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
                button    : new TextStyle(debugLabel: "blackRedmond button",    fontFamily: "Segoe UI", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
                overline  : new TextStyle(debugLabel: "blackRedmond overline",  fontFamily: "Segoe UI", inherit: true, color: Colors.black,   decoration: TextDecoration.none)
  );
        
      public static readonly TextTheme whiteRedmond = new TextTheme(
                headline1 : new TextStyle(debugLabel: "whiteRedmond headline1", fontFamily: "Segoe UI", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                headline2 : new TextStyle(debugLabel: "whiteRedmond headline2", fontFamily: "Segoe UI", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                headline3 : new TextStyle(debugLabel: "whiteRedmond headline3", fontFamily: "Segoe UI", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                headline4 : new TextStyle(debugLabel: "whiteRedmond headline4", fontFamily: "Segoe UI", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                headline5 : new TextStyle(debugLabel: "whiteRedmond headline5", fontFamily: "Segoe UI", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                headline6 : new TextStyle(debugLabel: "whiteRedmond headline6", fontFamily: "Segoe UI", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                bodyText1 : new TextStyle(debugLabel: "whiteRedmond bodyText1", fontFamily: "Segoe UI", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                bodyText2 : new TextStyle(debugLabel: "whiteRedmond bodyText2", fontFamily: "Segoe UI", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                subtitle1 : new TextStyle(debugLabel: "whiteRedmond subtitle1", fontFamily: "Segoe UI", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                subtitle2 : new TextStyle(debugLabel: "whiteRedmond subtitle2", fontFamily: "Segoe UI", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                caption   : new TextStyle(debugLabel: "whiteRedmond caption",   fontFamily: "Segoe UI", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
                button    : new TextStyle(debugLabel: "whiteRedmond button",    fontFamily: "Segoe UI", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
                overline  : new TextStyle(debugLabel: "whiteRedmond overline",  fontFamily: "Segoe UI", inherit: true, color: Colors.white,   decoration: TextDecoration.none)
  );
      static readonly List<string> _helsinkiFontFallbacks = new List<string>(){"Ubuntu", "Cantarell", "DejaVu Sans", "Liberation Sans", "Arial"};
  
  public static readonly TextTheme blackHelsinki = new TextTheme(
    headline1 : new TextStyle(debugLabel: "blackHelsinki headline1", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    headline2 : new TextStyle(debugLabel: "blackHelsinki headline2", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    headline3 : new TextStyle(debugLabel: "blackHelsinki headline3", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    headline4 : new TextStyle(debugLabel: "blackHelsinki headline4", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    headline5 : new TextStyle(debugLabel: "blackHelsinki headline5", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    headline6 : new TextStyle(debugLabel: "blackHelsinki headline6", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    bodyText1 : new TextStyle(debugLabel: "blackHelsinki bodyText1", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    bodyText2 : new TextStyle(debugLabel: "blackHelsinki bodyText2", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    subtitle1 : new TextStyle(debugLabel: "blackHelsinki subtitle1", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    subtitle2 : new TextStyle(debugLabel: "blackHelsinki subtitle2", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black,   decoration: TextDecoration.none),
    caption   : new TextStyle(debugLabel: "blackHelsinki caption",   fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    button    : new TextStyle(debugLabel: "blackHelsinki button",    fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    overline  : new TextStyle(debugLabel: "blackHelsinki overline",  fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.black,   decoration: TextDecoration.none)
  );
  
  public static readonly TextTheme whiteHelsinki = new TextTheme(
    headline1 : new TextStyle(debugLabel: "whiteHelsinki headline1", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    headline2 : new TextStyle(debugLabel: "whiteHelsinki headline2", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    headline3 : new TextStyle(debugLabel: "whiteHelsinki headline3", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    headline4 : new TextStyle(debugLabel: "whiteHelsinki headline4", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    headline5 : new TextStyle(debugLabel: "whiteHelsinki headline5", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    headline6 : new TextStyle(debugLabel: "whiteHelsinki headline6", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    bodyText1 : new TextStyle(debugLabel: "whiteHelsinki bodyText1", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    bodyText2 : new TextStyle(debugLabel: "whiteHelsinki bodyText2", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    subtitle1 : new TextStyle(debugLabel: "whiteHelsinki subtitle1", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    subtitle2 : new TextStyle(debugLabel: "whiteHelsinki subtitle2", fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    caption   : new TextStyle(debugLabel: "whiteHelsinki caption",   fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    button    : new TextStyle(debugLabel: "whiteHelsinki button",    fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    overline  : new TextStyle(debugLabel: "whiteHelsinki overline",  fontFamily: "Roboto", fontFamilyFallback: _helsinkiFontFallbacks, inherit: true, color: Colors.white,   decoration: TextDecoration.none)
  );
  
  public static readonly TextTheme blackCupertino = new TextTheme(
    headline1 : new TextStyle(debugLabel: "blackCupertino headline1", fontFamily: ".SF UI Display", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    headline2 : new TextStyle(debugLabel: "blackCupertino headline2", fontFamily: ".SF UI Display", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    headline3 : new TextStyle(debugLabel: "blackCupertino headline3", fontFamily: ".SF UI Display", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    headline4 : new TextStyle(debugLabel: "blackCupertino headline4", fontFamily: ".SF UI Display", inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    headline5 : new TextStyle(debugLabel: "blackCupertino headline5", fontFamily: ".SF UI Display", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    headline6 : new TextStyle(debugLabel: "blackCupertino headline6", fontFamily: ".SF UI Display", inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    bodyText1 : new TextStyle(debugLabel: "blackCupertino bodyText1", fontFamily: ".SF UI Text",    inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    bodyText2 : new TextStyle(debugLabel: "blackCupertino bodyText2", fontFamily: ".SF UI Text",    inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    subtitle1 : new TextStyle(debugLabel: "blackCupertino subtitle1", fontFamily: ".SF UI Text",    inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    subtitle2 : new TextStyle(debugLabel: "blackCupertino subtitle2", fontFamily: ".SF UI Text",    inherit: true, color: Colors.black,   decoration: TextDecoration.none),
    caption   : new TextStyle(debugLabel: "blackCupertino caption",   fontFamily: ".SF UI Text",    inherit: true, color: Colors.black54, decoration: TextDecoration.none),
    button    : new TextStyle(debugLabel: "blackCupertino button",    fontFamily: ".SF UI Text",    inherit: true, color: Colors.black87, decoration: TextDecoration.none),
    overline  : new TextStyle(debugLabel: "blackCupertino overline",  fontFamily: ".SF UI Text",    inherit: true, color: Colors.black,   decoration: TextDecoration.none)
  );


  public static readonly TextTheme whiteCupertino = new TextTheme(
    headline1 : new TextStyle(debugLabel: "whiteCupertino headline1", fontFamily: ".SF UI Display", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    headline2 : new TextStyle(debugLabel: "whiteCupertino headline2", fontFamily: ".SF UI Display", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    headline3 : new TextStyle(debugLabel: "whiteCupertino headline3", fontFamily: ".SF UI Display", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    headline4 : new TextStyle(debugLabel: "whiteCupertino headline4", fontFamily: ".SF UI Display", inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    headline5 : new TextStyle(debugLabel: "whiteCupertino headline5", fontFamily: ".SF UI Display", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    headline6 : new TextStyle(debugLabel: "whiteCupertino headline6", fontFamily: ".SF UI Display", inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    subtitle1 : new TextStyle(debugLabel: "whiteCupertino subtitle1", fontFamily: ".SF UI Text",    inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    bodyText1 : new TextStyle(debugLabel: "whiteCupertino bodyText1", fontFamily: ".SF UI Text",    inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    bodyText2 : new TextStyle(debugLabel: "whiteCupertino bodyText2", fontFamily: ".SF UI Text",    inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    caption   : new TextStyle(debugLabel: "whiteCupertino caption",   fontFamily: ".SF UI Text",    inherit: true, color: Colors.white70, decoration: TextDecoration.none),
    button    : new TextStyle(debugLabel: "whiteCupertino button",    fontFamily: ".SF UI Text",    inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    subtitle2 : new TextStyle(debugLabel: "whiteCupertino subtitle2", fontFamily: ".SF UI Text",    inherit: true, color: Colors.white,   decoration: TextDecoration.none),
    overline  : new TextStyle(debugLabel: "whiteCupertino overline",  fontFamily: ".SF UI Text",    inherit: true, color: Colors.white,   decoration: TextDecoration.none)
  );


  public static readonly TextTheme englishLike2014 = new TextTheme(
    headline1 : new TextStyle(debugLabel: "englishLike display4 2014", inherit: false, fontSize: 112.0f, fontWeight: FontWeight.w100, textBaseline: TextBaseline.alphabetic),
    headline2 : new TextStyle(debugLabel: "englishLike display3 2014", inherit: false, fontSize:  56.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline3 : new TextStyle(debugLabel: "englishLike display2 2014", inherit: false, fontSize:  45.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline4 : new TextStyle(debugLabel: "englishLike display1 2014", inherit: false, fontSize:  34.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline5 : new TextStyle(debugLabel: "englishLike headline 2014", inherit: false, fontSize:  24.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline6 : new TextStyle(debugLabel: "englishLike title 2014",    inherit: false, fontSize:  20.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic),
    bodyText1 : new TextStyle(debugLabel: "englishLike body2 2014",    inherit: false, fontSize:  14.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic),
    bodyText2 : new TextStyle(debugLabel: "englishLike body1 2014",    inherit: false, fontSize:  14.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    subtitle1 : new TextStyle(debugLabel: "englishLike subhead 2014",  inherit: false, fontSize:  16.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    subtitle2 : new TextStyle(debugLabel: "englishLike subtitle 2014", inherit: false, fontSize:  14.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.1f),
    caption   : new TextStyle(debugLabel: "englishLike caption 2014",  inherit: false, fontSize:  12.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    button    : new TextStyle(debugLabel: "englishLike button 2014",   inherit: false, fontSize:  14.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic),
    overline  : new TextStyle(debugLabel: "englishLike overline 2014", inherit: false, fontSize:  10.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 1.5f)
  );
  
  public static readonly TextTheme englishLike2018 = new TextTheme(
    headline1 : new TextStyle(debugLabel: "englishLike headline1 2018", fontSize: 96.0f, fontWeight: FontWeight.w300, textBaseline: TextBaseline.alphabetic, letterSpacing: -1.5f),
    headline2 : new TextStyle(debugLabel: "englishLike headline2 2018", fontSize: 60.0f, fontWeight: FontWeight.w300, textBaseline: TextBaseline.alphabetic, letterSpacing: -0.5f),
    headline3 : new TextStyle(debugLabel: "englishLike headline3 2018", fontSize: 48.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.0f),
    headline4 : new TextStyle(debugLabel: "englishLike headline4 2018", fontSize: 34.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.25f),
    headline5 : new TextStyle(debugLabel: "englishLike headline5 2018", fontSize: 24.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.0f),
    headline6 : new TextStyle(debugLabel: "englishLike headline6 2018", fontSize: 20.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.15f),
    bodyText1 : new TextStyle(debugLabel: "englishLike bodyText1 2018", fontSize: 16.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.5f),
    bodyText2 : new TextStyle(debugLabel: "englishLike bodyText2 2018", fontSize: 14.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.25f),
    subtitle1 : new TextStyle(debugLabel: "englishLike subtitle1 2018", fontSize: 16.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.15f),
    subtitle2 : new TextStyle(debugLabel: "englishLike subtitle2 2018", fontSize: 14.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.1f),
    button    : new TextStyle(debugLabel: "englishLike button 2018",    fontSize: 14.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.75f),
    caption   : new TextStyle(debugLabel: "englishLike caption 2018",   fontSize: 12.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 0.4f),
    overline  : new TextStyle(debugLabel: "englishLike overline 2018",  fontSize: 10.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic, letterSpacing: 1.5f)
  );


  public static readonly TextTheme dense2014 = new TextTheme(
    headline1 : new TextStyle(debugLabel: "dense display4 2014", inherit: false, fontSize: 112.0f, fontWeight: FontWeight.w100, textBaseline: TextBaseline.ideographic),
    headline2 : new TextStyle(debugLabel: "dense display3 2014", inherit: false, fontSize:  56.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    headline3 : new TextStyle(debugLabel: "dense display2 2014", inherit: false, fontSize:  45.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    headline4 : new TextStyle(debugLabel: "dense display1 2014", inherit: false, fontSize:  34.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    headline5 : new TextStyle(debugLabel: "dense headline 2014", inherit: false, fontSize:  24.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    headline6 : new TextStyle(debugLabel: "dense title 2014",    inherit: false, fontSize:  21.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.ideographic),
    bodyText1 : new TextStyle(debugLabel: "dense body2 2014",    inherit: false, fontSize:  15.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.ideographic),
    bodyText2 : new TextStyle(debugLabel: "dense body1 2014",    inherit: false, fontSize:  15.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    subtitle1 : new TextStyle(debugLabel: "dense subhead 2014",  inherit: false, fontSize:  17.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    subtitle2 : new TextStyle(debugLabel: "dense subtitle 2014", inherit: false, fontSize:  15.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.ideographic),
    caption   : new TextStyle(debugLabel: "dense caption 2014",  inherit: false, fontSize:  13.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    button    : new TextStyle(debugLabel: "dense button 2014",   inherit: false, fontSize:  15.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.ideographic),
    overline  : new TextStyle(debugLabel: "dense overline 2014", inherit: false, fontSize:  11.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic)
  );
  
  public static readonly TextTheme dense2018 = new TextTheme(
    headline1 : new TextStyle(debugLabel: "dense headline1 2018", fontSize: 96.0f, fontWeight: FontWeight.w100, textBaseline: TextBaseline.ideographic),
    headline2 : new TextStyle(debugLabel: "dense headline2 2018", fontSize: 60.0f, fontWeight: FontWeight.w100, textBaseline: TextBaseline.ideographic),
    headline3 : new TextStyle(debugLabel: "dense headline3 2018", fontSize: 48.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    headline4 : new TextStyle(debugLabel: "dense headline4 2018", fontSize: 34.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    headline5 : new TextStyle(debugLabel: "dense headline5 2018", fontSize: 24.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    headline6 : new TextStyle(debugLabel: "dense headline6 2018", fontSize: 21.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.ideographic),
    bodyText1 : new TextStyle(debugLabel: "dense bodyText1 2018", fontSize: 17.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    bodyText2 : new TextStyle(debugLabel: "dense bodyText2 2018", fontSize: 15.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    subtitle1 : new TextStyle(debugLabel: "dense subtitle1 2018", fontSize: 17.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    subtitle2 : new TextStyle(debugLabel: "dense subtitle2 2018", fontSize: 15.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.ideographic),
    button    : new TextStyle(debugLabel: "dense button 2018",    fontSize: 15.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.ideographic),
    caption   : new TextStyle(debugLabel: "dense caption 2018",   fontSize: 13.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic),
    overline  : new TextStyle(debugLabel: "dense overline 2018",  fontSize: 11.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.ideographic)
  );
  
  public static readonly TextTheme tall2014 = new TextTheme(
    headline1 : new TextStyle(debugLabel: "tall display4 2014", inherit: false, fontSize: 112.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline2 : new TextStyle(debugLabel: "tall display3 2014", inherit: false, fontSize:  56.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline3 : new TextStyle(debugLabel: "tall display2 2014", inherit: false, fontSize:  45.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline4 : new TextStyle(debugLabel: "tall display1 2014", inherit: false, fontSize:  34.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline5 : new TextStyle(debugLabel: "tall headline 2014", inherit: false, fontSize:  24.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline6 : new TextStyle(debugLabel: "tall title 2014",    inherit: false, fontSize:  21.0f, fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
    bodyText1 : new TextStyle(debugLabel: "tall body2 2014",    inherit: false, fontSize:  15.0f, fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
    bodyText2 : new TextStyle(debugLabel: "tall body1 2014",    inherit: false, fontSize:  15.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    subtitle1 : new TextStyle(debugLabel: "tall subhead 2014",  inherit: false, fontSize:  17.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    subtitle2 : new TextStyle(debugLabel: "tall subtitle 2014", inherit: false, fontSize:  15.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic),
    caption   : new TextStyle(debugLabel: "tall caption 2014",  inherit: false, fontSize:  13.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    button    : new TextStyle(debugLabel: "tall button 2014",   inherit: false, fontSize:  15.0f, fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
    overline  : new TextStyle(debugLabel: "tall overline 2014", inherit: false, fontSize:  11.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic)
  );
  
  public static readonly TextTheme tall2018 = new TextTheme(
    headline1 : new TextStyle(debugLabel: "tall headline1 2018", fontSize: 96.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline2 : new TextStyle(debugLabel: "tall headline2 2018", fontSize: 60.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline3 : new TextStyle(debugLabel: "tall headline3 2018", fontSize: 48.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline4 : new TextStyle(debugLabel: "tall headline4 2018", fontSize: 34.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline5 : new TextStyle(debugLabel: "tall headline5 2018", fontSize: 24.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    headline6 : new TextStyle(debugLabel: "tall headline6 2018", fontSize: 21.0f, fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
    bodyText1 : new TextStyle(debugLabel: "tall bodyText1 2018", fontSize: 17.0f, fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
    bodyText2 : new TextStyle(debugLabel: "tall bodyText2 2018", fontSize: 15.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    subtitle1 : new TextStyle(debugLabel: "tall subtitle1 2018", fontSize: 17.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    subtitle2 : new TextStyle(debugLabel: "tall subtitle2 2018", fontSize: 15.0f, fontWeight: FontWeight.w500, textBaseline: TextBaseline.alphabetic),
    button    : new TextStyle(debugLabel: "tall button 2018",    fontSize: 15.0f, fontWeight: FontWeight.w700, textBaseline: TextBaseline.alphabetic),
    caption   : new TextStyle(debugLabel: "tall caption 2018",   fontSize: 13.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic),
    overline  : new TextStyle(debugLabel: "tall overline 2018",  fontSize: 11.0f, fontWeight: FontWeight.w400, textBaseline: TextBaseline.alphabetic)
  );
    }
}