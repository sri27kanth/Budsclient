﻿// ReSharper disable InconsistentNaming
namespace GalaxyBudsClient.Message;

public partial class SppMessage
{
    public enum MsgType : byte
    {
        INVALID = 255,
        Request = 0,
        Response = 1
    }

    public enum Constants : byte
    {
        SOM = 0xFE,
        EOM = 0xEE,
        SOMPlus = 0xFD,
        EOMPlus = 0xDD
    }

    public enum MessageIds : byte
    {
        BATTERY_TYPE = 148,
        AMBIENT_MODE_UPDATED = 129,
        AMBIENT_VOICE_FOCUS = 133,
        AMBIENT_VOLUME = 132,
        AMBIENT_WEARING_STATUS_UPDATED = 137,
        CONNECTION_UPDATED = 98,
        DEBUG_BUILD_INFO = 40,
        DEBUG_GET_ALL_DATA = 38,
        DEBUG_GET_PE_RSSI = 35,
        DEBUG_GET_VERSION = 36,
        DEBUG_MODE_LOG = 33,
        DEBUG_SERIAL_NUMBER = 41,
        EQUALIZER = 134,
        EXTENDED_STATUS_UPDATED = 97,
        FIND_MY_EARBUDS_START = 160,
        FIND_MY_EARBUDS_STOP = 161,
            
        FOTA_DEVICE_INFO_SW_VERSION = 180,
            
        /* FOTA v1 API */
        FOTA_V1_ABORT = 181,
        FOTA_V1_CONTROL = 177,
        FOTA_V1_DOWNLOAD_DATA = 178,
        FOTA_V1_SESSION = 176,
        FOTA_V1_UPDATED = 179,
            
        /* FOTA v2 API */
        FOTA_CONTROL = 188,
        FOTA_DOWNLOAD_DATA = 189,
        FOTA_EMERGENCY = 186,
        FOTA_OPEN = 187,
        FOTA_RESULT = 185,
        FOTA_UPDATE = 190,
            
        GAME_MODE = 135,
        GET_MODE = 17,
        HIDDEN_CMD_DATA = 19,
        HIDDEN_CMD_MODE = 18,
        LOCK_TOUCHPAD = 144,
        LOG_COREDUMP_COMMIT_SUICIDE = 48,
        LOG_COREDUMP_COMPLETE = 51,
        LOG_COREDUMP_DATA = 50,
        LOG_COREDUMP_DATA_DONE = 56,
        LOG_COREDUMP_DATA_SIZE = 49,
        LOG_SESSION_CLOSE = 59,
        LOG_SESSION_OPEN = 58,
        LOG_TRACE_COMPLETE = 54,
        LOG_TRACE_DATA = 53,
        LOG_TRACE_DATA_DONE = 57,
        LOG_TRACE_ROLE_SWITCH = 55,
        LOG_TRACE_START = 52,
        MAIN_CHANGE = 112,
        MANAGER_INFO = 136,
        MUTE_EARBUD = 162,
        MUTE_EARBUD_STATUS_UPDATED = 163,
        NOTIFICATION_INFO = 166,
        PROFILE_CONTROL = 113,
        RESET = 80,
        RESP = 81,
        SELF_TEST = 171,
        SET_AMBIENT_MODE = 128,
        SET_DEBUG_MODE = 32,
        SET_MODE_CHANGE = 16,
        SET_TOUCHPAD_OPTION = 146,
        SET_TOUCHPAD_OTHER_OPTION = 147,
        SET_VOICE_CMD = 193,
        START_VOICE_RECORD = 168,
        STATUS_UPDATED = 96,
        TOUCH_UPDATED = 145,
        UPDATE_TIME = 167,
        UPDATE_VOICE_CMD = 192,
        USAGE_REPORT = 64,
        VOICE_NOTI_STATUS = 164,
        VOICE_NOTI_STOP = 165,
        SET_SEAMLESS_CONNECTION = 175,

        ADJUST_SOUND_SYNC = 133,
        EXTRA_HIGH_AMBIENT = 150,
        DEBUG_SKU = 34,
        OUTSIDE_DOUBLE_TAP = 149,
        SET_IN_BAND_RINGTONE = 138,
        SET_SIDETONE = 139,
        VERSION_INFO = 99,

        SET_ANC_WITH_ONE_EARBUD = 111,
        CHECK_THE_FIT_OF_EARBUDS = 157,
        CHECK_THE_FIT_OF_EARBUDS_RESULT = 158,
        GET_FMM_CONFIG = 173,
        NOISE_REDUCTION_MODE_UPDATE = 155,
        PASS_THROUGH = 159,
        SET_FMM_CONFIG = 172,
        SET_NOISE_REDUCTION = 152,
        SET_VOICE_WAKE_UP = 151,
        TAP_TEST_MODE = 141,
        TAP_TEST_MODE_EVENT = 142,
        VOICE_WAKE_UP_EVENT = 154,
        VOICE_WAKE_UP_LANGUAGE = 153,
        VOICE_WAKE_UP_LISTENING_STATUS = 156,

        /* Buds Pro */
        AGING_TEST_REPORT = 74,
        METERING_REPORT = 65,
        NOISE_CONTROLS_UPDATE = 119,
        AUTO_SWITCH_AUDIO_OUTPUT = 115, /* prior: SPP_ROLE_STATE */
        UNIVERSAL_MSG_ID_ACKNOWLEDGEMENT = 66,
        SET_TOUCH_AND_HOLD_NOISE_CONTROLS = 121,
        SET_DETECT_CONVERSATIONS = 122,
        SET_DETECT_CONVERSATIONS_DURATION = 123,
        SET_SPATIAL_AUDIO = 124,
        SET_SPEAK_SEAMLESSLY = 125,
        NOISE_CONTROLS = 120,
        NOISE_REDUCTION_LEVEL = 131, /* prior: A2DP_VOLUME_UPDATED */
        SET_HEARING_ENHANCEMENTS = 143,
        SPATIAL_AUDIO_DATA = 194,
        SPATIAL_AUDIO_CONTROL = 195,
            
        CUSTOMIZE_AMBIENT_SOUND = 130, /* prior: SET_A2DP_VOL */

        SOC_BATTERY_CYCLE = 206,
        
        /* Undocumented IDs (refer to: https://github.com/ThePBone/GalaxyBudsClient/blob/master/GalaxyBudsPlus_HiddenDebugFeatures.md) */
        UNK_DISCONNECT = 39,
        UNK_CRASH = 82,
        UNK_SHUTDOWN = 83,
        UNK_RF_TEST = 86,
        UNK_PAIRING_MODE = 114,
        UNK_CONN_INFO = 169,
        UNK_DEBUG_INFO_1 = 182,
        UNK_DUMP_BONDED_DEVICES = 212,
        UNK_BONDED_DEVICES = 214,
        UNK_GENERIC_EVENT = 242,
            
        /* Reserved. Auto-generated section */
        UNKNOWN_0 = 0,
        UNKNOWN_1 = 1,
        UNKNOWN_2 = 2,
        UNKNOWN_3 = 3,
        UNKNOWN_4 = 4,
        UNKNOWN_5 = 5,
        UNKNOWN_6 = 6,
        UNKNOWN_7 = 7,
        UNKNOWN_8 = 8,
        UNKNOWN_9 = 9,
        UNKNOWN_10 = 10,
        UNKNOWN_11 = 11,
        UNKNOWN_12 = 12,
        UNKNOWN_13 = 13,
        UNKNOWN_14 = 14,
        UNKNOWN_15 = 15,
        UNKNOWN_20 = 20,
        UNKNOWN_21 = 21,
        UNKNOWN_22 = 22,
        UNKNOWN_23 = 23,
        UNKNOWN_24 = 24,
        UNKNOWN_25 = 25,
        UNKNOWN_26 = 26,
        UNKNOWN_27 = 27,
        UNKNOWN_28 = 28,
        UNKNOWN_29 = 29,
        UNKNOWN_30 = 30,
        UNKNOWN_31 = 31,
        UNKNOWN_37 = 37,
        UNKNOWN_42 = 42,
        UNKNOWN_43 = 43,
        UNKNOWN_44 = 44,
        UNKNOWN_45 = 45,
        UNKNOWN_46 = 46,
        UNKNOWN_47 = 47,
        UNKNOWN_60 = 60,
        UNKNOWN_61 = 61,
        UNKNOWN_62 = 62,
        UNKNOWN_63 = 63,
        UNKNOWN_67 = 67,
        UNKNOWN_68 = 68,
        UNKNOWN_69 = 69,
        UNKNOWN_70 = 70,
        UNKNOWN_71 = 71,
        UNKNOWN_72 = 72,
        UNKNOWN_73 = 73,
        UNKNOWN_75 = 75,
        UNKNOWN_76 = 76,
        UNKNOWN_77 = 77,
        UNKNOWN_78 = 78,
        UNKNOWN_79 = 79,
        UNKNOWN_84 = 84,
        UNKNOWN_85 = 85,
        UNKNOWN_87 = 87,
        UNKNOWN_88 = 88,
        UNKNOWN_89 = 89,
        UNKNOWN_90 = 90,
        UNKNOWN_91 = 91,
        UNKNOWN_92 = 92,
        UNKNOWN_93 = 93,
        UNKNOWN_94 = 94,
        UNKNOWN_95 = 95,
        UNKNOWN_100 = 100,
        UNKNOWN_101 = 101,
        UNKNOWN_102 = 102,
        UNKNOWN_103 = 103,
        UNKNOWN_104 = 104,
        UNKNOWN_105 = 105,
        UNKNOWN_106 = 106,
        UNKNOWN_107 = 107,
        UNKNOWN_108 = 108,
        UNKNOWN_109 = 109,
        UNKNOWN_110 = 110,
        UNKNOWN_116 = 116,
        UNKNOWN_117 = 117,
        UNKNOWN_118 = 118,
        UNKNOWN_126 = 126,
        UNKNOWN_127 = 127,
        UNKNOWN_140 = 140,
        UNKNOWN_170 = 170,
        UNKNOWN_174 = 174,
        UNKNOWN_183 = 183,
        UNKNOWN_184 = 184,
        UNKNOWN_191 = 191,
        UNKNOWN_196 = 196,
        UNKNOWN_197 = 197,
        UNKNOWN_198 = 198,
        UNKNOWN_199 = 199,
        UNKNOWN_200 = 200,
        UNKNOWN_201 = 201,
        UNKNOWN_202 = 202,
        UNKNOWN_203 = 203,
        UNKNOWN_204 = 204,
        UNKNOWN_205 = 205,
        UNKNOWN_207 = 207,
        UNKNOWN_208 = 208,
        UNKNOWN_209 = 209,
        UNKNOWN_210 = 210,
        UNKNOWN_211 = 211,
        UNKNOWN_213 = 213,
        UNKNOWN_215 = 215,
        UNKNOWN_216 = 216,
        UNKNOWN_217 = 217,
        UNKNOWN_218 = 218,
        UNKNOWN_219 = 219,
        UNKNOWN_220 = 220,
        UNKNOWN_221 = 221,
        UNKNOWN_222 = 222,
        UNKNOWN_223 = 223,
        UNKNOWN_224 = 224,
        UNKNOWN_225 = 225,
        UNKNOWN_226 = 226,
        UNKNOWN_227 = 227,
        UNKNOWN_228 = 228,
        UNKNOWN_229 = 229,
        UNKNOWN_230 = 230,
        UNKNOWN_231 = 231,
        UNKNOWN_232 = 232,
        UNKNOWN_233 = 233,
        UNKNOWN_234 = 234,
        UNKNOWN_235 = 235,
        UNKNOWN_236 = 236,
        UNKNOWN_237 = 237,
        UNKNOWN_238 = 238,
        UNKNOWN_239 = 239,
        UNKNOWN_240 = 240,
        UNKNOWN_241 = 241,
        UNKNOWN_243 = 243,
        UNKNOWN_244 = 244,
        UNKNOWN_245 = 245,
        UNKNOWN_246 = 246,
        UNKNOWN_247 = 247,
        UNKNOWN_248 = 248,
        UNKNOWN_249 = 249,
        UNKNOWN_250 = 250,
        UNKNOWN_251 = 251,
        UNKNOWN_252 = 252,
        UNKNOWN_253 = 253,
        UNKNOWN_254 = 254,
        UNKNOWN_255 = 255
    }
}