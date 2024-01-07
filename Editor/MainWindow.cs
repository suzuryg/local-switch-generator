using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Suzuryg.LocalSwitchGenerator
{
    internal class MainWindow : EditorWindow
    {
        private static readonly float Margin = 5;
        private static readonly float LabelWidth = 180;

        private GameObject _targetObject = null;
        private bool _defaultState = false;
        private bool _toggleButtons = false;
        private GameObject _singleSwitch = null;
        private GameObject _offToOn = null;
        private GameObject _onToOff = null;

        private LocalizationSetting _loc;
        private GUIStyle _radioButtonStyle;

        private void OnEnable()
        {
            _loc = new LocalizationSetting();

            titleContent = new GUIContent(Constants.SystemName);
            minSize = new Vector2(435, 230);
        }

        private void OnGUI()
        {
            GetStyles();

            // locale
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("言語設定 (Language Setting)");
                var locale = (LocalizationSetting.Locale)EditorGUILayout.EnumPopup(string.Empty, _loc.CurrentLocale, GUILayout.Width(65));
                if (locale != _loc.CurrentLocale)
                {
                    _loc.CurrentLocale = locale;
                }
                GUILayout.Space(10);
            }

            GUILayout.Space(Margin);

            // target object
            _targetObject = ObjectField(_targetObject, _loc.Table.TargetObject);

            GUILayout.Space(Margin);

            // initial state
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(_loc.Table.InitialState, GUILayout.Width(LabelWidth));
                var options = new[] { new GUIContent("ON"), new GUIContent("OFF"), };
                var index = _defaultState ? 0 : 1;
                var newValue = GUILayout.SelectionGrid(index, options, xCount: 2, _radioButtonStyle);
                if (newValue != index)
                {
                    _defaultState = !_defaultState;
                }
            }

            GUILayout.Space(Margin);

            // toggle buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(_loc.Table.ToggleButtons, GUILayout.Width(LabelWidth));
                _toggleButtons = GUILayout.Toggle(_toggleButtons, string.Empty);
            }

            // double switch
            if (_toggleButtons)
            {
                _offToOn = ObjectField(_offToOn, _loc.Table.OffToOn);
                _onToOff = ObjectField(_onToOff, _loc.Table.OnToOff);
            }
            // single switch
            else
            {
                _singleSwitch = ObjectField(_singleSwitch, _loc.Table.Button);
            }

            EditorGUILayout.HelpBox(_loc.Table.ComponentsWillBeAdded, MessageType.Info);

            GUILayout.FlexibleSpace();

            // generate button
            if (GUILayout.Button(_loc.Table.Generate, GUILayout.Height(30)))
            {
                var check = CheckArguments();
                if (check.canExecute)
                {
                    if (EditorUtility.DisplayDialog(Constants.SystemName, _loc.Table.ConfirmGeneration, _loc.Table.Generate, _loc.Table.Cancel))
                    {
                        Generate();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog(Constants.SystemName, check.message, "OK");
                }
            }

            GUILayout.Space(15);
        }

        private void GetStyles()
        {
            if (_radioButtonStyle == null)
            {
                try
                {
                    _radioButtonStyle = new GUIStyle(EditorStyles.radioButton);
                }
                catch (NullReferenceException)
                {
                    _radioButtonStyle = new GUIStyle();
                }
                var padding = _radioButtonStyle.padding;
                _radioButtonStyle.padding = new RectOffset(padding.left + 3, padding.right, padding.top, padding.bottom);
            }
        }

        private GameObject ObjectField(GameObject exising, string label)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(LabelWidth));
                var newValue = EditorGUILayout.ObjectField(string.Empty, exising, typeof(GameObject), allowSceneObjects: true) as GameObject;
                if (newValue != null)
                {
                    if (newValue.scene.IsValid())
                    {
                        return newValue;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(Constants.SystemName, _loc.Table.PleaseSelectSceneObject, "OK");
                        return exising;
                    }
                }
                else
                {
                    return newValue;
                }
            }
        }

        private (bool canExecute, string message) CheckArguments()
        {
            if (_targetObject == null) { return (false, _loc.Table.NoTarget); }

            // double switch
            if (_toggleButtons)
            {
                if (_offToOn == null || _onToOff == null)
                {
                    return (false, _loc.Table.NoButtons);
                }
                else if (_targetObject == _offToOn || _targetObject == _onToOff || _offToOn == _onToOff)
                {
                    return (false, _loc.Table.SameObjects);
                }
                else
                {
                    return (true, string.Empty);
                }
            }
            // single switch
            else
            {
                if (_singleSwitch == null)
                {
                    return (false, _loc.Table.NoButtons);
                }
                else if (_targetObject == _singleSwitch)
                {
                    return (false, _loc.Table.SameObjects);
                }
                else
                {
                    return (true, string.Empty);
                }
            }
        }

        private void Generate()
        {
            try
            {
                var switchGenerator = new SwitchGenerator(Guid.NewGuid().ToString("N"), _targetObject);
                while (AssetDatabase.IsValidFolder(switchGenerator.AssetDir))
                {
                    switchGenerator = new SwitchGenerator(Guid.NewGuid().ToString("N"), _targetObject);
                }

                var message = new StringBuilder();
                message.AppendLine(_loc.Table.GenerationCompleted).AppendLine();

                // double switch
                if (_toggleButtons)
                {
                    switchGenerator.Generate(_offToOn, _onToOff, _defaultState);
                    message.AppendLine(_loc.Table.ComponentsAddedDouble.Replace("<0>", _offToOn.name).Replace("<1>", _onToOff.name)).AppendLine();
                }
                // single switch
                else
                {
                    switchGenerator.Generate(_singleSwitch, _defaultState);
                    message.AppendLine(_loc.Table.ComponentsAddedSingle.Replace("<0>", _singleSwitch.name)).AppendLine();
                }

                message.AppendLine(_loc.Table.ObjectGenerated.Replace("<0>", switchGenerator.RootObjectName)).AppendLine();
                EditorUtility.DisplayDialog(Constants.SystemName, message.ToString(), "OK");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog(Constants.SystemName, _loc.Table.GenerationError + "\n\n" + ex.Message, "OK");
            }
        }

        [MenuItem("Tools/LocalSwitchGenerator")]
        private static void Open() => GetWindow<MainWindow>();
    }
}
