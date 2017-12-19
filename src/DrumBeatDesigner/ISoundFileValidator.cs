using System;


namespace DrumBeatDesigner
{
    public interface ISoundFileValidator
    {
        bool Validate(Uri path);
    }
}