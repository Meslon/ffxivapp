// FFXIVAPP.Plugin.Parse
// TimelineChangedEvent.cs
// 
// � 2013 Ryan Wilson

#region Usings

using System;
using FFXIVAPP.Plugin.Parse.Enums;

#endregion

namespace FFXIVAPP.Plugin.Parse.Models.Timelines
{
    public class TimelineChangedEvent : EventArgs
    {
        /// <summary>
        /// </summary>
        /// <param name="eventType"> </param>
        /// <param name="eventArgs"> </param>
        public TimelineChangedEvent(TimelineEventType eventType, params object[] eventArgs)
        {
            EventType = eventType;
            EventArgs = eventArgs;
        }

        private TimelineEventType EventType { get; set; }
        private object[] EventArgs { get; set; }
    }
}