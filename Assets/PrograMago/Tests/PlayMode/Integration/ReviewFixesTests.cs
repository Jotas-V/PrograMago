using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.UnityIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace PrograMago.Tests.Integration
{
    public class ReviewFixesTests
    {
        private string savePath;
        private byte[] save;
        private TMP_InputField input;
        private GameplayBootstrapper game;
        private float master, effects;
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private Component Editor {
            get {
                var type = typeof(GameplayBootstrapper).Assembly.GetType("PrograMago.UnityIntegration.CodeEditorSession");
                Assert.That(type, Is.Not.Null, "Editor precisa de historico por documento.");
                var component = input.GetComponent(type);
                Assert.That(component, Is.Not.Null);
                return component;
            }
        }
        private void Load(string key, string text) => Editor.GetType().GetMethod("LoadDocument").Invoke(Editor, new object[]{key,text});
        private bool Undo() => (bool)Editor.GetType().GetMethod("Undo").Invoke(Editor, null);

        [UnitySetUp] public IEnumerator SetUp()
        {
            savePath = Path.Combine(Application.persistentDataPath, "programago-phase1.json");
            save = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
            if (save != null) File.Delete(savePath);
            master = GameAudioSettings.Master; effects = GameAudioSettings.Effects;
            GameAudioSettings.Set(GameAudioSettings.MasterKey, 1);
            GameAudioSettings.Set(GameAudioSettings.EffectsKey, 1);
            SceneManager.LoadScene("MainScene"); yield return null; yield return null;
            game = Object.FindFirstObjectByType<GameplayBootstrapper>();
            input = GameObject.Find("CodeInput").GetComponent<TMP_InputField>();
            foreach(var source in GameAudio.Instance.GetComponentsInChildren<AudioSource>()) if(!source.loop) source.Stop();
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if(game != null) Object.Destroy(game.gameObject);
            yield return null;
            if(save == null) { if(File.Exists(savePath)) File.Delete(savePath); }
            else File.WriteAllBytes(savePath,save);
            GameAudioSettings.Set(GameAudioSettings.MasterKey, master);
            GameAudioSettings.Set(GameAudioSettings.EffectsKey, effects);
        }
        [UnityTest] public IEnumerator UndoRestoresPasteAndKeepsDocumentHistoriesIndependent()
        {
            Load("mago", "class Mago {}"); input.stringPosition = 6; input.selectionStringFocusPosition = 10;
            yield return null;
            input.text = "class Bruxo {}"; yield return null;
            Load("estrategia", "ação"); input.text = "ação\nágua"; yield return null;
            Assert.That(Undo(), Is.True); Assert.That(input.text, Is.EqualTo("ação"));
            Load("mago", "class Bruxo {}");
            Assert.That(Undo(), Is.True); Assert.That(input.text, Is.EqualTo("class Mago {}"));
            Assert.That(input.selectionStringAnchorPosition, Is.EqualTo(6)); Assert.That(input.selectionStringFocusPosition, Is.EqualTo(10));
            Assert.That(Undo(), Is.False);
        }
        [UnityTest] public IEnumerator ContinuousTypingUndoesAsOneEdit()
        {
            Load("mago", ""); input.text="a"; input.text="ab"; input.text="abc";
            yield return null;
            Assert.That(Undo(), Is.True); Assert.That(input.text, Is.Empty);
        }
        [UnityTest] public IEnumerator LongCodeUsesAnInternalScrollbarAndStableTextGeometry()
        {
            Load("mago", string.Join("\n", Enumerable.Repeat("// água e ação < >",100)));
            yield return null;
            Assert.That(input.verticalScrollbar, Is.Not.Null);
            Assert.That(input.verticalScrollbar.GetComponent<RectTransform>().rect.width, Is.InRange(4f,12f));
            Assert.That(input.textComponent.enableAutoSizing, Is.False);
            Assert.That(input.textComponent.richText, Is.False);
            Assert.That(input.textComponent.margin, Is.EqualTo(Vector4.zero));
            input.verticalScrollbar.value=1; yield return null;
            Assert.That(input.textComponent.rectTransform.anchoredPosition.y, Is.GreaterThan(0));
            Assert.That(input.interactable && !input.readOnly, Is.True);
        }
        [UnityTest] public IEnumerator SwitchingFromScrolledLongCodeToShortCodeKeepsTextAtTop()
        {
            Load("long", string.Join("\n", Enumerable.Repeat("// exemplo",60)));
            yield return null;
            input.verticalScrollbar.value=.8f;
            yield return null;
            Load("short", "mago.setAlcance(5);");
            yield return null; yield return null;
            Assert.That(input.textComponent.rectTransform.anchoredPosition.y,Is.EqualTo(0).Within(1f));
            Assert.That(input.verticalScrollbar.value,Is.EqualTo(0).Within(.01f));
        }
        [UnityTest] public IEnumerator NativeEditingAfterScrollAndAccentsKeepsCaretAligned()
        {
            string source=string.Join("\n",Enumerable.Repeat("// exemplo",50))+"\nágua <alvo>";
            Load("mago",source);
            input.caretBlinkRate=0; input.ActivateInputField();
            yield return null; yield return null;
            int position=source.IndexOf("água",StringComparison.Ordinal)+1;
            input.stringPosition=position; input.ForceLabelUpdate();
            input.ProcessEvent(new Event { type=EventType.KeyDown, keyCode=KeyCode.X, character='x' });
            yield return null; yield return null;
            Assert.That(input.text,Is.EqualTo(source.Insert(position,"x")));
            Assert.That(input.stringPosition,Is.EqualTo(position+1));
            var caret=input.GetComponentsInChildren<RectTransform>().First(t=>t.name.EndsWith("Caret",StringComparison.Ordinal));
            Assert.That(Vector2.Distance(caret.anchoredPosition,input.textComponent.rectTransform.anchoredPosition),Is.LessThan(.01f));
            var mesh=caret.GetComponent<CanvasRenderer>().GetMesh();
            Assert.That(mesh.vertexCount,Is.GreaterThanOrEqualTo(4));
            float expected=input.textComponent.textInfo.characterInfo[input.caretPosition].origin;
            Assert.That(mesh.vertices[0].x,Is.EqualTo(expected).Within(2f));
            Assert.That(Undo(),Is.True); yield return null;
            Assert.That(input.text,Is.EqualTo(source));
            Assert.That(input.isFocused,Is.True);
        }
        [UnityTest] public IEnumerator BothGrimoirePagesHaveThinInternalScrollbars()
        {
            SceneManager.LoadScene("GrimoireScene"); yield return null;
            var scrolls=Object.FindObjectsByType<ScrollRect>(FindObjectsSortMode.None);
            Assert.That(scrolls.Length, Is.EqualTo(2));
            foreach(var scroll in scrolls) {
                Assert.That(scroll.verticalScrollbar,Is.Not.Null);
                Assert.That(scroll.verticalScrollbar.GetComponent<RectTransform>().rect.width, Is.InRange(4f,12f));
            }
        }
        [Test] public void GameplayHoverHasHalfTheMenuGain()
        {
            var method=typeof(GameAudio).GetMethod("HoverGainForScene",BindingFlags.Static|BindingFlags.Public);
            Assert.That(method,Is.Not.Null);
            Assert.That(method.Invoke(null,new object[]{"MenuScene"}),Is.EqualTo(1f));
            Assert.That(method.Invoke(null,new object[]{"MainScene"}),Is.EqualTo(.5f));
            Assert.That(method.Invoke(null,new object[]{"GrimoireScene"}),Is.EqualTo(.5f));
        }
        private GameObject PrepareActors()
        {
            var constructor=typeof(MagoState).GetConstructors(Private).Single();
            game.ShowMago((MagoState)constructor.Invoke(new object[]{"mago",10,4,5,3,3,0}));
            game.ShowEnemies(new[]{new EnemyState("golem","Golem de Gelo",12,"gelo")});
            return GameObject.Find("EnemyMarker-golem");
        }
        [UnityTest] public IEnumerator EnemyFacesWizardAndWalksInsteadOfTeleporting()
        {
            var enemy=PrepareActors(); yield return null;
            Assert.That(enemy.GetComponent<SpriteRenderer>().flipX,Is.True);
            var method=typeof(GameplayBootstrapper).GetMethod("AnimateEnemyMove",Private);
            Assert.That(method,Is.Not.Null);
            float before=enemy.transform.position.x;
            method.Invoke(game,new object[]{enemy,14}); yield return new WaitForSecondsRealtime(.1f);
            Assert.That(enemy.transform.position.x,Is.LessThan(before));
            Assert.That(enemy.GetComponent<Animator>().GetBool("Walking"),Is.True);
            yield return new WaitForSecondsRealtime(.6f);
            Assert.That(enemy.GetComponent<Animator>().GetBool("Walking"),Is.False);
        }
        [UnityTest] public IEnumerator WizardMovementUsesApprovedMapFootstep()
        {
            PrepareActors();
            typeof(GameplayBootstrapper).GetMethod("AnimateWizardMove",Private).Invoke(game,new object[]{1});
            yield return null;
            Assert.That(GameAudio.Instance.GetComponentsInChildren<AudioSource>().Any(s=>!s.loop && s.isPlaying && s.clip.name=="MapStep"),Is.True);
        }
        [UnityTest] public IEnumerator WizardDamageIsRedEvenForNeutralDamage()
        {
            PrepareActors(); var wizard=(GameObject)typeof(GameplayBootstrapper).GetField("wizardInstance",Private).GetValue(game);
            typeof(GameplayBootstrapper).GetMethod("FlashCombatTarget",Private).Invoke(game,new object[]{wizard,CombatElement.Neutral});
            yield return null;
            var sprite=wizard.GetComponent<SpriteRenderer>();
            Assert.That(sprite.color.r,Is.GreaterThan(sprite.color.g));
            Assert.That(sprite.color.r,Is.GreaterThan(sprite.color.b));
            yield return new WaitForSecondsRealtime(.35f);
            Assert.That(sprite.color.g,Is.EqualTo(1f).Within(.01f));
        }
    }
}
