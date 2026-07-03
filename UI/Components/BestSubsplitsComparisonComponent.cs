using LiveSplit.Model;
using LiveSplit.Model.Comparisons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class BestSubsplitsComparisonComponent : LogicComponent
    {
        private readonly BestSubsplitsComparisonSettings settings;
        private LiveSplitState state;
        private IRun currentRun;
        private BestSubsplitsComparisonGenerator generator;
        private string installedComparisonName;
        private bool dirty;
        private int lastHistoryFingerprint;

        public override string ComponentName => "Best Subsplits Comparison";

        public BestSubsplitsComparisonComponent(LiveSplitState state)
        {
            settings = new BestSubsplitsComparisonSettings();
            settings.SettingsChanged += Settings_SettingsChanged;
            AttachState(state);
            dirty = true;
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return settings;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return settings.GetSettings(document);
        }

        public override void SetSettings(XmlNode settingsNode)
        {
            settings.SetSettings(settingsNode);
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (!ReferenceEquals(this.state, state))
            {
                AttachState(state);
                dirty = true;
            }

            if (state?.Run == null)
            {
                currentRun = null;
                generator = null;
                installedComparisonName = null;
                return;
            }

            if (!ReferenceEquals(currentRun, state.Run))
            {
                currentRun = state.Run;
                generator = null;
                installedComparisonName = null;
                dirty = true;
            }

            EnsureGenerator();

            int fingerprint = CalculateHistoryFingerprint(currentRun);
            if (dirty || fingerprint != lastHistoryFingerprint)
            {
                generator.Generate(state.Settings);
                lastHistoryFingerprint = fingerprint;
                dirty = false;
            }
        }

        public override void Dispose()
        {
            DetachState();
            settings.SettingsChanged -= Settings_SettingsChanged;
            GC.SuppressFinalize(this);
        }

        private void AttachState(LiveSplitState newState)
        {
            DetachState();
            state = newState;
            if (state != null)
            {
                state.RunManuallyModified += State_RunManuallyModified;
                state.OnReset += State_OnReset;
            }
        }

        private void DetachState()
        {
            if (state != null)
            {
                state.RunManuallyModified -= State_RunManuallyModified;
                state.OnReset -= State_OnReset;
            }
        }

        private void Settings_SettingsChanged(object sender, EventArgs e)
        {
            dirty = true;
            if (currentRun != null)
            {
                EnsureGenerator();
            }
        }

        private void State_RunManuallyModified(object sender, EventArgs e)
        {
            dirty = true;
        }

        private void State_OnReset(object sender, TimerPhase oldPhase)
        {
            dirty = true;
        }

        private void EnsureGenerator()
        {
            string comparisonName = settings.ComparisonName;

            if (generator == null || installedComparisonName != comparisonName)
            {
                RemoveInstalledGenerators(currentRun);
                generator = new BestSubsplitsComparisonGenerator(
                    currentRun,
                    comparisonName,
                    () => settings.IgnoreComparisonsWithSkippedSplits);
                currentRun.ComparisonGenerators.Add(generator);
                installedComparisonName = comparisonName;
                dirty = true;
            }
            else if (!currentRun.ComparisonGenerators.Contains(generator))
            {
                currentRun.ComparisonGenerators.Add(generator);
                dirty = true;
            }
        }

        private static void RemoveInstalledGenerators(IRun run)
        {
            IList<IComparisonGenerator> existing = run.ComparisonGenerators
                .Where(x => x is BestSubsplitsComparisonGenerator)
                .ToList();

            foreach (IComparisonGenerator oldGenerator in existing)
            {
                run.ComparisonGenerators.Remove(oldGenerator);
                foreach (ISegment segment in run)
                {
                    segment.Comparisons.Remove(oldGenerator.Name);
                }
            }
        }

        private static int CalculateHistoryFingerprint(IRun run)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + run.Count;
                hash = (hash * 31) + run.AttemptCount;
                hash = (hash * 31) + run.AttemptHistory.Count;

                foreach (ISegment segment in run)
                {
                    hash = (hash * 31) + (segment.Name?.GetHashCode() ?? 0);
                    hash = (hash * 31) + segment.SegmentHistory.Count;
                }

                return hash;
            }
        }
    }
}
