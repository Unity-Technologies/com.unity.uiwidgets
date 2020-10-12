using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.external;

namespace Unity.UIWidgets.uiOld{
    public class uiPicture : PoolObject {
        public uiPicture() {
        }

        public static uiPicture create(List<uiDrawCmd> drawCmds,
            uiRect paintBounds,
            BBoxHierarchy<IndexedRect> bbh = null,
            uiList<int> stateUpdateIndices = null) {
            var picture = ObjectPool<uiPicture>.alloc();
            picture.drawCmds = drawCmds;
            picture.paintBounds = paintBounds;
            picture.bbh = bbh;
            picture.stateUpdatesIndices = stateUpdateIndices;
            return picture;
        }

        public List<uiDrawCmd> drawCmds;
        public uiRect paintBounds;
        public BBoxHierarchy<IndexedRect> bbh; 
        public uiList<int> stateUpdatesIndices; 

        public override void clear() {
            //the recorder will dispose the draw commands
            drawCmds = null;
            ObjectPool<uiList<int>>.release(stateUpdatesIndices);
        }
    }

    public class uiPictureRecorder {
        readonly List<uiDrawCmd> _drawCmds = new List<uiDrawCmd>(128);

        readonly List<uiCanvasState> _states = new List<uiCanvasState>(32);
        
        readonly BBoxHierarchy<IndexedRect> _bbh = new RTree<IndexedRect>();

        readonly List<int> _stateUpdateIndices = new List<int>();

        public uiPictureRecorder() {
            reset();
        }

        uiCanvasState _getState() {
            D.assert(_states.Count > 0);
            return _states[_states.Count - 1];
        }

        void _setState(uiCanvasState state) {
            _states[_states.Count - 1] = state;
        }

        public uiMatrix3 getTotalMatrix() {
            return _getState().xform;
        }

        public void reset() {
            foreach (var drawCmd in _drawCmds) {
                drawCmd.release();
            }

            _drawCmds.Clear();
            _states.Clear();
            _states.Add(new uiCanvasState {
                xform = uiMatrix3.I(),
                scissor = null,
                saveLayer = false,
                layerOffset = null,
                paintBounds = uiRectHelper.zero
            });
            _bbh.Clear();
            _stateUpdateIndices.Clear();
        }

        public uiPicture endRecording() {
            if (_states.Count > 1) {
                throw new Exception("unmatched save/restore commands");
            }
            
            uiList<int> stateUpdateIndices = ObjectPool<uiList<int>>.alloc();
            stateUpdateIndices.AddRange(_stateUpdateIndices);
            var state = _getState();
            return uiPicture.create(
                _drawCmds,
                state.paintBounds,
                bbh: _bbh,
                stateUpdateIndices: stateUpdateIndices);
        }

        public void addDrawCmd(uiDrawCmd drawCmd) {
            _drawCmds.Add(drawCmd);

            switch (drawCmd) {
                case uiDrawSave _:
                    _states.Add(_getState().copy());
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                case uiDrawSaveLayer cmd: {
                    _states.Add(new uiCanvasState {
                        xform = uiMatrix3.I(),
                        scissor = cmd.rect.Value.shift(-cmd.rect.Value.topLeft),
                        saveLayer = true,
                        layerOffset = cmd.rect.Value.topLeft,
                        paintBounds = uiRectHelper.zero
                    });
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawRestore _: {
                    var stateToRestore = _getState();
                    _states.RemoveAt(_states.Count - 1);
                    var state = _getState();

                    if (!stateToRestore.saveLayer) {
                        state.paintBounds = stateToRestore.paintBounds;
                    }
                    else {
                        var paintBounds = stateToRestore.paintBounds.shift(stateToRestore.layerOffset.Value);
                        paintBounds = state.xform.mapRect(paintBounds);
                        _addPaintBounds(paintBounds);
                    }

                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawTranslate cmd: {
                    var state = _getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preTranslate(cmd.dx, cmd.dy);
                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawScale cmd: {
                    var state = _getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preScale(cmd.sx, (cmd.sy ?? cmd.sx));
                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawRotate cmd: {
                    var state = _getState();
                    state.xform = new uiMatrix3(state.xform);
                    if (cmd.offset == null) {
                        state.xform.preRotate(cmd.radians);
                    }
                    else {
                        state.xform.preRotate(cmd.radians,
                            cmd.offset.Value.dx,
                            cmd.offset.Value.dy);
                    }

                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawSkew cmd: {
                    var state = _getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preSkew(cmd.sx, cmd.sy);
                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawConcat cmd: {
                    var state = _getState();
                    state.xform = new uiMatrix3(state.xform);
                    state.xform.preConcat(cmd.matrix.Value);
                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawResetMatrix _: {
                    var state = _getState();
                    state.xform = uiMatrix3.I();
                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawSetMatrix cmd: {
                    var state = _getState();
                    state.xform = new uiMatrix3(cmd.matrix.Value);
                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawClipRect cmd: {
                    var state = _getState();

                    var rect = state.xform.mapRect(cmd.rect.Value);
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawClipRRect cmd: {
                    var state = _getState();

                    var rect = state.xform.mapRect(uiRectHelper.fromRect(cmd.rrect.outerRect).Value);
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    _setState(state);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawClipPath cmd: {
                    var state = _getState();
                    var scale = uiXformUtils.getScale(state.xform);

                    var rectPathCache = cmd.path.flatten(
                        scale * Window.instance.devicePixelRatio);
                    rectPathCache.computeFillMesh(0.0f, out _);
                    var rectMesh = rectPathCache.fillMesh;
                    var transformedMesh = rectMesh.transform(state.xform);
                    var rect = transformedMesh.bounds;
                    state.scissor = state.scissor == null ? rect : state.scissor.Value.intersect(rect);
                    _setState(state);
                    ObjectPool<uiMeshMesh>.release(transformedMesh);
                    _stateUpdateIndices.Add(_drawCmds.Count - 1);
                    break;
                }
                case uiDrawPath cmd: {
                    var state = _getState();
                    var scale = uiXformUtils.getScale(state.xform);
                    var path = cmd.path;
                    var paint = cmd.paint;
                    var devicePixelRatio = Window.instance.devicePixelRatio;

                    uiMeshMesh mesh;
                    if (paint.style == PaintingStyle.fill) {
                        var cache = path.flatten(scale * devicePixelRatio);
                        cache.computeFillMesh(0.0f, out _);
                        var fillMesh = cache.fillMesh;
                        mesh = fillMesh.transform(state.xform);
                    }
                    else {
                        float strokeWidth = (paint.strokeWidth * scale).clamp(0, 200.0f);
                        float fringeWidth = 1 / devicePixelRatio;

                        if (strokeWidth < fringeWidth) {
                            strokeWidth = fringeWidth;
                        }

                        var cache = path.flatten(scale * devicePixelRatio);
                        cache.computeStrokeMesh(
                            strokeWidth / scale * 0.5f,
                            0.0f,
                            paint.strokeCap,
                            paint.strokeJoin,
                            paint.strokeMiterLimit);
                        var strokenMesh = cache.strokeMesh;

                        mesh = strokenMesh.transform(state.xform);
                    }

                    if (paint.maskFilter != null && paint.maskFilter.Value.sigma != 0) {
                        float sigma = scale * paint.maskFilter.Value.sigma;
                        float sigma3 = 3 * sigma;
                        _addPaintBounds(uiRectHelper.inflate(mesh.bounds, sigma3));
                        _bbh.Insert(new IndexedRect(uiRectHelper.inflate(mesh.bounds, sigma3 + 5),
                            _drawCmds.Count - 1));
                    }
                    else {
                        _addPaintBounds(mesh.bounds);
                        _bbh.Insert(new IndexedRect(uiRectHelper.inflate(mesh.bounds, 5),
                            _drawCmds.Count - 1));
                    }

                    ObjectPool<uiMeshMesh>.release(mesh);
                    break;
                }
                case uiDrawImage cmd: {
                    var state = _getState();
                    var rect = uiRectHelper.fromLTWH(cmd.offset.Value.dx, cmd.offset.Value.dy,
                        cmd.image.width, cmd.image.height);
                    rect = state.xform.mapRect(rect);
                    _addPaintBounds(rect);
                    _bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        _drawCmds.Count - 1));
                    break;
                }
                case uiDrawImageRect cmd: {
                    var state = _getState();
                    var rect = state.xform.mapRect(cmd.dst.Value);
                    _addPaintBounds(rect);
                    _bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        _drawCmds.Count - 1));
                    break;
                }
                case uiDrawImageNine cmd: {
                    var state = _getState();
                    var rect = state.xform.mapRect(cmd.dst.Value);
                    _addPaintBounds(rect);
                    _bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        _drawCmds.Count - 1));
                    break;
                }
                case uiDrawPicture cmd: {
                    var state = _getState();
                    var rect = state.xform.mapRect(uiRectHelper.fromRect(cmd.picture.paintBounds).Value);
                    _addPaintBounds(rect);
                    _bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        _drawCmds.Count - 1));
                    break;
                }
                case uiDrawTextBlob cmd: {
                    var state = _getState();
                    var scale = uiXformUtils.getScale(state.xform);
                    var rect = uiRectHelper.fromRect(
                        cmd.textBlob.Value.shiftedBoundsInText(cmd.offset.Value.dx, cmd.offset.Value.dy)).Value;
                    rect = state.xform.mapRect(rect);
                    _bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                        _drawCmds.Count - 1));

                    var paint = cmd.paint;
                    if (paint.maskFilter != null && paint.maskFilter.Value.sigma != 0) {
                        float sigma = scale * paint.maskFilter.Value.sigma;
                        float sigma3 = 3 * sigma;
                        _addPaintBounds(uiRectHelper.inflate(rect, sigma3));
                        _bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, sigma3 + 5),
                            _drawCmds.Count - 1));
                    }
                    else {
                        _addPaintBounds(rect);
                        _bbh.Insert(new IndexedRect(uiRectHelper.inflate(rect, 5),
                            _drawCmds.Count - 1));
                    }

                    break;
                }
                default:
                    throw new Exception("unknown drawCmd: " + drawCmd);
            }
        }

        void _addPaintBounds(uiRect? paintBounds) {
            var state = _getState();
            if (state.scissor != null) {
                paintBounds = paintBounds.Value.intersect(state.scissor.Value);
            }

            if (paintBounds == null || paintBounds.Value.isEmpty) {
                return;
            }

            if (state.paintBounds.isEmpty) {
                state.paintBounds = paintBounds.Value;
            }
            else {
                state.paintBounds = state.paintBounds.expandToInclude(paintBounds.Value);
            }

            _setState(state);
        }

        struct uiCanvasState {
            public uiMatrix3 xform;
            public uiRect? scissor;
            public bool saveLayer;
            public uiOffset? layerOffset;
            public uiRect paintBounds;

            public uiCanvasState copy() {
                return new uiCanvasState {
                    xform = xform,
                    scissor = scissor,
                    saveLayer = false,
                    layerOffset = null,
                    paintBounds = paintBounds
                };
            }
        }
    }
}