using NAudio.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace DrumBeatDesigner
{
    public class StreamTracker : IDisposable
    {
        private IList<Stream> _streams = new List<Stream>();

        public void AddStream(Stream stream)
        {
            _streams.Add(stream);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (int i = _streams.Count; i-- > 0;)
                {
                    var stream = _streams[i];
                    if (stream is IgnoreDisposeStream disposeStream)
                    {
                        disposeStream.IgnoreDispose = false;
                    }
                    stream.Dispose();

                    _streams.RemoveAt(i);
                }
                _streams = null;
            }
        }
    }
}
