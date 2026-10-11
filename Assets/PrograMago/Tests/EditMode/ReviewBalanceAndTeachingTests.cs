using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PrograMago.Domain;
using UnityEditor;

namespace PrograMago.Tests.UnityIntegration
{
    public class ReviewBalanceAndTeachingTests
    {
        private static LearningPath LoadPath()
        {
            var type=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("PrograMago.UnityIntegration.LearningPathAsset")).First(t=>t!=null);
            var asset=AssetDatabase.LoadAssetAtPath("Assets/PrograMago/Content/LearningPath.asset",type);
            return (LearningPath)type.GetMethod("ToDomain").Invoke(asset,null);
        }
        [TestCase("gelo",2,4,5)]
        [TestCase("fogo",2,4,5)]
        [TestCase("água",2,4,5)]
        [TestCase("gelo",2,2,3)]
        [TestCase("gelo",7,4,1)]
        [TestCase("fogo",7,4,1)]
        [TestCase("água",7,4,1)]
        [TestCase("gelo",9,2,3)]
        [TestCase("fogo",9,2,3)]
        [TestCase("água",9,2,3)]
        public void ElementalEnemyCanCounterattackWhileCorrectBuildCanWin(string element,int range,int damage,int speed)
        {
            int life = 25-range-damage-speed-3;
            var wizard=new CombatWizard(life,damage,range,3,speed,CombatElement.Fire,CombatElement.Water,CombatElement.Electric);
            var strategy=new CombatStrategy(new Dictionary<string,string>{{"gelo","piromante"},{"fogo","hidromante"},{"água","eletromante"}},"neutro");
            var engine=new CombatEngine(wizard,new[]{new EnemyState("alvo","Alvo",12,element)},strategy);
            int attacks=0;
            for(int tick=0;tick<600 && engine.Outcome==CombatOutcome.InProgress;tick++) {
                engine.Tick();
                attacks+=engine.EventsThisTick.Count(e=>e.Actor=="alvo" && e.Kind==CombatEventKind.Hit);
            }
            Assert.That(attacks,Is.GreaterThan(0),"Contra-ataque precisa acontecer antes da vitoria.");
            Assert.That(engine.Outcome,Is.EqualTo(CombatOutcome.Victory));
            Assert.That(engine.WizardLife,Is.GreaterThan(0));
        }
        [Test] public void TrainingDummyRemainsHarmless()
        {
            var engine=new CombatEngine(new CombatWizard(10,3,5,3,4,CombatElement.Neutral),new[]{new EnemyState("boneco","Boneco de Treinamento",10,"neutro")});
            for(int i=0;i<600 && engine.Outcome==CombatOutcome.InProgress;i++) engine.Tick();
            Assert.That(engine.Outcome,Is.EqualTo(CombatOutcome.Victory));
            Assert.That(engine.WizardLife,Is.EqualTo(10));
        }
        [Test] public void IntroductoryPyromancerWithFourLifeCanStillWin()
        {
            var wizard = new CombatWizard(4,3,5,1,2,CombatElement.Fire);
            var engine = new CombatEngine(wizard,new[]{new EnemyState("golem","Golem de Gelo",12,"gelo")},
                new CombatStrategy(null,"piromante"));
            for(int i=0;i<600 && engine.Outcome==CombatOutcome.InProgress;i++) engine.Tick();
            Assert.That(engine.Outcome,Is.EqualTo(CombatOutcome.Victory));
        }
        [Test] public void SettersLessonExplainsUsingAdjustmentsAndPointBudget()
        {
            var path=LoadPath();
            var lesson=path.Battles[3].Lesson;
            Assert.That(lesson.Task,Does.Contain("Ajustes"));
            Assert.That(lesson.Task,Does.Contain("25"));
            Assert.That(lesson.Task,Does.Contain("setVida"));
            Assert.That(lesson.Task,Does.Contain("chamadas"));
        }
        [Test] public void EveryGrimoireLessonHasDistinctExampleFromAnotherContext()
        {
            var path=LoadPath();
            Assert.That(path.Battles.Select(b=>b.Lesson.UsageExample).Distinct().Count(),Is.EqualTo(9));
            foreach(var battle in path.Battles) {
                string example=battle.Lesson.UsageExample;
                Assert.That(example,Does.Contain("<noparse>"));
                foreach(var code in System.Text.RegularExpressions.Regex.Matches(example,@"<noparse>(.*?)</noparse>",System.Text.RegularExpressions.RegexOptions.Singleline).Cast<System.Text.RegularExpressions.Match>()) {
                    Assert.That(code.Groups[1].Value,Does.Not.Match(@"\b(Mago|Inimigo|Piromante|Hidromante|Eletromante)\b"));
                }
            }
        }
    }
}
