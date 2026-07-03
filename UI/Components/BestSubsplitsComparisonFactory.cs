using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;

[assembly: ComponentFactory(typeof(BestSubsplitsComparisonFactory))]

namespace LiveSplit.UI.Components
{
    public class BestSubsplitsComparisonFactory : IComponentFactory
    {
        public string ComponentName => "Best Subsplits Comparison";

        public string Description => "Generates a comparison from your best subsplit sections.";

        public ComponentCategory Category => ComponentCategory.Other;

        public IComponent Create(LiveSplitState state)
        {
            return new BestSubsplitsComparisonComponent(state);
        }

        public string UpdateName => ComponentName;

        public string XMLURL => string.Empty;

        public string UpdateURL => string.Empty;

        public Version Version => Version.Parse("0.1.0");
    }
}
