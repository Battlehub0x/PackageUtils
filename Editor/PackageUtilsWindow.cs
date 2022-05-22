using System;
using UnityEditor;
using UnityEngine.UIElements;
using SimpleJSON;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Battlehub.PackageUtils
{

    public class PackageUtilsWindow : EditorWindow
    {
        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/net.battlehub.packageutils/Editor/PackageUtilsWindow.uxml");
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/net.battlehub.packageutils/Editor/PackageUtilsWindow.uss");
            root.styleSheets.Add(styleSheet);

            var packages = ReadManifestJson();

            var packagesListViewItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/net.battlehub.packageutils/Editor/PackagesListViewItem.uxml");

            Func<VisualElement> makePackagesListViewItem = () => packagesListViewItem.Instantiate();

            Action<VisualElement, int> bindPackagesListViewItem = (e, i) =>
            {
                var label = e.Q<Label>();
                label.text = packages[i].Name;
            };

            var packagesListView = root.Q<ListView>();
            packagesListView.makeItem = makePackagesListViewItem;
            packagesListView.bindItem = bindPackagesListViewItem;
            packagesListView.itemsSource = packages;
            packagesListView.selectionType = SelectionType.Single;
            packagesListView.onSelectionChange -= OnTreePackagesListItemChosen;
            packagesListView.onSelectionChange += OnTreePackagesListItemChosen;   
        }

        private void OnTreePackagesListItemChosen(IEnumerable<object> obj)
        {
            var selectedPackage = (ValueTuple<string, string>)obj.FirstOrDefault();
            Debug.Log($"{selectedPackage.Item1}:{selectedPackage.Item2}");
        }

        public (string Name, string Version)[] ReadManifestJson()
        {
            var packageToVersion = new Dictionary<string, string>();
            string dataPath = Path.GetDirectoryName(Application.dataPath);
            string json = File.ReadAllText($"{dataPath}/Packages/manifest.json");

            var root = JSON.Parse(json);
            if (!root.HasKey("dependencies"))
            {
                return new (string, string)[0];
            }

            JSONNode dependencies = root["dependencies"];
            foreach(KeyValuePair<string, JSONNode> kvp in (JSONObject)dependencies)
            {
                packageToVersion.Add(kvp.Key, kvp.Value.Value);
            }
            return packageToVersion.Select(kvp => (kvp.Key, kvp.Value)).ToArray();
        }

    }
}

