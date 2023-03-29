using XSharp.Math.Geometry;

namespace XSharp.Math.Physics;

public static class MovementUtil
{
    /// <summary>
    /// Compute the initial velocity of an object launched obliquely to hit a target position with a given launch speed (scalar) and gravity.
    /// </summary>
    /// <param name="launchOrigin">The origin of launching.</param>
    /// <param name="targetOrigin">The origin of target.</param>
    /// <param name="launchSpeed">The initial scalar speed of launch.</param>
    /// <param name="gravity">The gravity.</param>
    /// <param name="initialVelocity">Output vectorial velocity of lauching.</param>
    /// <returns>True if is possible to hit the target, false othersie. The output passed to initialVelocity will be the the maximum possible velocity (where the object pass as close as possible) if is not possible to hit the target (i.e., the return is false).</returns>
    public static bool GetObliqueLaunchVelocity(Vector launchOrigin, Vector targetOrigin, FixedSingle launchSpeed, FixedSingle gravity, out Vector initialVelocity)
    {
        // The following algorithm is used to determine the vectorial initial velocity of an launched object, based on its position, target position, launch speed (scalar) and gravity.
        // The object is launched in a parabolic trajectory, under the effect of the gravity and governed by the equations of uniformly varied motion.
        // The object trajectory is aimed to hit the target position and the initial velocity is calculated based on the target position and the object launch position.
        // To compute the initial velocity, we need to solve the following quadratic equation in the unknow tanTheta:
        //
        //   g * dx^2 * tanTheta^2 + 2 * v^2 * dx * tanTheta + g * dx^2 - 2 * v^2 * dy = 0
        //
        // Where tanTheta = Tan(theta) and theta is the angle of launch.
        //
        // The quatratic equation above can be deducted easily by using by isolating and eliminating the unknow dt in the following equation system:
        //
        //   dx = vx * dt                (from uniform motion equation)
        //   dy = vy * dt + g * dt^2 / 2  (from uniformly variable motion equation)
        //
        // Also, once we have:
        //
        //   vx = v * Cos(theta)
        //   vy = v * Sin(theta)
        // 
        // Then vy / vx = Sin(theta) / Cos(theta) = Tan(theta) = tanTheta, the unknow we are looking for.

        double v = launchSpeed;
        double g = gravity;
        double dx = targetOrigin.X - launchOrigin.X;
        double dy = targetOrigin.Y - launchOrigin.Y;

        double cosTheta;
        double sinTheta;

        bool result = true;

        if (dx == 0)
        {
            // When dx is zero the quadradic equation can't be solved due to division by zero.
            // Instead it, we can notice the launching in this case is just a vertical launch, i.e., theta is 90 degrees up.

            cosTheta = 0;
            sinTheta = -1;
        }
        else
        {
            double alpha = v / (g * dx);
            double discriminant = v * v + 2 * g * dy - 1 / (alpha * alpha); // This is the discritimant of the quadratic equation.

            if (discriminant < 0)
            {
                discriminant = 0; // If the discriminant is negative then is impossible to hit the target position using the initial speed. A fallback approach is used by simply setting the discriminant to zero.
                result = false;
            }

            double tanTheta = alpha * (-v - System.Math.Sqrt(discriminant));

            // The computation of explicit angle is not needed, once we have its tangent and the launch it's up, i.e., initial vertical speed is negative.
            // Instead, we compute its cosine end sine using the existing fundamental trigonometric relationships:
            //
            //   secTheta^2 = 1 / cosTheta^2 = 1 + tanTheta^2
            //   sinTheta^2 + cosTheta^2 = 1
            //
            // Where secTheta = Sec(theta), sinTheta = Sin(theta) and cosTheta = Cos(theta).
            // 
            // Also remembering the restrictions previously imposed:
            //
            //   sinTheta is negative.
            //   Signal of cosTheta is the same signal of dx.

            cosTheta = System.Math.Sign(dx) / System.Math.Sqrt(1 + tanTheta * tanTheta);
            sinTheta = -System.Math.Sqrt(1 - cosTheta * cosTheta);
        }

        FixedSingle vx = v * cosTheta;
        FixedSingle vy = v * sinTheta;
        initialVelocity = (vx, vy);
        return result;
    }
}