namespace XSharp.Engine
{
    public enum Keys
    {
        NONE = 0,
        LEFT = 1,
        UP = 2,
        RIGHT = 4,
        DOWN = 8,
        SHOT = 16,
        JUMP = 32,
        DASH = 64,
        WEAPON = 128,
        LWS = 256,
        RWS = 512,
        START = 1024,
        SELECT = 2048,

        MOVEMENT_KEYS = LEFT | UP | RIGHT| DOWN,
        ACTION_KEYS = SHOT | JUMP | DASH | WEAPON
    }

    public static class KeysExtensions
    {
        public static bool HasLeft(this Keys keys)
        {
            return (keys & Keys.LEFT) != 0;
        }

        public static bool HasRight(this Keys keys)
        {
            return (keys & Keys.RIGHT) != 0;
        }

        public static bool HasUp(this Keys keys)
        {
            return (keys & Keys.UP) != 0;
        }

        public static bool HasDown(this Keys keys)
        {
            return (keys & Keys.DOWN) != 0;
        }

        public static bool HasShot(this Keys keys)
        {
            return (keys & Keys.SHOT) != 0;
        }

        public static bool HasWeapon(this Keys keys)
        {
            return (keys & Keys.WEAPON) != 0;
        }

        public static bool HasJump(this Keys keys)
        {
            return (keys & Keys.JUMP) != 0;
        }

        public static bool HasDash(this Keys keys)
        {
            return (keys & Keys.DASH) != 0;
        }

        public static bool HasLeftWeaponSwitch(this Keys keys)
        {
            return (keys & Keys.LWS) != 0;
        }

        public static bool HasRightWeaponSwitch(this Keys keys)
        {
            return (keys & Keys.RWS) != 0;
        }

        public static bool HasStart(this Keys keys)
        {
            return (keys & Keys.START) != 0;
        }

        public static bool HasSelect(this Keys keys)
        {
            return (keys & Keys.SELECT) != 0;
        }

        public static bool HasAction(this Keys keys)
        {
            return (keys & Keys.ACTION_KEYS) != 0;
        }

        public static bool HasMovement(this Keys keys)
        {
            return (keys & Keys.MOVEMENT_KEYS) != 0;
        }

        public static bool HasActionOrMovement(this Keys keys)
        {
            return HasAction(keys) || HasMovement(keys);
        }
    }
}
