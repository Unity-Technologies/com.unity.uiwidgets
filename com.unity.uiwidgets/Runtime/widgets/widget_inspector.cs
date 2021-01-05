using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.async2;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler2;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets {
    public delegate Widget InspectorSelectButtonBuilder(BuildContext context, VoidCallback onPressed);
    public delegate Dictionary<string, object> AddAdditionalPropertiesCallback(DiagnosticsNode diagnosticsNode, InspectorSerializationDelegate inspectorSerializationDelegate);
    
    public class WidgetInspectorUtils{
        public static float _kScreenEdgeMargin = 10.0f;
        public static float _kTooltipPadding = 5.0f;
        public static float  _kInspectButtonMargin = 10.0f;
        public static float _kOffScreenMargin = 1.0f;

        public static TextStyle _messageStyle = new TextStyle(
            color: new Color(0xFFFFFFFF),
            fontSize: 10.0f,
            height: 1.2f
        );
        public static int _kMaxTooltipLines = 5;
        public static Color _kTooltipBackgroundColor = Color.fromARGB(230, 60, 60, 60);
        public static Color _kHighlightedRenderObjectFillColor = Color.fromARGB(128, 128, 128, 255);
        public static Color _kHighlightedRenderObjectBorderColor = Color.fromARGB(128, 64, 64, 128);

        /*bool _isDebugCreator(DiagnosticsNode node) => node is DiagnosticsDebugCreator; 
        IEnumerable<DiagnosticsNode> transformDebugCreator(IEnumerable<DiagnosticsNode> properties) sync* { 
            List<DiagnosticsNode> pending = new List<DiagnosticsNode>();
            bool foundStackTrace = false;
            foreach ( DiagnosticsNode node in properties) { 
                if (!foundStackTrace && node is DiagnosticsStackTrace)
                    foundStackTrace = true;
                if (_isDebugCreator(node)) {
                    yield* _parseDiagnosticsNode(node);
                } else {
                    if (foundStackTrace) {
                        pending.Add(node);
                    } else {
                        yield node;
                    }
                }
            }
            yield* pending;
        }*/
        public static readonly Dictionary<_Location, int> _locationToId = new Dictionary<_Location, int>();
        public static readonly List<_Location> _locations = new List<_Location>();

        public static int _toLocationId(_Location location) {
            int id = _locationToId[location];
            if (id != null) {
                return id;
            }
            id = _locations.Count;
            _locations.Add(location);
            _locationToId[location] = id;
            return id;
        }
        /*public IEnumerable<DiagnosticsNode> _parseDiagnosticsNode(DiagnosticsNode node) { 
            if (!_isDebugCreator(node))
                return null;
            DebugCreator debugCreator = node.value as DebugCreator;
            Element element = debugCreator.element;
            return _describeRelevantUserCode(element);
        }*/
        public IEnumerable<DiagnosticsNode> _describeRelevantUserCode(Element element) {
            if (!WidgetInspectorService.instance.isWidgetCreationTracked()) {
                return new List<DiagnosticsNode>() {
                    new ErrorDescription(
                        "Widget creation tracking is currently disabled. Enabling " +
                        "it enables improved error messages. It can be enabled by passing " +
                        "`--track-widget-creation` to `flutter run` or `flutter test`."
                        ),
                    new ErrorSpacer()
                };
                
            }

            List<DiagnosticsNode> nodes = new List<DiagnosticsNode>();

            bool processElement(Element target) {
                if (_isLocalCreationLocation(target)) {
                    nodes.Add(
                        new DiagnosticsBlock(
                            name: "The relevant error-causing widget was",
                            children: new List<DiagnosticsNode>() {
                                new ErrorDescription(
                                    "${target.widget.toStringShort()} ${_describeCreationLocation(target)}"),
                            }
                        ));
                    nodes.Add(new ErrorSpacer());
                    return false;
                }

                return true;
            }
            if (processElement(element))
                element.visitAncestorElements(processElement);
            return nodes;
        }

        public bool _isLocalCreationLocation(object _object){ 
            _Location location = _getCreationLocation(_object);
            if (location == null)
                return false;
            return WidgetInspectorService.instance._isLocalCreationLocation(location);
        }
        public string _describeCreationLocation(object _object) {
            _Location location = _getCreationLocation(_object);
            return location?.ToString();
        }
        public static _Location _getCreationLocation(object _object) { 
            object candidate =  _object is Element ? ((Element)_object).widget : _object;
            return candidate is _HasCreationLocation ? ((_HasCreationLocation)candidate)._location : null;
        }
        Rect _calculateSubtreeBoundsHelper(RenderObject _object, Matrix4 transform) {
            Rect bounds = MatrixUtils.transformRect(transform, _object.semanticBounds);

            _object.visitChildren((RenderObject child) =>{
                Matrix4 childTransform = transform.clone();
                _object.applyPaintTransform(child, childTransform);
                Rect childBounds = _calculateSubtreeBoundsHelper(child, childTransform);
                Rect paintClip = _object.describeApproximatePaintClip(child);
                if (paintClip != null) {
                    Rect transformedPaintClip = MatrixUtils.transformRect(
                        transform,
                        paintClip
                    );
                    childBounds = childBounds.intersect(transformedPaintClip);
                }

                if (childBounds.isFinite && !childBounds.isEmpty) {
                    bounds = bounds.isEmpty ? childBounds : bounds.expandToInclude(childBounds);
                }
            });

            return bounds;
        }

        /// Calculate bounds for a render object and all of its descendants.
        Rect _calculateSubtreeBounds(RenderObject _object){
            return _calculateSubtreeBoundsHelper(_object, Matrix4.identity());
        }

    }
    public abstract class _HasCreationLocation {
        public _Location _location { get; }
    }

    public class _ProxyLayer : Layer {
        public _ProxyLayer(Layer _layer) {
            this._layer = _layer;
        }

        public readonly  Layer _layer;

        public override void addToScene(ui.SceneBuilder builder,  Offset layerOffset = null ) {
            layerOffset = layerOffset ?? Offset.zero;
            _layer.addToScene(builder, layerOffset);
        }

        public override bool findAnnotations<S>(
            AnnotationResult<S> result,
            Offset localPosition, 
            bool onlyFirst) {
            return _layer.findAnnotations(result, localPosition, onlyFirst: onlyFirst);
        }
    }
    public class _MulticastCanvas : Canvas { 
        public _MulticastCanvas(
            Canvas main,
            Canvas screenshot
        ) : base( new PictureRecorder()) {
            D.assert(main != null);
            D.assert(screenshot != null);
            _main = main;
            _screenshot = screenshot;
        }
        public readonly Canvas _main;
        public readonly Canvas _screenshot;
        public override void clipPath(Path path,  bool doAntiAlias = true ) {
            _main.clipPath(path, doAntiAlias: doAntiAlias);
            _screenshot.clipPath(path, doAntiAlias: doAntiAlias);
        }
        public override void clipRRect(RRect rrect,  bool doAntiAlias = true ) {
            _main.clipRRect(rrect, doAntiAlias: doAntiAlias);
            _screenshot.clipRRect(rrect, doAntiAlias: doAntiAlias);
        }

        public override void clipRect(Rect rect,  ui.ClipOp clipOp = ui.ClipOp.intersect, bool doAntiAlias = true ) {
            _main.clipRect(rect, clipOp: clipOp, doAntiAlias: doAntiAlias);
            _screenshot.clipRect(rect, clipOp: clipOp, doAntiAlias: doAntiAlias);
        }

        public override void drawArc(Rect rect, float startAngle, float sweepAngle, bool useCenter, Paint paint) {
            _main.drawArc(rect, startAngle, sweepAngle, useCenter, paint);
            _screenshot.drawArc(rect, startAngle, sweepAngle, useCenter, paint);
        }

        public override void drawAtlas(ui.Image atlas, List<RSTransform> transforms, List<Rect> rects, List<Color> colors, BlendMode blendMode, Rect cullRect, Paint paint) {
            _main.drawAtlas(atlas, transforms, rects, colors, blendMode, cullRect, paint);
            _screenshot.drawAtlas(atlas, transforms, rects, colors, blendMode, cullRect, paint);
        }

        public override void drawCircle(Offset c, float radius, Paint paint) {
            _main.drawCircle(c, radius, paint);
            _screenshot.drawCircle(c, radius, paint);
        }

        public override void drawColor(Color color, BlendMode blendMode) {
            _main.drawColor(color, blendMode);
            _screenshot.drawColor(color, blendMode);
        }

        public override void drawDRRect(RRect outer, RRect inner, Paint paint) {
            _main.drawDRRect(outer, inner, paint);
            _screenshot.drawDRRect(outer, inner, paint);
        }

        public override void drawImage(ui.Image image, Offset p, Paint paint) {
            _main.drawImage(image, p, paint);
            _screenshot.drawImage(image, p, paint);
        }

        public override void drawImageNine(ui.Image image, Rect center, Rect dst, Paint paint) {
            _main.drawImageNine(image, center, dst, paint);
            _screenshot.drawImageNine(image, center, dst, paint);
        }

        public override void drawImageRect(ui.Image image, Rect src, Rect dst, Paint paint) {
            _main.drawImageRect(image, src, dst, paint);
            _screenshot.drawImageRect(image, src, dst, paint);
        }

        public override void drawLine(Offset p1, Offset p2, Paint paint) {
            _main.drawLine(p1, p2, paint);
            _screenshot.drawLine(p1, p2, paint);
        }

        public override void drawOval(Rect rect, Paint paint) {
            _main.drawOval(rect, paint);
            _screenshot.drawOval(rect, paint);
        }

        public override void drawPaint(Paint paint) {
            _main.drawPaint(paint);
            _screenshot.drawPaint(paint);
        }

        public override void drawParagraph(ui.Paragraph paragraph, Offset offset) {
            _main.drawParagraph(paragraph, offset);
            _screenshot.drawParagraph(paragraph, offset);
        }

        public override void drawPath(Path path, Paint paint) {
            _main.drawPath(path, paint);
            _screenshot.drawPath(path, paint);
        }

        public override void drawPicture(ui.Picture picture) {
            _main.drawPicture(picture);
            _screenshot.drawPicture(picture);
        }

        public override void drawPoints(ui.PointMode pointMode, List<Offset> points, Paint paint) {
            _main.drawPoints(pointMode, points, paint);
            _screenshot.drawPoints(pointMode, points, paint);
        }

        public override void drawRRect(RRect rrect, Paint paint) {
            _main.drawRRect(rrect, paint);
            _screenshot.drawRRect(rrect, paint);
        }

        public override void drawRawAtlas(ui.Image atlas,
            float[] rstTransforms,
            float[] rects,
            uint[] colors,
            BlendMode blendMode,
            Rect cullRect,
            Paint paint) {
            _main.drawRawAtlas(atlas, rstTransforms, rects, colors, blendMode, cullRect, paint);
            _screenshot.drawRawAtlas(atlas, rstTransforms, rects, colors, blendMode, cullRect, paint);
        }

        public override void drawRawPoints(PointMode pointMode, float[] points, Paint paint) {
            _main.drawRawPoints(pointMode, points, paint);
            _screenshot.drawRawPoints(pointMode, points, paint);
        }

        public override void drawRect(Rect rect, Paint paint) {
            _main.drawRect(rect, paint);
            _screenshot.drawRect(rect, paint);
        }

        public override void drawShadow(Path path, Color color, float elevation, bool transparentOccluder) {
            _main.drawShadow(path, color, elevation, transparentOccluder);
            _screenshot.drawShadow(path, color, elevation, transparentOccluder);
        }

        public override void drawVertices(ui.Vertices vertices, BlendMode blendMode, Paint paint) {
            _main.drawVertices(vertices, blendMode, paint);
            _screenshot.drawVertices(vertices, blendMode, paint);
        }

        public override int getSaveCount() {
            return _main.getSaveCount();
        }

        public override void restore() {
            _main.restore();
            _screenshot.restore();
        }

        public override void rotate(float radians) {
            _main.rotate(radians);
            _screenshot.rotate(radians);
        }

        public override void save() {
            _main.save();
            _screenshot.save();
        }

        public override void saveLayer(Rect bounds, Paint paint) {
            _main.saveLayer(bounds, paint);
            _screenshot.saveLayer(bounds, paint);
        }

        public override void scale(float sx,  float? sy = null ) {
            _main.scale(sx, sy);
            _screenshot.scale(sx, sy);
        }

        public override void skew(float sx, float sy) {
            _main.skew(sx, sy);
            _screenshot.skew(sx, sy);
        }

        public override void transform(float[] matrix4) {
            _main.transform(matrix4);
            _screenshot.transform(matrix4);
        }

        public override void translate(float dx, float dy) {
            _main.translate(dx, dy);
            _screenshot.translate(dx, dy);
        }
    }
    public class _ScreenshotContainerLayer : OffsetLayer {
        public override void addToScene(ui.SceneBuilder builder ,Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            addChildrenToScene(builder, layerOffset);
        }
    }

    

    
    public class _SerializeConfig {
        public _SerializeConfig(string groupName, bool summaryTree = false,
            int subtreeDepth = 1, List<Diagnosticable> pathToInclude = null,
            bool includeProperties = false, bool expandPropertyValues = true) {
            this.groupName = groupName;
            this.summaryTree = summaryTree;
            this.subtreeDepth = subtreeDepth;
            this.pathToInclude = pathToInclude;
            this.includeProperties = includeProperties;
            this.expandPropertyValues = expandPropertyValues;
        }

        public static _SerializeConfig merge(_SerializeConfig from, int? subtreeDepth = null,
            List<Diagnosticable> pathToInclude = null) {
            return new _SerializeConfig(from.groupName, from.summaryTree, subtreeDepth ?? from.subtreeDepth,
                pathToInclude ?? from.pathToInclude, from.includeProperties, from.expandPropertyValues);
        }

        public readonly string groupName;
        public readonly bool summaryTree;
        public readonly int subtreeDepth;
        public readonly List<Diagnosticable> pathToInclude;
        public readonly bool includeProperties;
        public readonly bool expandPropertyValues;
    }

    public delegate void InspectorSelectionChangedCallback();

    public delegate void InspectorShowCallback();

    public delegate void DeveloperInspect();

    class _InspectorReferenceData {
        public readonly object obj;
        public int count = 1;

        public _InspectorReferenceData(object obj) {
            this.obj = obj;
        }
    }

    public class WidgetInspectorService {
        public readonly WidgetsBinding widgetsBinding;
        bool _debugShowInspector;

        readonly Dictionary<string, HashSet<_InspectorReferenceData>> _groups =
            new Dictionary<string, HashSet<_InspectorReferenceData>>();

        readonly Dictionary<string, _InspectorReferenceData> _idToReferenceData =
            new Dictionary<string, _InspectorReferenceData>();

        readonly Dictionary<object, string> _objectToId = new Dictionary<object, string>();
        int _nextId = 0;
        public readonly InspectorSelection selection = new InspectorSelection();
        public InspectorSelectionChangedCallback selectionChangedCallback;
        public DeveloperInspect developerInspect;
        public InspectorShowCallback inspectorShowCallback;
        
        List<String> _pubRootDirectories;

        public static WidgetInspectorService instance {
            get { return WidgetsBinding.instance.widgetInspectorService; }
        }

        public WidgetInspectorService(WidgetsBinding widgetsBinding) {
            this.widgetsBinding = widgetsBinding;
        }


//        private static WidgetInspectorService _instance;
//
//        public WidgetInspectorService instance
//        {
//            get { return _instance; }
//        }

        public bool _isLocalCreationLocation(_Location location) {
            if (location == null || location.file == null) {
                return false;
            }

            string file = new Uri(location.file).LocalPath; //Uri.parse(location.file).path;

            // By default check whether the creation location was within package:flutter.
            if (_pubRootDirectories == null) {
                // TODO(chunhtai): Make it more robust once
                // https://github.com/flutter/flutter/issues/32660 is fixed.
                return !file.Contains("packages/flutter/");
            }

            foreach (string directory in _pubRootDirectories) {
                if(file.StartsWith(directory))
                {
                    return true;
                }
            }
            return false;
        }

        public List<DiagnosticsNode> _filterChildren(
            List<DiagnosticsNode> nodes,
            InspectorSerializationDelegate _delegate
            ) {
            
            List<DiagnosticsNode> result = new List<DiagnosticsNode>();
            foreach ( DiagnosticsNode child in nodes) {
                if (!_delegate.summaryTree || _shouldShowInSummaryTree(child))
                    result.Add(child);
                else
                    result = _getChildrenFiltered(child, _delegate);
            }
            List<DiagnosticsNode> children = result;
            return children;
        }

        public List<DiagnosticsNode> _truncateNodes(IEnumerable<DiagnosticsNode>nodes, int maxDescendentsTruncatableNode) {
            int count = 0;
            foreach (var node in  nodes) {
                if (node is DiagnosticsNode) {
                    if (((DiagnosticsNode) node).value is Element) {
                        count++;
                    }
                }
            }
            if (count == nodes.Count() && isWidgetCreationTracked()) {
                List<DiagnosticsNode> localNodes = nodes.Where((DiagnosticsNode node) =>
                    _isValueCreatedByLocalProject(node.value)).ToList();
                if (localNodes.isNotEmpty()) {
                    return localNodes;
                }
            }
            //return nodes.take(maxDescendentsTruncatableNode).toList();
            List<DiagnosticsNode> results = new List<DiagnosticsNode>();
            for (int i = 0; i < maxDescendentsTruncatableNode; i++) {
                results.Add(nodes.ToList()[i]);
            }
            return results;
        }
        
        public Dictionary<string, object> getRootWidget(string groupName) {
            return _nodeToJson(WidgetsBinding.instance?.renderViewElement?.toDiagnosticsNode(), new InspectorSerializationDelegate(groupName: groupName, service: this));
        }

        public Dictionary<string, object> getRootWidgetSummaryTree(string groupName) {
            return _nodeToJson(
                WidgetsBinding.instance?.renderViewElement?.toDiagnosticsNode(),
                new InspectorSerializationDelegate(groupName: groupName, subtreeDepth: 1000000, summaryTree: true, service: this));
        }

        public Dictionary<string, object> getRootRenderObject(string groupName) {
            return _nodeToJson(RendererBinding.instance?.renderView?.toDiagnosticsNode(),
                new InspectorSerializationDelegate(groupName: groupName, service: this));
        
        }

        public Dictionary<string, object> getDetailsSubtree(string id, string groupName,int subtreeDepth) {
            var root = toObject(id) as DiagnosticsNode;
            if (root == null) {
                return null;
            }

            return  _nodeToJson(
                root,
                new InspectorSerializationDelegate(
                    groupName: groupName,
                    summaryTree: false,
                    subtreeDepth: subtreeDepth,
                    includeProperties: true,
                    service: this
                )
            );
        }

        public bool setSelectionById(string id, string groupName = "") {
            return setSelection(toObject(id), groupName);
        }


        public Dictionary<string, object> getSelectedRenderObject(string previousSelectionId, string groupName) {
            DiagnosticsNode previousSelection = toObject(previousSelectionId) as DiagnosticsNode;
            RenderObject current = selection?.current;
            return _nodeToJson(current == previousSelection?.value ? previousSelection : current?.toDiagnosticsNode(), new InspectorSerializationDelegate(groupName: groupName, service: this));
        }


        public Dictionary<string, object> getSelectedWidget(string previousSelectionId, string groupName) {
            DiagnosticsNode previousSelection = toObject(previousSelectionId) as DiagnosticsNode;
            Element current = selection?.currentElement;
            return _nodeToJson(current == previousSelection?.value ? previousSelection : current?.toDiagnosticsNode(), new InspectorSerializationDelegate(groupName: groupName, service: this));
        }

        public Dictionary<string, object> getSelectedSummaryWidget(string previousSelectionId, string groupName) {
            return getSelectedWidget(previousSelectionId, groupName);
        }

        public bool debugShowInspector {
            get { return _debugShowInspector; }
            set {
                var old = _debugShowInspector;
                _debugShowInspector = value;
                if (_debugShowInspector != old && inspectorShowCallback != null) {
                    inspectorShowCallback();
                }
            }
        }

        public void disposeGroup(string groupName) {
            HashSet<_InspectorReferenceData> references;
            _groups.TryGetValue(groupName, out references);
            _groups.Remove(groupName);
            if (references != null) {
                foreach (var r in references) {
                    _decrementReferenceCount(r);
                }
            }


            D.assert(() => {
//                var groupNames = _groups.Select((entry) => string.Format("groupName={0} count={1}", entry.Key, entry.Value.Count));
//                Debug.LogFormat("groups {0} idsCount={1} objectsCount={2}", string.Join(",", groupNames.ToArray()),
//                    _idToReferenceData.Count, _objectToId.Count);
                return true;
            });
        }

        protected bool setSelection(object obj, string groupName = "") {
            if (obj is Element || obj is RenderObject) {
                if (obj is Element) {
                    if (ReferenceEquals(obj, selection.currentElement)) {
                        return false;
                    }

                    selection.currentElement = (Element) obj;
                }
                else {
                    if (obj == selection.current) {
                        return false;
                    }

                    selection.current = (RenderObject) obj;
                }

                if (selectionChangedCallback != null) {
                    if (WidgetsBinding.instance.schedulerPhase == SchedulerPhase.idle) {
                        selectionChangedCallback();
                    }
                    else {
                        // todo schedule task ?
                        WidgetsBinding.instance.scheduleFrameCallback(
                            duration => { selectionChangedCallback(); }
                        );
                    }
                }

                return true;
            }

            return false;
        }
        public Dictionary<string, object> _nodeToJson(
            DiagnosticsNode node,
            InspectorSerializationDelegate _delegate
            ) {
            return node?.toJsonMap(_delegate);
        }

        /*Dictionary<string, object> _nodeToJson(DiagnosticsNode node, _SerializeConfig config) {
            if (node == null) {
                return null;
            }

            var ret = node.toJsonMap( DiagnosticsSerializationDelegate());
            var value = node.valueObject;
            ret["objectId"] = toId(node, config.groupName);
            ret["valueId"] = toId(value, config.groupName);

            if (config.summaryTree) {
                ret["summaryTree"] = config.summaryTree;
            }

            var createdByLocalProject = true; // todo;
            if (config.subtreeDepth > 0 || (config.pathToInclude != null && config.pathToInclude.Count > 0)) {
                ret["children"] = _nodesToJson(_getChildrenFiltered(node, config), config);
            }

            if (config.includeProperties) {
                ret["properties"] = _nodesToJson(
                    node.getProperties().Where((n) =>
                        !n.isFiltered(createdByLocalProject ? DiagnosticLevel.fine : DiagnosticLevel.info)).ToList(),
                    new _SerializeConfig(groupName: config.groupName, subtreeDepth: 1, expandPropertyValues: true)
                );
            }

            var typeDef = typeof(DiagnosticsProperty<>);
            var nodeType = node.GetType();
            if (nodeType.IsGenericType && nodeType.GetGenericTypeDefinition() == typeDef) {
                if (value is Color) {
                    ret["valueProperties"] = new Dictionary<string, object> {
                        {"red", ((Color) value).red},
                        {"green", ((Color) value).green},
                        {"blue", ((Color) value).blue},
                        {"alpha", ((Color) value).alpha},
                    };
                }
                else if (value is IconData) {
                    ret["valueProperties"] = new Dictionary<string, object> {
                        {"codePoint", ((IconData) value).codePoint}
                    };
                }

                if (config.expandPropertyValues && value is Diagnosticable) {
                    ret["properties"] = _nodesToJson(
                        ((Diagnosticable) value).toDiagnosticsNode().getProperties()
                        .Where(n => !n.isFiltered(DiagnosticLevel.info)).ToList(),
                        new _SerializeConfig(groupName: config.groupName, subtreeDepth: 0, expandPropertyValues: false)
                    );
                }
            }

            return ret;
        }*/

        List<Dictionary<string, object>> _nodesToJson(
            List<DiagnosticsNode> nodes,
            InspectorSerializationDelegate _delegate, 
            DiagnosticsNode parent
        ) {
            return DiagnosticsNode.toJsonList(nodes, parent, _delegate);
        }

        List<Dictionary<string, object>> _getProperties(string diagnosticsNodeId, string groupName) {
            DiagnosticsNode node = toObject(diagnosticsNodeId) as DiagnosticsNode;
            return _nodesToJson(node == null ? new List<DiagnosticsNode>() : node.getProperties(), new InspectorSerializationDelegate(groupName: groupName, service: this), parent: node);
        }


        List<DiagnosticsNode> _getChildrenFiltered(
            DiagnosticsNode node,
            InspectorSerializationDelegate _delegate
            ) {
            return _filterChildren(node.getChildren(), _delegate);
        }

        public bool _shouldShowInSummaryTree(DiagnosticsNode node) {
            var value = node.valueObject;
            if (!(value is Diagnosticable)) {
                return true;
            }

            if (!(value is Element) || !isWidgetCreationTracked()) {
                return true;
            }

            return _isValueCreatedByLocalProject(value);
        }

        public string toId(object obj, string groupName) {
            if (obj == null) {
                return null;
            }

            HashSet<_InspectorReferenceData> group;
            _groups.TryGetValue(groupName, out group);
            if (group == null) {
                group = new HashSet<_InspectorReferenceData>();
                _groups.Add(groupName, group);
            }

            string id;
            _objectToId.TryGetValue(obj, out id);

            _InspectorReferenceData referenceData;
            if (id == null) {
                id = $"inspector-{_nextId}";
                _nextId++;
                _objectToId[obj] = id;
                referenceData = new _InspectorReferenceData(obj);
                _idToReferenceData[id] = referenceData;
                group.Add(referenceData);
            }
            else {
                referenceData = _idToReferenceData[id];
                if (group.Add(referenceData)) {
                    referenceData.count += 1;
                }
            }

            return id;
        }

        object toObject(string id) {
            if (id == null) {
                return null;
            }

            _InspectorReferenceData data;
            _idToReferenceData.TryGetValue(id, out data);
            if (data == null) {
                throw new UIWidgetsError("Id does not exist.");
            }

            return data.obj;
        }

        public bool isWidgetCreationTracked() {
            return false; //todo
        }

        bool _isValueCreatedByLocalProject(object value) {
            return true; // todo
        }

        void _decrementReferenceCount(_InspectorReferenceData reference) {
            reference.count -= 1;
            D.assert(reference.count >= 0);
            if (reference.count == 0) {
                string id;
                _objectToId.TryGetValue(reference.obj, out id);
                D.assert(id != null);
                _objectToId.Remove(reference.obj);
                _idToReferenceData.Remove(id);
            }
        }
    }

    public class WidgetInspector : StatefulWidget {
        public readonly Widget child;

        public WidgetInspector(
            Key key,
            Widget child,
            InspectorSelectButtonBuilder selectButtonBuilder
            ) : base(key) {
            D.assert(child != null);
            this.child = child;
            this.selectButtonBuilder = selectButtonBuilder;
        }

        public readonly InspectorSelectButtonBuilder selectButtonBuilder;

        public override State createState() {
            return new _WidgetInspectorState();
        }
    }
    public class InspectorSerializationDelegate : DiagnosticsSerializationDelegate {
        public InspectorSerializationDelegate(
            string groupName = null,
            bool? summaryTree = null,
            int? maxDescendentsTruncatableNode = null,
            bool? expandPropertyValues = null,
            int? subtreeDepth = null,
            bool? includeProperties = null,
            WidgetInspectorService service = null,
            AddAdditionalPropertiesCallback addAdditionalPropertiesCallback = null
            ) {
            this.groupName = groupName;
            this.summaryTree = summaryTree ?? false;
            this.maxDescendentsTruncatableNode = maxDescendentsTruncatableNode ?? -1;
            this.expandPropertyValues = expandPropertyValues ?? true;
            this.subtreeDepth = subtreeDepth ?? 1;
            this.includeProperties = includeProperties ?? false;
            this.service = service;
            this.addAdditionalPropertiesCallback = addAdditionalPropertiesCallback;
        }
        public readonly string groupName;
        public readonly bool summaryTree;
        public readonly int maxDescendentsTruncatableNode;
        public readonly bool expandPropertyValues;
        public readonly int subtreeDepth;
        public readonly bool includeProperties;
        public readonly WidgetInspectorService service; 
        public readonly AddAdditionalPropertiesCallback addAdditionalPropertiesCallback ;
        public readonly List<DiagnosticsNode> _nodesCreatedByLocalProject = new List<DiagnosticsNode>();
        public bool _interactive {
          get { return groupName != null;}
        }

        public override Dictionary<string, object> additionalNodeProperties(DiagnosticsNode node) { 
            Dictionary<string, object> result = new Dictionary<string, object>();
            object value = node.value;
            if (_interactive) {
                result["objectId"] = service.toId(node, groupName);
                result["valueId"] = service.toId(value, groupName);
            }
            if (summaryTree) {
                result["summaryTree"] = true;
            }
            _Location creationLocation = WidgetInspectorUtils._getCreationLocation(value);
            if (creationLocation != null) { 
                result["locationId"] = WidgetInspectorUtils._toLocationId(creationLocation);
                result["creationLocation"] = creationLocation.toJsonMap();
                if (service._isLocalCreationLocation(creationLocation)) {
                    _nodesCreatedByLocalProject.Add(node);
                    result["createdByLocalProject"] = true;
                }
            }
            if (addAdditionalPropertiesCallback != null) {
                if (addAdditionalPropertiesCallback(node, this) != null) {
                    foreach (var callback in addAdditionalPropertiesCallback(node,this)) {
                        result.Add(callback.Key,callback.Value);
                    }
                }
                
                //result.addAll(addAdditionalPropertiesCallback(node, this) ?? <string, object>{});
            }
            return result;
        } 
        public override DiagnosticsSerializationDelegate delegateForNode(DiagnosticsNode node) {
            return summaryTree || subtreeDepth > 1 || service._shouldShowInSummaryTree(node)
            ? copyWith(subtreeDepth: subtreeDepth - 1)
            : this;
        }
        public override List<DiagnosticsNode> filterChildren(List<DiagnosticsNode> children, DiagnosticsNode owner) {
            return service._filterChildren(children, this);
        }
        public override List<DiagnosticsNode> filterProperties(List<DiagnosticsNode> properties, DiagnosticsNode owner) { 
            bool createdByLocalProject = _nodesCreatedByLocalProject.Contains(owner);
            return properties.Where((DiagnosticsNode node)=>{
                return !node.isFiltered(createdByLocalProject ? DiagnosticLevel.fine : DiagnosticLevel.info);
            }).ToList();
        }
        public override List<DiagnosticsNode> truncateNodesList(List<DiagnosticsNode> nodes, DiagnosticsNode owner) {
            if (maxDescendentsTruncatableNode >= 0 &&
                owner?.allowTruncate == true &&
                nodes.Count > maxDescendentsTruncatableNode) {
                nodes = service._truncateNodes(nodes, maxDescendentsTruncatableNode);
            } 
            return nodes;
        }
        public override DiagnosticsSerializationDelegate copyWith(int? subtreeDepth = null, bool? includeProperties = null) {
            return new InspectorSerializationDelegate(
                groupName: groupName,
                summaryTree: summaryTree,
                maxDescendentsTruncatableNode: maxDescendentsTruncatableNode,
                expandPropertyValues: expandPropertyValues,
                subtreeDepth: subtreeDepth ?? this.subtreeDepth,
                includeProperties: includeProperties ?? this.includeProperties,
                service: service,
                addAdditionalPropertiesCallback: addAdditionalPropertiesCallback
            );
        }
    }

    public class _Location {
        public _Location(
            int line,
            int column,
            string file = null,
            string name = null,
            List<_Location> parameterLocations = null
        ) {
            this.file = file;
            this.line = line;
            this.column = column;
            this.name = name;
            this.parameterLocations = parameterLocations;
        }

        public readonly string file;
        public readonly int line;
        public readonly int column;
        public readonly string name;
        public readonly List<_Location> parameterLocations;

        public Dictionary<string, object> toJsonMap() {
            Dictionary<string, object> json = new Dictionary<string, object>();
            json["file"] = file;
            json["line"] = line;
            json["column"] = column;
            if (name != null) {
                json["name"] = name;
            }

            if (parameterLocations != null) {
                json["parameterLocations"] = parameterLocations.Select(
                    (_Location location) => location.toJsonMap()).ToList();
            }

            return json;
        }

        public override string ToString() {
            List<string> parts = new List<string>();
            if (name != null) {
                parts.Add(name);
            }

            if (file != null) {
                parts.Add(file);
            }
            parts.Add("$line");
            parts.Add("$column");
            string result = "";
            foreach (var part in parts) {
                result += part;
                if(part!= parts.last())
                    result += ":";
            }
            return result;
        }
    }



    class _WidgetInspectorState : State<WidgetInspector>, WidgetsBindingObserver {
        public _WidgetInspectorState() {
            selection = WidgetInspectorService.instance.selection;
        }

        Offset _lastPointerLocation;
        public readonly InspectorSelection selection;
        public bool isSelectMode = true;
        readonly GlobalKey _ignorePointerKey = GlobalKey.key();
        const float _edgeHitMargin = 2.0f;
        const float _kOffScreenMargin = 1.0f;
        const float _kScreenEdgeMargin = 10.0f;
        const float _kTooltipPadding = 5.0f;
        const float _kInspectButtonMargin = 10.0f;

        public override void initState() {
            base.initState();
            WidgetInspectorService.instance.selectionChangedCallback += _selectionChangedCallback;
        }

        public override void dispose() {
            WidgetInspectorService.instance.selectionChangedCallback -= _selectionChangedCallback;
            base.dispose();
        }

        bool _hitTestHelper(
            List<RenderObject> hits,
            List<RenderObject> edgeHits,
            Offset position,
            RenderObject renderObject,
            Matrix4 transform
        ) {
            var hit = false;
            

            var inverse = Matrix4.inverted(transform);
            if (inverse == null) {
                return false;
            }

            var localPosition = MatrixUtils.transformPoint(inverse, position);

            List<DiagnosticsNode> children = renderObject.debugDescribeChildren();
            for (int i = children.Count - 1; i >= 0; --i) {
                DiagnosticsNode diagnostics = children[i];
                D.assert(diagnostics != null);
                if (diagnostics.style == DiagnosticsTreeStyle.offstage ||
                    (!(diagnostics.valueObject is RenderObject))) {
                    continue;
                }

                RenderObject child = (RenderObject) diagnostics.valueObject;
                Rect paintClip = renderObject.describeApproximatePaintClip(child);
                if (paintClip != null && !paintClip.contains(localPosition)) {
                    continue;
                }

                var childTransform = new Matrix4(transform);
                renderObject.applyPaintTransform(child, childTransform);
                if (_hitTestHelper(hits, edgeHits, position, child, childTransform)) {
                    hit = true;
                }
            }

            Rect bounds = renderObject.semanticBounds;
            if (bounds.contains(localPosition)) {
                hit = true;
                if (!bounds.deflate(_edgeHitMargin).contains(localPosition)) {
                    edgeHits.Add(renderObject);
                }
            }

            if (hit) {
                hits.Add(renderObject);
            }

            return hit;
        }

        List<RenderObject> hitTest(Offset position, RenderObject root) {
            List<RenderObject> regularHits = new List<RenderObject>();
            List<RenderObject> edgeHits = new List<RenderObject>();

            _hitTestHelper(regularHits, edgeHits, position, root, root.getTransformTo(null));
            regularHits.Sort(CompareByArea);
            HashSet<RenderObject> hits = new HashSet<RenderObject>(edgeHits);
            foreach (var obj in regularHits) {
                if (!hits.Contains(obj)) {
                    hits.Add(obj);
                    edgeHits.Add(obj);
                }
            }

            return edgeHits;
        }

        void _inspectAt(Offset position) {
            if (!isSelectMode) {
                return;
            }

            RenderIgnorePointer ignorePointer =
                (RenderIgnorePointer) _ignorePointerKey.currentContext.findRenderObject();
            RenderObject userRender = ignorePointer.child;
            List<RenderObject> selected = hitTest(position, userRender);
            setState(() => { selection.candidates = selected; });
        }

        void _handlePanDown(DragDownDetails evt) {
            _lastPointerLocation = evt.globalPosition;
            _inspectAt(evt.globalPosition);
        }

        void _handlePanUpdate(DragUpdateDetails evt) {
            _lastPointerLocation = evt.globalPosition;
            _inspectAt(evt.globalPosition);
        }

        void _handlePanEnd(DragEndDetails details) {
            Rect bounds =
                (Offset.zero & (Window.instance.physicalSize / Window.instance.devicePixelRatio)).deflate(
                    _kOffScreenMargin);
            if (!bounds.contains(_lastPointerLocation)) {
                setState(() => { selection.clear(); });
            }
        }

        void _handleTap() {
            if (!isSelectMode) {
                return;
            }

            if (_lastPointerLocation != null) {
                _inspectAt(_lastPointerLocation);

                if (selection != null) {
                    if (WidgetInspectorService.instance.developerInspect != null) {
                        WidgetInspectorService.instance.developerInspect();
                    }
                }
            }

            setState(() => {
                if (widget.selectButtonBuilder != null) {
                    isSelectMode = false;
                }
            });
        }

        void _handleEnableSelect() {
            setState(() => { isSelectMode = true; });
        }

        public override Widget build(BuildContext context) {
            List<Widget> children = new List<Widget>();
            children.Add(new GestureDetector(
                onTap: _handleTap,
                onPanDown: _handlePanDown,
                onPanEnd: _handlePanEnd,
                onPanUpdate: _handlePanUpdate,
                behavior: HitTestBehavior.opaque,
                child: new IgnorePointer(
                    ignoring: isSelectMode,
                    key: _ignorePointerKey,
                    child: widget.child
                )
            ));

            if (!isSelectMode && widget.selectButtonBuilder != null) {
                children.Add(new Positioned(
                    left: _kInspectButtonMargin,
                    bottom: _kInspectButtonMargin,
                    child: widget.selectButtonBuilder(context, _handleEnableSelect)
                ));
            }

            children.Add(new _InspectorOverlay(null, selection));
            return new Stack(children: children);
        }

        public void didChangeMetrics() {
        }

        public void didChangeTextScaleFactor() {
        }

        public void didChangePlatformBrightness() {
        }

        public void didChangeLocales(List<Locale> locale) {
        }

        public Future<bool> didPopRoute() {
            return Future.value(false).to<bool>();
        }

        public Future<bool> didPushRoute(string route) {
            return Future.value(false).to<bool>();
        }

        void _selectionChangedCallback() {
            setState(() => { });
        }

        static int CompareByArea(RenderObject o1, RenderObject o2) {
            return _area(o1).CompareTo(_area(o2));
        }

        static float _area(RenderObject obj) {
            var bounds = obj.semanticBounds;
            Size size = null;
            if (bounds != null) {
                size = bounds.size;
            }

            return size == null ? float.PositiveInfinity : size.width * size.height;
        }
    }


    public class InspectorSelection {
        RenderObject _current;
        Element _currentElement;
        List<RenderObject> _candidates = new List<RenderObject>();

        public List<RenderObject> candidates {
            get { return _candidates; }
            set {
                _candidates = value;
                _index = 0;
                _computeCurrent();
            }
        }

        int _index = 0;

        public int index {
            get { return _index; }
            set {
                _index = value;
                _computeCurrent();
            }
        }

        public void clear() {
            candidates = new List<RenderObject>();
        }

        public RenderObject current {
            get { return _current; }
            set {
                if (_current != value) {
                    _current = value;
                    DebugCreator creator = value.debugCreator as DebugCreator;
                    _currentElement = creator.element;
                }
            }
        }

        public Element currentElement {
            get { return _currentElement; }
            set {
                if (!ReferenceEquals(currentElement, value)) {
                    _currentElement = value;
                    _current = value.findRenderObject();
                }
            }
        }

        void _computeCurrent() {
            if (_index < candidates.Count) {
                _current = candidates[index];
                _currentElement = ((DebugCreator) _current.debugCreator).element;
            }
            else {
                _current = null;
                _currentElement = null;
            }
        }


        public bool active {
            get { return _current != null && _current.attached; }
        }
    }

    class _InspectorOverlay : LeafRenderObjectWidget {
        public _InspectorOverlay(Key key, InspectorSelection selection) : base(key) {
            this.selection = selection;
        }

        public readonly InspectorSelection selection;

        public override RenderObject createRenderObject(BuildContext context) {
            return new _RenderInspectorOverlay(selection);
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject) {
            ((_RenderInspectorOverlay) renderObject).selection = selection;
        }
    }

    class _RenderInspectorOverlay : RenderBox {
        public _RenderInspectorOverlay(InspectorSelection selection) {
            D.assert(selection != null);
            _selection = selection;
        }

        InspectorSelection _selection;

        public InspectorSelection selection {
            get { return _selection; }
            set {
                if (value != _selection) {
                    _selection = value;
                    markNeedsPaint();
                }
            }
        }

        protected override bool sizedByParent {
            get { return true; }
        }

        protected override bool alwaysNeedsCompositing {
            get { return true; }
        }

        protected override void performResize() {
            size = constraints.constrain(new Size(float.PositiveInfinity, float.PositiveInfinity));
        }

        public override void paint(PaintingContext context, Offset offset) {
            D.assert(needsCompositing);
            context.addLayer(new _InspectorOverlayLayer(
                Rect.fromLTWH(offset.dx, offset.dy, size.width, size.height), selection
            ));
        }
    }

    class _TransformedRect : IEquatable<_TransformedRect> {
        public readonly Rect rect;
        public readonly Matrix4 transform;

        public _TransformedRect(RenderObject obj) {
            rect = obj.semanticBounds;
            transform = obj.getTransformTo(null);
        }

        public bool Equals(_TransformedRect other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(rect, other.rect) && transform.Equals(other.transform);
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

            return Equals((_TransformedRect) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((rect != null ? rect.GetHashCode() : 0) * 397) ^ transform.GetHashCode();
            }
        }

        public static bool operator ==(_TransformedRect left, _TransformedRect right) {
            return Equals(left, right);
        }

        public static bool operator !=(_TransformedRect left, _TransformedRect right) {
            return !Equals(left, right);
        }
    }

    class _InspectorOverlayRenderState : IEquatable<_InspectorOverlayRenderState> {
        public readonly Rect overlayRect;
        public readonly _TransformedRect selected;
        public readonly List<_TransformedRect> candidates;
        public readonly string tooltip;
        public readonly TextDirection textDirection;

        public _InspectorOverlayRenderState(Rect overlayRect, _TransformedRect selected,
            List<_TransformedRect> candidates, string tooltip, TextDirection textDirection) {
            this.overlayRect = overlayRect;
            this.selected = selected;
            this.candidates = candidates;
            this.tooltip = tooltip;
            this.textDirection = textDirection;
        }

        public bool Equals(_InspectorOverlayRenderState other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(overlayRect, other.overlayRect) && Equals(selected, other.selected)
                                                               && string.Equals(tooltip, other.tooltip) &&
                                                               textDirection == other.textDirection
                                                               && (candidates == other.candidates ||
                                                                   (candidates != null &&
                                                                    candidates.SequenceEqual(
                                                                        other.candidates)));
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

            return Equals((_InspectorOverlayRenderState) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (overlayRect != null ? overlayRect.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (selected != null ? selected.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (candidates != null
                               ? candidates.Aggregate(0,
                                   (hash, rect) => (hash * 397) ^ (rect == null ? 0 : rect.GetHashCode()))
                               : 0);
                hashCode = (hashCode * 397) ^ (tooltip != null ? tooltip.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) textDirection;
                return hashCode;
            }
        }

        public static bool operator ==(_InspectorOverlayRenderState left, _InspectorOverlayRenderState right) {
            return Equals(left, right);
        }

        public static bool operator !=(_InspectorOverlayRenderState left, _InspectorOverlayRenderState right) {
            return !Equals(left, right);
        }
    }


    class _InspectorOverlayLayer : Layer {

        public _InspectorOverlayLayer(Rect overlayRect, InspectorSelection selection) {
            D.assert(overlayRect != null);
            D.assert(selection != null);
            this.overlayRect = overlayRect;
            this.selection = selection;
            bool inDebugMode = false;
            D.assert(()=> {
                inDebugMode = true;
                return true;
            });
            if (inDebugMode == false) {
                throw new UIWidgetsError(
                    "The inspector should never be used in production mode due to the " + 
                "negative performance impact."
                    );
            }
        }

        public InspectorSelection selection;
        public readonly Rect overlayRect;
        _InspectorOverlayRenderState _lastState;

        public Picture _picture;
        public  TextPainter _textPainter;
        public float _textPainterMaxWidth;


        public override void addToScene(ui.SceneBuilder builder,  Offset layerOffset = null) {
            layerOffset = layerOffset ?? Offset.zero;
            if (!selection.active)
                return;

            RenderObject selected = selection.current;
            List<_TransformedRect> candidates = new List<_TransformedRect>();
            foreach ( RenderObject candidate in selection.candidates) {
                if (candidate == selected || !candidate.attached)
                    continue;
                candidates.Add(new _TransformedRect(candidate));
            }

            _InspectorOverlayRenderState state = new _InspectorOverlayRenderState(
                overlayRect: overlayRect,
                selected: new _TransformedRect(selected),
                tooltip: selection.currentElement.toStringShort(),
                textDirection: TextDirection.ltr,
                candidates: candidates
            );

            if (state != _lastState) {
                _lastState = state;
                _picture = _buildPicture(state);
            }
            builder.addPicture(layerOffset, _picture);
        }


        Picture _buildPicture(_InspectorOverlayRenderState state) {
            PictureRecorder recorder = new PictureRecorder();
            Canvas canvas = new Canvas(recorder, state.overlayRect);
            Size size = state.overlayRect.size;

            var fillPaint = new Paint() {style = PaintingStyle.fill,color = WidgetInspectorUtils._kHighlightedRenderObjectFillColor};
            var borderPaint = new Paint() {
                color = WidgetInspectorUtils._kHighlightedRenderObjectBorderColor, 
                style = PaintingStyle.stroke,
                strokeWidth = 1
            };
            Rect selectedPaintRect = state.selected.rect.deflate(0.5f);
            canvas.save();
            canvas.transform(state.selected.transform._m4storage);
            canvas.drawRect(selectedPaintRect, fillPaint);
            canvas.drawRect(selectedPaintRect, borderPaint);
            canvas.restore();

            foreach (var transformedRect in state.candidates) {
                canvas.save();
                canvas.transform(transformedRect.transform._m4storage);
                canvas.drawRect(transformedRect.rect.deflate(0.5f), borderPaint);
                canvas.restore();
            }

            // todo paint descipion
            Rect targetRect = MatrixUtils.transformRect(
                state.selected.transform, state.selected.rect);
            Offset target = new Offset(targetRect.left, targetRect.center.dy); 
            float offsetFromWidget = 9.0f;
            float verticalOffset = (targetRect.height) / 2 + offsetFromWidget;

            _paintDescription(canvas, state.tooltip, state.textDirection, target, verticalOffset, size, targetRect);

            // TODO(jacobr): provide an option to perform a debug paint of just the
            // selected widget.
            return recorder.endRecording();
        }
        public void _paintDescription(
            Canvas canvas,
            String message,
            TextDirection textDirection,
            Offset target,
            float verticalOffset,
            Size size,
            Rect targetRect
        ) {
            canvas.save();
            float maxWidth = size.width - 2 * (WidgetInspectorUtils._kScreenEdgeMargin + WidgetInspectorUtils._kTooltipPadding);
            TextSpan textSpan = _textPainter?.text as TextSpan;
            if (_textPainter == null || textSpan.text != message || _textPainterMaxWidth != maxWidth) {
                _textPainterMaxWidth = maxWidth;
                _textPainter = new TextPainter();
                _textPainter.maxLines = WidgetInspectorUtils._kMaxTooltipLines;
                _textPainter.ellipsis = "...";
                _textPainter.text = new TextSpan(style: WidgetInspectorUtils._messageStyle, text: message);
                _textPainter.textDirection = textDirection;
                _textPainter.layout(maxWidth: maxWidth);
            }

            Size tooltipSize = _textPainter.size + new Offset(WidgetInspectorUtils._kTooltipPadding * 2, WidgetInspectorUtils._kTooltipPadding * 2);
            Offset tipOffset = Geometry.positionDependentBox(
                size: size,
                childSize: tooltipSize,
                target: target,
                verticalOffset: verticalOffset,
                preferBelow: false
            );

            Paint tooltipBackground = new Paint();
            tooltipBackground.style = PaintingStyle.fill;
            tooltipBackground.color = WidgetInspectorUtils._kTooltipBackgroundColor;
            canvas.drawRect(
            Rect.fromPoints(
            tipOffset,
            tipOffset.translate(tooltipSize.width, tooltipSize.height)
            ),
            tooltipBackground
            );

            float wedgeY = tipOffset.dy;
            bool tooltipBelow = tipOffset.dy > target.dy;
            if (!tooltipBelow)
                wedgeY += tooltipSize.height;
            float wedgeSize = WidgetInspectorUtils._kTooltipPadding * 2;
            float wedgeX = Mathf.Max(tipOffset.dx, target.dx) + wedgeSize * 2;
            wedgeX = Mathf.Min(wedgeX, tipOffset.dx + tooltipSize.width - wedgeSize * 2);
            List<Offset> wedge = new List<Offset>(){
                new Offset(wedgeX - wedgeSize, wedgeY),
                new Offset(wedgeX + wedgeSize, wedgeY),
                new Offset(wedgeX, wedgeY + (tooltipBelow ? -wedgeSize : wedgeSize)),
            };
            Path path = new Path();
            path.addPolygon(wedge,true);
            canvas.drawPath(path, tooltipBackground);
            _textPainter.paint(canvas, tipOffset + new  Offset(WidgetInspectorUtils._kTooltipPadding, WidgetInspectorUtils._kTooltipPadding));
            canvas.restore();
        }


        public override bool findAnnotations<S>(
            AnnotationResult<S> result,
            Offset localPosition, 
            bool onlyFirst) {
            return false;
        }
    }
}