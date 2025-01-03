﻿using System;
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

        private const string RegexDigitsOnly = "^[0-9]+$";

        private EnhancedGameData _enhancedGameData;

        public FriendlySetPlayTimeWindow(ILogger logger, IPlayniteAPI playniteAPI, Game selectedGame)
        {
            InitializeComponent();

            DataContext = this;

            _logger = logger;
            _playniteAPI = playniteAPI;
            _selectedGame = selectedGame;

            _enhancedGameData = LoadCurrentGameData(_logger, _playniteAPI, _selectedGame);
        }

        private EnhancedGameData LoadCurrentGameData(ILogger logger, IPlayniteAPI playniteAPI, Game selectedGame)
        {
            return _enhancedGameData = new EnhancedGameData(logger, playniteAPI, selectedGame);
        }

        private void CompletionStatusComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CompletionStatusCheckBox.IsChecked = true;
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
            if (LastActivityDatePicker.SelectedDate.HasValue)
            {
                LastActivityCheckBox.IsChecked = true;

                _enhancedGameData.LastActivity = LastActivityDatePicker.SelectedDate.Value;
            }
        }

        private static bool VerifyTextBoxInput(TextBox inputField)
        {
            string input = inputField.Text;
            if (string.IsNullOrEmpty(input) || Regex.IsMatch(input, RegexDigitsOnly))
            {
                inputField.ClearValue(BackgroundProperty);
                return true;
            }

            inputField.Background = Brushes.Red;
            return false;
        }

        private bool VerifyPlayTimeInput()
        {
            return VerifyTextBoxInput(PlayTimeSeconds) && VerifyTextBoxInput(PlayTimeMinutes) && VerifyTextBoxInput(PlayTimeHours) && VerifyTextBoxInput(PlayTimeDays);
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
