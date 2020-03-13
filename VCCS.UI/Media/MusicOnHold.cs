using log4net;
using SIPSorcery.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCCS.UI.Media
{
    public class MusicOnHold
    {
        private const string AUDIO_FILE_PCMU = @"content\Macroform_-_Simplicity.ulaw";
        private const int AUDIO_SAMPLE_PERIOD_MILLISECONDS = 40;

        private ILog logger = AppState.logger;

        private bool _stop = false;
        private Task _samplesTask;

        /// <summary>
        /// Fires when a music on hold audio sample is available.
        /// [sample].
        /// </summary>
        public event Action<byte[]> OnAudioSampleReady;

        /// <summary>
        /// Creates a default music on hold class.
        /// </summary>
        public MusicOnHold()
        { }

        public void Start()
        {
            _stop = false;

            if (_samplesTask == null || _samplesTask.Status != TaskStatus.Running)
            {
                logger.Debug("Music on hold samples task starting.");

                _samplesTask = Task.Run(async () =>
                {
                    // Read the same file in an endless loop while samples are still requried.
                    while (!_stop)
                    {
                        using (StreamReader sr = new StreamReader(AUDIO_FILE_PCMU))
                        {
                            int sampleSize = (SDPMediaFormatInfo.GetClockRate(SDPMediaFormatsEnum.PCMU) / 1000) * AUDIO_SAMPLE_PERIOD_MILLISECONDS;
                            byte[] sample = new byte[sampleSize];
                            int bytesRead = sr.BaseStream.Read(sample, 0, sample.Length);

                            while (bytesRead > 0 && !_stop)
                            {
                                if (OnAudioSampleReady == null)
                                {
                                    // Nobody needs music on hold so exit.
                                    logger.Debug("Music on hold has no subscribers, stopping.");
                                    return;
                                }
                                else
                                {
                                    OnAudioSampleReady(sample);
                                }

                                await Task.Delay(AUDIO_SAMPLE_PERIOD_MILLISECONDS);
                                bytesRead = sr.BaseStream.Read(sample, 0, sample.Length);
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Called when the music on hold is no longer required.
        /// </summary>
        public void Stop()
        {
            _stop = true;
        }
    }
}
