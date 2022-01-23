﻿using Microsoft.Xna.Framework.Graphics;
using Rampastring.XNAUI;
using Rampastring.XNAUI.XNAControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TSMapEditor.Settings;

namespace TSMapEditor.UI
{
    /// <summary>
    /// A single screen resolution.
    /// </summary>
    sealed class ScreenResolution : IComparable<ScreenResolution>
    {
        public ScreenResolution(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// The width of the resolution in pixels.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the resolution in pixels.
        /// </summary>
        public int Height { get; set; }

        public override string ToString()
        {
            return Width + "x" + Height;
        }

        public int CompareTo(ScreenResolution res2)
        {
            if (this.Width < res2.Width)
                return -1;
            else if (this.Width > res2.Width)
                return 1;
            else // equal
            {
                if (this.Height < res2.Height)
                    return -1;
                else if (this.Height > res2.Height)
                    return 1;
                else return 0;
            }
        }

        public override bool Equals(object obj)
        {
            var resolution = obj as ScreenResolution;

            if (resolution == null)
                return false;

            return CompareTo(resolution) == 0;
        }

        public override int GetHashCode()
        {
            return Width * 10000 + Height;
        }
    }

    public class SettingsPanel : EditorPanel
    {
        public SettingsPanel(WindowManager windowManager) : base(windowManager)
        {
        }

        private XNADropDown ddDisplayResolution;
        private XNADropDown ddRenderResolution;
        private XNACheckBox chkBorderless;
        private XNACheckBox chkUpscaleUI;

        public override void Initialize()
        {
            Width = 230;

            var lblHeader = new XNALabel(WindowManager);
            lblHeader.Name = nameof(lblHeader);
            lblHeader.FontIndex = Constants.UIBoldFont;
            lblHeader.Text = "Settings";
            lblHeader.Y = Constants.UIEmptyTopSpace;
            AddChild(lblHeader);
            lblHeader.CenterOnParentHorizontally();

            var lblDisplayResolution = new XNALabel(WindowManager);
            lblDisplayResolution.Name = nameof(lblDisplayResolution);
            lblDisplayResolution.Text = "Window Resolution:";
            lblDisplayResolution.X = Constants.UIEmptySideSpace;
            lblDisplayResolution.Y = lblHeader.Bottom + Constants.UIVerticalSpacing * 2;
            AddChild(lblDisplayResolution);

            ddDisplayResolution = new XNADropDown(WindowManager);
            ddDisplayResolution.Name = nameof(ddDisplayResolution);
            ddDisplayResolution.X = 120;
            ddDisplayResolution.Y = lblDisplayResolution.Y - 1;
            ddDisplayResolution.Width = Width - ddDisplayResolution.X - Constants.UIEmptySideSpace;
            AddChild(ddDisplayResolution);

            var lblRenderResolution = new XNALabel(WindowManager);
            lblRenderResolution.Name = nameof(lblRenderResolution);
            lblRenderResolution.Text = "Render Resolution:";
            lblRenderResolution.X = lblDisplayResolution.X;
            lblRenderResolution.Y = ddDisplayResolution.Bottom + Constants.UIVerticalSpacing + 1;
            AddChild(lblRenderResolution);

            ddRenderResolution = new XNADropDown(WindowManager);
            ddRenderResolution.Name = nameof(ddRenderResolution);
            ddRenderResolution.X = 120;
            ddRenderResolution.Y = lblRenderResolution.Y - 1;
            ddRenderResolution.Width = Width - ddRenderResolution.X - Constants.UIEmptySideSpace;
            AddChild(ddRenderResolution);

            chkBorderless = new XNACheckBox(WindowManager);
            chkBorderless.Name = nameof(chkBorderless);
            chkBorderless.X = Constants.UIEmptySideSpace;
            chkBorderless.Y = ddRenderResolution.Bottom + Constants.UIVerticalSpacing;
            chkBorderless.Text = "Borderless Mode";
            AddChild(chkBorderless);

            chkUpscaleUI = new XNACheckBox(WindowManager);
            chkUpscaleUI.Name = nameof(chkUpscaleUI);
            chkUpscaleUI.X = Constants.UIEmptySideSpace;
            chkUpscaleUI.Y = chkBorderless.Bottom + Constants.UIVerticalSpacing;
            chkUpscaleUI.Text = "Upscale Windows";
            AddChild(chkUpscaleUI);

            const int MinWidth = 1024;
            const int MinHeight = 600;
            const int MaxWidth = 16384;
            const int MaxHeight = 16384;

            var resolutions = GetResolutions(MinWidth, MinHeight, MaxWidth, MaxHeight);
            resolutions.ForEach(res => ddDisplayResolution.AddItem(new XNADropDownItem() { Text = res.Width + "x" + res.Height, Tag = res }));
            resolutions.ForEach(res => ddRenderResolution.AddItem(new XNADropDownItem() { Text = res.Width + "x" + res.Height, Tag = res }));

            LoadSettings();

            base.Initialize();
        }

        private void LoadSettings()
        {
            var userSettings = UserSettings.Instance;

            string dispResolution = userSettings.ResolutionWidth.GetValue() + "x" + userSettings.ResolutionHeight.GetValue();
            ddDisplayResolution.SelectedIndex = ddDisplayResolution.Items.FindIndex(i => i.Text == dispResolution);

            string renderResolution = userSettings.RenderResolutionWidth.GetValue() + "x" + userSettings.RenderResolutionHeight.GetValue();
            ddRenderResolution.SelectedIndex = ddRenderResolution.Items.FindIndex(i => i.Text == renderResolution);

            chkBorderless.Checked = userSettings.Borderless;
            chkUpscaleUI.Checked = userSettings.UpscaleUI;
        }

        public void ApplySettings()
        {
            var userSettings = UserSettings.Instance;

            ScreenResolution dispRes = null;
            ScreenResolution renderRes = null;

            userSettings.Borderless.UserDefinedValue = chkBorderless.Checked;
            userSettings.UpscaleUI.UserDefinedValue = chkUpscaleUI.Checked;

            if (ddDisplayResolution.SelectedItem != null)
            {
                dispRes = (ScreenResolution)ddDisplayResolution.SelectedItem.Tag;
                userSettings.ResolutionWidth.UserDefinedValue = dispRes.Width;
                userSettings.ResolutionHeight.UserDefinedValue = dispRes.Height;

                if (dispRes.Width >= Screen.PrimaryScreen.Bounds.Width && dispRes.Height >= Screen.PrimaryScreen.Bounds.Height && chkBorderless.Checked)
                    userSettings.FullscreenWindowed.UserDefinedValue = chkBorderless.Checked;
                else
                    userSettings.FullscreenWindowed.UserDefinedValue = false;
            }

            if (ddRenderResolution.SelectedItem != null)
            {
                renderRes = (ScreenResolution)ddRenderResolution.SelectedItem.Tag;
                userSettings.RenderResolutionWidth.UserDefinedValue = renderRes.Width;
                userSettings.RenderResolutionHeight.UserDefinedValue = renderRes.Height;
            }
        }

        private List<ScreenResolution> GetResolutions(int minWidth, int minHeight, int maxWidth, int maxHeight)
        {
            var screenResolutions = new List<ScreenResolution>();

            foreach (DisplayMode dm in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
            {
                if (dm.Width < minWidth || dm.Height < minHeight || dm.Width > maxWidth || dm.Height > maxHeight)
                    continue;

                var resolution = new ScreenResolution(dm.Width, dm.Height);

                // SupportedDisplayModes can include the same resolution multiple times
                // because it takes the refresh rate into consideration.
                // Which means that we have to check if the resolution is already listed
                if (screenResolutions.Find(res => res.Equals(resolution)) != null)
                    continue;

                screenResolutions.Add(resolution);
            }

            return screenResolutions;
        }
    }
}
