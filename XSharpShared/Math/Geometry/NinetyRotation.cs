using System;

namespace XSharp.Math.Geometry;

public enum NinetyRotation
{
    ANGLE_0 = 0,
    ANGLE_90 = 1,
    ANGLE_180 = 2,
    ANGLE_270 = 3
}

public static class NinetyRotationExtensions
{
    public static NinetyRotation Normalize(this NinetyRotation rotation)
    {
        int value = (int) rotation;
        if (value is >= 0 and < 4)
            return rotation;

        if (value > 0)
            return (NinetyRotation) (value % 4);

        return (NinetyRotation) (4 + value % 4);
    }

    public static NinetyRotation Inverse(this NinetyRotation rotation)
    {
        return rotation switch
        {
            NinetyRotation.ANGLE_0 => NinetyRotation.ANGLE_0,
            NinetyRotation.ANGLE_90 => NinetyRotation.ANGLE_270,
            NinetyRotation.ANGLE_180 => NinetyRotation.ANGLE_180,
            NinetyRotation.ANGLE_270 => NinetyRotation.ANGLE_90,
            _ => rotation.Normalize().Inverse()
        };
    }

    public static NinetyRotation FromDegrees(this int degrees)
    {
        if (degrees == 0)
            return NinetyRotation.ANGLE_0;

        if (degrees == 90)
            return NinetyRotation.ANGLE_90;

        if (degrees == 180)
            return NinetyRotation.ANGLE_180;

        if (degrees == 270)
            return NinetyRotation.ANGLE_270;

        if (degrees % 90 == 0)
            return (NinetyRotation) (degrees / 90);

        throw new ArgumentException($"Angle in degrees '{degrees}' is not multiple of 90.");
    }

    public static int ToDegrees(this NinetyRotation rotation)
    {
        return (int) rotation.Normalize() * 90;
    }

    public static NinetyRotation FromRadians(this FixedSingle radians)
    {
        if (radians == 0)
            return NinetyRotation.ANGLE_0;

        if (radians == FixedSingle.PI / 2)
            return NinetyRotation.ANGLE_90;

        if (radians == FixedSingle.PI)
            return NinetyRotation.ANGLE_180;

        if (radians == 3 * FixedSingle.PI / 2)
            return NinetyRotation.ANGLE_270;

        var multiple = (FixedSingle) (2 * (FixedDouble) radians / FixedDouble.PI);
        if (multiple.FracPart == 0)
            return (NinetyRotation) (int) multiple;

        throw new ArgumentException($"Angle in radians '{radians}' is not multiple of PI / 2.");
    }

    public static FixedSingle ToRadians(this NinetyRotation rotation)
    {
        return (int) rotation.Normalize() * FixedSingle.PI / 2;
    }

    public static NinetyRotation FromGrads(this int grads)
    {
        if (grads == 0)
            return NinetyRotation.ANGLE_0;

        if (grads == 100)
            return NinetyRotation.ANGLE_90;

        if (grads == 200)
            return NinetyRotation.ANGLE_180;

        if (grads == 300)
            return NinetyRotation.ANGLE_270;

        if (grads % 100 == 0)
            return (NinetyRotation) (grads / 100);

        throw new ArgumentException($"Angle in grads '{grads}' is not multiple of 100.");
    }

    public static int ToGrads(this NinetyRotation rotation)
    {
        return (int) rotation.Normalize() * 100;
    }
}