using Microsoft.Xna.Framework;

namespace SFR.Helper;

internal static class Vector
{
    internal static Vector2 GetRotatedVector(this Vector2 vector, double angle)
    {
        double cosAngle = System.Math.Cos(angle);
        double sinAngle = System.Math.Sin(angle);
        return new Vector2((float)(cosAngle * vector.X - sinAngle * vector.Y), (float)(sinAngle * vector.X + cosAngle * vector.Y));
    }

    internal static float GetAngle(this Vector2 vector) => (float)System.Math.Atan2(vector.Y, vector.X);

    internal static float GetRotatedAngle(this Vector2 vector) => (float)System.Math.Atan2(vector.Y, vector.X) - (float)System.Math.PI / 2;
}