using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.DevTools;
using Unity.UIWidgets.DevTools.ui;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace Unity.UIWidgets.DevTools.inspector
{

  public class DiagnosticsNodeUtils
  {
    EnumUtils<DiagnosticLevel> diagnosticLevelUtils = 
      new EnumUtils<DiagnosticLevel>(Enum.GetValues(typeof(DiagnosticLevel)).Cast<DiagnosticLevel>().ToList());

    EnumUtils<DiagnosticsTreeStyle> treeStyleUtils =
      new EnumUtils<DiagnosticsTreeStyle>(Enum.GetValues(typeof(DiagnosticsTreeStyle)).Cast<DiagnosticsTreeStyle>().ToList());
  }
  
public class RemoteDiagnosticsNode : DiagnosticableTree {
  public RemoteDiagnosticsNode(
    Dictionary<string, object> json,
    Future<ObjectGroup> inspectorService,
    bool isProperty,
    RemoteDiagnosticsNode parent
  )
  {
    this.json = json;
    this.inspectorService = inspectorService;
    this.isProperty = isProperty;
    this.parent = parent;
  }

  public static readonly CustomIconMaker iconMaker = new CustomIconMaker();

  public static BoxConstraints deserializeConstraints(Dictionary<string, object> json) {
    if (json == null) return null;
    return new BoxConstraints(
      minWidth: float.Parse((string)json["minWidth"] ?? "0.0f"),
      maxWidth: float.Parse((string)json["maxWidth"] ?? "Infinity"),
      minHeight: float.Parse((string)json["minHeight"] ?? "0.0f"),
      maxHeight: float.Parse((string)json["maxHeight"] ?? "Infinity")
    );
  }

  static BoxParentData deserializeParentData(Dictionary<string, object> json) {
    if (json == null) return null;
    BoxParentData boxParentData = new BoxParentData();
    boxParentData.offset = new Offset(
      float.Parse((string)json["offsetX"] ?? "0.0f"),
      float.Parse((string)json["offsetY"] ?? "0.0f")
    );
    return boxParentData;

  }

  static Size deserializeSize(Dictionary<string, object> json) {
    if (json == null) return null;
    return new Size(
      float.Parse((string)json["width"]),
      float.Parse((string)json["height"])
    );
  }

  static FlexFit? deserializeFlexFit(String flexFit) {
    if (flexFit == null) {
      return null;
    } else if (flexFit == "tight") return FlexFit.tight;
    return FlexFit.loose;
  }

  /// This node's parent (if it's been set).
  public RemoteDiagnosticsNode parent;

  Future<string> propertyDocFuture;

  List<RemoteDiagnosticsNode> cachedProperties;

  /// Service used to retrieve more detailed information about the value of
  /// the property and its children and properties.
  public readonly Future<ObjectGroup> inspectorService;

  /// JSON describing the diagnostic node.
  public readonly Dictionary<string, object> json;

  Future<Dictionary<string, InstanceRef>> _valueProperties;

  public readonly bool isProperty;

  private List<string> Flex = new List<string> {"Row", "Column", "Flex"};
  // TODO(albertusangga): Refactor to cleaner/more robust solution
  public bool isFlex
  {
    get
    {
      return Flex.Contains(widgetRuntimeType);
    }
  }


  bool isBox
  {
    get
    {
      return (bool)json.getOrDefault("isBox");
    }
  }

  public float  flexFactor
  {
    get
    {
      return (float)json.getOrDefault("flexFactor");
    }
  }

  public FlexFit? flexFit
  {
    get
    {
      return deserializeFlexFit((string)json["flexFit"]);
    }
  }

  public RemoteDiagnosticsNode renderObject {
    get
    {
      if (_renderObject != null) return _renderObject;
      Dictionary<string, object> data = (Dictionary<string, object>)json["renderObject"];
      if (data == null) return null;
      _renderObject = new RemoteDiagnosticsNode(data, inspectorService, false, null);
      return _renderObject;
    }
    
  }

  RemoteDiagnosticsNode _renderObject;

  public RemoteDiagnosticsNode  parentRenderElement {
    get
    {
      var data = json["parentRenderElement"];
      if (data == null) return null;
      _parentRenderElement =
        DevTools.RemoteDiagnosticsNode(data, inspectorService, false, null);
      return _parentRenderElement;
    }
    
  }

  RemoteDiagnosticsNode _parentRenderElement;

  public BoxConstraints constraints
  {
    get
    {
      return deserializeConstraints((Dictionary<string, object>)json["constraints"]);
    }
  }

  public BoxParentData parentData
  {
    get
    {
      return deserializeParentData((Dictionary<string, object>)json["parentData"]);
    }
  }

  public Size size
  {
    get
    {
      return deserializeSize((Dictionary<string, object>)json["size"]);
    }
  }

  //TODO return type may has error
  bool? isLocalClass {
    get
    {
      var objectGroup = inspectorService;
      if (objectGroup is ObjectGroup) {
        return _isLocalClass = _isLocalClass?? objectGroup.inspectorService.isLocalClass(this);
      } else {
        // TODO(jacobr): if objectGroup is a Future<ObjectGroup> we cannot compute
        // whether classes are local as for convenience we need this method to
        // return synchronously.
        return _isLocalClass = false;
      }
    }
    
  }

  bool? _isLocalClass;

  
  public override bool operator ==(dynamic other) {
    if (!(other is RemoteDiagnosticsNode)) return false;
    return dartDiagnosticRef == other.dartDiagnosticRef;
  
  public override int hashCode
  {
    get
    {
      return dartDiagnosticRef.hashCode;
    }
  }

  public string separator
  {
    get
    {
      return showSeparator ? ":" : "";
    }
  }
  
  public string name
  {
    get
    {
      return getStringMember("name");
    }
  }

  public bool  showSeparator
  {
    get
    {
      return getBooleanMember("showSeparator", true);
    }
  }
  
  public string description
  {
    get
    {
      return getStringMember("description");
    }
  }
  
  public DiagnosticLevel  level
  {
    get
    {
      return getLevelMember("level", DiagnosticLevel.info);
    }
  }

  public bool showName
  {
    get
    {
      return getBooleanMember("showName", true);
    }
  }
  
  string getEmptyBodyDescription()
  {
    return getStringMember("emptyBodyDescription");
  }

  /// Hint for how the node should be displayed.
  public DiagnosticsTreeStyle? style {
    get
    {
        return _style = _style?? getStyleMember("style", DiagnosticsTreeStyle.sparse);
    }
    set 
    {
        _style = value;
    }
  }

  DiagnosticsTreeStyle? _style;

 
  public string type
  {
    get
    {
      return getStringMember("type");
    }
  }
   
  bool isQuoted
  {
    get
    {
      return getBooleanMember("quoted", false);
    }
  }

  bool hasIsQuoted
  {
    get
    {
      return json.ContainsKey("quoted");
    }
  }

  /// Optional unit the [value] is measured in.
  ///
  /// Unit must be acceptable to display immediately after a number with no
  /// spaces. For example: 'physical pixels per logical pixel' should be a
  /// [tooltip] not a [unit].
  ///
  /// Only specified for Number properties.
  string unit
  {
    get
    {
      return getStringMember("unit");
    }
  }

  bool hasUnit
  {
    get
    {
      return json.ContainsKey("unit");
    }
  }
  
  string numberToString
  {
    get
    {
      return getStringMember("numberToString");
    }
  }

  bool hasNumberToString
  {
    get
    {
      return json.ContainsKey("numberToString");
    }
  }
  
  string ifTrue
  {
    get
    {
      return getStringMember("ifTrue");
    }
  }

  bool hasIfTrue
  {
    get
    {
      return json.ContainsKey("ifTrue");
    }
  }

  string ifFalse
  {
    get
    {
      return getStringMember("ifFalse");
    }
  }

  bool hasIfFalse
  {
    get
    {
      return json.ContainsKey("ifFalse");
    }
  }

  
  //[!!!] may has error
  List<string> values {
    get
    {
      var rawValues = json["values"];
      if (rawValues == null) {
        return null;
      }
      return rawValues.ToList();
    }
    
  }

  bool hasValues
  {
    get
    {
      return json.ContainsKey("values");
    }
  }

  /// Description to use if the property [value] is not null.
  ///
  /// If the property [value] is not null and [ifPresent] is null, the
  /// [level] for the property is [DiagnosticsLevel.hidden] and the description
  /// from superclass is used.
  ///
  /// Only specified for ObjectFlagProperty.
  string ifPresent
  {
    get
    {
      return getStringMember("ifPresent");
    }
  }

  bool hasIfPresent
  {
    get
    {
      return json.ContainsKey("ifPresent");
    }
  }
  
  string defaultValue
  {
    get
    {
      return getStringMember("defaultValue");
    }
  }
  
  public bool hasDefaultValue
  {
    get
    {
      return json.ContainsKey("defaultValue");
    }
  }


  string ifEmpty
  {
    get
    {
      return getStringMember("ifEmpty");
    }
  }
  
  string ifNull
  {
    get
    {
      return getStringMember("ifNull");
    }
  }

  bool allowWrap
  {
    get
    {
      return getBooleanMember("allowWrap", true);
    }
  }
  
  string tooltip
  {
    get
    {
      return getStringMember("tooltip") ?? "";
    }
  }

  bool hasTooltip
  {
    get
    {
      return json.ContainsKey("tooltip");
    }
  }
  
  bool missingIfNull
  {
    get
    {
      return getBooleanMember("missingIfNull", false);
    }
  }

  string exception
  {
    get
    {
      return getStringMember("exception");
    }
  }

  /// Whether accessing the property throws an exception.
  bool hasException
  {
    get
    {
      return json.ContainsKey("exception");
    }
  }

  bool hasCreationLocation {
    get
    {
      return _creationLocation != null || json.ContainsKey("creationLocation");
    }
    
  }
  
  int locationId
  {
    get
    {
      return JsonUtils.getIntMember(json, "locationId");
    }
    
  }

  

  InspectorSourceLocation _creationLocation;

  InspectorSourceLocation creationLocation {
    get
    {
      if (_creationLocation != null) {
        return _creationLocation;
      }
      if (!hasCreationLocation) {
        return null;
      }
      _creationLocation = new InspectorSourceLocation(json["creationLocation"], null);
      return _creationLocation;
    }
    set {
      _creationLocation = value;
    }
    
  }

  public string propertyType
  {
    get
    {
      return getStringMember("propertyType");
    }
  }

  DiagnosticLevel defaultLevel {
    get
    {
      return getLevelMember("defaultLevel", DiagnosticLevel.info);
    }
    
  }
  
  /// TODO(jacobr): add helpers to get the properties and children of
  /// this diagnosticable value even if getChildren and getProperties
  /// would return null. This will allow showing nested data for properties
  /// that don't show children by default in other debugging output but
  /// could.
  public bool  isDiagnosticableValue {
    get
    {
      return getBooleanMember("isDiagnosticableValue", false);
    }
    
  }

  string getStringMember(string memberName) {
    return JsonUtils.getStringMember(json, memberName);
  }

  bool getBooleanMember(string memberName, bool defaultValue) {
    if (json[memberName] == null) {
      return defaultValue;
    }
    return json[memberName];
  }

  DiagnosticLevel getLevelMember(
      string memberName, DiagnosticLevel defaultValue) {
      string value = json[memberName];
      if (value == null) {
          return defaultValue;
      }
      var level = diagnosticLevelUtils.enumEntry(value);
      D.assert(level != null, () => "Unabled to find level for $value");
    return level ?? defaultValue;
  }

  DiagnosticsTreeStyle getStyleMember(
      string memberName, DiagnosticsTreeStyle defaultValue) {
    if (!json.ContainsKey(memberName)) {
      return defaultValue;
    }
    string value = json[memberName];
    if (value == null) {
      return defaultValue;
    }
    var style = treeStyleUtils.enumEntry(value);
    D.assert(style != null);
    return style ?? defaultValue;
  }
  
  public InspectorInstanceRef valueRef
  {
    get
    {
        return InspectorInstanceRef(json["valueId"]);
    }
  }

  bool isEnumProperty() {
    return type != null && type.StartsWith("EnumProperty<");
  }
  
  Future<Dictionary<string, InstanceRef>> valueProperties {
    get
    {
      if (_valueProperties == null)
      {
        if (propertyType == null || valueRef?.id == null)
        {
          _valueProperties = Future.value().to<Dictionary<string, InstanceRef>>();
          return _valueProperties;
        }

        if (isEnumProperty())
        {
          return (await inspectorService)?.getEnumPropertyValues(valueRef);
        }
        
        List<string> propertyNames;

        switch (propertyType)
        {
          case "Color":
            propertyNames = new List<string> {"red", "green", "blue", "alpha"};
            break;
          case "IconData":
            propertyNames = new List<string> {"codePoint"};
            break;
          default:
            _valueProperties = Future.value().to<Dictionary<string, InstanceRef>>();
            return _valueProperties;
        }

        _valueProperties = (await inspectorService)
          ?.getDartObjectProperties(valueRef, propertyNames);
      }

      return _valueProperties;
    }
  }

  public Dictionary<string, object> valuePropertiesJson
  {
    get
    {
      return json["valueProperties"];
    }
  }

  public bool hasChildren {
    get
    {
      List<object> children = (List<object>)json["children"];
      if (children != null) {
        return children.isNotEmpty();
      }
      return getBooleanMember("hasChildren", false);
    }
    
  }

  public bool isCreatedByLocalProject {
    get
    {
      return getBooleanMember("createdByLocalProject", false);
    }
    
  }

  /// Whether this node is being displayed as a full tree or a filtered tree.
  public bool isSummaryTree
  {
    get
    {
      return getBooleanMember("summaryTree", false);
    }
  }

  /// Whether this node is being displayed as a full tree or a filtered tree.
  bool isStateful
  {
    get
    {
      return getBooleanMember("stateful", false);
    }
  }

  string widgetRuntimeType
  {
    get
    {
      return getStringMember("widgetRuntimeType");
    }
  }
  
  public bool childrenReady {
    get
    {
      return json.ContainsKey("children") || _children != null || !hasChildren;  
    }
    
  }

  public Future<List<RemoteDiagnosticsNode>> children {
    get
    {
      _computeChildren().then(() =>
      {
        if (_children != null) return _children;
        return _childrenFuture;
      });
      return null;
    }
   
  }

  public List<RemoteDiagnosticsNode> childrenNow {
    get
    {
      _maybePopulateChildren();
      return _children;
    }
    
  }

  Future _computeChildren() {
    _maybePopulateChildren();
    if (!hasChildren || _children != null) {
      return null;
    }

    if (_childrenFuture != null){
      return _childrenFuture;
    }

    _childrenFuture = _getChildrenHelper();
    try
    {
      _childrenFuture.then((_children) => { _children = _children ?? new List<RemoteDiagnosticsNode>(); });
    }
    catch (Exception e)
    {
      D.assert(false,()=>"Future _computeChildren() has an unknown exception");
    }

    return null;
  }

  Future<List<RemoteDiagnosticsNode>> _getChildrenHelper() {
    return (await inspectorService)?.getChildren(
      dartDiagnosticRef,
      isSummaryTree,
      this
    );
  }

  void _maybePopulateChildren() {
    if (!hasChildren || _children != null) {
      return;
    }

    List<object> jsonArray = json["children"];
    if (jsonArray?.isNotEmpty() == true) {
      List<RemoteDiagnosticsNode> nodes = new List<RemoteDiagnosticsNode>();
      foreach (var element in jsonArray)
      {
        var child =
          new RemoteDiagnosticsNode(element, inspectorService, false, parent);
        child.parent = this;
        nodes.Add(child);
      }
      _children = nodes;
    }
  }

  Future<List<RemoteDiagnosticsNode>> _childrenFuture;
  List<RemoteDiagnosticsNode> _children;

  /// Reference the actual Dart DiagnosticsNode object this object is referencing.
  public InspectorInstanceRef dartDiagnosticRef {
    get
    {
      return new InspectornstanceRef(json["objectId"]);
    }
    
  }

  /// Properties to show inline in the widget tree.
  public List<RemoteDiagnosticsNode> inlineProperties {
    get
    {
      if (cachedProperties == null) {
        cachedProperties = new List<RemoteDiagnosticsNode>();
        if (json.ContainsKey("properties")) { 
          List<object> jsonArray = json["properties"];
          foreach (Dictionary<string, object> element in jsonArray) {
            cachedProperties.Add(
              new RemoteDiagnosticsNode(element, inspectorService, true, parent));
          }
          trackPropertiesMatchingParameters(cachedProperties);
        }
      }
      return cachedProperties;
    }
    
  }

  Future<List<RemoteDiagnosticsNode>> getProperties(ObjectGroup objectGroup)
  {
    return objectGroup.getProperties(dartDiagnosticRef).then((v) => { trackPropertiesMatchingParameters(v);});
  }

  List<RemoteDiagnosticsNode> trackPropertiesMatchingParameters(
      List<RemoteDiagnosticsNode> nodes) {
      List<InspectorSourceLocation> parameterLocations =
        creationLocation?.getParameterLocations();
    if (parameterLocations != null) {
      Dictionary<String, InspectorSourceLocation> names = new Dictionary<string, InspectorSourceLocation>();
      foreach (InspectorSourceLocation location in parameterLocations) {
        string name = location.getName();
        if (name != null) {
          names[name] = location;
        }
      }
      foreach (RemoteDiagnosticsNode node in nodes) {
        node.parent = this;
        string name = node.name;
        if (name != null) {
          InspectorSourceLocation parameterLocation = names[name];
          if (parameterLocation != null) {
            node.creationLocation = parameterLocation;
          }
        }
      }
    }
    return nodes;
  }

  Future<string> propertyDoc {
    get
    {
      propertyDocFuture = propertyDocFuture ?? _createPropertyDocFuture();
      return propertyDocFuture;
    }
    
  }

  Future<string> _createPropertyDocFuture() {
    // TODO(jacobr): We need access to the analyzer to support this feature.
    /*
    if (parent != null) {
      DartVmServiceValue vmValue = inspectorService.toDartVmServiceValueForSourceLocation(parent.getValueRef());
      if (vmValue == null) {
       return null;
      }
      return inspectorService.getPropertyLocation(vmValue.getInstanceRef(), getName())
          .thenApplyAsync((XSourcePosition sourcePosition) -> {
      if (sourcePosition != null) {
      final VirtualFile file = sourcePosition.getFile();
      final int offset = sourcePosition.getOffset();

      final Project project = getProject(file);
      if (project != null) {
      final List<HoverInformation> hovers =
      DartAnalysisServerService.getInstance(project).analysis_getHover(file, offset);
      if (!hovers.isEmpty()) {
      return hovers.get(0).getDartdoc();
      }
      }
      }
      return 'Unable to find property source';
      });
      });
    }
*/
    return Future.value("Unable to find property source").to<string>();
  }

  public Widget icon {
    get
    {
      if (isProperty) return null;
      return iconMaker.fromWidgetName(widgetRuntimeType);
    }
  }
  
  bool identicalDisplay(RemoteDiagnosticsNode node) {
    if (node == null) {
      return false;
    }
    var entries = json.entries;
    if (entries.length != node.json.entries.length) {
      return false;
    }
    foreach (var entry in entries) {
      string key = entry.key;
      if (key == "objectId" || key == "valueId") {
        continue;
      }
      if (entry.value == node.json[key]) {
        return false;
      }
    }
    return true;
  }

  
  public override void debugFillProperties(DiagnosticPropertiesBuilder properties) {
    base.debugFillProperties(properties);
    foreach (var property in inlineProperties) {
      properties.add(new DiagnosticsProperty<RemoteDiagnosticsNode>(property.name, property));
    }
  }

  
  public override List<DiagnosticsNode> debugDescribeChildren() {
    var children = childrenNow;
    if (children == null || children.isEmpty()) return new List<DiagnosticsNode>();
    var regularChildren = new List<DiagnosticsNode>();
    foreach (var child in children) {
      regularChildren.Add(child.toDiagnosticsNode());
    }
    return regularChildren;
  }

  
  public override DiagnosticsNode toDiagnosticsNode(string name = null, DiagnosticsTreeStyle style = DiagnosticsTreeStyle.none) {
    return base.toDiagnosticsNode(
      name: name ?? this.name,
      style: DiagnosticsTreeStyle.sparse
    );
  }

  
  public override string toStringShort() {
    return description;
  }

  public Future setSelectionInspector(bool uiAlreadyUpdated) {
    await (await inspectorService)
        ?.setSelectionInspector(valueRef, uiAlreadyUpdated);
    return null;
  }
}

class InspectorSourceLocation {
  public InspectorSourceLocation(Dictionary<string, object> json, InspectorSourceLocation parent)
  {
    this.json = json;
    this.parent = parent;
  }

  public readonly Dictionary<string, object> json;
  public readonly InspectorSourceLocation parent;

  string path
  {
    get
    {
      return JsonUtils.getStringMember(json, "file");
    }
  }

  string getFile() {
    var fileName = path;
    if (fileName == null) {
      return parent != null ? parent.getFile() : null;
    }

    return fileName;
  }

  int getLine()
  {
    
    return JsonUtils.getIntMember(json, "line");
  }

  public string getName()
  {
    
    return JsonUtils.getStringMember(json, "name");
  }

  int getColumn()
  {
    
    return JsonUtils.getIntMember(json, "column");
  }

  SourcePosition getXSourcePosition() {
    var file = getFile();
    if (file == null) {
      return null;
    }
    int line = getLine();
    int column = getColumn();
    if (line < 0 || column < 0) {
      return null;
    }
    return new SourcePosition(file: file, line: line - 1, column: column - 1);
  }

  public List<InspectorSourceLocation> getParameterLocations() {
    if (json.ContainsKey("parameterLocations")) {
      List<object> parametersJson = json["parameterLocations"];
      List<InspectorSourceLocation> ret = new List<InspectorSourceLocation>();
      for (int i = 0; i < parametersJson.Count; ++i) {
        ret.Add(new InspectorSourceLocation(parametersJson[i], this));
      }
      return ret;
    }
    return null;
  }
}

    public class SourcePosition {
        public SourcePosition(string file, int line, int column)
        {
            this.file = file;
            this.line = line;
            this.column = column;
        }

        public readonly string file;
        public readonly int line;
        public readonly int column;
    }
}