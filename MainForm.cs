using System.Diagnostics;
using System.Windows.Forms;

namespace LGAStudio;

// The UI requires explicit row selection and confirmation before any permission or startup change.
internal sealed class MainForm : Form
{
    private readonly DataGridView _browserGrid = CreateGrid();
    private readonly DataGridView _startupGrid = CreateGrid();
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly TabPage _browserPage = new();
    private readonly TabPage _startupPage = new();
    private readonly Label _browserInstructions = new() { AutoSize = true, MaximumSize = new Size(1250, 0), Padding = new Padding(6) };
    private readonly Label _startupInstructions = new() { AutoSize = true, MaximumSize = new Size(1250, 0), Padding = new Padding(6) };
    private readonly Label _browserStatus = new() { AutoSize = true, MaximumSize = new Size(1250, 0), Padding = new Padding(6) };
    private readonly Label _startupStatus = new() { AutoSize = true, MaximumSize = new Size(1250, 0), Padding = new Padding(6) };
    private readonly Label _stepLabel = new() { AutoSize = true, MaximumSize = new Size(850, 0), Padding = new Padding(8, 8, 8, 8) };
    private readonly Label _languageLabel = new() { AutoSize = true, Padding = new Padding(8) };
    private readonly Button _oneClickButton = new() { AutoSize = true, MinimumSize = new Size(250, 52), Padding = new Padding(12, 6, 12, 6) };
    private readonly Button _browserScanButton = new() { AutoSize = true, MinimumSize = new Size(150, 46), Padding = new Padding(10, 6, 10, 6) };
    private readonly Button _blockButton = new() { AutoSize = true, MinimumSize = new Size(150, 46), Padding = new Padding(10, 6, 10, 6) };
    private readonly Button _resetButton = new() { AutoSize = true, MinimumSize = new Size(150, 46), Padding = new Padding(10, 6, 10, 6) };
    private readonly Button _startupScanButton = new() { AutoSize = true, MinimumSize = new Size(170, 46), Padding = new Padding(10, 6, 10, 6) };
    private readonly Button _removeButton = new() { AutoSize = true, MinimumSize = new Size(160, 46), Padding = new Padding(10, 6, 10, 6) };
    private readonly ComboBox _languageCombo = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 160,
        Height = 38,
        Font = new Font("Malgun Gothic", 10F, FontStyle.Regular, GraphicsUnit.Point)
    };
    private readonly ToolTip _toolTips = new();
    private bool _browserScanComplete;
    private bool _startupScanComplete;
    private int _browserResultCount;
    private int _startupResultCount;
    private bool _isApplyingLanguage;

    public MainForm()
    {
        Text = "LGA Studio";
        Font = new Font("Malgun Gothic", 10F, FontStyle.Regular, GraphicsUnit.Point);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        Width = 1380;
        Height = 900;
        MinimumSize = new Size(1000, 700);
        StartPosition = FormStartPosition.CenterScreen;

        ConfigureBrowserGrid();
        ConfigureStartupGrid();
        BuildBrowserTab();
        BuildStartupTab();
        BuildLayout();
        PopulateLanguages();
        _languageCombo.SelectedIndexChanged += HandleLanguageChanged;
        ApplyLanguage();
    }

    private void BuildLayout()
    {
        var header = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10),
            MinimumSize = new Size(0, 72),
            WrapContents = true,
            FlowDirection = FlowDirection.LeftToRight
        };
        _oneClickButton.Click += (_, _) => OneClickScan();
        header.Controls.Add(_oneClickButton);
        header.Controls.Add(_stepLabel);
        header.Controls.Add(_languageLabel);
        header.Controls.Add(_languageCombo);

        _tabs.TabPages.Add(_browserPage);
        _tabs.TabPages.Add(_startupPage);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(_tabs, 0, 1);
        Controls.Add(layout);
    }

    private void BuildBrowserTab()
    {
        var controls = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10),
            WrapContents = true
        };
        _browserScanButton.Click += (_, _) => ScanBrowsers();
        _blockButton.Click += (_, _) => ChangeBrowserPermissions(block: true);
        _resetButton.Click += (_, _) => ChangeBrowserPermissions(block: false);
        controls.Controls.Add(_browserInstructions);
        controls.SetFlowBreak(_browserInstructions, true);
        controls.Controls.Add(_browserScanButton);
        controls.Controls.Add(_blockButton);
        controls.Controls.Add(_resetButton);
        controls.Controls.Add(_browserStatus);

        _browserPage.Controls.Add(_browserGrid);
        _browserPage.Controls.Add(controls);
    }

    private void BuildStartupTab()
    {
        var controls = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(10),
            WrapContents = true
        };
        _startupScanButton.Click += (_, _) => ScanStartupItems();
        _removeButton.Click += (_, _) => RemoveStartupItems();
        controls.Controls.Add(_startupInstructions);
        controls.SetFlowBreak(_startupInstructions, true);
        controls.Controls.Add(_startupScanButton);
        controls.Controls.Add(_removeButton);
        controls.Controls.Add(_startupStatus);

        _startupPage.Controls.Add(_startupGrid);
        _startupPage.Controls.Add(controls);
    }

    private void ConfigureBrowserGrid()
    {
        _browserGrid.Columns.Add(CheckColumn());
        _browserGrid.Columns.Add(TextColumn("Browser", 150));
        _browserGrid.Columns.Add(TextColumn("Profile", 200));
        _browserGrid.Columns.Add(TextColumn("Domain", 360));
        _browserGrid.Columns.Add(TextColumn("Database", 560));
    }

    private void ConfigureStartupGrid()
    {
        _startupGrid.Columns.Add(CheckColumn());
        _startupGrid.Columns.Add(TextColumn("Type", 170));
        _startupGrid.Columns.Add(TextColumn("Name", 220));
        _startupGrid.Columns.Add(TextColumn("Location", 430));
        _startupGrid.Columns.Add(TextColumn("Command", 460));
        _startupGrid.Columns.Add(TextColumn("Assessment", 430));
    }

    private static DataGridViewCheckBoxColumn CheckColumn()
    {
        return new DataGridViewCheckBoxColumn
        {
            Name = "Selected",
            HeaderText = "Select",
            Width = 72,
            MinimumWidth = 72,
            FlatStyle = FlatStyle.Standard,
            ReadOnly = false
        };
    }

    private static DataGridView CreateGrid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
            ScrollBars = ScrollBars.Both,
            BackgroundColor = SystemColors.Window,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = true,
            EditMode = DataGridViewEditMode.EditOnEnter,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Malgun Gothic", 10F, FontStyle.Regular, GraphicsUnit.Point),
                WrapMode = DataGridViewTriState.True,
                Padding = new Padding(5, 4, 5, 4)
            },
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                Font = new Font("Malgun Gothic", 10F, FontStyle.Bold, GraphicsUnit.Point),
                WrapMode = DataGridViewTriState.True,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(6, 4, 6, 4)
            },
            RowTemplate = { MinimumHeight = 38 },
            ColumnHeadersHeight = 48,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        };
    }

    private static DataGridViewTextBoxColumn TextColumn(string name, int width)
    {
        return new DataGridViewTextBoxColumn
        {
            Name = name,
            HeaderText = name,
            ReadOnly = true,
            Width = width,
            MinimumWidth = Math.Min(110, width),
            DefaultCellStyle = new DataGridViewCellStyle
            {
                WrapMode = DataGridViewTriState.True,
                Padding = new Padding(5, 4, 5, 4)
            }
        };
    }

    private void PopulateLanguages()
    {
        _languageCombo.Items.Add(UiText.Get("english"));
        _languageCombo.Items.Add(UiText.Get("korean"));
        _languageCombo.SelectedIndex = 0;
    }

    private void ApplyLanguage()
    {
        bool wasApplyingLanguage = _isApplyingLanguage;
        _isApplyingLanguage = true;
        try
        {
            Text = "LGA Studio";
            _languageLabel.Text = UiText.Get("language");
            int languageIndex = UiText.Language == UiLanguage.Korean ? 1 : 0;
            if (_languageCombo.SelectedIndex != languageIndex)
                _languageCombo.SelectedIndex = languageIndex;
            _oneClickButton.Text = UiText.Get("oneClick");
            _stepLabel.Text = UiText.Get("step") + "\n" + UiText.Get("intro");
            _browserPage.Text = UiText.Get("browserTab");
            _startupPage.Text = UiText.Get("startupTab");
            _browserInstructions.Text = UiText.Get("browserInstructions");
            _startupInstructions.Text = UiText.Get("startupInstructions");
            _browserScanButton.Text = UiText.Get("scanBrowsers");
            _blockButton.Text = UiText.Get("blockSelected");
            _resetButton.Text = UiText.Get("resetSelected");
            _startupScanButton.Text = UiText.Get("scanStartup");
            _removeButton.Text = UiText.Get("removeSelected");

            SetColumnHeaders(_browserGrid, ["select", "browser", "profile", "domain", "database"]);
            SetColumnHeaders(_startupGrid, ["select", "type", "name", "location", "command", "assessment"]);
            FitTranslatedColumnHeaders(_browserGrid);
            FitTranslatedColumnHeaders(_startupGrid);

            _toolTips.SetToolTip(_languageCombo, UiText.Get("languageTip"));
            _toolTips.SetToolTip(_oneClickButton, UiText.Get("oneClickTip"));
            _toolTips.SetToolTip(_browserScanButton, UiText.Get("scanTip"));
            _toolTips.SetToolTip(_startupScanButton, UiText.Get("scanTip"));
            _toolTips.SetToolTip(_browserGrid, UiText.Get("reviewTip"));
            _toolTips.SetToolTip(_startupGrid, UiText.Get("reviewTip"));
            _toolTips.SetToolTip(_blockButton, UiText.Get("blockTip"));
            _toolTips.SetToolTip(_resetButton, UiText.Get("resetTip"));
            _toolTips.SetToolTip(_removeButton, UiText.Get("removeTip"));

            if (_browserScanComplete)
                _browserStatus.Text = UiText.Format("browserScanStatus", _browserResultCount);
            if (_startupScanComplete)
                _startupStatus.Text = UiText.Format("startupScanStatus", _startupResultCount);

            foreach (DataGridViewRow row in _startupGrid.Rows)
            {
                if (row.Tag is StartupItem item)
                {
                    row.Cells[1].Value = item.Kind == StartupItemKind.Registry
                        ? UiText.Get("registry")
                        : UiText.Get("scheduledTask");
                    row.Cells[5].Value = UiText.Assessment(item.Assessment);
                }
            }
        }
        finally
        {
            _isApplyingLanguage = wasApplyingLanguage;
        }
    }

    private void HandleLanguageChanged(object? sender, EventArgs e)
    {
        if (_isApplyingLanguage || _languageCombo.SelectedIndex < 0)
            return;

        UiLanguage previousLanguage = UiText.Language;
        try
        {
            UiText.Language = _languageCombo.SelectedIndex == 1
                ? UiLanguage.Korean
                : UiLanguage.English;
            ApplyLanguage();
        }
        catch (Exception ex)
        {
            UiText.Language = previousLanguage;
            string recoveryError = "";
            try
            {
                _isApplyingLanguage = true;
                _languageCombo.SelectedIndex = previousLanguage == UiLanguage.Korean ? 1 : 0;
                Text = "LGA Studio";
            }
            catch (Exception recoveryException)
            {
                recoveryError = $"\n\nCould not restore the previous language: {recoveryException.Message}";
            }
            finally
            {
                _isApplyingLanguage = false;
            }

            MessageBox.Show(
                this,
                $"{UiText.Get("languageError")}\n\n{ex.Message}{recoveryError}",
                UiText.Get("languageErrorTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static void SetColumnHeaders(DataGridView grid, IReadOnlyList<string> keys)
    {
        for (int index = 0; index < keys.Count; index++)
            grid.Columns[index].HeaderText = UiText.Get(keys[index]);
    }

    private static void FitTranslatedColumnHeaders(DataGridView grid)
    {
        Font headerFont = grid.ColumnHeadersDefaultCellStyle.Font ?? grid.Font;
        foreach (DataGridViewColumn column in grid.Columns)
        {
            Size textSize = TextRenderer.MeasureText(
                column.HeaderText,
                headerFont,
                Size.Empty,
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            int padding = grid.ColumnHeadersDefaultCellStyle.Padding.Horizontal + 24;
            column.Width = Math.Max(column.Width, textSize.Width + padding);
        }

        grid.AutoResizeColumnHeadersHeight();
    }

    private void OneClickScan()
    {
        bool browserScanSucceeded = ScanBrowsers();
        bool startupScanSucceeded = ScanStartupItems();
        if (!browserScanSucceeded || !startupScanSucceeded)
            return;

        MessageBox.Show(this, UiText.Get("oneClickResult"), UiText.Get("oneClickTitle"),
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private bool ScanBrowsers()
    {
        try
        {
            BrowserScanResult result = BrowserNotificationCleaner.Scan();
            _browserGrid.Rows.Clear();
            foreach (BrowserPermission permission in result.Permissions)
            {
                int rowIndex = _browserGrid.Rows.Add(
                    false,
                    permission.Browser,
                    permission.Profile,
                    permission.Domain,
                    permission.SourcePath);
                _browserGrid.Rows[rowIndex].Tag = permission;
            }

            _browserScanComplete = true;
            _browserResultCount = result.Permissions.Count;
            _browserStatus.Text = UiText.Format("browserScanStatus", _browserResultCount);
            ShowWarnings("browserWarningsTitle", result.Warnings);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"{UiText.Get("scanFailed")}\n\n{UiText.Format("technicalDetail", ex.Message)}",
                UiText.Get("scanErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void ChangeBrowserPermissions(bool block)
    {
        _browserGrid.EndEdit();
        List<BrowserPermission> selected = GetSelectedItems<BrowserPermission>(_browserGrid);
        if (selected.Count == 0)
        {
            MessageBox.Show(this, UiText.Get("selectBrowser"), UiText.Get("nothingSelected"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        string[] runningBrowsers = GetRunningBrowserProcesses();
        if (runningBrowsers.Length > 0)
        {
            MessageBox.Show(
                this,
                UiText.Format("closeBrowsers", string.Join(", ", runningBrowsers)),
                UiText.Get("closeBrowsersTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        string action = UiText.Get(block ? "blockAction" : "resetAction");
        DialogResult confirmation = Confirm(
            UiText.Get("browserConfirmTitle"),
            UiText.Format("browserConfirm", action, selected.Count));
        if (confirmation != DialogResult.Yes)
            return;

        int changed = 0;
        var failures = new List<string>();
        foreach (BrowserPermission permission in selected)
        {
            try
            {
                changed += BrowserNotificationCleaner.Apply(permission, block);
            }
            catch (Exception ex)
            {
                failures.Add($"{permission.Browser} / {permission.Profile} / {permission.Domain}: {UiText.Format("technicalDetail", ex.Message)}");
            }
        }

        ShowOperationResult(block ? UiText.Get("blockAction") : UiText.Get("resetAction"), changed, failures);
        ScanBrowsers();
    }

    private bool ScanStartupItems()
    {
        try
        {
            StartupScanResult result = StartupCleaner.Scan();
            _startupGrid.Rows.Clear();
            foreach (StartupItem item in result.Items)
            {
                int rowIndex = _startupGrid.Rows.Add(
                    false,
                    item.Kind == StartupItemKind.Registry ? UiText.Get("registry") : UiText.Get("scheduledTask"),
                    item.Name,
                    item.Location,
                    item.Command,
                    UiText.Assessment(item.Assessment));
                _startupGrid.Rows[rowIndex].Tag = item;
            }

            _startupScanComplete = true;
            _startupResultCount = result.Items.Count;
            _startupStatus.Text = UiText.Format("startupScanStatus", _startupResultCount);
            ShowWarnings("startupWarningsTitle", result.Warnings);
            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"{UiText.Get("scanFailed")}\n\n{UiText.Format("technicalDetail", ex.Message)}",
                UiText.Get("scanErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void RemoveStartupItems()
    {
        _startupGrid.EndEdit();
        List<StartupItem> selected = GetSelectedItems<StartupItem>(_startupGrid);
        if (selected.Count == 0)
        {
            MessageBox.Show(this, UiText.Get("selectStartup"), UiText.Get("nothingSelected"),
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult confirmation = Confirm(
            UiText.Get("removeConfirmTitle"),
            UiText.Format("removeConfirm", selected.Count));
        if (confirmation != DialogResult.Yes)
            return;

        int removed = 0;
        var failures = new List<string>();
        foreach (StartupItem item in selected)
        {
            try
            {
                StartupCleaner.Remove(item);
                removed++;
            }
            catch (Exception ex)
            {
                string itemType = UiText.Get(item.Kind == StartupItemKind.Registry ? "registry" : "scheduledTask");
                failures.Add($"{itemType} / {item.Location} / {item.Name}: {UiText.Format("technicalDetail", ex.Message)}");
            }
        }

        ShowOperationResult(UiText.Get("removeSelected"), removed, failures);
        ScanStartupItems();
    }

    private static List<T> GetSelectedItems<T>(DataGridView grid) where T : class
    {
        var selected = new List<T>();
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells["Selected"].Value is true && row.Tag is T item)
                selected.Add(item);
        }
        return selected;
    }

    private static string[] GetRunningBrowserProcesses()
    {
        string[] names = ["chrome", "msedge", "brave", "vivaldi", "opera", "chromium", "firefox"];
        var running = new List<string>();
        foreach (string name in names)
        {
            try
            {
                foreach (Process process in Process.GetProcessesByName(name))
                {
                    using (process)
                        running.Add($"{name} (PID {process.Id})");
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
            {
                running.Add($"{UiText.Format("browserProcessUnavailable", name)}: {ex.Message}");
            }
        }
        return running.ToArray();
    }

    private void ShowOperationResult(string action, int changed, IReadOnlyCollection<string> failures)
    {
        string message = failures.Count == 0
            ? UiText.Format("completed", action, changed)
            : UiText.Format("completedErrors", action, changed, failures.Count) + "\n\n" + string.Join("\n", failures);
        MessageBox.Show(
            this,
            message,
            UiText.Get(failures.Count == 0 ? "completedTitle" : "operationErrorTitle"),
            MessageBoxButtons.OK,
            failures.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private DialogResult Confirm(string title, string message)
    {
        using var dialog = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false,
            MaximizeBox = false,
            ShowInTaskbar = false,
            ClientSize = new Size(520, 180)
        };

        var prompt = new Label
        {
            Text = message,
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            AutoSize = false
        };
        var yesButton = new Button
        {
            Text = UiText.Get("yes"),
            DialogResult = DialogResult.Yes,
            AutoSize = true,
            MinimumSize = new Size(90, 32)
        };
        var noButton = new Button
        {
            Text = UiText.Get("no"),
            DialogResult = DialogResult.No,
            AutoSize = true,
            MinimumSize = new Size(90, 32)
        };
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8)
        };
        buttons.Controls.Add(noButton);
        buttons.Controls.Add(yesButton);
        dialog.Controls.Add(prompt);
        dialog.Controls.Add(buttons);
        dialog.AcceptButton = yesButton;
        dialog.CancelButton = noButton;
        return dialog.ShowDialog(this);
    }

    private void ShowWarnings(string titleKey, IReadOnlyCollection<string> warnings)
    {
        if (warnings.Count == 0)
            return;

        string details = string.Join("\n\n", warnings.Select(UiText.Diagnostic));
        MessageBox.Show(
            this,
            UiText.Get("warningHeader") + "\n\n" + details,
            UiText.Get(titleKey),
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }
}
