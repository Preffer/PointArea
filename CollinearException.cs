using System;

namespace PointArea {
    class CollinearException : Exception {
        public CollinearException() : base() { }
        public CollinearException(string message) : base(message) { }
        public CollinearException(string message, Exception inner) : base(message, inner) { }
    }
}
