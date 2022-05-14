using System.IO;
using UnityEditor;

namespace Battlehub.PackageUtils
{
    public class PathUtil
    {
        public string DefaultAssetsPath = "Assets/Battlehub/Generated";
        public string EditorPrefsKey = "Battlehub.PackageUtils.GeneratedAssetsPath";

        public PathUtil(string defaultAssetsPath, string editorPrefsKey)
        {
            DefaultAssetsPath = defaultAssetsPath;
            EditorPrefsKey = editorPrefsKey;
        }    

        public string GeneratedAssetsPath
        {
            get
            {
                string generateAssetsPath = EditorPrefs.GetString(EditorPrefsKey);
                if (string.IsNullOrEmpty(generateAssetsPath))
                {
                    return DefaultAssetsPath.TrimEnd(new[] { '/', '\\' });
                }
                
                return generateAssetsPath.Trim(new[] { '/', '\\' }); 
            }
            set
            {
                if(value == null)
                {
                    EditorPrefs.DeleteKey("RTSL_Data_RootFolder");
                }
                else
                {
                    EditorPrefs.SetString("RTSL_Data_RootFolder", value);
                }
            }
        }

        public string GeneratedAssetsFullPath
        {
            get { return Path.GetFullPath(GeneratedAssetsPath); }
        }

        public void EnsureGeneratedAssetFolderExists()
        {
            if(!Directory.Exists(GeneratedAssetsFullPath))
            {
                Directory.CreateDirectory(GeneratedAssetsFullPath);
            }
        }

        public void DeleteGenerateAssetsFolder()
        {
            if(Directory.Exists(GeneratedAssetsFullPath))
            {
                Directory.Delete(GeneratedAssetsFullPath, true);
            }
        }
    }
}
