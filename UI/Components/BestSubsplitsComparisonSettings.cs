using LiveSplit.UI;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.UI.Components
{
    public class BestSubsplitsComparisonSettings : UserControl
    {
        public const string DefaultComparisonName = "Best Subsplits";

        private readonly TextBox comparisonNameTextBox;
        private readonly CheckBox ignoreSkippedCheckBox;

        private bool loading;

        public event EventHandler SettingsChanged;

        public string ComparisonName
        {
            get => string.IsNullOrWhiteSpace(comparisonNameTextBox.Text)
                ? DefaultComparisonName
                : comparisonNameTextBox.Text.Trim();
            set => comparisonNameTextBox.Text = string.IsNullOrWhiteSpace(value)
                ? DefaultComparisonName
                : value.Trim();
        }

        public bool IgnoreComparisonsWithSkippedSplits
        {
            get => ignoreSkippedCheckBox.Checked;
            set => ignoreSkippedCheckBox.Checked = value;
        }

        public BestSubsplitsComparisonSettings()
        {
            AutoScaleMode = AutoScaleMode.Inherit;
            Size = new Size(460, 72);
            MinimumSize = new Size(360, 72);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(7)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 29));

            var comparisonNameLabel = new Label
            {
                Text = "Comparison Name:",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };

            comparisonNameTextBox = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                Width = 290
            };
            comparisonNameTextBox.TextChanged += OnControlChanged;

            ignoreSkippedCheckBox = new CheckBox
            {
                Text = "Ignore comparisons with skipped splits",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };
            ignoreSkippedCheckBox.CheckedChanged += OnControlChanged;

            layout.Controls.Add(comparisonNameLabel, 0, 0);
            layout.Controls.Add(comparisonNameTextBox, 1, 0);
            layout.Controls.Add(ignoreSkippedCheckBox, 1, 1);

            Controls.Add(layout);

            loading = true;
            ComparisonName = DefaultComparisonName;
            IgnoreComparisonsWithSkippedSplits = true;
            loading = false;
        }

        private void OnControlChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                SettingsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            XmlElement parent = document.CreateElement("Settings");
            SettingsHelper.CreateSetting(document, parent, "ComparisonName", ComparisonName);
            SettingsHelper.CreateSetting(document, parent, "IgnoreComparisonsWithSkippedSplits", IgnoreComparisonsWithSkippedSplits);
            return parent;
        }

        public void SetSettings(XmlNode settings)
        {
            if (settings == null)
            {
                return;
            }

            loading = true;
            ComparisonName = SettingsHelper.ParseString(settings["ComparisonName"], DefaultComparisonName);
            IgnoreComparisonsWithSkippedSplits = SettingsHelper.ParseBool(
                settings["IgnoreComparisonsWithSkippedSplits"] ?? settings["IgnoreBestWithSkippedSplit"],
                true);
            loading = false;

            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
