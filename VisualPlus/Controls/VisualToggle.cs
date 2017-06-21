﻿namespace VisualPlus.Controls
{
    #region Namespace

    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Text;
    using System.Windows.Forms;

    using VisualPlus.Enums;
    using VisualPlus.Framework;
    using VisualPlus.Framework.GDI;
    using VisualPlus.Framework.Handlers;
    using VisualPlus.Framework.Structure;
    using VisualPlus.Localization;
    using VisualPlus.Styles;

    #endregion

    [ToolboxItem(true)]
    [ToolboxBitmap(typeof(Control))]
    [DefaultEvent("ToggledChanged")]
    [DefaultProperty("Toggled")]
    [Description("The Visual Toggle")]
    [Designer(DesignManager.VisualToggle)]
    public sealed class VisualToggle : Control
    {
        #region Variables

        private readonly Timer animationTimer = new Timer
            {
                Interval = 1
            };

        private Gradient backgroundDisabledGradient = new Gradient();
        private Gradient backgroundEnabledGradient = new Gradient();
        private Border border = new Border();
        private Border buttonBorder = new Border();
        private Gradient buttonDisabledGradient = new Gradient();
        private Gradient buttonGradient = new Gradient();
        private Rectangle buttonRectangle;
        private Size buttonSize = new Size(20, 20);
        private GraphicsPath controlGraphicsPath;
        private ControlState controlState = ControlState.Normal;
        private Point endPoint;
        private Color foreColor = Settings.DefaultValue.Font.ForeColor;
        private Point startPoint;
        private StyleManager styleManager = new StyleManager();
        private Color textDisabledColor = Settings.DefaultValue.Font.ForeColorDisabled;
        private string textProcessor;
        private TextRenderingHint textRendererHint = Settings.DefaultValue.TextRenderingHint;
        private bool toggled;
        private int toggleLocation;
        private ToggleTypes toggleType = ToggleTypes.YesNo;

        #endregion

        #region Constructors

        public VisualToggle()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor,
                true);

            UpdateStyles();

            BackColor = Color.Transparent;
            Size = new Size(50, 25);
            Font = new Font(Settings.DefaultValue.Font.FontFamily, Settings.DefaultValue.Font.FontSize, Settings.DefaultValue.Font.FontStyle);
            animationTimer.Tick += AnimationTimerTick;

            border.Rounding = 20;
            buttonBorder.Rounding = 18;

            DefaultGradient();
            ConfigureStyleManager();
        }

        public delegate void ToggledChangedEventHandler();

        public event ToggledChangedEventHandler ToggledChanged;

        public enum ToggleTypes
        {
            /// <summary>Yes / No toggle.</summary>
            YesNo,

            /// <summary>On / Off toggle.</summary>
            OnOff,

            /// <summary>I / O toggle.</summary>
            IO
        }

        #endregion

        #region Properties

        [TypeConverter(typeof(GradientConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Localize.Category.Appearance)]
        public Gradient BackgroundDisabled
        {
            get
            {
                return backgroundDisabledGradient;
            }

            set
            {
                backgroundDisabledGradient = value;
                Invalidate();
            }
        }

        [TypeConverter(typeof(GradientConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Localize.Category.Appearance)]
        public Gradient BackgroundEnabled
        {
            get
            {
                return backgroundEnabledGradient;
            }

            set
            {
                backgroundEnabledGradient = value;
                Invalidate();
            }
        }

        [TypeConverter(typeof(BorderConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Localize.Category.Appearance)]
        public Border Border
        {
            get
            {
                return border;
            }

            set
            {
                border = value;
                Invalidate();
            }
        }

        [TypeConverter(typeof(BorderConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Localize.Category.Appearance)]
        public Border ButtonBorder
        {
            get
            {
                return buttonBorder;
            }

            set
            {
                buttonBorder = value;
                Invalidate();
            }
        }

        [TypeConverter(typeof(GradientConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Localize.Category.Appearance)]
        public Gradient ButtonDisabled
        {
            get
            {
                return buttonDisabledGradient;
            }

            set
            {
                buttonDisabledGradient = value;
                Invalidate();
            }
        }

        [TypeConverter(typeof(GradientConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Localize.Category.Appearance)]
        public Gradient ButtonGradient
        {
            get
            {
                return buttonGradient;
            }

            set
            {
                buttonGradient = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Layout)]
        [Description(Localize.Description.ButtonSize)]
        public Size ButtonSize
        {
            get
            {
                return buttonSize;
            }

            set
            {
                buttonSize = value;
                Invalidate();
            }
        }

        public new Color ForeColor
        {
            get
            {
                return foreColor;
            }

            set
            {
                base.ForeColor = value;
                foreColor = value;
                Invalidate();
            }
        }

        [TypeConverter(typeof(VisualStyleManagerConverter))]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category(Localize.Category.Appearance)]
        public StyleManager StyleManager
        {
            get
            {
                return styleManager;
            }

            set
            {
                styleManager = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.ComponentColor)]
        public Color TextDisabledColor
        {
            get
            {
                return textDisabledColor;
            }

            set
            {
                textDisabledColor = value;
                Invalidate();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.TextRenderingHint)]
        public TextRenderingHint TextRendering
        {
            get
            {
                return textRendererHint;
            }

            set
            {
                textRendererHint = value;
                Invalidate();
            }
        }

        [DefaultValue(false)]
        [Category(Localize.Category.Behavior)]
        [Description(Localize.Description.Toggled)]
        public bool Toggled
        {
            get
            {
                return toggled;
            }

            set
            {
                toggled = value;
                Invalidate();

                ToggledChanged?.Invoke();
            }
        }

        [Category(Localize.Category.Appearance)]
        [Description(Localize.Description.ToggleType)]
        public ToggleTypes Type
        {
            get
            {
                return toggleType;
            }

            set
            {
                toggleType = value;
                Invalidate();
            }
        }

        #endregion

        #region Events

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            animationTimer.Start();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            controlState = ControlState.Hover;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            controlState = ControlState.Normal;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Toggled = !Toggled;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.Clear(Parent.BackColor);
            graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.TextRenderingHint = textRendererHint;

            if (styleManager.LockedStyle)
            {
                ConfigureStyleManager();
            }

            controlGraphicsPath = GDI.GetBorderShape(ClientRectangle, border.Shape, border.Rounding);

            // Update button location points
            startPoint = new Point(0 + 2, (ClientRectangle.Height / 2) - (buttonSize.Height / 2));
            endPoint = new Point(ClientRectangle.Width - buttonSize.Width - 2, (ClientRectangle.Height / 2) - (buttonSize.Height / 2));

            Gradient buttonTemp = Enabled ? buttonGradient : buttonDisabledGradient;
            Gradient backTemp = Enabled ? backgroundEnabledGradient : backgroundDisabledGradient;

            var gradientPoints = new[] { new Point { X = ClientRectangle.Width, Y = 0 }, new Point { X = ClientRectangle.Width, Y = ClientRectangle.Height } };
            LinearGradientBrush buttonGradientBrush = GDI.CreateGradientBrush(buttonTemp.Colors, gradientPoints, buttonTemp.Angle, buttonTemp.Positions);
            LinearGradientBrush backgroundGradientBrush = GDI.CreateGradientBrush(backTemp.Colors, gradientPoints, backTemp.Angle, backTemp.Positions);

            graphics.FillPath(backgroundGradientBrush, controlGraphicsPath);

            if (border.Visible)
            {
                GDI.DrawBorderType(graphics, controlState, controlGraphicsPath, border.Thickness, border.Color, border.HoverColor, border.HoverVisible);
            }

            // Determines button state to draw
            Point buttonPoint = toggled ? endPoint : startPoint;
            buttonRectangle = new Rectangle(buttonPoint, buttonSize);

            DrawToggleType(graphics);

            GraphicsPath buttonPath = GDI.GetBorderShape(buttonRectangle, buttonBorder.Shape, buttonBorder.Rounding);
            graphics.FillPath(buttonGradientBrush, buttonPath);

            if (buttonBorder.Visible)
            {
                GDI.DrawBorderType(graphics, controlState, buttonPath, buttonBorder.Thickness, buttonBorder.Color, buttonBorder.HoverColor, buttonBorder.HoverVisible);
            }
        }

        private void AnimationTimerTick(object sender, EventArgs e)
        {
            // Create a slide animation when toggled.
            if (toggled)
            {
                if (toggleLocation >= 100)
                {
                    return;
                }

                toggleLocation += 10;
                Invalidate(false);
            }
            else if (toggleLocation > 0)
            {
                toggleLocation -= 10;
                Invalidate(false);
            }
        }

        private void ConfigureStyleManager()
        {
            if (styleManager.VisualStylesManager != null)
            {
                // Load style manager settings 
                IBorder borderStyle = styleManager.VisualStylesManager.BorderStyle;
                IFont fontStyle = styleManager.VisualStylesManager.FontStyle;

                border.Color = borderStyle.Color;
                border.HoverColor = borderStyle.HoverColor;
                border.HoverVisible = styleManager.VisualStylesManager.BorderHoverVisible;
                border.Rounding = styleManager.VisualStylesManager.BorderRounding;
                border.Shape = styleManager.VisualStylesManager.BorderShape;
                border.Thickness = styleManager.VisualStylesManager.BorderThickness;
                border.Visible = styleManager.VisualStylesManager.BorderVisible;

                buttonBorder.Color = borderStyle.Color;
                buttonBorder.HoverColor = borderStyle.HoverColor;
                buttonBorder.HoverVisible = styleManager.VisualStylesManager.BorderHoverVisible;
                buttonBorder.Rounding = styleManager.VisualStylesManager.BorderRounding;
                buttonBorder.Shape = styleManager.VisualStylesManager.BorderShape;
                buttonBorder.Thickness = styleManager.VisualStylesManager.BorderThickness;
                buttonBorder.Visible = styleManager.VisualStylesManager.BorderVisible;

                Font = new Font(fontStyle.FontFamily, fontStyle.FontSize, fontStyle.FontStyle);
                foreColor = fontStyle.ForeColor;
                textRendererHint = styleManager.VisualStylesManager.TextRenderingHint;
            }
            else
            {
                // Load default settings
                border.HoverVisible = Settings.DefaultValue.BorderHoverVisible;
                border.Rounding = Settings.DefaultValue.Rounding.ToggleBorder;
                border.Shape = Settings.DefaultValue.BorderShape;
                border.Thickness = Settings.DefaultValue.BorderThickness;
                border.Visible = Settings.DefaultValue.BorderVisible;

                buttonBorder.HoverVisible = Settings.DefaultValue.BorderHoverVisible;
                buttonBorder.Rounding = Settings.DefaultValue.Rounding.ToggleButton;
                buttonBorder.Shape = Settings.DefaultValue.BorderShape;
                buttonBorder.Thickness = Settings.DefaultValue.BorderThickness;
                buttonBorder.Visible = Settings.DefaultValue.BorderVisible;

                Font = new Font(Settings.DefaultValue.Font.FontFamily, Settings.DefaultValue.Font.FontSize, Settings.DefaultValue.Font.FontStyle);
                foreColor = Settings.DefaultValue.Font.ForeColor;
                textRendererHint = Settings.DefaultValue.TextRenderingHint;
            }
        }

        private void DefaultGradient()
        {
            backgroundEnabledGradient.Colors = Settings.DefaultValue.Progress.Background.Colors;
            backgroundEnabledGradient.Positions = Settings.DefaultValue.Progress.Background.Positions;

            backgroundDisabledGradient.Colors = Settings.DefaultValue.Control.ControlDisabled.Colors;
            backgroundDisabledGradient.Positions = Settings.DefaultValue.Control.ControlDisabled.Positions;

            buttonGradient.Colors = Settings.DefaultValue.Control.ControlEnabled.Colors;
            buttonGradient.Positions = Settings.DefaultValue.Control.ControlEnabled.Positions;

            buttonDisabledGradient.Colors = Settings.DefaultValue.Control.ControlDisabled.Colors;
            buttonDisabledGradient.Positions = Settings.DefaultValue.Control.ControlDisabled.Positions;
        }

        private void DrawToggleType(Graphics graphics)
        {
            // Determines the type of toggle to draw.
            switch (toggleType)
            {
                case ToggleTypes.YesNo:
                    {
                        textProcessor = Toggled ? "Yes" : "No";

                        break;
                    }

                case ToggleTypes.OnOff:
                    {
                        textProcessor = Toggled ? "On" : "Off";

                        break;
                    }

                case ToggleTypes.IO:
                    {
                        textProcessor = Toggled ? "I" : "O";

                        break;
                    }
            }

            // Set control state color
            foreColor = Enabled ? foreColor : textDisabledColor;

            // Draw string
            Rectangle textBoxRectangle;

            const int XOff = 5;
            const int XOn = 7;

            if (toggled)
            {
                textBoxRectangle = new Rectangle(XOff, 0, Width - 1, Height - 1);
            }
            else
            {
                textBoxRectangle = new Rectangle(Width - (int)Font.SizeInPoints - (XOn * 2), 0, Width - 1, Height - 1);
            }

            StringFormat stringFormat = new StringFormat
                {
                    // Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

            // Draw the string
            graphics.DrawString(
                textProcessor,
                new Font(Font.FontFamily, 7f, Font.Style),
                new SolidBrush(foreColor),
                textBoxRectangle,
                stringFormat);
        }

        #endregion

        #region Methods

        public class PillStyle
        {
            #region Variables

            public bool Left;
            public bool Right;

            #endregion
        }

        #endregion
    }
}