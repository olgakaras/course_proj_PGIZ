using SharpDX.Multimedia;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Template.Sound
{
    /// <summary>
    /// Delegate for sound stop
    /// </summary>
    /// <param name="voice">Voice that generate event</param>
    public delegate void SoundStop(SharpAudioVoice voice);

    public class SharpAudioVoice : IDisposable
    {
        SourceVoice _voice;
        AudioBuffer _buffer;
        SoundStream _stream;
        Thread _checkThread;

        private bool isPlaying;
        /// <summary>
        /// Voice
        /// </summary>
        public SourceVoice Voice
        {
            get { return _voice; }
        }

        /// <summary>
        /// Raise event when stopped
        /// </summary>
        public event SoundStop Stopped;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="device">Device</param>
        /// <param name="filename">Filename</param>
        public SharpAudioVoice(SharpAudioDevice device, string filename)
        {
            isPlaying = false;
            _stream = new SoundStream(File.OpenRead(filename));

            var waveFormat = _stream.Format;
            _voice = new SourceVoice(device.Device, waveFormat);

            _buffer = new AudioBuffer
            {
                Stream = _stream.ToDataStream(),
                AudioBytes = (int)_stream.Length,
                Flags = BufferFlags.EndOfStream
            };

        }



        /// <summary>
        /// Play
        /// </summary>
        public void Play()
        {
            if (isPlaying) return;
            _voice.SubmitSourceBuffer(_buffer, _stream.DecodedPacketsInfo);
            _voice.Start();
            isPlaying = true;
            _checkThread = new Thread(new ThreadStart(Check));
            _checkThread.Start();

        }

        //check voice status
        private void Check()
        {
            try
            {
                while (Voice.State.BuffersQueued > 0)
                {
                    Thread.Sleep(10);
                }
                _voice.Stop();
                _voice.FlushSourceBuffers();
                isPlaying = false;
                Stopped.Invoke(this);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Stop audio
        /// </summary>
        public void Stop()
        {
            if (!isPlaying) return;
            _voice.ExitLoop();
            _voice.Stop();
            _voice.FlushSourceBuffers();
            isPlaying = false;
            _checkThread.Abort();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            _voice.DestroyVoice();
            _voice.Dispose();
            _stream.Dispose();
            _buffer.Stream.Dispose();

        }
    }
}
