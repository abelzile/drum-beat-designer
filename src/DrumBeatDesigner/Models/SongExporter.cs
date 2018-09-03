using System;


namespace DrumBeatDesigner.Models
{
    public class SongExporter
    {
        public void Export(PatternCollection patterns, int bpm, string outputPath, int sampleRate, int bitsPerSample, int channels)
        {
            if (patterns.MaxPatternItemIndex < 0)
            {
                throw new ArgumentException("Song must have at least one pattern with beats checked.");
            }

            int maxPatternItemCount = patterns.MaxPatternItemIndex + 1;

            var bigPattern = new Pattern("Big Pattern", maxPatternItemCount);

            foreach (var patternItem in bigPattern.PatternItems)
            {
                patternItem.IsEnabled = true;
            }

            int maxBeatCount = GetMaxBeatCount(patterns);

            foreach (var pattern in patterns)
            {
                foreach (var instrument in pattern.Instruments)
                {
                    var newInstrument = instrument.Clone();
                    newInstrument.Beats.Clear();

                    for (int i = 0; i < maxPatternItemCount; i++)
                    {
                        PatternItem patternItem = pattern.PatternItems[i];

                        if (patternItem.IsEnabled)
                        {
                            foreach (var beat in instrument.Beats)
                            {
                                newInstrument.Beats.Add(beat.Clone());
                            }
                        }
                        else
                        {
                            for (int j = 0; j < instrument.Beats.Count; j++)
                            {
                                newInstrument.Beats.Add(new Beat { IsEnabled = false });
                            }
                        }

                        // pad if required.
                        if (maxBeatCount > instrument.Beats.Count)
                        {
                            int diff = maxBeatCount - instrument.Beats.Count;

                            for (int j = 0; j < diff; ++j)
                            {
                                newInstrument.Beats.Add(new Beat { IsEnabled = false });
                            }
                        }
                    }

                    bigPattern.Instruments.Add(newInstrument);
                }
            }

            //__Debug(bigPattern);

            PatternExporter exp = new PatternExporter();
            exp.Export(bigPattern, bpm, outputPath, sampleRate, bitsPerSample, channels);
        }

        private static void __Debug(Pattern bigPattern)
        {
            foreach (var instrument in bigPattern.Instruments)
            {
                string output = "";

                foreach (var beat in instrument.Beats)
                {
                    output += beat.IsEnabled ? 1 : 0;
                }

                Console.WriteLine(output);
            }
        }

        private static int GetMaxBeatCount(PatternCollection patterns)
        {
            int maxBeatCount = 0;

            foreach (var pattern in patterns)
            {
                foreach (var instrument in pattern.Instruments)
                {
                    if (instrument.Beats.Count > maxBeatCount)
                    {
                        maxBeatCount = instrument.Beats.Count;
                    }
                }
            }

            return maxBeatCount;
        }
    }
}