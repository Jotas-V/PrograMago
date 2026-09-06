using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrograMago.Tests.UnityIntegration
{
    public sealed class GameplaySceneAssetTests
    {
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";

        private Scene scene;

        [SetUp]
        public void OpenGameplayScene()
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
        }

        [TearDown]
        public void CloseGameplayScene()
        {
            if (scene.IsValid())
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void Scene_PersistsBattleButtonAndBootstrapReferences()
        {
            GameObject codeInput = FindSceneObject("CodeInput");
            GameObject feedbackText = FindSceneObject("FeedbackText");
            GameObject battleButton = FindSceneObject("BattleButton");
            MonoBehaviour bootstrapper = FindBehaviourWithProperty("codeInput");
            var serializedBootstrapper = new SerializedObject(bootstrapper);

            Assert.That(
                serializedBootstrapper.FindProperty("codeInput").objectReferenceValue,
                Is.SameAs(FindBehaviourWithProperty(codeInput, "m_ReadOnly")));
            Assert.That(
                serializedBootstrapper.FindProperty("feedbackText").objectReferenceValue,
                Is.SameAs(FindBehaviourWithProperty(feedbackText, "m_text")));
            Assert.That(
                serializedBootstrapper.FindProperty("battleButton").objectReferenceValue,
                Is.SameAs(FindBehaviourWithProperty(battleButton, "m_OnClick")));
        }

        [Test]
        public void TutorialPanel_ContainsItsTextsWithExpectedContent()
        {
            GameObject panel = FindSceneObject("TutorialPanel");
            GameObject title = FindSceneObject("Title");
            GameObject objective = FindSceneObject("ObjectiveText");
            GameObject feedback = FindSceneObject("FeedbackText");

            Assert.That(title.transform.parent, Is.SameAs(panel.transform));
            Assert.That(objective.transform.parent, Is.SameAs(panel.transform));
            Assert.That(feedback.transform.parent, Is.SameAs(panel.transform));
            Assert.That(ReadString(title, "m_text"), Is.EqualTo("Classes"));
            Assert.That(
                ReadString(objective, "m_text"),
                Is.EqualTo("Declare a classe pública Mago."));
            Assert.That(ReadString(feedback, "m_text"), Is.Empty);
            AssertContainedByPanel(title, panel);
            AssertContainedByPanel(objective, panel);
            AssertContainedByPanel(feedback, panel);
        }

        [Test]
        public void TutorialTexts_DoNotCaptureEditorRaycasts()
        {
            Assert.That(ReadBool(FindSceneObject("Title"), "m_RaycastTarget"), Is.False);
            Assert.That(ReadBool(FindSceneObject("ObjectiveText"), "m_RaycastTarget"), Is.False);
            Assert.That(ReadBool(FindSceneObject("FeedbackText"), "m_RaycastTarget"), Is.False);
        }

        [Test]
        public void CodeInput_IsEditableAndConfiguredForMultipleLinesInScene()
        {
            GameObject codeInput = FindSceneObject("CodeInput");
            MonoBehaviour input = FindBehaviourWithProperty(codeInput, "m_ReadOnly");
            var serializedInput = new SerializedObject(input);

            Assert.That(serializedInput.FindProperty("m_Interactable").boolValue, Is.True);
            Assert.That(serializedInput.FindProperty("m_ReadOnly").boolValue, Is.False);
            Assert.That(serializedInput.FindProperty("m_LineType").enumValueIndex, Is.EqualTo(2));
            Assert.That(serializedInput.FindProperty("m_Text").stringValue, Is.Empty);
        }

        [Test]
        public void BattleButton_IsPartOfCodeEditorAndUsesExpectedLabel()
        {
            GameObject editorPanel = FindSceneObject("CodeEditorPanel");
            GameObject battleButton = FindSceneObject("BattleButton");

            Assert.That(battleButton.transform.parent, Is.SameAs(editorPanel.transform));
            Assert.That(ReadString(FindSceneObject("BattleButtonLabel"), "m_text"), Is.EqualTo("Batalhar"));
        }

        [Test]
        public void WizardPlaceholder_ContainsAVisibleSilhouette()
        {
            GameObject wizard = FindSceneObject("WizardPlaceholder");
            MonoBehaviour[] renderers = wizard.GetComponentsInChildren<MonoBehaviour>(true)
                .Where(candidate =>
                    HasProperty(candidate, "m_Sprite") &&
                    HasProperty(candidate, "m_Color") &&
                    HasProperty(candidate, "m_RaycastTarget"))
                .ToArray();

            Assert.That(wizard.transform.parent.name, Is.EqualTo("ArenaFrame"));
            Assert.That(wizard.GetComponent<RectTransform>(), Is.Not.Null);
            Assert.That(renderers, Has.Length.GreaterThanOrEqualTo(3));
            Assert.That(
                renderers.All(renderer =>
                    new SerializedObject(renderer).FindProperty("m_Sprite").objectReferenceValue != null),
                Is.True);
            Assert.That(renderers.All(renderer => renderer.enabled), Is.True);
        }

        private GameObject FindSceneObject(string objectName)
        {
            GameObject sceneObject = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(candidate => candidate.name == objectName)
                .Select(candidate => candidate.gameObject)
                .SingleOrDefault();
            Assert.That(sceneObject, Is.Not.Null, $"Objeto {objectName} não encontrado na cena.");
            return sceneObject;
        }

        private MonoBehaviour FindBehaviourWithProperty(string propertyName)
        {
            MonoBehaviour behaviour = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .SingleOrDefault(candidate => HasProperty(candidate, propertyName));
            Assert.That(behaviour, Is.Not.Null, $"Componente com {propertyName} não encontrado.");
            return behaviour;
        }

        private static MonoBehaviour FindBehaviourWithProperty(GameObject sceneObject, string propertyName)
        {
            MonoBehaviour behaviour = sceneObject.GetComponents<MonoBehaviour>()
                .SingleOrDefault(candidate => HasProperty(candidate, propertyName));
            Assert.That(
                behaviour,
                Is.Not.Null,
                $"{sceneObject.name} não possui componente com {propertyName}.");
            return behaviour;
        }

        private static bool HasProperty(MonoBehaviour behaviour, string propertyName)
        {
            return behaviour != null && new SerializedObject(behaviour).FindProperty(propertyName) != null;
        }

        private static string ReadString(GameObject sceneObject, string propertyName)
        {
            MonoBehaviour behaviour = FindBehaviourWithProperty(sceneObject, propertyName);
            return new SerializedObject(behaviour).FindProperty(propertyName).stringValue;
        }

        private static bool ReadBool(GameObject sceneObject, string propertyName)
        {
            MonoBehaviour behaviour = FindBehaviourWithProperty(sceneObject, propertyName);
            return new SerializedObject(behaviour).FindProperty(propertyName).boolValue;
        }

        private static void AssertContainedByPanel(GameObject child, GameObject panel)
        {
            var childCorners = new Vector3[4];
            var panelCorners = new Vector3[4];
            child.GetComponent<RectTransform>().GetWorldCorners(childCorners);
            panel.GetComponent<RectTransform>().GetWorldCorners(panelCorners);

            const float tolerance = 0.01f;
            foreach (Vector3 corner in childCorners)
            {
                Assert.That(corner.x, Is.InRange(panelCorners[0].x - tolerance, panelCorners[2].x + tolerance));
                Assert.That(corner.y, Is.InRange(panelCorners[0].y - tolerance, panelCorners[2].y + tolerance));
            }
        }
    }
}
