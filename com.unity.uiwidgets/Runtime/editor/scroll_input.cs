using UnityEngine;

namespace Unity.UIWidgets.editor {
    public class ScrollInput {
        readonly float _bufferSize = 20.0f / 60;            // a scroll action leads to 20 frames, i.e., ( 20 / 60 ) seconds smoothly-scrolling when fps = 60 (default)
        readonly float _scrollScale = 20;

        float _scrollDeltaX;
        float _scrollDeltaY;

        float _bufferIndex;
        float _curDeltaX;
        float _curDeltaY;

        float _pointerX;
        float _pointerY;
        int _buttonId;

        public ScrollInput(float? bufferSize = null, float? scrollScale = null) {
            _bufferSize = bufferSize ?? _bufferSize;
            _scrollScale = scrollScale ?? _scrollScale;

            _bufferIndex = _bufferSize;
            _scrollDeltaX = 0;
            _scrollDeltaY = 0;
            _curDeltaX = 0;
            _curDeltaY = 0;
        }

        public void onScroll(float deltaX, float deltaY, float pointerX, float pointerY, int buttonId) {
            _scrollDeltaX += deltaX * _scrollScale;
            _scrollDeltaY += deltaY * _scrollScale;
            _bufferIndex = _bufferSize;
            _curDeltaX = _scrollDeltaX / _bufferIndex;
            _curDeltaY = _scrollDeltaY / _bufferIndex;

            _pointerX = pointerX;
            _pointerY = pointerY;
            _buttonId = buttonId;
        }

        public int getDeviceId() {
            return _buttonId;
        }

        public float getPointerPosX() {
            return _pointerX;
        }

        public float getPointerPosY() {
            return _pointerY;
        }

        public Vector2 getScrollDelta(float deltaTime) {
            if (_scrollDeltaX == 0 && _scrollDeltaY == 0) {
                return Vector2.zero;
            }

            var deltaScroll = new Vector2();
            if (_bufferIndex <= deltaTime) {
                deltaScroll.x = _scrollDeltaX;
                deltaScroll.y = _scrollDeltaY;
                _scrollDeltaX = 0;
                _scrollDeltaY = 0;
            }
            else {
                deltaScroll.x = _curDeltaX * deltaTime;
                deltaScroll.y = _curDeltaY * deltaTime;
                _scrollDeltaX = _curDeltaX > 0
                    ? Mathf.Max(0, _scrollDeltaX - _curDeltaX * deltaTime)
                    : Mathf.Min(0, _scrollDeltaX - _curDeltaX * deltaTime);
                _scrollDeltaY = _curDeltaY > 0
                    ? Mathf.Max(0, _scrollDeltaY - _curDeltaY * deltaTime)
                    : Mathf.Min(0, _scrollDeltaY - _curDeltaY * deltaTime);
                _bufferIndex -= deltaTime;
            }
            
            return deltaScroll;
        }
    }
}