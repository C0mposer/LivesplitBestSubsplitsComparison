using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using LiveSplit.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LiveSplit.Model.Comparisons
{
    public class BestSubsplitsComparisonGenerator : IComparisonGenerator
    {
        private readonly Func<bool> ignoreBestWithSkippedSplit; // If a best section skips a split, look for the next best without skipped splits.

        public IRun Run { get; set; }

        public string Name { get; private set; }

        public BestSubsplitsComparisonGenerator(IRun run, string name, Func<bool> ignoreBestWithSkippedSplit)
        {
            Run = run;
            Name = string.IsNullOrWhiteSpace(name)
                ? UI.Components.BestSubsplitsComparisonSettings.DefaultComparisonName
                : name.Trim();
            this.ignoreBestWithSkippedSplit = ignoreBestWithSkippedSplit ?? (() => false);
        }

        public void Generate(ISettings settings)
        {
            foreach (ISegment segment in Run)
            {
                segment.Comparisons[Name] = default(Time);
            }

            if (Run.Count == 0 || Run.AttemptHistory.Count == 0)
            {
                return;
            }

            IList<Section> sections = GetSubsplitSections(Run);
            if (sections.Count == 0)
            {
                return;
            }

            IList<int> attemptIds = Run.AttemptHistory
                .Select(x => x.Index)
                .Where(x => x > 0)
                .Distinct()
                .ToList();

            if (attemptIds.Count == 0)
            {
                return;
            }

            Generate(TimingMethod.RealTime, sections, attemptIds);
            Generate(TimingMethod.GameTime, sections, attemptIds);
        }

        private void Generate(TimingMethod method, IList<Section> sections, IList<int> attemptIds)
        {
            TimeSpan stitchedTime = TimeSpan.Zero;
            bool strict = ignoreBestWithSkippedSplit();

            foreach (Section section in sections)
            {
                int? bestAttemptId = FindBestAttempt(section, attemptIds, method, strict);
                if (!bestAttemptId.HasValue)
                {
                    break;
                }

                for (int index = section.StartIndex; index <= section.EndIndex; index++)
                {
                    TimeSpan? segmentTime = GetSegmentTime(index, bestAttemptId.Value, method);
                    if (!segmentTime.HasValue)
                    {
                        continue;
                    }

                    stitchedTime += segmentTime.Value;
                    SetComparisonTime(index, method, stitchedTime);
                }
            }
        }

        private int? FindBestAttempt(Section section, IEnumerable<int> attemptIds, TimingMethod method, bool strict)
        {
            int? bestAttemptId = null;
            TimeSpan? bestDuration = null;

            foreach (int attemptId in attemptIds)
            {
                TimeSpan? duration = GetSectionDuration(section, attemptId, method, strict);
                if (duration.HasValue && (!bestDuration.HasValue || duration.Value < bestDuration.Value))
                {
                    bestDuration = duration.Value;
                    bestAttemptId = attemptId;
                }
            }

            return bestAttemptId;
        }

        private TimeSpan? GetSectionDuration(Section section, int attemptId, TimingMethod method, bool strict)
        {
            TimeSpan total = TimeSpan.Zero;
            bool sawTimedSegment = false;
            bool endWasTimed = false;

            for (int index = section.StartIndex; index <= section.EndIndex; index++)
            {
                TimeSpan? segmentTime = GetSegmentTime(index, attemptId, method);
                if (segmentTime.HasValue)
                {
                    if (segmentTime.Value < TimeSpan.Zero)
                    {
                        return null;
                    }

                    total += segmentTime.Value;
                    sawTimedSegment = true;
                    endWasTimed = index == section.EndIndex;
                }
                else if (strict)
                {
                    return null;
                }
            }

            return sawTimedSegment && endWasTimed ? total : (TimeSpan?)null;
        }

        private TimeSpan? GetSegmentTime(int segmentIndex, int attemptId, TimingMethod method)
        {
            if (Run[segmentIndex].SegmentHistory.TryGetValue(attemptId, out Time time))
            {
                return time[method];
            }

            return null;
        }

        private void SetComparisonTime(int segmentIndex, TimingMethod method, TimeSpan time)
        {
            Time comparison = new Time(Run[segmentIndex].Comparisons[Name]);
            comparison[method] = time;
            Run[segmentIndex].Comparisons[Name] = comparison;
        }

        private static IList<Section> GetSubsplitSections(IRun run)
        {
            var sections = new List<Section>();

            for (int splitIndex = run.Count - 1; splitIndex >= 0; splitIndex--)
            {
                int sectionEnd = splitIndex;
                while (splitIndex > 0 && IsSubsplit(run[splitIndex - 1], splitIndex - 1, run.Count))
                {
                    splitIndex--;
                }

                sections.Insert(0, new Section(splitIndex, sectionEnd));
            }

            return sections;
        }

        private static bool IsSubsplit(ISegment segment, int segmentIndex, int runCount)
        {
            return segmentIndex < runCount - 1
                && !string.IsNullOrEmpty(segment.Name)
                && segment.Name.StartsWith("-", StringComparison.Ordinal);
        }

        private struct Section
        {
            public int StartIndex { get; }

            public int EndIndex { get; }

            public Section(int startIndex, int endIndex)
            {
                StartIndex = startIndex;
                EndIndex = endIndex;
            }
        }
    }
}
