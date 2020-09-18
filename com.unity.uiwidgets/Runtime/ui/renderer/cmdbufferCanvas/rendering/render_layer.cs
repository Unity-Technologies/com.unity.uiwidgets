using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace Unity.UIWidgets.ui {
    public partial class PictureFlusher {
        internal class RenderLayer : PoolObject {
            public int rtID;
            public int width;
            public int height;
            public FilterMode filterMode = FilterMode.Bilinear;
            public bool noMSAA = false;
            public uiRect layerBounds;
            public uiPaint? layerPaint;
            public readonly List<RenderCmd> draws = new List<RenderCmd>(128);
            public readonly List<RenderLayer> layers = new List<RenderLayer>(16);
            public readonly List<State> states = new List<State>(16);
            public State currentState;
            public ClipStack clipStack;
            public uint lastClipGenId;
            public uiRect lastClipBounds;
            public bool ignoreClip = true;

            Vector4? _viewport;

            public Vector4 viewport {
                get {
                    if (!_viewport.HasValue) {
                        _viewport = new Vector4(
                            layerBounds.left,
                            layerBounds.top,
                            layerBounds.width,
                            layerBounds.height);
                    }

                    return _viewport.Value;
                }
            }

            public static RenderLayer create(int rtID = 0, int width = 0, int height = 0,
                FilterMode filterMode = FilterMode.Bilinear,
                bool noMSAA = false, 
                uiRect? layerBounds = null, uiPaint? layerPaint = null, bool ignoreClip = true) {
                D.assert(layerBounds != null);
                var newLayer = ObjectPool<RenderLayer>.alloc();
                newLayer.rtID = rtID;
                newLayer.width = width;
                newLayer.height = height;
                newLayer.filterMode = filterMode;
                newLayer.noMSAA = noMSAA;
                newLayer.layerBounds = layerBounds.Value;
                newLayer.layerPaint = layerPaint;
                newLayer.ignoreClip = ignoreClip;
                newLayer.currentState = State.create();
                newLayer.states.Add(newLayer.currentState);
                newLayer.clipStack = ClipStack.create();

                return newLayer;
            }

            public void addLayer(RenderLayer layer) {
                layers.Add(layer);
                draws.Add(CmdLayer.create(layer: layer));
            }

            public override void clear() {
                //these two list should have been cleared in PictureFlusher._clearLayer
                D.assert(draws.Count == 0);
                D.assert(layers.Count == 0);
                draws.Clear();
                layers.Clear();

                foreach (var state in states) {
                    ObjectPool<State>.release(state);
                }

                states.Clear();
                ObjectPool<ClipStack>.release(clipStack);
                _viewport = null;
            }
        }

        internal class State : PoolObject {
            public State() {
            }

            static readonly uiMatrix3 _id = uiMatrix3.I();

            uiMatrix3? _matrix;
            float? _scale;
            uiMatrix3? _invMatrix;

            public static State create(uiMatrix3? matrix = null, float? scale = null, uiMatrix3? invMatrix = null) {
                State newState = ObjectPool<State>.alloc();
                newState._matrix = matrix ?? _id;
                newState._scale = scale;
                newState._invMatrix = invMatrix;

                return newState;
            }

            public override void clear() {
                _matrix = null;
                _scale = null;
                _invMatrix = null;
            }

            public uiMatrix3? matrix {
                get { return _matrix; }
                set {
                    _matrix = value ?? _id;
                    _scale = null;
                    _invMatrix = null;
                }
            }

            public float scale {
                get {
                    if (_scale == null) {
                        _scale = uiXformUtils.getScale(_matrix.Value);
                    }

                    return _scale.Value;
                }
            }

            public uiMatrix3 invMatrix {
                get {
                    if (_invMatrix == null) {
                        _invMatrix = _matrix.Value.invert();
                    }

                    return _invMatrix.Value;
                }
            }

            public State copy() {
                return create(_matrix, _scale, _invMatrix);
            }
        }
    }
}