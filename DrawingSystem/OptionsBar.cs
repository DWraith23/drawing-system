using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace DrawingSystem;

public partial class OptionsBar : Control
{
    [Signal] public delegate void LineStylesButtonPressedEventHandler(int index);
    [Signal] public delegate void LineStylesButtonToggledEventHandler(int index, bool toggledOn);
    [Signal] public delegate void LineColorChangedEventHandler(Color color);
    [Signal] public delegate void LineThicknessChangedEventHandler(int thickness);
    [Signal] public delegate void ClearButtonPressedEventHandler();
    [Signal] public delegate void MenuButtonPressedEventHandler(int index);

    // Base contents
    protected BoxContainer Contents => GetNode<BoxContainer>("Panel/Contents");
    // Style selection
    protected RolloutMenu LineStyles => Contents.GetNode<RolloutMenu>("Line Styles");
    // Line properties
    protected RolloutPanel LineProperties => Contents.GetNode<RolloutPanel>("Line Properties");
    protected BoxContainer LinePropertiesContents => LineProperties.GetNode<BoxContainer>("Child Positioner/Panel/Properties");
    protected ColorPickerButton ColorSelect => LinePropertiesContents.GetNode<ColorPickerButton>("Color");
    protected Label ThicknessLabel => LinePropertiesContents.GetNode<Label>("Thickness Label");
    protected HSlider ThicknessSlider => LinePropertiesContents.GetNode<HSlider>("Thickness");
    // Other
    protected Button ClearButton => Contents.GetNode<Button>("Clear");
    protected RolloutMenu Menu => Contents.GetNode<RolloutMenu>("Menu");


    public override void _Ready()
    {
        ConnectSignals();
        LineStyles.Buttons.First().SetPressedNoSignal(true);
    }

    private void ConnectSignals()
    {
        ThicknessSlider.ValueChanged += OnThicknessSliderValueChanged;

        LineStyles.OnButtonPressed += OnLineStylesButtonPressed;
        LineStyles.OnButtonToggled += OnLineStylesButtonToggled;
        ColorSelect.ColorChanged += OnLineColorChanged;
        ThicknessSlider.ValueChanged += OnLineThicknessChanged;

        ClearButton.Pressed += OnClearButtonPressed;

        Menu.OnButtonPressed += OnMenuButtonPressed;
    }


    #region Events

    private void OnThicknessSliderValueChanged(double value) => ThicknessLabel.Text = $"Size({(int)value}):";

    private void OnLineStylesButtonPressed(int index) => EmitSignal(SignalName.LineStylesButtonPressed, index);
    private void OnLineStylesButtonToggled(int index, bool toggledOn) => EmitSignal(SignalName.LineStylesButtonToggled, index, toggledOn);
    private void OnLineColorChanged(Color color) => EmitSignal(SignalName.LineColorChanged, color);
    private void OnLineThicknessChanged(double thickness) => EmitSignal(SignalName.LineThicknessChanged, (int)thickness);
    private void OnClearButtonPressed() => EmitSignal(SignalName.ClearButtonPressed);
    private void OnMenuButtonPressed(int index) => EmitSignal(SignalName.MenuButtonPressed, index);
    

    #endregion

}
