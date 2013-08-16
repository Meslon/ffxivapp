﻿// FFXIVAPP.Plugin.Parse
// Filter.Cure.cs
// 
// © 2013 Ryan Wilson

#region Usings

using System;
using System.Text.RegularExpressions;
using FFXIVAPP.Plugin.Parse.Enums;
using FFXIVAPP.Plugin.Parse.Helpers;
using FFXIVAPP.Plugin.Parse.Models;
using FFXIVAPP.Plugin.Parse.Models.Events;
using NLog;

#endregion

namespace FFXIVAPP.Plugin.Parse.Utilities
{
    public static partial class Filter
    {
        private static void ProcessCure(Event e, Expressions exp)
        {
            var line = new Line
            {
                RawLine = e.RawLine
            };
            var cure = Regex.Match("ph", @"^\.$");
            switch (e.Subject)
            {
                case EventSubject.You:
                case EventSubject.Party:
                    switch (e.Direction)
                    {
                        case EventDirection.Self:
                        case EventDirection.You:
                        case EventDirection.Party:
                            cure = exp.pCure;
                            if (cure.Success)
                            {
                                line.Source = _lastPlayer;
                                if (e.Subject == EventSubject.You)
                                {
                                    line.Source = String.IsNullOrWhiteSpace(Constants.CharacterName) ? "You" : Constants.CharacterName;
                                }
                                UpdatePlayerHealing(cure, line, exp);
                            }
                            break;
                    }
                    break;
            }
            if (cure.Success)
            {
                return;
            }
            ClearLast();
            ParsingLogHelper.Log(LogManager.GetCurrentClassLogger(), "Cure", e, exp);
        }

        private static void UpdatePlayerHealing(Match cure, Line line, Expressions exp)
        {
            try
            {
                line.Action = _lastPlayerAction;
                line.Amount = cure.Groups["amount"].Success ? Convert.ToDecimal(cure.Groups["amount"].Value) : 0m;
                line.Crit = cure.Groups["crit"].Success;
                line.Modifier = cure.Groups["modifier"].Success ? Convert.ToDecimal(cure.Groups["modifier"].Value) / 100 : 0m;
                line.Target = Convert.ToString(cure.Groups["target"].Value);
                if (Regex.IsMatch(line.Target.ToLower(), exp.You))
                {
                    line.Target = String.IsNullOrWhiteSpace(Constants.CharacterName) ? "You" : Constants.CharacterName;
                }
                line.HpMpTp = Convert.ToString(cure.Groups["type"].Value.ToUpper());
                if (line.IsEmpty() || (!_isMulti && _lastEvent.Type != EventType.Actions && _lastEvent.Type != EventType.Items))
                {
                    ClearLast(true);
                    return;
                }
                _lastPlayer = line.Source;
                ParseControl.Instance.Timeline.GetSetPlayer(line.Source)
                    .SetHealing(line);
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }
    }
}