using System.Linq;
using UnityEditor;

namespace Suzuryg.LocalSwitchGenerator
{
    internal static class AssetUtility
    {
        // https://hacchi-man.hatenablog.com/entry/2020/08/23/220000
        public static void CreateFolderRecursively(string path)
        {
            // If it doesn't start with Assets, it can't be processed.
            if (!path.StartsWith("Assets/"))
            {
                return;
            }
         
            // AssetDatabase, so the delimiter is /.
            var dirs = path.Split('/');
            var combinePath = dirs[0];

            // Skip the Assets part.
            foreach (var dir in dirs.Skip(1))
            {
                // Check existence of directory
                if (!AssetDatabase.IsValidFolder(combinePath + '/' + dir))
                {
                    AssetDatabase.CreateFolder(combinePath, dir);
                }
                combinePath += '/' + dir;
            }
        }
    }
}
