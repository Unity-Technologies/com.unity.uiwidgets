using System;
using System.Collections.Generic;
using UIWidgetsGallery.demo.shrine.model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace UIWidgetsGallery.demo.shrine
{
    public class CategoryMenuPage : StatelessWidget {
        public CategoryMenuPage(
            Key key = null,
            VoidCallback onCategoryTap = null
        ) : base(key: key)
        {
            this.onCategoryTap = onCategoryTap;
        }

        public readonly VoidCallback onCategoryTap;

        Widget _buildCategory(Category category, BuildContext context)
        {
            string categoryString = category.ToString().Replace("Category", "").ToUpper();
            ThemeData theme = Theme.of(context);
            return new ScopedModelDescendant<AppStateModel>(
                builder: (BuildContext context2, Widget child, AppStateModel model) =>
                new GestureDetector(
                    onTap: () =>
                    {
                        model.setCategory(category);
                        if (onCategoryTap != null)
                        {
                            onCategoryTap();
                        }
                    },
                    child: model.selectedCategory == category
                    ? new Container( 
                        child:new Column(
                            children: new List<Widget>
                            {
                                new SizedBox(height: 16.0f),
                                new Text(
                                    categoryString,
                                    style: theme.textTheme.bodyText1,
                                    textAlign: TextAlign.center
                                ),
                                new SizedBox(height: 14.0f),
                                new Container(
                                    width: 70.0f,
                                    height: 2.0f,
                                    color: shrineColorsUtils.kShrinePink400
                                ),
                            }
                        )
                    )
                    : new Container(
                        child: new Padding(
                            padding: EdgeInsets.symmetric(vertical: 16.0f),
                            child: new Text(
                                categoryString,
                                style: theme.textTheme.bodyText1.copyWith(
                                    color: shrineColorsUtils.kShrineBrown900.withAlpha(153)
                                ),
                                textAlign: TextAlign.center
                            )
                        )
                    )
                )
            );
        }
        

  
  public override Widget build(BuildContext context) {
      var count = Enum.GetNames(typeof(Category)).Length;
      List<Widget> widgets = new List<Widget>();
      for (int i = 0;i<count;i++)
      {
          widgets.Add( _buildCategory((Category)i, context));
      }
      return new Center(
      child: new Container(
        padding: EdgeInsets.only(top: 40.0f),
        color: shrineColorsUtils.kShrinePink100,
        child: new ListView(
          children: widgets
        )
      )
    );
  }
}

}