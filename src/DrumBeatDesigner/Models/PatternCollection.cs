using MoonAndSun.Commons.Mvvm;

namespace DrumBeatDesigner.Models
{
    public class PatternCollection : ObservableCollection<Pattern>
    {
        public int MaxPatternItemIndex
        {
            get
            {
                if (Items.Count == 0)
                {
                    return -1;
                }

                int startPatternItemIndex = Items[0].PatternItems.Count - 1;
                int maxPatternItemIndex = -1;

                foreach (var pattern in Items)
                {
                    for (int i = startPatternItemIndex; i-- > 0;)
                    {
                        if (pattern.PatternItems[i].IsEnabled)
                        {
                            if (i > maxPatternItemIndex)
                            {
                                maxPatternItemIndex = i;
                            }

                            break;
                        }
                    }
                }

                return maxPatternItemIndex;
            }
        }
    }
}
