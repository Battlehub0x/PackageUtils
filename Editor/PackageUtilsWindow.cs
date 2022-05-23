using System;
using UnityEditor;
using UnityEngine.UIElements;
using SimpleJSON;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;

namespace Battlehub.PackageUtils
{
    public class PackageUtilsWindow : EditorWindow
    {
        private string[] m_ignorePackages = new[] { "com.unity", "net.battlehub.packageutils" };

        private JSONNode m_manifest;
        private JSONNode m_prodManifest;
        private JSONNode m_devManifest;

        private Dictionary<string, string> m_packages;
        private Dictionary<string, string> m_prodPackages;
        private Dictionary<string, string> m_devPackages;
        private bool m_requiresUpdate;

        private TextField m_searchTextField;
        private TextField m_prodPathTextField;
        private TextField m_devPathTextField;
        private Toggle m_devToggle;
        private ListView m_packagesListView;
        private VisualElement m_settingsPanel;

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/net.battlehub.packageutils/Editor/Content/PackageUtilsWindow.uxml");
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/net.battlehub.packageutils/Editor/Content/PackageUtilsWindow.uss");
            root.styleSheets.Add(styleSheet);

            m_searchTextField = rootVisualElement.Q<TextField>("search-input");
            m_searchTextField.RegisterValueChangedCallback(OnSearchTextChanged);

            m_prodPathTextField = rootVisualElement.Q<TextField>("prod-path-input");
            m_prodPathTextField.RegisterValueChangedCallback(OnProdPathTextChanged);
            m_prodPathTextField.RegisterCallback<BlurEvent>(OnProdPathBlur);

            m_devPathTextField = rootVisualElement.Q<TextField>("dev-path-input");
            m_devPathTextField.RegisterValueChangedCallback(OnDevPathTextChanged);
            m_devPathTextField.RegisterCallback<BlurEvent>(OnDevPathBlur);

            m_devToggle = rootVisualElement.Q<Toggle>("dev-mode-toggle");
            m_devToggle.RegisterValueChangedCallback(OnDevModeChanged);

            m_settingsPanel = rootVisualElement.Q<VisualElement>("settings-panel");
            m_settingsPanel.SetEnabled(false);

            m_manifest = ReadManifest("manifest");
            m_packages = ReadPackagesFromManifest(m_manifest);
            m_devManifest = ReadManifest("manifest-dev");
            m_devPackages = ReadPackagesFromManifest(m_devManifest);
            m_prodManifest = ReadManifest("manifest-prod");
            m_prodPackages = ReadPackagesFromManifest(m_prodManifest);
            m_packagesListView = root.Q<ListView>();

            PopulatePackagesList();
            m_packagesListView.selectionType = SelectionType.Single;
            m_packagesListView.onSelectionChange += OnTreePackagesListItemChosen;
        }

        private void PopulatePackagesList()
        {
            List<KeyValuePair<string, string>> packagesList = GetPackagesList();
            var packagesListViewItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/net.battlehub.packageutils/Editor/Content/PackagesListViewItem.uxml");
            Func<VisualElement> makePackagesListViewItem = () => packagesListViewItem.Instantiate();
            Action<VisualElement, int> bindPackagesListViewItem = (e, i) =>
            {
                var label = e.Q<Label>();
                if (i < packagesList.Count)
                {
                    label.text = packagesList[i].Key;
                }
            };
            m_packagesListView.makeItem = makePackagesListViewItem;
            m_packagesListView.bindItem = bindPackagesListViewItem;
            m_packagesListView.itemsSource = packagesList;
            m_packagesListView.Rebuild();
            m_packagesListView.selectedIndex = -1;
        }

        private List<KeyValuePair<string, string>> GetPackagesList()
        {
            return m_packages
                .Where(kvp => !m_ignorePackages.Any(package => kvp.Key.ToLower().StartsWith(package.ToLower())))
                .Where(kvp => kvp.Key.ToLower().Contains(m_searchTextField.value.ToLower())).ToList();
        }

        private void OnSearchTextChanged(ChangeEvent<string> evt)
        {
            PopulatePackagesList();
        }

        private void OnProdPathTextChanged(ChangeEvent<string> evt)
        {
            if(m_packagesListView.selectedItem != null)
            {
                var selectedPackage = (KeyValuePair<string, string>)m_packagesListView.selectedItem;
                m_prodPackages[selectedPackage.Key] = evt.newValue;
                WritePackagesToManifest("manifest-prod", m_prodManifest, m_prodPackages);

                if (!m_devToggle.value)
                {
                    m_packages[selectedPackage.Key] = evt.newValue;
                    WritePackagesToManifest("manifest", m_manifest, m_packages);
                    m_requiresUpdate = true;
                }
            }
        }

        private void OnProdPathBlur(BlurEvent evt)
        {
            if (m_requiresUpdate)
            {
                Client.Resolve();
                m_requiresUpdate = false;
            }
        }

        private void OnDevPathTextChanged(ChangeEvent<string> evt)
        {
            if(m_packagesListView.selectedItem != null)
            {
                var selectedPackage = (KeyValuePair<string, string>)m_packagesListView.selectedItem;
                m_devPackages[selectedPackage.Key] = evt.newValue;
                WritePackagesToManifest("manifest-dev", m_devManifest, m_devPackages);

                if (m_devToggle.value)
                {
                    m_packages[selectedPackage.Key] = evt.newValue;
                    WritePackagesToManifest("manifest", m_manifest, m_packages);
                    m_requiresUpdate = true;
                }
            }
        }

        private void OnDevPathBlur(BlurEvent evt)
        {
            if(m_requiresUpdate)
            {
                Client.Resolve();
                m_requiresUpdate = false;
            }
        }

        private void OnDevModeChanged(ChangeEvent<bool> evt)
        {
            if (m_packagesListView.selectedItem != null)
            {
                var selectedPackage = (KeyValuePair<string, string>)m_packagesListView.selectedItem;
                if (evt.newValue)
                {
                    if(m_devPackages.TryGetValue(selectedPackage.Key, out string path))
                    {
                        m_packages[selectedPackage.Key] = path;
                        WritePackagesToManifest("manifest", m_manifest, m_packages);
                        m_requiresUpdate = false;
                        Client.Resolve();
                    }
                }
                else
                {
                    if(m_prodPackages.TryGetValue(selectedPackage.Key, out string path))
                    {
                        m_packages[selectedPackage.Key] = path;
                        WritePackagesToManifest("manifest", m_manifest, m_packages);
                        m_requiresUpdate = false;
                        Client.Resolve();
                    }
                }
            }
        }

        private void OnTreePackagesListItemChosen(IEnumerable<object> chosenItems)
        {
            object chosenItem = chosenItems.FirstOrDefault();
            if(chosenItem == null)
            {
                m_settingsPanel.SetEnabled(false);
                m_prodPathTextField.SetValueWithoutNotify(string.Empty);
                m_devPathTextField.SetValueWithoutNotify(string.Empty);
                m_devToggle.SetValueWithoutNotify(false);
                return;
            }

            var selectedPackage = (KeyValuePair<string, string>)chosenItem;
            m_settingsPanel.SetEnabled(true);

            if (m_prodPackages.TryGetValue(selectedPackage.Key, out string prodPath))
            {
                m_prodPathTextField.SetValueWithoutNotify(prodPath);
            }
            else
            {
                m_prodPathTextField.SetValueWithoutNotify(selectedPackage.Value);
            }
            
            if(m_devPackages.TryGetValue(selectedPackage.Key, out string devPath))
            {
                m_devPathTextField.SetValueWithoutNotify(devPath);
            }
            else
            {
                devPath = string.Empty;
                m_devPathTextField.SetValueWithoutNotify(devPath);   
            }

            m_devToggle.SetValueWithoutNotify(selectedPackage.Value.ToLower() == devPath.ToLower() && devPath.ToLower() != prodPath.ToLower());
        }

        private static string GetDataPath()
        {
            return Path.GetDirectoryName(Application.dataPath);
        }

        public JSONNode ReadManifest(string file)
        {
            string dataPath = GetDataPath();
            string path = $"{dataPath}/Packages/{file}.json";
            if (!File.Exists(path))
            {
                return new JSONObject();
            }
            string json = File.ReadAllText(path);
            return JSON.Parse(json);
        }
       
        public Dictionary<string, string> ReadPackagesFromManifest(JSONNode manifest)
        {
            if (!manifest.HasKey("dependencies"))
            {
                return new Dictionary<string, string>();
            }

            var packageToVersion = new Dictionary<string, string>();
            JSONNode dependencies = manifest["dependencies"];
            foreach (KeyValuePair<string, JSONNode> kvp in (JSONObject)dependencies)
            {
                packageToVersion.Add(kvp.Key, kvp.Value.Value);
            }
            return packageToVersion;
        }

        public void WritePackagesToManifest(string file, JSONNode manifest, IEnumerable<KeyValuePair<string, string>> packages)
        {
            JSONObject dependencies = new JSONObject();
            manifest["dependencies"] = dependencies;
            foreach(var kvp in packages)
            {
                dependencies.Add(kvp.Key, new JSONString(kvp.Value));
            }

            string dataPath = GetDataPath();
            File.WriteAllText($"{dataPath}/Packages/{file}.json", manifest.ToString(2));
        }
    }
}

