// This file is part of Friendly Set Play Time. A Playnite extension to simplify the input of play time values.
// Copyright CanRanBan, 2022-2025, Licensed under the EUPL-1.2 or later.

using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace FriendlySetPlayTime
{
    public partial class FriendlySetPlayTimeWindow
    {
        private readonly ILogger _logger;
        private readonly IPlayniteAPI _playniteAPI;
        private readonly Game _selectedGame;

        private readonly EnhancedGameData _enhancedGameData;

        private bool _loadingCurrentCompletionStatusFinished;
        private bool _loadingCurrentLastActivityStepOneFinished;
        private bool _loadingCurrentLastActivityStepTwoFinished;

        private const string RegexVerifyDecimalInput = @"^\+?[0-9]+([,.][0-9]+)?$";
        private const string RegexVerifyDigitsOnly = @"^\+?[0-9]+$";

        public FriendlySetPlayTimeWindow(ILogger logger, IPlayniteAPI playniteAPI, Game selectedGame)
        {
            InitializeComponent();

            _logger = logger;
            _playniteAPI = playniteAPI;
            _selectedGame = selectedGame;
            _enhancedGameData = new EnhancedGameData(_logger, _playniteAPI, _selectedGame);

            DataContext = _enhancedGameData;
        }

        private void CompletionStatusComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingCurrentCompletionStatusFinished)
            {
                CompletionStatusCheckBox.IsChecked = true;
            }
            else
            {
                _loadingCurrentCompletionStatusFinished = true;
            }
        }

        private void LastActivityRadioButtonToday_OnClick(object sender, RoutedEventArgs e)
        {
            LastActivityCheckBox.IsChecked = true;

            _enhancedGameData.LastActivity = DateTime.Today.Date;
        }

        private void LastActivityRadioButtonDatePicker_OnClick(object sender, RoutedEventArgs e)
        {
            LastActivityCheckBox.IsChecked = true;
        }

        private void LastActivityDatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loadingCurrentLastActivityStepOneFinished && _loadingCurrentLastActivityStepTwoFinished)
            {
                LastActivityCheckBox.IsChecked = true;
            }
            else if (!_loadingCurrentLastActivityStepOneFinished && !_loadingCurrentLastActivityStepTwoFinished)
            {
                _loadingCurrentLastActivityStepOneFinished = true;
            }
            else if (_loadingCurrentLastActivityStepOneFinished && !_loadingCurrentLastActivityStepTwoFinished)
            {
                _loadingCurrentLastActivityStepTwoFinished = true;
            }
        }

        private static bool VerifyTextBoxInput(TextBox inputField, bool allowDecimalInput)
        {
            string input = inputField.Text;
            if (allowDecimalInput)
            {
                if (Regex.IsMatch(input, RegexVerifyDecimalInput))
                {
                    inputField.ClearValue(BorderBrushProperty);
                    return true;
                }
            }
            else
            {
                if (Regex.IsMatch(input, RegexVerifyDigitsOnly))
                {
                    inputField.ClearValue(BorderBrushProperty);
                    return true;
                }
            }

            inputField.BorderBrush = Brushes.Red;
            return false;
        }

        private void TextBox_DecimalInput_OnLostFocus(object sender, RoutedEventArgs e)
        {
            VerifyTextBoxInput((TextBox)sender, true);
        }

        private void TextBox_DigitsOnly_OnLostFocus(object sender, RoutedEventArgs e)
        {
            VerifyTextBoxInput((TextBox)sender, false);
        }

        private bool VerifyPlayTimeInput()
        {
            return VerifyTextBoxInput(PlayTimeSeconds, false)
                   && VerifyTextBoxInput(PlayTimeMinutes, true)
                   && VerifyTextBoxInput(PlayTimeHours, true)
                   && VerifyTextBoxInput(PlayTimeDays, true);
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!VerifyPlayTimeInput())
                {
                    return;
                }

                _selectedGame.Playtime = _enhancedGameData.SimplifyPlayTime();

                if (CompletionStatusCheckBox.IsChecked.GetValueOrDefault())
                {
                    _selectedGame.CompletionStatusId = _enhancedGameData.SimplifyCompletionStatus();
                }

                if (LastActivityCheckBox.IsChecked.GetValueOrDefault())
                {
                    _selectedGame.LastActivity = _enhancedGameData.LastActivity;
                }
                _playniteAPI.Database.Games.Update(_selectedGame);
                ((Window)Parent).Close();
            }
            catch (Exception saveException)
            {
                const string errorMessage = "Failed to save data for selected game.";
                const string errorCaption = "Friendly Set Play Time - Save Error";
                _logger.Error(saveException, errorMessage);
                _playniteAPI.Dialogs.ShowErrorMessage(errorMessage + @"\n\nException: " + saveException.Message, errorCaption);
            }
        }
    }
}
