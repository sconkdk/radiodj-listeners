using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace rdjInterface
{
    // Compile-time stand-ins for the real RadioDJ SDK types. Only members actually
    // used by the plugin are declared for IHost/TrackPlayer/Events/Features; IPlugin
    // declares every member of the real interface so PluginClass implements it fully.

    public interface IHost
    {
        string GetSetting(string section, string key, string defaultValue);
        bool SaveSetting(string section, string key, string value);
        void DebugAddLine(string line);
    }

    public class TrackPlayer
    {
    }

    public enum Features
    {
        MAIN_AUTODJ_STATE_CHANGED,
        MAIN_ASSISTED_STATE_CHANGED,
        MAIN_INSERT_STATE_CHANGED,
        MAIN_INPUT_STATE_CHANGED,
        MAIN_RECORD_STATE_CHANGED,
        MAIN_PAUSE_STATE_CHANGED,
        MAIN_STOP_STATE_CHANGED,
        MAIN_LOOP_STATE_CHANGED,
        MAIN_NOW_PLAYING_POSITION_CHANGED,
        MAIN_NOW_PLAYING_TRACK_CHANGED,
        MAIN_PLAYLIST_SHUFFLE,
        MAIN_PLAYLIST_CHANGED,
        MAIN_MUTE_STATE_CHANGED,
        MAIN_GET_PLAYLISTS,
        MAIN_GET_PLAYLIST_ITEM,
        MAIN_GET_SEARCH_RESULTS,
        MAIN_PLAYER_STARTED,
        TRACK_MODIFIED,
        TRACK_IMPORTED,
        TRACK_ENDING,
        Track_ENDED,
        AUX_PLAYING,
        AUXS_NUMBER_CHANGED,
        CART_PLAYING,
        CARTS_CHANGED,
        EVENTS_CHANGED,
        CATEGORIES_CHANGED,
        ROTATIONS_CHANGED,
        LANGUAGE_CHANGED,
        GUI_CHANGED,
        SOUND_ENGINE_CHANGED_START,
        SOUND_ENGINE_CHANGED,
        SOUND_OUTPUT_CHANGED,
    }

    public class Events
    {
        public class EventAction
        {
        }
    }

    public interface IPlugin
    {
        string PluginName { get; }
        string PluginTitle { get; }
        string PluginDescription { get; }
        string PluginVersion { get; }
        int PluginZone { get; }
        bool HasActions { get; }

        void Initialize(IHost host);
        void Closing();
        void ShowMain();
        void ShowConfig();
        void ShowAbout();
        UserControl LoadGUI();
        void KeyDown(object sender, KeyEventArgs e);
        bool RunAction(string action, string[] parameters, Guid? guid);
        List<Events.EventAction> AvailableActions();
        void AddTrack2Plugin(long trackId, long id2, int id3);
        void AddTrack2Plugin(TrackPlayer track, long id2, int id3);
        void StateChanged(Features feature, object data);
    }
}
