﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolsfactory.Protocols.AndroidTVRemote
{
    public enum RCKeyCode
    {
        Key_UNKNOWN = 0,
        Key_SOFT_LEFT = 1,
        Key_SOFT_RIGHT = 2,
        Key_HOME = 3,
        Key_BACK = 4,
        Key_CALL = 5,
        Key_ENDCALL = 6,
        Key_0 = 7,
        Key_1 = 8,
        Key_2 = 9,
        Key_3 = 10,
        Key_4 = 11,
        Key_5 = 12,
        Key_6 = 13,
        Key_7 = 14,
        Key_8 = 15,
        Key_9 = 16,
        Key_STAR = 17,
        Key_POUND = 18,
        Key_DPAD_UP = 19,
        Key_DPAD_DOWN = 20,
        Key_DPAD_LEFT = 21,
        Key_DPAD_RIGHT = 22,
        Key_DPAD_CENTER = 23,
        Key_VOLUME_UP = 24,
        Key_VOLUME_DOWN = 25,
        Key_POWER = 26,
        Key_CAMERA = 27,
        Key_CLEAR = 28,
        Key_A = 29,
        Key_B = 30,
        Key_C = 31,
        Key_D = 32,
        Key_E = 33,
        Key_F = 34,
        Key_G = 35,
        Key_H = 36,
        Key_I = 37,
        Key_J = 38,
        Key_K = 39,
        Key_L = 40,
        Key_M = 41,
        Key_N = 42,
        Key_O = 43,
        Key_P = 44,
        Key_Q = 45,
        Key_R = 46,
        Key_S = 47,
        Key_T = 48,
        Key_U = 49,
        Key_V = 50,
        Key_W = 51,
        Key_X = 52,
        Key_Y = 53,
        Key_Z = 54,
        Key_COMMA = 55,
        Key_PERIOD = 56,
        Key_ALT_LEFT = 57,
        Key_ALT_RIGHT = 58,
        Key_SHIFT_LEFT = 59,
        Key_SHIFT_RIGHT = 60,
        Key_TAB = 61,
        Key_SPACE = 62,
        Key_SYM = 63,
        Key_EXPLORER = 64,
        Key_ENVELOPE = 65,
        Key_ENTER = 66,
        Key_DEL = 67,
        Key_GRAVE = 68,
        Key_MINUS = 69,
        Key_EQUALS = 70,
        Key_LEFT_BRACKET = 71,
        Key_RIGHT_BRACKET = 72,
        Key_BACKSLASH = 73,
        Key_SEMICOLON = 74,
        Key_APOSTROPHE = 75,
        Key_SLASH = 76,
        Key_AT = 77,
        Key_NUM = 78,
        Key_HEADSETHOOK = 79,
        Key_FOCUS = 80,
        Key_PLUS = 81,
        Key_MENU = 82,
        Key_NOTIFICATION = 83,
        Key_SEARCH = 84,
        Key_MEDIA_PLAY_PAUSE = 85,
        Key_MEDIA_STOP = 86,
        Key_MEDIA_NEXT = 87,
        Key_MEDIA_PREVIOUS = 88,
        Key_MEDIA_REWIND = 89,
        Key_MEDIA_FAST_FORWARD = 90,
        Key_MUTE = 91,
        Key_PAGE_UP = 92,
        Key_PAGE_DOWN = 93,
        Key_PICTSYMBOLS = 94,
        Key_SWITCH_CHARSET = 95,
        Key_BUTTON_A = 96,
        Key_BUTTON_B = 97,
        Key_BUTTON_C = 98,
        Key_BUTTON_X = 99,
        Key_BUTTON_Y = 100,
        Key_BUTTON_Z = 101,
        Key_BUTTON_L1 = 102,
        Key_BUTTON_R1 = 103,
        Key_BUTTON_L2 = 104,
        Key_BUTTON_R2 = 105,
        Key_BUTTON_THUMBL = 106,
        Key_BUTTON_THUMBR = 107,
        Key_BUTTON_START = 108,
        Key_BUTTON_SELECT = 109,
        Key_BUTTON_MODE = 110,
        Key_ESCAPE = 111,
        Key_FORWARD_DEL = 112,
        Key_CTRL_LEFT = 113,
        Key_CTRL_RIGHT = 114,
        Key_CAPS_LOCK = 115,
        Key_SCROLL_LOCK = 116,
        Key_META_LEFT = 117,
        Key_META_RIGHT = 118,
        Key_FUNCTION = 119,
        Key_SYSRQ = 120,
        Key_BREAK = 121,
        Key_MOVE_HOME = 122,
        Key_MOVE_END = 123,
        Key_INSERT = 124,
        Key_FORWARD = 125,
        Key_MEDIA_PLAY = 126,
        Key_MEDIA_PAUSE = 127,
        Key_MEDIA_CLOSE = 128,
        Key_MEDIA_EJECT = 129,
        Key_MEDIA_RECORD = 130,
        Key_F1 = 131,
        Key_F2 = 132,
        Key_F3 = 133,
        Key_F4 = 134,
        Key_F5 = 135,
        Key_F6 = 136,
        Key_F7 = 137,
        Key_F8 = 138,
        Key_F9 = 139,
        Key_F10 = 140,
        Key_F11 = 141,
        Key_F12 = 142,
        Key_NUM_LOCK = 143,
        Key_NUMPAD_0 = 144,
        Key_NUMPAD_1 = 145,
        Key_NUMPAD_2 = 146,
        Key_NUMPAD_3 = 147,
        Key_NUMPAD_4 = 148,
        Key_NUMPAD_5 = 149,
        Key_NUMPAD_6 = 150,
        Key_NUMPAD_7 = 151,
        Key_NUMPAD_8 = 152,
        Key_NUMPAD_9 = 153,
        Key_NUMPAD_DIVIDE = 154,
        Key_NUMPAD_MULTIPLY = 155,
        Key_NUMPAD_SUBTRACT = 156,
        Key_NUMPAD_ADD = 157,
        Key_NUMPAD_DOT = 158,
        Key_NUMPAD_COMMA = 159,
        Key_NUMPAD_ENTER = 160,
        Key_NUMPAD_EQUALS = 161,
        Key_NUMPAD_LEFT_PAREN = 162,
        Key_NUMPAD_RIGHT_PAREN = 163,
        Key_VOLUME_MUTE = 164,
        Key_INFO = 165,
        Key_CHANNEL_UP = 166,
        Key_CHANNEL_DOWN = 167,
        Key_ZOOM_IN = 168,
        Key_ZOOM_OUT = 169,
        Key_TV = 170,
        Key_WINDOW = 171,
        Key_GUIDE = 172,
        Key_DVR = 173,
        Key_BOOKMARK = 174,
        Key_CAPTIONS = 175,
        Key_SETTINGS = 176,
        Key_TV_POWER = 177,
        Key_TV_INPUT = 178,
        Key_STB_POWER = 179,
        Key_STB_INPUT = 180,
        Key_AVR_POWER = 181,
        Key_AVR_INPUT = 182,
        Key_PROG_RED = 183,
        Key_PROG_GREEN = 184,
        Key_PROG_YELLOW = 185,
        Key_PROG_BLUE = 186,
        Key_APP_SWITCH = 187,
        Key_BUTTON_1 = 188,
        Key_BUTTON_2 = 189,
        Key_BUTTON_3 = 190,
        Key_BUTTON_4 = 191,
        Key_BUTTON_5 = 192,
        Key_BUTTON_6 = 193,
        Key_BUTTON_7 = 194,
        Key_BUTTON_8 = 195,
        Key_BUTTON_9 = 196,
        Key_BUTTON_10 = 197,
        Key_BUTTON_11 = 198,
        Key_BUTTON_12 = 199,
        Key_BUTTON_13 = 200,
        Key_BUTTON_14 = 201,
        Key_BUTTON_15 = 202,
        Key_BUTTON_16 = 203,
        Key_LANGUAGE_SWITCH = 204,
        Key_MANNER_MODE = 205,
        Key_3D_MODE = 206,
        Key_CONTACTS = 207,
        Key_CALENDAR = 208,
        Key_MUSIC = 209,
        Key_CALCULATOR = 210,
        Key_ZENKAKU_HANKAKU = 211,
        Key_EISU = 212,
        Key_MUHENKAN = 213,
        Key_HENKAN = 214,
        Key_KATAKANA_HIRAGANA = 215,
        Key_YEN = 216,
        Key_RO = 217,
        Key_KANA = 218,
        Key_ASSIST = 219,
        Key_BRIGHTNESS_DOWN = 220,
        Key_BRIGHTNESS_UP = 221,
        Key_MEDIA_AUDIO_TRACK = 222,
        Key_SLEEP = 223,
        Key_WAKEUP = 224,
        Key_PAIRING = 225,
        Key_MEDIA_TOP_MENU = 226,
        Key_11 = 227,
        Key_12 = 228,
        Key_LAST_CHANNEL = 229,
        Key_TV_DATA_SERVICE = 230,
        Key_VOICE_ASSIST = 231,
        Key_TV_RADIO_SERVICE = 232,
        Key_TV_TELETEXT = 233,
        Key_TV_NUMBER_ENTRY = 234,
        Key_TV_TERRESTRIAL_ANALOG = 235,
        Key_TV_TERRESTRIAL_DIGITAL = 236,
        Key_TV_SATELLITE = 237,
        Key_TV_SATELLITE_BS = 238,
        Key_TV_SATELLITE_CS = 239,
        Key_TV_SATELLITE_SERVICE = 240,
        Key_TV_NETWORK = 241,
        Key_TV_ANTENNA_CABLE = 242,
        Key_TV_INPUT_HDMI_1 = 243,
        Key_TV_INPUT_HDMI_2 = 244,
        Key_TV_INPUT_HDMI_3 = 245,
        Key_TV_INPUT_HDMI_4 = 246,
        Key_TV_INPUT_COMPOSITE_1 = 247,
        Key_TV_INPUT_COMPOSITE_2 = 248,
        Key_TV_INPUT_COMPONENT_1 = 249,
        Key_TV_INPUT_COMPONENT_2 = 250,
        Key_TV_INPUT_VGA_1 = 251,
        Key_TV_AUDIO_DESCRIPTION = 252,
        Key_TV_AUDIO_DESCRIPTION_MIX_UP = 253,
        Key_TV_AUDIO_DESCRIPTION_MIX_DOWN = 254,
        Key_TV_ZOOM_MODE = 255,
        Key_TV_CONTENTS_MENU = 256,
        // Media context menu key.
        // Goes to the context menu of media contents. Corresponds to Media Context-sensitive
        // Menu (0x11) of CEC User Control Code.
        Key_TV_MEDIA_CONTEXT_MENU = 257,
        // Timer programming key.
        // Goes to the timer recording menu. Corresponds to Timer Programming (0x54) of
        // CEC User Control Code.
        Key_TV_TIMER_PROGRAMMING = 258,
        // Help key.
        Key_HELP            = 259,
        Key_NAVIGATE_PREVIOUS = 260,
        Key_NAVIGATE_NEXT   = 261,
        Key_NAVIGATE_IN     = 262,
        Key_NAVIGATE_OUT    = 263,
        // Primary stem key for Wear
        // Main power/reset button on watch.
        Key_STEM_PRIMARY = 264,
        // Generic stem key 1 for Wear
        Key_STEM_1 = 265,
        // Generic stem key 2 for Wear
        Key_STEM_2 = 266,
        // Generic stem key 3 for Wear
        Key_STEM_3 = 267,
        // Directional Pad Up-Left
        Key_DPAD_UP_LEFT    = 268,
        // Directional Pad Down-Left
        Key_DPAD_DOWN_LEFT  = 269,
        // Directional Pad Up-Right
        Key_DPAD_UP_RIGHT   = 270,
        // Directional Pad Down-Right
        Key_DPAD_DOWN_RIGHT = 271,
        // Skip forward media key
        Key_MEDIA_SKIP_FORWARD = 272,
        // Skip backward media key
        Key_MEDIA_SKIP_BACKWARD = 273,
        // Step forward media key.
        // Steps media forward one from at a time.
        Key_MEDIA_STEP_FORWARD = 274,
        // Step backward media key.
        // Steps media backward one from at a time.
        Key_MEDIA_STEP_BACKWARD = 275,
        // Put device to sleep unless a wakelock is held.
        Key_SOFT_SLEEP = 276,
        // Cut key.
        Key_CUT = 277,
        // Copy key.
        Key_COPY = 278,
        // Paste key.
        Key_PASTE = 279,
        // fingerprint navigation key, up.
        Key_SYSTEM_NAVIGATION_UP = 280,
        // fingerprint navigation key, down.
        Key_SYSTEM_NAVIGATION_DOWN = 281,
        // fingerprint navigation key, left.
        Key_SYSTEM_NAVIGATION_LEFT = 282,
        // fingerprint navigation key, right.
        Key_SYSTEM_NAVIGATION_RIGHT = 283,
        // all apps
        Key_ALL_APPS = 284,
        // refresh key
        Key_REFRESH = 285,
        // Thumbs up key. Apps can use this to let user upvote content.
        Key_THUMBS_UP = 286,
        // Thumbs down key. Apps can use this to let user downvote content.
        Key_THUMBS_DOWN = 287,
        // Used to switch current account that is consuming content.
        // May be consumed by system to switch current viewer profile.
        Key_PROFILE_SWITCH = 288,
        Key_VIDEO_APP_1 = 289,
        Key_VIDEO_APP_2 = 290,
        Key_VIDEO_APP_3 = 291,
        Key_VIDEO_APP_4 = 292,
        Key_VIDEO_APP_5 = 293,
        Key_VIDEO_APP_6 = 294,
        Key_VIDEO_APP_7 = 295,
        Key_VIDEO_APP_8 = 296,
        Key_FEATURED_APP_1 = 297,
        Key_FEATURED_APP_2 = 298,
        Key_FEATURED_APP_3 = 299,
        Key_FEATURED_APP_4 = 300,
        Key_DEMO_APP_1 = 301,
        Key_DEMO_APP_2 = 302,
        Key_DEMO_APP_3 = 303,
        Key_DEMO_APP_4 = 304,
    }
}
