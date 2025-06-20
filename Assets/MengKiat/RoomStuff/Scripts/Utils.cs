using UnityEngine;

namespace RMG
{
    // Direction enum used to identify spawn directions on a room
    public enum Dir
    {
        top, bottom, left, right
    }

    public static class Utils
    {
        // Converts a normalized Vector3 direction into a Dir enum
        public static Dir Vector3ToDir(Vector3 pos)
        {
            Vector3 norm = pos.normalized;
            if (norm.x == 1) return Dir.right;
            if (norm.x == -1) return Dir.left;
            if (norm.z == -1) return Dir.bottom;
            return Dir.top;
        }

        // Returns the opposite direction of the one provided
        public static Dir FlipDir(Dir dir)
        {
            if (dir == Dir.bottom) return Dir.top;
            if (dir == Dir.top) return Dir.bottom;
            if (dir == Dir.left) return Dir.right;
            return Dir.left;
        }
    }
}
