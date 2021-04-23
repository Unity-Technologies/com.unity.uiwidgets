using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;
using Unity.UIWidgets.cupertino;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Random = System.Random;

namespace UIWidgetsGallery.gallery {
    class CupertinoRefreshControlDemo : StatefulWidget {
      public static string routeName = "/cupertino/refresh";

      public override State createState()
      {
        return new _CupertinoRefreshControlDemoState();
      }
    }

    class _CupertinoRefreshControlDemoState : State<CupertinoRefreshControlDemo> {
      
      
      List<List<string>> contacts = new List<List<string>>{
      new List<string>{"George Washington", "Westmoreland County", " 4/30/1789"},
      new List<string>{"John Adams", "Braintree", " 3/4/1797"},
      new List<string>{"Thomas Jefferson", "Shadwell", " 3/4/1801"},
      new List<string>{"James Madison", "Port Conway", " 3/4/1809"},
      new List<string>{"James Monroe", "Monroe Hall", " 3/4/1817"},
      new List<string>{"Andrew Jackson", "Waxhaws Region South/North", " 3/4/1829"},
      new List<string>{"John Quincy Adams", "Braintree", " 3/4/1825"},
      new List<string>{"William Henry Harrison", "Charles City County", " 3/4/1841"},
      new List<string>{"Martin Van Buren", "Kinderhook New", " 3/4/1837"},
      new List<string>{"Zachary Taylor", "Barboursville", " 3/4/1849"},
      new List<string>{"John Tyler", "Charles City County", " 4/4/1841"},
      new List<string>{"James Buchanan", "Cove Gap", " 3/4/1857"},
      new List<string>{"James K. Polk", "Pineville North", " 3/4/1845"},
      new List<string>{"Millard Fillmore", "Summerhill New", "7/9/1850"},
      new List<string>{"Franklin Pierce", "Hillsborough New", " 3/4/1853"},
      new List<string>{"Andrew Johnson", "Raleigh North", " 4/15/1865"},
      new List<string>{"Abraham Lincoln", "Sinking Spring", " 3/4/1861"},
      new List<string>{"Ulysses S. Grant", "Point Pleasant", " 3/4/1869"},
      new List<string>{"Rutherford B. Hayes", "Delaware", " 3/4/1877"},
      new List<string>{"Chester A. Arthur", "Fairfield", " 9/19/1881"},
      new List<string>{"James A. Garfield", "Moreland Hills", " 3/4/1881"},
      new List<string>{"Benjamin Harrison", "North Bend", " 3/4/1889"},
      new List<string>{"Grover Cleveland", "Caldwell New", " 3/4/1885"},
      new List<string>{"William McKinley", "Niles", " 3/4/1897"},
      new List<string>{"Woodrow Wilson", "Staunton", " 3/4/1913"},
      new List<string>{"William H. Taft", "Cincinnati", " 3/4/1909"},
      new List<string>{"Theodore Roosevelt", "New York City New", " 9/14/1901"},
      new List<string>{"Warren G. Harding", "Blooming Grove", " 3/4/1921"},
      new List<string>{"Calvin Coolidge", "Plymouth", "8/2/1923"},
      new List<string>{"Herbert Hoover", "West Branch", " 3/4/1929"},
      new List<string>{"Franklin D. Roosevelt", "Hyde Park New", " 3/4/1933"},
      new List<string>{"Harry S. Truman", "Lamar", " 4/12/1945"},
      new List<string>{"Dwight D. Eisenhower", "Denison", " 1/20/1953"},
      new List<string>{"Lyndon B. Johnson", "Stonewall", "11/22/1963"},
      new List<string>{"Ronald Reagan", "Tampico", " 1/20/1981"},
      new List<string>{"Richard Nixon", "Yorba Linda", " 1/20/1969"},
      new List<string>{"Gerald Ford", "Omaha", "August 9/1974"},
      new List<string>{"John F. Kennedy", "Brookline", " 1/20/1961"},
      new List<string>{"George H. W. Bush", "Milton", " 1/20/1989"},
      new List<string>{"Jimmy Carter", "Plains", " 1/20/1977"},
      new List<string>{"George W. Bush", "New Haven", " 1/20, 2001"},
      new List<string>{"Bill Clinton", "Hope", " 1/20/1993"},
      new List<string>{"Barack Obama", "Honolulu", " 1/20/2009"},
      new List<string>{"Donald J. Trump", "New York City", " 1/20/2017"}
    };
      
      
      
      List<List<string>> randomizedContacts;

      public override void initState()
      {
       
        base.initState();
        repopulateList();
      }

      void repopulateList()
      {
        //List<string> nullStr = new List<string>();//{" "};
        randomizedContacts = new List<List<string>>();
        
        Random random = new Random();
        
        for (int index = 0; index < 100; index++ )
        {
          var id = random.Next(contacts.Count);
          if (id < contacts.Count)
          {
              contacts[id].Add(id %  2 == 0 ? true.ToString() : false.ToString());
              randomizedContacts.Add(new List<string>());
              for (int i = 0; i < 4; i++)
              {
                randomizedContacts[index].Add(contacts[id][i]);
              }
          }
        }
      }

      public override Widget build(BuildContext context) {
        _ListItem getListItem(int index)
        {
          if (index < randomizedContacts.Count && index > 0 )
          {
            return new _ListItem(
              name: randomizedContacts[index][0],
              place: randomizedContacts[index][1],
              date: randomizedContacts[index][2],
              called: randomizedContacts[index][3] == "true"
            );
          }
          else
          {
            return new _ListItem();
          }
        }

        return new DefaultTextStyle(
          style: CupertinoTheme.of(context).textTheme.textStyle,
          child: new CupertinoPageScaffold(
            backgroundColor: CupertinoColors.systemGroupedBackground,
            child: new CustomScrollView(
              physics: new BouncingScrollPhysics(parent: new AlwaysScrollableScrollPhysics()),
              slivers: new List<Widget>{
                new CupertinoSliverNavigationBar(
                  largeTitle: new Text("Refresh"),
                  previousPageTitle: "Cupertino"
                  //trailing: CupertinoDemoDocumentationButton(CupertinoRefreshControlDemo.routeName),
                ),
                new CupertinoSliverRefreshControl(
                  onRefresh: () =>{
                    return Future.delayed(new TimeSpan(0,0,0,2)).then((_)=> {
                        if (mounted) {
                          setState(() => repopulateList());
                        }
                    });
                  }
                ),
                new SliverSafeArea(
                  top: false, // Top safe area is consumed by the navigation bar.
                  sliver: new SliverList(
                    del: new SliverChildBuilderDelegate(
                      (BuildContext context1, int index)=>
                      {
                        return getListItem(index);
                      },
                      childCount: 20
                    )
                  )
                ),
              }
            )
          )
        );
      }
    }

    class _ListItem : StatelessWidget { 
      public _ListItem(
        string name = null,
        string place = null,
        string date = null,
        bool called = false
      )
      {
        this.name = name;
        this.place = place;
        this.date = date;
        this.called = called;
      }

      public readonly string name;
      public readonly string place;
      public readonly string date;
      public readonly bool called;

      public override Widget build(BuildContext context) {
        return new Container(
          color: CupertinoDynamicColor.resolve(CupertinoColors.systemBackground, context),
          height: 60.0f,
          padding: EdgeInsets.only(top: 9.0f),
          child: new Row(
            children: new List<Widget>{
              new Container(
                width: 38.0f,
                child: called
                    ? new Align(
                        alignment: Alignment.topCenter,
                        child: new Icon(
                          CupertinoIcons.phone_solid,
                          color: CupertinoColors.inactiveGray.resolveFrom(context),
                          size: 18.0f
                        )
                      )
                    : null
              ),
              new Expanded(
                child: new Container(
                  decoration: new  BoxDecoration(
                    border: new Border(
                      bottom: new BorderSide(color: new Color(0xFFBCBBC1), width: 0.0f)
                    )
                  ),
                  padding: EdgeInsets.only(left: 1.0f, bottom: 9.0f, right: 10.0f),
                  child: new Row(
                    children: new List<Widget>{
                      new Expanded(
                        child: new Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          mainAxisAlignment: MainAxisAlignment.spaceBetween,
                          children: new List<Widget>{
                            new Text(
                              name ?? "",
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: new TextStyle(
                                fontWeight: FontWeight.w600,
                                letterSpacing: -0.18f
                              )
                            ),
                            new Text(
                              place ?? "",
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                              style: new TextStyle(
                                fontSize: 15.0f,
                                letterSpacing: -0.24f,
                                color: CupertinoColors.inactiveGray.resolveFrom(context)
                              )
                            ),
                          }
                        )
                      ),
                      new Text(
                        date ?? "",
                        style: new TextStyle(
                          color: CupertinoColors.inactiveGray.resolveFrom(context),
                          fontSize: 15.0f,
                          letterSpacing: -0.41f
                        )
                      ),
                       new Padding(
                        padding: EdgeInsets.only(left: 9.0f),
                        child: new Icon(
                          CupertinoIcons.info,
                          color: CupertinoTheme.of(context).primaryColor
                        )
                      )
                    }
                  )
                )
              )
            }
          )
        );
      }
    }

}