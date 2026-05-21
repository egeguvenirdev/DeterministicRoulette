public enum GameAudioEventId
{
    None = 0,

    UiButtonClick,
    UiDropdownOpen,
    UiDropdownChanged,
    UiToggleOn,
    UiToggleOff,
    UiSliderChanged,
    UiInputFocus,

    SpinStart,
    BallDrop,
    RoundWon,
    RoundLost,
    ChipDrop,
    AmbientMusic
}

public enum GameAudioBus
{
    UI,
    SFX,
    Music
}
