using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Rect = Unity.UIWidgets.ui.Rect;

namespace Unity.UIWidgets.painting {
    public enum ImageRepeat {
        repeat,
        repeatX,
        repeatY,
        noRepeat,
    }

    public class DecorationImage : IEquatable<DecorationImage> {
        public DecorationImage(
            ImageProvider image = null,
            ColorFilter colorFilter = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            Rect centerSlice = null,
            ImageRepeat repeat = ImageRepeat.noRepeat
        ) {
            D.assert(image != null);
            this.image = image;
            this.colorFilter = colorFilter;
            this.fit = fit;
            this.alignment = alignment ?? Alignment.center;
            this.centerSlice = centerSlice;
            this.repeat = repeat;
        }

        public readonly ImageProvider image;
        public readonly ColorFilter colorFilter;
        public readonly BoxFit? fit;
        public readonly Alignment alignment;
        public readonly Rect centerSlice;
        public readonly ImageRepeat repeat;

        public DecorationImagePainter createPainter(VoidCallback onChanged) {
            D.assert(onChanged != null);
            return new DecorationImagePainter(this, onChanged);
        }

        public bool Equals(DecorationImage other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }

            if (ReferenceEquals(this, other)) {
                return true;
            }

            return Equals(image, other.image) && Equals(colorFilter, other.colorFilter) &&
                   fit == other.fit && Equals(alignment, other.alignment) &&
                   Equals(centerSlice, other.centerSlice) && repeat == other.repeat;
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

            return Equals((DecorationImage) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (image != null ? image.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (colorFilter != null ? colorFilter.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ fit.GetHashCode();
                hashCode = (hashCode * 397) ^ (alignment != null ? alignment.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (centerSlice != null ? centerSlice.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) repeat;
                return hashCode;
            }
        }

        public static bool operator ==(DecorationImage left, DecorationImage right) {
            return Equals(left, right);
        }

        public static bool operator !=(DecorationImage left, DecorationImage right) {
            return !Equals(left, right);
        }

        public override string ToString() {
            var properties = new List<string>();
            properties.Add($"{image}");

            if (colorFilter != null) {
                properties.Add($"{colorFilter}");
            }

            if (fit != null &&
                !(fit == BoxFit.fill && centerSlice != null) &&
                !(fit == BoxFit.scaleDown && centerSlice == null)) {
                properties.Add($"{fit}");
            }

            properties.Add($"{alignment}");

            if (centerSlice != null) {
                properties.Add($"centerSlice: {centerSlice}");
            }

            if (repeat != ImageRepeat.noRepeat) {
                properties.Add($"{repeat}");
            }

            return $"{GetType()}({string.Join(", ", properties)})";
        }
    }

    public class DecorationImagePainter : IDisposable {
        internal DecorationImagePainter(DecorationImage details, VoidCallback onChanged) {
            D.assert(details != null);
            _details = details;
            _onChanged = onChanged;
        }

        readonly DecorationImage _details;

        readonly VoidCallback _onChanged;

        ImageStream _imageStream;

        ImageInfo _image;

        public void paint(Canvas canvas, Rect rect, Path clipPath, ImageConfiguration configuration) {
            D.assert(canvas != null);
            D.assert(rect != null);
            D.assert(configuration != null);

            ImageStream newImageStream = _details.image.resolve(configuration);
            if (newImageStream.key != _imageStream?.key) {
                _imageStream?.removeListener(_imageListener);
                _imageStream = newImageStream;
                _imageStream.addListener(_imageListener);
            }

            if (_image == null) {
                return;
            }

            if (clipPath != null) {
                canvas.save();
                canvas.clipPath(clipPath);
            }

            ImageUtils.paintImage(
                canvas: canvas,
                rect: rect,
                image: _image.image,
                scale: _image.scale,
                colorFilter: _details.colorFilter,
                fit: _details.fit,
                alignment: _details.alignment,
                centerSlice: _details.centerSlice,
                repeat: _details.repeat
            );

            if (clipPath != null) {
                canvas.restore();
            }
        }

        void _imageListener(ImageInfo value, bool synchronousCall) {
            if (_image == value) {
                return;
            }

            _image = value;

            D.assert(_onChanged != null);
            if (!synchronousCall) {
                _onChanged();
            }
        }

        public void Dispose() {
            _imageStream?.removeListener(_imageListener);
        }

        public override string ToString() {
            return $"{GetType()}(stream: {_imageStream}, image: {_image}) for {_details}";
        }
    }

    public static class ImageUtils {
        public static void paintImage(
            Canvas canvas = null,
            Rect rect = null,
            Image image = null,
            float scale = 1.0f,
            ColorFilter colorFilter = null,
            BoxFit? fit = null,
            Alignment alignment = null,
            Rect centerSlice = null,
            ImageRepeat repeat = ImageRepeat.noRepeat,
            bool invertColors = false,
            FilterMode filterMode = FilterMode.Bilinear
        ) {
            D.assert(canvas != null);
            D.assert(rect != null);
            D.assert(image != null);
            alignment = alignment ?? Alignment.center;

            if (rect.isEmpty) {
                return;
            }

            Size outputSize = rect.size;
            Size inputSize = new Size(image.width, image.height);
            Offset sliceBorder = null;
            if (centerSlice != null) {
                sliceBorder = new Offset(
                    centerSlice.left + inputSize.width - centerSlice.right,
                    centerSlice.top + inputSize.height - centerSlice.bottom
                );
                outputSize -= sliceBorder;
                inputSize -= sliceBorder;
            }

            fit = fit ?? (centerSlice == null ? BoxFit.scaleDown : BoxFit.fill);
            D.assert(centerSlice == null || (fit != BoxFit.none && fit != BoxFit.cover),
                () => $"centerSlice was used with a BoxFit {fit} that is not supported.");
            FittedSizes fittedSizes = FittedSizes.applyBoxFit(fit.Value, inputSize / scale, outputSize);
            Size sourceSize = fittedSizes.source * scale;
            Size destinationSize = fittedSizes.destination;
            if (centerSlice != null) {
                outputSize += sliceBorder;
                destinationSize += sliceBorder;
                D.assert(sourceSize == inputSize,
                    () =>
                        $"centerSlice was used with a BoxFit {fit} that does not guarantee that the image is fully visible.");
            }

            if (repeat != ImageRepeat.noRepeat && destinationSize == outputSize) {
                repeat = ImageRepeat.noRepeat;
            }

            Paint paint = new Paint();
            if (colorFilter != null) {
                paint.colorFilter = colorFilter;
                paint.color = colorFilter.color;
                paint.blendMode = colorFilter.blendMode;
            }

            if (sourceSize != destinationSize) {
                paint.filterMode = filterMode;
            }

            paint.invertColors = invertColors;

            float halfWidthDelta = (outputSize.width - destinationSize.width) / 2.0f;
            float halfHeightDelta = (outputSize.height - destinationSize.height) / 2.0f;
            float dx = halfWidthDelta + alignment.x * halfWidthDelta;
            float dy = halfHeightDelta + alignment.y * halfHeightDelta;
            Offset destinationPosition = rect.topLeft.translate(dx, dy);
            Rect destinationRect = destinationPosition & destinationSize;
            bool needSave = repeat != ImageRepeat.noRepeat;
            if (needSave) {
                canvas.save();
            }

            if (repeat != ImageRepeat.noRepeat) {
                canvas.clipRect(rect);
            }

            if (centerSlice == null) {
                Rect sourceRect = alignment.inscribe(
                    sourceSize, Offset.zero & inputSize
                );
                if (repeat == ImageRepeat.noRepeat) {
                    canvas.drawImageRect(image, sourceRect, destinationRect, paint);
                }
                else {
                    foreach (Rect tileRect in _generateImageTileRects(rect, destinationRect, repeat)) {
                        canvas.drawImageRect(image, sourceRect, tileRect, paint);
                    }
                }
            }
            else {
                if (repeat == ImageRepeat.noRepeat) {
                    canvas.drawImageNine(image, centerSlice, destinationRect, paint);
                }
                else {
                    foreach (Rect tileRect in _generateImageTileRects(rect, destinationRect, repeat)) {
                        canvas.drawImageNine(image, centerSlice, tileRect, paint);
                    }
                }
            }

            if (needSave) {
                canvas.restore();
            }
        }

        static IEnumerable<Rect> _generateImageTileRects(Rect outputRect, Rect fundamentalRect,
            ImageRepeat repeat) {
            int startX = 0;
            int startY = 0;
            int stopX = 0;
            int stopY = 0;
            float strideX = fundamentalRect.width;
            float strideY = fundamentalRect.height;

            if (repeat == ImageRepeat.repeat || repeat == ImageRepeat.repeatX) {
                startX = ((outputRect.left - fundamentalRect.left) / strideX).floor();
                stopX = ((outputRect.right - fundamentalRect.right) / strideX).ceil();
            }

            if (repeat == ImageRepeat.repeat || repeat == ImageRepeat.repeatY) {
                startY = ((outputRect.top - fundamentalRect.top) / strideY).floor();
                stopY = ((outputRect.bottom - fundamentalRect.bottom) / strideY).ceil();
            }

            for (int i = startX; i <= stopX; ++i) {
                for (int j = startY; j <= stopY; ++j) {
                    yield return fundamentalRect.shift(new Offset(i * strideX, j * strideY));
                }
            }
        }
    }
}