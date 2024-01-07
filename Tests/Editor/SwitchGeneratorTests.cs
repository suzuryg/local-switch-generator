using ClusterVR.CreatorKit;
using ClusterVR.CreatorKit.Gimmick;
using ClusterVR.CreatorKit.Gimmick.Implements;
using ClusterVR.CreatorKit.Item.Implements;
using ClusterVR.CreatorKit.Operation;
using ClusterVR.CreatorKit.Operation.Implements;
using ClusterVR.CreatorKit.Trigger;
using ClusterVR.CreatorKit.Trigger.Implements;
using ClusterVR.CreatorKit.World.Implements.PlayerLocalUI;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using TriggerValue = ClusterVR.CreatorKit.Trigger.Implements.Value;

namespace Suzuryg.LocalSwitchGenerator
{
    [TestFixture]
    internal class SwitchGeneratorTests
    {
        private static readonly string TargetName = "target";

        private string _identifier;
        private GameObject _targetObject;
        private SwitchGenerator _switchGenerator;

        [SetUp]
        public void SetUp()
        {
            _identifier = System.Guid.NewGuid().ToString("N");
            _targetObject = new GameObject();
            _targetObject.name = TargetName;
            _switchGenerator = new SwitchGenerator(_identifier, _targetObject);
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(_switchGenerator.AssetDir);
            GameObject.DestroyImmediate(_targetObject);
        }

        [Test]
        public void SingleSwitch()
        {
            var switchObject = new GameObject();
            GameObject rootObject = null;
            try
            {
                _switchGenerator.Generate(switchObject, defaultState: false);

                CheckSwitchObject(switchObject);

                rootObject = GameObject.Find("/" + _switchGenerator.RootObjectName);
                Assert.That(rootObject.activeInHierarchy, Is.True);
                CheckPlayerLocalUI(rootObject);

                var playerLogic = rootObject.GetComponent<PlayerLogic>();
                Assert.That((playerLogic as IGimmick).Target, Is.EqualTo(GimmickTarget.Player));
                Assert.That((playerLogic as IGimmick).Key, Is.EqualTo(Constants.Prefix_Interact + _identifier));
                Assert.That((playerLogic as ILogic).Logic.Statements.Count(), Is.EqualTo(2));
                CheckStatement((playerLogic as ILogic).Logic.Statements[0], Constants.Prefix_On + _identifier, Constants.Prefix_On + _identifier);
                CheckStatement((playerLogic as ILogic).Logic.Statements[1], Constants.Prefix_Off + _identifier, Constants.Prefix_On + _identifier);

                var on = rootObject.transform.Find("On").gameObject;
                Assert.That(on.activeInHierarchy, Is.False);
                CheckActiveGimmick(on, Constants.Prefix_On + _identifier);
                CheckPlayableDirector(on, _switchGenerator.AssetDir + "/On.playable", new[] { (_targetObject, true) });

                var off = rootObject.transform.Find("Off").gameObject;
                Assert.That(off.activeInHierarchy, Is.True);
                CheckActiveGimmick(off, Constants.Prefix_Off + _identifier);
                CheckPlayableDirector(off, _switchGenerator.AssetDir + "/Off.playable", new[] { (_targetObject, false) });
            }
            finally
            {
                GameObject.DestroyImmediate(switchObject);
                GameObject.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void DoubleSwitch()
        {
            var offToOn = new GameObject();
            var onToOff = new GameObject();
            GameObject rootObject = null;
            try
            {
                _switchGenerator.Generate(offToOn, onToOff, defaultState: true);

                CheckSwitchObject(offToOn);
                CheckSwitchObject(onToOff);

                rootObject = GameObject.Find("/" + _switchGenerator.RootObjectName);
                Assert.That(rootObject.activeInHierarchy, Is.True);
                CheckPlayerLocalUI(rootObject);

                var playerLogic = rootObject.GetComponent<PlayerLogic>();
                Assert.That((playerLogic as IGimmick).Target, Is.EqualTo(GimmickTarget.Player));
                Assert.That((playerLogic as IGimmick).Key, Is.EqualTo(Constants.Prefix_Interact + _identifier));
                Assert.That((playerLogic as ILogic).Logic.Statements.Count(), Is.EqualTo(2));
                CheckStatement((playerLogic as ILogic).Logic.Statements[0], Constants.Prefix_Off + _identifier, Constants.Prefix_Off + _identifier);
                CheckStatement((playerLogic as ILogic).Logic.Statements[1], Constants.Prefix_On + _identifier, Constants.Prefix_Off + _identifier);

                var on = rootObject.transform.Find("On").gameObject;
                Assert.That(on.activeInHierarchy, Is.True);
                CheckActiveGimmick(on, Constants.Prefix_On + _identifier);
                CheckPlayableDirector(on, _switchGenerator.AssetDir + "/On.playable", new[] { (_targetObject, true), (offToOn, false), (onToOff, true) });

                var off = rootObject.transform.Find("Off").gameObject;
                Assert.That(off.activeInHierarchy, Is.False);
                CheckActiveGimmick(off, Constants.Prefix_Off + _identifier);
                CheckPlayableDirector(off, _switchGenerator.AssetDir + "/Off.playable", new[] { (_targetObject, false), (offToOn, true), (onToOff, false) });
            }
            finally
            {
                GameObject.DestroyImmediate(offToOn);
                GameObject.DestroyImmediate(onToOff);
                GameObject.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void ExistingSwitch()
        {
            var switchObject = new GameObject();
            GameObject rootObject = null;
            try
            {
                switchObject.gameObject.AddComponent<Item>();
                var interactItemTrigger = switchObject.gameObject.AddComponent<InteractItemTrigger>();
                ReflectionUtility.AddElementsToArray(interactItemTrigger, "triggers", new[]
                {
                    new ConstantTriggerParam(TriggerTarget.Player, null, System.Guid.NewGuid().ToString("N"), ParameterType.Signal, new TriggerValue()),
                });

                Assert.That(switchObject.gameObject.GetComponents<Item>().Count(), Is.EqualTo(1));
                Assert.That(switchObject.gameObject.GetComponents<InteractItemTrigger>().Count(), Is.EqualTo(1));
                Assert.That((switchObject.gameObject.GetComponents<InteractItemTrigger>().First() as ITrigger).TriggerParams.Count(), Is.EqualTo(1));

                _switchGenerator.Generate(switchObject, defaultState: false);
                rootObject = GameObject.Find("/" + _switchGenerator.RootObjectName);

                Assert.That(switchObject.gameObject.GetComponents<Item>().Count(), Is.EqualTo(1));
                Assert.That(switchObject.gameObject.GetComponents<InteractItemTrigger>().Count(), Is.EqualTo(1));
                Assert.That((switchObject.gameObject.GetComponents<InteractItemTrigger>().First() as ITrigger).TriggerParams.Count(), Is.EqualTo(2));
            }
            finally
            {
                GameObject.DestroyImmediate(switchObject);
                GameObject.DestroyImmediate(rootObject);
            }
        }

        private void CheckSwitchObject(GameObject switchObject)
        {
            Assert.That(switchObject.activeInHierarchy, Is.True);

            var item = switchObject.GetComponent<Item>();
            var interactItemTrigger = switchObject.GetComponent<InteractItemTrigger>();

            Assert.That(item != null, Is.True);
            Assert.That((interactItemTrigger as ITrigger).TriggerParams.Count(), Is.EqualTo(1));
            Assert.That((interactItemTrigger as ITrigger).TriggerParams.First().Target, Is.EqualTo(TriggerTarget.Player));
            Assert.That((interactItemTrigger as ITrigger).TriggerParams.First().RawKey, Is.EqualTo(Constants.Prefix_Interact + _identifier));
            Assert.That((interactItemTrigger as ITrigger).TriggerParams.First().ParameterType, Is.EqualTo(ParameterType.Signal));
        }

        private void CheckPlayerLocalUI(GameObject rootObject)
        {
            var playerLocalUI = rootObject.GetComponent<PlayerLocalUI>();
            var canvas = rootObject.GetComponent<Canvas>();
            var canvasScaler = rootObject.GetComponent<CanvasScaler>();
            Assert.That(playerLocalUI.enabled, Is.False);
            Assert.That(canvas.enabled, Is.False);
            Assert.That(canvasScaler.enabled, Is.False);
        }

        private void CheckStatement(Statement statement, string leftHandKey, string rightHandKey)
        {
            Assert.That(statement.SingleStatement.TargetState.Target, Is.EqualTo(TargetStateTarget.Player));
            Assert.That(statement.SingleStatement.TargetState.Key, Is.EqualTo(leftHandKey));
            Assert.That(statement.SingleStatement.TargetState.ParameterType, Is.EqualTo(ParameterType.Bool));
            Assert.That(statement.SingleStatement.Expression.Type, Is.EqualTo(ExpressionType.OperatorExpression));
            Assert.That(statement.SingleStatement.Expression.OperatorExpression.Operator, Is.EqualTo(Operator.Not));
            Assert.That(statement.SingleStatement.Expression.OperatorExpression.Operands.Count(), Is.EqualTo(1));
            Assert.That(statement.SingleStatement.Expression.OperatorExpression.Operands[0].Type, Is.EqualTo(ExpressionType.Value));
            Assert.That(statement.SingleStatement.Expression.OperatorExpression.Operands[0].Value.Type, Is.EqualTo(ValueType.RoomState));
            Assert.That(statement.SingleStatement.Expression.OperatorExpression.Operands[0].Value.SourceState.Target, Is.EqualTo(GimmickTarget.Player));
            Assert.That(statement.SingleStatement.Expression.OperatorExpression.Operands[0].Value.SourceState.Key, Is.EqualTo(rightHandKey));
            Assert.That(statement.SingleStatement.Expression.OperatorExpression.Operands[0].Value.SourceState.Type, Is.EqualTo(ParameterType.Double));
        }

        private void CheckActiveGimmick(GameObject gimmickObect, string gimmickKey)
        {
            var setGameObjectActiveGimmick = gimmickObect.GetComponent<SetGameObjectActiveGimmick>();
            Assert.That((setGameObjectActiveGimmick as IGimmick).Target, Is.EqualTo(GimmickTarget.Player));
            Assert.That((setGameObjectActiveGimmick as IGimmick).Key, Is.EqualTo(gimmickKey));
        }

        private void CheckPlayableDirector(GameObject gimmickObect, string assetPath, IReadOnlyList<(GameObject gameObject, bool activate)> targets)
        {
            var playableDirector = gimmickObect.GetComponent<PlayableDirector>();

            Assert.That(playableDirector.timeUpdateMode, Is.EqualTo(DirectorUpdateMode.GameTime));
            Assert.That(playableDirector.playOnAwake, Is.EqualTo(true));
            Assert.That(playableDirector.extrapolationMode, Is.EqualTo(DirectorWrapMode.None));
            Assert.That(playableDirector.initialTime, Is.EqualTo(0));

            var timelineAsset = playableDirector.playableAsset as TimelineAsset;
            Assert.That(AssetDatabase.GetAssetPath(timelineAsset), Is.EqualTo(assetPath));
            Assert.That(timelineAsset.GetRootTracks().Count(), Is.EqualTo(targets.Count));

            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                Assert.That(playableDirector.GetGenericBinding(timelineAsset.GetRootTrack(i)) == target.gameObject, Is.True);

                var activationTrack = timelineAsset.GetRootTrack(i) as ActivationTrack;
                Assert.That(AssetDatabase.GetAssetPath(activationTrack), Is.EqualTo(assetPath));

                Assert.That(activationTrack.GetClips().Count(), Is.EqualTo(target.activate ? 1 : 0));

                if (target.activate)
                {
                    var timelineClip = activationTrack.GetClips().First();
                    Assert.That(AssetDatabase.GetAssetPath(timelineClip.asset), Is.EqualTo(assetPath));
                    Assert.That(timelineClip.start, Is.EqualTo(0));
                    Assert.That(timelineClip.duration, Is.EqualTo(1));
                }
            }
        }
    }
}
