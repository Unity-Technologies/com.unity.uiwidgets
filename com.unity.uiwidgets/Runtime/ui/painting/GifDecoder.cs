using System;
using System.IO;
using System.Text;

namespace Unity.UIWidgets.ui {
    // from https://github.com/avianbc/NGif/blob/master/Components/GifDecoder.cs
    // https://gist.github.com/devunwired/4479231
    // No DISPOSAL_PREVIOUS as its not actually widely used.
    public class GifDecoder : IDisposable {
        /**
         * File read status: No errors.
         */
        public const int STATUS_OK = 0;

        /**
         * File read status: Error decoding file (may be partially decoded)
         */
        public const int STATUS_FORMAT_ERROR = 1;

        /**
         * File read status: Unable to open source.
         */
        public const int STATUS_OPEN_ERROR = 2;

        // max decoder pixel stack size
        const int MAX_STACK_SIZE = 4096;

        // input stream
        Stream _inStream;

        /**
        * Global status code of GIF data parsing
        */
        int _status;

        // Global File Header values and parsing flags
        volatile int _width; // full image width
        volatile int _height; // full image height
        bool _gctFlag; // global color table used
        int _gctSize; // size of global color table
        volatile int _loopCount = 1; // iterations; 0 = repeat forever

        int[] _gct; // global color table
        int[] _lct; // local color table
        int[] _act; // active color table

        int _bgIndex; // background color index
        int _bgColor; // background color
        int _lastBgColor; // previous bg color
        int _pixelAspect; // pixel aspect ratio

        bool _lctFlag; // local color table flag
        bool _interlace; // interlace flag
        int _lctSize; // local color table size

        int _ix, _iy, _iw, _ih; // current image rectangle
        int _lix, _liy, _liw, _lih; // last image rect
        int[] _image; // current frame

        byte[] _block = new byte[256]; // current data block
        int _blockSize = 0; // block size

        // last graphic control extension info
        int _dispose = 0;

        // 0=no action; 1=leave in place; 2=restore to bg; 3=restore to prev
        int _lastDispose = 0;
        bool _transparency = false; // use transparent color
        int _delay = 0; // delay in milliseconds
        int _transIndex; // transparent color index

        // LZW decoder working arrays
        short[] _prefix;
        byte[] _suffix;
        byte[] _pixelStack;
        byte[] _pixels;

        volatile GifFrame _currentFrame; // frames read from current file
        volatile int _frameCount;
        volatile bool _done;

        public class GifFrame {
            public byte[] bytes;
            public int delay;
        }

        public int frameWidth {
            get { return _width; }
        }

        public int frameHeight {
            get { return _height; }
        }

        public GifFrame currentFrame {
            get { return _currentFrame; }
        }

        public int frameCount {
            get { return _frameCount; }
        }

        public int loopCount {
            get { return _loopCount; }
        }

        public bool done {
            get { return _done; }
        }

        void _setPixels() {
            // fill in starting image contents based on last image's dispose code
            if (_lastDispose > 0) {
                var n = _frameCount - 1;
                if (n > 0) {
                    if (_lastDispose == 2) {
                        // fill last image rect area with background color 
                        var fillcolor = _transparency ? 0 : _lastBgColor;
                        for (var i = 0; i < _lih; i++) {
                            var line = i + _liy;
                            if (line >= _height) {
                                continue;
                            }

                            line = _height - line - 1;
                            var dx = line * _width + _lix;
                            var endx = dx + _liw;
                            while (dx < endx) {
                                _image[dx++] = fillcolor;
                            }
                        }
                    }
                }
            }

            // copy each source line to the appropriate place in the destination
            int pass = 1;
            int inc = 8;
            int iline = 0;
            for (int i = 0; i < _ih; i++) {
                int line = i;
                if (_interlace) {
                    if (iline >= _ih) {
                        pass++;
                        switch (pass) {
                            case 2:
                                iline = 4;
                                break;
                            case 3:
                                iline = 2;
                                inc = 4;
                                break;
                            case 4:
                                iline = 1;
                                inc = 2;
                                break;
                        }
                    }

                    line = iline;
                    iline += inc;
                }

                line += _iy;
                if (line >= _height) {
                    continue;
                }

                var sx = i * _iw;
                line = _height - line - 1;
                var dx = line * _width + _ix;
                var endx = dx + _iw;

                for (; dx < endx; dx++) {
                    var c = _act[_pixels[sx++] & 0xff];
                    if (c != 0) {
                        _image[dx] = c;
                    }
                }
            }
        }

        /**
         * Reads GIF image from stream
         *
         * @param BufferedInputStream containing GIF file.
         * @return read status code (0 = no errors)
         */
        public int read(Stream inStream) {
            _init();
            if (inStream != null) {
                _inStream = inStream;
                _readHeader();
            }
            else {
                _status = STATUS_OPEN_ERROR;
            }

            return _status;
        }

        public void Dispose() {
            if (_inStream != null) {
                _inStream.Dispose();
                _inStream = null;
            }
        }

        /**
         * Decodes LZW image data into pixel array.
         * Adapted from John Cristy's ImageMagick.
         */
        void _decodeImageData() {
            const int NullCode = -1;
            int npix = _iw * _ih;
            int available,
                clear,
                code_mask,
                code_size,
                end_of_information,
                in_code,
                old_code,
                bits,
                code,
                count,
                i,
                datum,
                data_size,
                first,
                top,
                bi,
                pi;

            if ((_pixels == null) || (_pixels.Length < npix)) {
                _pixels = new byte[npix]; // allocate new pixel array
            }

            if (_prefix == null) {
                _prefix = new short[MAX_STACK_SIZE];
            }

            if (_suffix == null) {
                _suffix = new byte[MAX_STACK_SIZE];
            }

            if (_pixelStack == null) {
                _pixelStack = new byte[MAX_STACK_SIZE + 1];
            }

            //  Initialize GIF data stream decoder.

            data_size = _read();
            clear = 1 << data_size;
            end_of_information = clear + 1;
            available = clear + 2;
            old_code = NullCode;
            code_size = data_size + 1;
            code_mask = (1 << code_size) - 1;
            for (code = 0; code < clear; code++) {
                _prefix[code] = 0;
                _suffix[code] = (byte) code;
            }

            //  Decode GIF pixel stream.

            datum = bits = count = first = top = pi = bi = 0;

            for (i = 0; i < npix;) {
                if (top == 0) {
                    if (bits < code_size) {
                        //  Load bytes until there are enough bits for a code.
                        if (count == 0) {
                            // Read a new data block.
                            count = _readBlock();
                            if (count <= 0) {
                                break;
                            }

                            bi = 0;
                        }

                        datum += (_block[bi] & 0xff) << bits;
                        bits += 8;
                        bi++;
                        count--;
                        continue;
                    }

                    //  Get the next code.

                    code = datum & code_mask;
                    datum >>= code_size;
                    bits -= code_size;

                    //  Interpret the code

                    if ((code > available) || (code == end_of_information)) {
                        break;
                    }

                    if (code == clear) {
                        //  Reset decoder.
                        code_size = data_size + 1;
                        code_mask = (1 << code_size) - 1;
                        available = clear + 2;
                        old_code = NullCode;
                        continue;
                    }

                    if (old_code == NullCode) {
                        _pixelStack[top++] = _suffix[code];
                        old_code = code;
                        first = code;
                        continue;
                    }

                    in_code = code;
                    if (code == available) {
                        _pixelStack[top++] = (byte) first;
                        code = old_code;
                    }

                    while (code > clear) {
                        _pixelStack[top++] = _suffix[code];
                        code = _prefix[code];
                    }

                    first = _suffix[code] & 0xff;

                    //  Add a new string to the string table,

                    if (available >= MAX_STACK_SIZE) {
                        break;
                    }

                    _pixelStack[top++] = (byte) first;
                    _prefix[available] = (short) old_code;
                    _suffix[available] = (byte) first;
                    available++;
                    if (((available & code_mask) == 0)
                        && (available < MAX_STACK_SIZE)) {
                        code_size++;
                        code_mask += available;
                    }

                    old_code = in_code;
                }

                //  Pop a pixel off the pixel stack.

                top--;
                _pixels[pi++] = _pixelStack[top];
                i++;
            }

            for (i = pi; i < npix; i++) {
                _pixels[i] = 0; // clear missing pixels
            }
        }

        /**
         * Returns true if an error was encountered during reading/decoding
         */
        bool _error() {
            return _status != STATUS_OK;
        }

        /**
         * Initializes or re-initializes reader
         */
        void _init() {
            _status = STATUS_OK;
            _currentFrame = null;
            _frameCount = 0;
            _done = false;
            _gct = null;
            _lct = null;
        }

        /**
         * Reads a single byte from the input stream.
         */
        int _read() {
            int curByte = 0;
            try {
                curByte = _inStream.ReadByte();
            }
            catch (IOException) {
                _status = STATUS_FORMAT_ERROR;
            }

            return curByte;
        }

        /**
         * Reads next variable length block from input.
         *
         * @return number of bytes stored in "buffer"
         */
        int _readBlock() {
            _blockSize = _read();
            int n = 0;
            if (_blockSize > 0) {
                try {
                    int count = 0;
                    while (n < _blockSize) {
                        count = _inStream.Read(_block, n, _blockSize - n);
                        if (count == -1) {
                            break;
                        }

                        n += count;
                    }
                }
                catch (IOException) {
                }

                if (n < _blockSize) {
                    _status = STATUS_FORMAT_ERROR;
                }
            }

            return n;
        }

        /**
         * Reads color table as 256 RGB integer values
         *
         * @param ncolors int number of colors to read
         * @return int array containing 256 colors (packed ARGB with full alpha)
         */
        int[] _readColorTable(int ncolors) {
            int nbytes = 3 * ncolors;
            int[] tab = null;
            byte[] c = new byte[nbytes];
            int n = 0;
            try {
                n = _inStream.Read(c, 0, c.Length);
            }
            catch (IOException) {
            }

            if (n < nbytes) {
                _status = STATUS_FORMAT_ERROR;
            }
            else {
                tab = new int[256]; // max size to avoid bounds checks
                int i = 0;
                int j = 0;
                while (i < ncolors) {
                    int r = c[j++] & 0xff;
                    int g = c[j++] & 0xff;
                    int b = c[j++] & 0xff;
                    tab[i++] = (int) (0xff000000 | ((uint) r << 16) | ((uint) g << 8) | (uint) b);
                }
            }

            return tab;
        }

        /**
         * Main file parser.  Reads GIF content blocks.
         */
        public int nextFrame() {
            // read GIF file content blocks
            bool done = false;
            while (!(done || _error())) {
                int code = _read();
                switch (code) {
                    case 0x2C: // image separator
                        _readImage();
                        done = true;
                        break;

                    case 0x21: // extension
                        code = _read();
                        switch (code) {
                            case 0xf9: // graphics control extension
                                _readGraphicControlExt();
                                break;

                            case 0xff: // application extension
                                _readBlock();

                                var appBuilder = new StringBuilder();
                                for (int i = 0; i < 11; i++) {
                                    appBuilder.Append((char) _block[i]);
                                }

                                string app = appBuilder.ToString();
                                if (app.Equals("NETSCAPE2.0")) {
                                    _readNetscapeExt();
                                }
                                else {
                                    _skip(); // don't care
                                }

                                break;

                            default: // uninteresting extension
                                _skip();
                                break;
                        }

                        break;

                    case 0x3b: // terminator
                        _done = true;
                        done = true;
                        break;

                    case 0x00: // bad byte, but keep going and see what happens
                        break;

                    default:
                        _status = STATUS_FORMAT_ERROR;
                        break;
                }
            }

            return _status;
        }

        /**
         * Reads Graphics Control Extension values
         */
        void _readGraphicControlExt() {
            _read(); // block size
            int packed = _read(); // packed fields
            _dispose = (packed & 0x1c) >> 2; // disposal method
            if (_dispose == 0) {
                _dispose = 1; // elect to keep old image if discretionary
            }

            _transparency = (packed & 1) != 0;
            _delay = _readShort() * 10; // delay in milliseconds
            _transIndex = _read(); // transparent color index
            _read(); // block terminator
        }

        /**
         * Reads GIF file header information.
         */
        void _readHeader() {
            var idBuilder = new StringBuilder();
            for (int i = 0; i < 6; i++) {
                idBuilder.Append((char) _read());
            }

            var id = idBuilder.ToString();
            if (!id.StartsWith("GIF")) {
                _status = STATUS_FORMAT_ERROR;
                return;
            }

            _readLSD();
            if (_gctFlag && !_error()) {
                _gct = _readColorTable(_gctSize);
                _bgColor = _gct[_bgIndex];
            }

            _currentFrame = new GifFrame {
                bytes = new byte[_width * _height * sizeof(int)],
                delay = 0
            };
        }

        /**
         * Reads next frame image
         */
        void _readImage() {
            _ix = _readShort(); // (sub)image position & size
            _iy = _readShort();
            _iw = _readShort();
            _ih = _readShort();

            int packed = _read();
            _lctFlag = (packed & 0x80) != 0; // 1 - local color table flag
            _interlace = (packed & 0x40) != 0; // 2 - interlace flag
            // 3 - sort flag
            // 4-5 - reserved
            _lctSize = 2 << (packed & 7); // 6-8 - local color table size

            if (_lctFlag) {
                _lct = _readColorTable(_lctSize); // read table
                _act = _lct; // make local table active
            }
            else {
                _act = _gct; // make global table active
                if (_bgIndex == _transIndex) {
                    _bgColor = 0;
                }
            }

            int save = 0;
            if (_transparency) {
                save = _act[_transIndex];
                _act[_transIndex] = 0; // set transparent color if specified
            }

            if (_act == null) {
                _status = STATUS_FORMAT_ERROR; // no color table defined
            }

            if (_error()) {
                return;
            }

            _decodeImageData(); // decode pixel data
            _skip();

            if (_error()) {
                return;
            }

            // create new image to receive frame data
            //		image =
            //			new BufferedImage(width, height, BufferedImage.TYPE_INT_ARGB_PRE);

            _image = _image ?? new int[_width * _height];

            _setPixels(); // transfer pixel data to image

            Buffer.BlockCopy(_image, 0, _currentFrame.bytes, 0, _currentFrame.bytes.Length);
            _currentFrame.delay = _delay;
            _frameCount++;

            if (_transparency) {
                _act[_transIndex] = save;
            }

            _resetFrame();
        }

        /**
         * Reads Logical Screen Descriptor
         */
        void _readLSD() {
            // logical screen size
            _width = _readShort();
            _height = _readShort();

            // packed fields
            int packed = _read();
            _gctFlag = (packed & 0x80) != 0; // 1   : global color table flag
            // 2-4 : color resolution
            // 5   : gct sort flag
            _gctSize = 2 << (packed & 7); // 6-8 : gct size

            _bgIndex = _read(); // background color index
            _pixelAspect = _read(); // pixel aspect ratio
        }

        /**
         * Reads Netscape extenstion to obtain iteration count
         */
        void _readNetscapeExt() {
            do {
                _readBlock();
                if (_block[0] == 1) {
                    // loop count sub-block
                    int b1 = _block[1] & 0xff;
                    int b2 = _block[2] & 0xff;
                    _loopCount = (b2 << 8) | b1;
                }
            } while (_blockSize > 0 && !_error());
        }

        /**
         * Reads next 16-bit value, LSB first
         */
        int _readShort() {
            // read 16-bit value, LSB first
            return _read() | (_read() << 8);
        }

        /**
         * Resets frame state for reading next image.
         */
        void _resetFrame() {
            _lastDispose = _dispose;
            _lix = _ix;
            _liy = _iy;
            _liw = _iw;
            _lih = _ih;
            _lastBgColor = _bgColor;
            _lct = null;
        }

        /**
         * Skips variable length blocks up to and including
         * next zero length block.
         */
        void _skip() {
            do {
                _readBlock();
            } while ((_blockSize > 0) && !_error());
        }
    }
}