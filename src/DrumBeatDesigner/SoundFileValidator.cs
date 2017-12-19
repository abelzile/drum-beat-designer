using System;
using NAudio.Wave;


namespace DrumBeatDesigner
{
    public class SoundFileValidator : ISoundFileValidator
    {
         public bool Validate(Uri path)
         {
             try
             {
                 using (var reader = new WaveFileReader(path.LocalPath))
                 {
                     return true;
                 }
             }
             catch
             {
                 return false;
             }
         }
    }
}