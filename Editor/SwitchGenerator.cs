using ClusterVR.CreatorKit;
using ClusterVR.CreatorKit.Gimmick;
using ClusterVR.CreatorKit.Gimmick.Implements;
using ClusterVR.CreatorKit.Operation;
using ClusterVR.CreatorKit.Operation.Implements;
using ClusterVR.CreatorKit.Trigger;
using ClusterVR.CreatorKit.Trigger.Implements;
using ClusterVR.CreatorKit.World.Implements.PlayerLocalUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TriggerValue = ClusterVR.CreatorKit.Trigger.Implements.Value;
using ExpressionValue = ClusterVR.CreatorKit.Operation.Value;

namespace Suzuryg.LocalSwitchGenerator
{
    internal class SwitchGenerator
    {
        public string RootObjectName => $"Switch_{_targetObject.name}";
        public string AssetDir => $"{Constants.AssetBaseDir}/Switch_{_targetObject.name}_{_identifier}";

        private string _identifier;
        private GameObject _targetObject;
        private MethodInfo _createActivationClipMethod;

        public SwitchGenerator(string identifier, GameObject targetObject)
        {
            _identifier = identifier;
            _targetObject = targetObject;

            // TODO: Create an activation clip without reflection.
            // clip.asset is not serialized when created with CreateDefaultClip().
            var assembly = Assembly.GetAssembly(typeof(ActivationTrack));  
            var type = assembly.GetType("UnityEngine.Timeline.ActivationPlayableAsset");  
            var method = typeof(TrackAsset).GetMethod(nameof(TrackAsset.CreateClip));
            _createActivationClipMethod = method.MakeGenericMethod(type);
        }

        public void Generate(GameObject switchObject, bool defaultState)
        {
            Generate(new[] { (switchObject, SwitchObjectType.Single) }, defaultState);
        }

        public void Generate(GameObject offToOn, GameObject onToOff, bool defaultState)
        {
            Generate(new[] { (offToOn, SwitchObjectType.OffToOn), (onToOff, SwitchObjectType.OnToOff) }, defaultState);
        }

        private enum SwitchObjectType
        {
            Single,
            OffToOn,
            OnToOff,
        }

        private void Generate(IEnumerable<(GameObject gameObject, SwitchObjectType type)> switchObjects, bool defaultState)
        {
            // existing objects
            foreach (var switchObject in switchObjects)
            {
                var interactItemTrigger = switchObject.gameObject.GetComponent<InteractItemTrigger>();
                if (interactItemTrigger == null)
                {
                    interactItemTrigger = switchObject.gameObject.AddComponent<InteractItemTrigger>();
                }
                ReflectionUtility.AddElementsToArray(interactItemTrigger, "triggers", new[]
                {
                    new ConstantTriggerParam(TriggerTarget.Player, null, Constants.Prefix_Interact + _identifier, ParameterType.Signal, new TriggerValue()),
                });
            }

            // generated objects
            var rootObject = new GameObject(RootObjectName);

            var playerLogic = rootObject.AddComponent<PlayerLogic>();
            ReflectionUtility.SetValue(playerLogic, "key", new PlayerGimmickKey(Constants.Prefix_Interact + _identifier));

            var statements = defaultState ? new Statement[]
            {
                NotStatement(Constants.Prefix_Off + _identifier, Constants.Prefix_Off + _identifier),
                NotStatement(Constants.Prefix_On + _identifier, Constants.Prefix_Off + _identifier),
            } : new Statement[]
            {
                NotStatement(Constants.Prefix_On + _identifier, Constants.Prefix_On + _identifier),
                NotStatement(Constants.Prefix_Off + _identifier, Constants.Prefix_On + _identifier),
            };
            ReflectionUtility.SetValue(playerLogic, "logic", new Logic(statements));

            // A warning appears when adding PlayerLocalUI, but this is also the case when adding it from the inspector.
            var playerLocalUI = rootObject.AddComponent<PlayerLocalUI>();
            var canvas = rootObject.GetComponent<Canvas>();
            var canvasScaler = rootObject.GetComponent<CanvasScaler>();

            playerLocalUI.enabled = false;
            canvas.enabled = false;
            canvasScaler.enabled = false;

            var safeArea = rootObject.transform.Find("SafeArea");
            if (safeArea != null) { GameObject.DestroyImmediate(safeArea.gameObject); }

            var on = CreateActiveGimmickObject("On", Constants.Prefix_On + _identifier, rootObject);
            var off = CreateActiveGimmickObject("Off", Constants.Prefix_Off + _identifier, rootObject);

            on.SetActive(defaultState);
            off.SetActive(!defaultState);

            AssetUtility.CreateFolderRecursively(AssetDir);

            var onTargets = new List<(GameObject gameObject, bool activate)>() { (_targetObject, true) };
            var offTargets = new List<(GameObject gameObject, bool activate)>() { (_targetObject, false) };
            foreach (var switchObject in switchObjects)
            {
                switch (switchObject.type)
                {
                    case SwitchObjectType.OffToOn:
                        onTargets.Add((switchObject.gameObject, false));
                        offTargets.Add((switchObject.gameObject, true));
                        break;
                    case SwitchObjectType.OnToOff:
                        onTargets.Add((switchObject.gameObject, true));
                        offTargets.Add((switchObject.gameObject, false));
                        break;
                }
            }

            AddPlayableDirector(on, "On", onTargets);
            AddPlayableDirector(off, "Off", offTargets);
        }

        private Statement NotStatement(string leftHandKey, string rightHandKey)
        {
            var targetState = new TargetState(TargetStateTarget.Player, leftHandKey, ParameterType.Bool);

            var sourceState = new SourceState(GimmickTarget.Player, rightHandKey, ParameterType.Double);
            var operand = new Expression(new ExpressionValue(sourceState));
            var expression = new Expression(new OperatorExpression(Operator.Not, new[] { operand }));

            return new Statement(new SingleStatement(targetState, expression));
        }

        private GameObject CreateActiveGimmickObject(string objectName, string key, GameObject rootObject)
        {
            var gimmickObject = new GameObject(objectName);
            gimmickObject.transform.parent = rootObject.transform;

            var gimmickKey = new GlobalGimmickKey();
            ReflectionUtility.SetValue(gimmickKey.Key, "target", GimmickTarget.Player);
            ReflectionUtility.SetValue(gimmickKey.Key, "key", key);

            var gimmick = gimmickObject.AddComponent<SetGameObjectActiveGimmick>();
            ReflectionUtility.SetValue(gimmick, "globalGimmickKey", gimmickKey);

            return gimmickObject;
        } 

        private void AddPlayableDirector(GameObject attachTo, string timelineName, IEnumerable<(GameObject gameObject, bool activate)> targets)
        {
            var playableDirector = attachTo.AddComponent<PlayableDirector>();
            var timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            playableDirector.playableAsset = timelineAsset;

            var subAssets = new List<UnityEngine.Object>();
            foreach (var target in targets)
            {
                var activationTrack = timelineAsset.CreateTrack<ActivationTrack>(parent: null, "Activation_" + target.gameObject.name);
                subAssets.Add(activationTrack);
                if (target.activate)
                {
                    var clip = _createActivationClipMethod.Invoke(activationTrack, null) as TimelineClip;
                    subAssets.Add(clip.asset);
                    clip.start = 0;
                    clip.duration = 1;
                }
                playableDirector.SetGenericBinding(activationTrack, target.gameObject);
            }

            AssetDatabase.CreateAsset(timelineAsset, AssetDir + $"/{timelineName}.playable");

            foreach (var asset in subAssets)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(asset)))
                {
                    AssetDatabase.AddObjectToAsset(asset, timelineAsset);
                }
            }
        }
    }
}
