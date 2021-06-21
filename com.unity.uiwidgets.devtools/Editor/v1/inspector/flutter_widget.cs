/*using System;
using System.Collections.Generic;
using Unity.UIWidgets.DevTools.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector
{

  public class Category {
      public Category(string label, Image icon)
      {
        this.label = label;
        this.icon = icon;
      }

  public static Category accessibility = new Category(
    "Accessibility",
    IconsUtils.createImageIcon("icons/inspector/balloonInformation.png")
  );
  public static Category animationAndMotion = new Category(
    "Animation and Motion",
    IconsUtils.createImageIcon("icons/inspector/resume.png")
  );
  public static Category assetsImagesAndIcons = new Category(
    "Assets, Images, and Icons",
    IconsUtils.createImageIcon("icons/inspector/any_type.png")
  );
  public static Category asyncCategory = new Category(
    "Async",
    IconsUtils.createImageIcon("icons/inspector/threads.png")
  );
  public static readonly Category basics = new Category(
    "Basics",
    null // TODO(jacobr): add an icon.
  );
  public static readonly Category cupertino = new Category(
    "Cupertino (iOS-style widgets)",
    null // TODO(jacobr): add an icon.
  );
  public static Category input = new Category(
    "Input",
    IconsUtils.createImageIcon("icons/inspector/renderer.png")
  );
  public static Category paintingAndEffects = new Category(
    "Painting and effects",
    IconsUtils.createImageIcon("icons/inspector/colors.png")
  );
  public static Category scrolling = new Category(
    "Scrolling",
    IconsUtils.createImageIcon("icons/inspector/scrollbar.png")
  );
  public static Category stack = new Category(
    "Stack",
    IconsUtils.createImageIcon("icons/inspector/value.png")
  );
  public static Category styling = new Category(
    "Styling",
    IconsUtils.createImageIcon("icons/inspector/atrule.png")
  );
  public static Category text = new Category(
    "Text",
    IconsUtils.createImageIcon("icons/inspector/textArea.png")
  );

  public static List<Category> values = new List<Category> {
    accessibility,
    animationAndMotion,
    assetsImagesAndIcons,
    asyncCategory,
    basics,
    cupertino,
    input,
    paintingAndEffects,
    scrolling,
    stack,
    styling,
    text,
  };

  public readonly string label;
  public readonly Image icon;

  static Dictionary<string, Category> _categories;

  public static Category forLabel(string label) {
    if (_categories == null) {
      _categories = new Dictionary<string, Category>();
      foreach (var category in values) {
        _categories[category.label] = category;
      }
    }
    return _categories[label];
  }
}

public class FlutterWidget {
  public FlutterWidget(Dictionary<string, object> json)
  {
    this.json = json;
    icon = initIcon(json);
  }

  public readonly Dictionary<string, object> json;
  public static Image icon;

  
  //[!!!] may has error
  static Image initIcon(Dictionary<string, object> json)
  {
    List<object> categories = new List<object>();
    categories.Add(json.getOrDefault("categories"));
    if (categories != null) {
      // TODO(pq): consider priority over first match.
      foreach (string label in categories) {
        Category category = Category.forLabel(label);
        if (category != null) {
          icon = category.icon;
          if (icon != null) return icon;
        }
      }
    }
    return null;
  }

  string name
  {
    get
    {
      return JsonUtils.getStringMember(json, "name");
    }
  }

  List<string> categories
  {
    get
    {
      return JsonUtils.getValues(json, "categories");
    }
  }

  List<string> subCategories
  {
    get
    {
      return JsonUtils.getValues(json, "subcategories");
    }
  }
}

    
}*/