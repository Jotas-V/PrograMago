using System;
using System.Collections.Generic;

namespace PrograMago.Domain
{
    public enum CombatElement { Neutral, Fire, Water, Electric }

    public enum CombatAction { AnalyzeTarget, SelectSpell, Attack }

    public enum CombatOutcome { InProgress, Victory, Defeat }

    public enum CombatEventKind { Move, Hit, Ineffective, NoTarget }

    public sealed class CombatWizard
    {
        public CombatWizard(int life, int damage, int range, int initiative,
            int attackSpeed, params CombatElement[] spells)
        {
            if (life <= 0 || damage <= 0 || range <= 0 || initiative <= 0 ||
                attackSpeed <= 0 || attackSpeed >= 16)
                throw new ArgumentOutOfRangeException(nameof(life),
                    "Atributos de combate fora da faixa aceita.");
            if (spells == null || spells.Length == 0)
                throw new ArgumentException("O Mago precisa de uma magia.", nameof(spells));
            foreach (CombatElement spell in spells)
                if (!Enum.IsDefined(typeof(CombatElement), spell))
                    throw new ArgumentOutOfRangeException(nameof(spells));
            Life = life;
            Damage = damage;
            Range = range;
            Initiative = initiative;
            AttackSpeed = attackSpeed;
            Spells = Array.AsReadOnly((CombatElement[])spells.Clone());
        }

        public int Life { get; }
        public int Damage { get; }
        public int Range { get; }
        public int Initiative { get; }
        public int AttackSpeed { get; }
        public IReadOnlyList<CombatElement> Spells { get; }

        public static CombatWizard FromMago(MagoState mago,
            params CombatElement[] extraSpells)
        {
            if (mago == null) throw new ArgumentNullException(nameof(mago));
            var spells = new List<CombatElement> { CombatElement.Neutral };
            if (extraSpells != null) spells.AddRange(extraSpells);
            return new CombatWizard(mago.Vida, mago.Dano, mago.Alcance,
                mago.Iniciativa, mago.VelocidadeAtaque, spells.ToArray());
        }
    }

    public sealed class CombatEnemy
    {
        internal CombatEnemy(EnemyState source, int position)
        {
            Source = source;
            Life = source.Vida;
            Position = position;
            bool dummy = source.Elemento == "neutro";
            Damage = dummy ? 1 : 2;
            Range = 1;
            Initiative = dummy ? 1 : 5;
            AttackSpeed = dummy ? 3 : 5;
        }

        public EnemyState Source { get; }
        public int Life { get; internal set; }
        public int Position { get; internal set; }
        public int Damage { get; }
        public int Range { get; }
        public int Initiative { get; }
        public int AttackSpeed { get; }
        internal int NextDueTick { get; set; }
    }

    public sealed class CombatEvent
    {
        internal CombatEvent(int tick, string actor, CombatEventKind kind,
            string target, CombatElement element, int amount, int position)
        {
            Tick = tick;
            Actor = actor;
            Kind = kind;
            Target = target;
            Element = element;
            Amount = amount;
            Position = position;
        }

        public int Tick { get; }
        public string Actor { get; }
        public CombatEventKind Kind { get; }
        public string Target { get; }
        public CombatElement Element { get; }
        public int Amount { get; }
        public int Position { get; }
    }

    public sealed class CombatEngine
    {
        private readonly CombatWizard wizard;
        private readonly List<CombatEnemy> enemies = new List<CombatEnemy>();
        private readonly List<CombatAction> actionOrder = new List<CombatAction>
        {
            CombatAction.AnalyzeTarget,
            CombatAction.SelectSpell,
            CombatAction.Attack
        };
        private int wizardNextDueTick;

        public CombatEngine(CombatWizard wizard, IReadOnlyList<EnemyState> enemies)
        {
            this.wizard = wizard ?? throw new ArgumentNullException(nameof(wizard));
            if (enemies == null || enemies.Count == 0)
                throw new ArgumentException("A batalha precisa de inimigos.", nameof(enemies));
            for (int index = 0; index < enemies.Count; index++)
            {
                if (enemies[index] == null)
                    throw new ArgumentException("Inimigo ausente.", nameof(enemies));
                this.enemies.Add(new CombatEnemy(enemies[index], 4 + index * 2));
            }
            WizardLife = wizard.Life;
        }

        public int CurrentTick { get; private set; }
        public int WizardLife { get; private set; }
        public int WizardPosition { get; private set; }
        public CombatOutcome Outcome { get; private set; }
        public bool IsPaused { get; private set; }
        public IReadOnlyList<CombatEnemy> Enemies => enemies.AsReadOnly();
        public IReadOnlyList<CombatAction> ActionOrder => actionOrder.AsReadOnly();

        public CombatEvent Tick()
        {
            if (IsPaused || Outcome != CombatOutcome.InProgress) return null;

            int actor = FindReadyActor();
            CombatEvent result = null;
            if (actor == -1)
            {
                result = WizardTurn();
                wizardNextDueTick = CurrentTick + 16 - wizard.AttackSpeed;
            }
            else if (actor >= 0)
            {
                result = EnemyTurn(enemies[actor]);
                enemies[actor].NextDueTick = CurrentTick + 16 - enemies[actor].AttackSpeed;
            }
            CurrentTick++;
            return result;
        }

        public void Pause()
        {
            if (Outcome == CombatOutcome.InProgress) IsPaused = true;
        }

        public void MoveAction(int fromIndex, int toIndex)
        {
            if (!IsPaused || Outcome != CombatOutcome.InProgress)
                throw new InvalidOperationException("Reordene apenas durante a pausa.");
            if (fromIndex < 0 || fromIndex >= actionOrder.Count ||
                toIndex < 0 || toIndex >= actionOrder.Count)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
            CombatAction action = actionOrder[fromIndex];
            actionOrder.RemoveAt(fromIndex);
            actionOrder.Insert(toIndex, action);
        }

        public void Resume()
        {
            IsPaused = false;
        }

        public void Restart()
        {
            WizardLife = wizard.Life;
            WizardPosition = 0;
            CurrentTick = 0;
            wizardNextDueTick = 0;
            Outcome = CombatOutcome.InProgress;
            IsPaused = false;
            for (int index = 0; index < enemies.Count; index++)
            {
                enemies[index].Life = enemies[index].Source.Vida;
                enemies[index].Position = 4 + index * 2;
                enemies[index].NextDueTick = 0;
            }
        }

        private int FindReadyActor()
        {
            int actor = -2;
            int earliest = int.MaxValue;
            int initiative = int.MinValue;
            if (wizardNextDueTick <= CurrentTick)
            {
                actor = -1;
                earliest = wizardNextDueTick;
                initiative = wizard.Initiative;
            }
            for (int index = 0; index < enemies.Count; index++)
            {
                CombatEnemy enemy = enemies[index];
                if (enemy.Life <= 0 || enemy.NextDueTick > CurrentTick) continue;
                if (enemy.NextDueTick < earliest ||
                    (enemy.NextDueTick == earliest && enemy.Initiative > initiative))
                {
                    actor = index;
                    earliest = enemy.NextDueTick;
                    initiative = enemy.Initiative;
                }
            }
            return actor;
        }

        private CombatEvent WizardTurn()
        {
            CombatEnemy target = null;
            CombatElement selectedSpell = CombatElement.Neutral;
            CombatEvent result = null;
            foreach (CombatAction action in actionOrder)
            {
                switch (action)
                {
                    case CombatAction.AnalyzeTarget:
                        target = NearestLivingEnemy();
                        break;
                    case CombatAction.SelectSpell:
                        selectedSpell = SelectSpell(target);
                        break;
                    case CombatAction.Attack:
                        result = WizardAttack(target, selectedSpell);
                        break;
                }
            }
            return result;
        }

        private CombatEnemy NearestLivingEnemy()
        {
            CombatEnemy target = null;
            int bestDistance = int.MaxValue;
            foreach (CombatEnemy enemy in enemies)
            {
                int distance = Math.Abs(enemy.Position - WizardPosition);
                if (enemy.Life > 0 && distance < bestDistance)
                {
                    target = enemy;
                    bestDistance = distance;
                }
            }
            return target;
        }

        private CombatElement SelectSpell(CombatEnemy target)
        {
            CombatElement weakness = target == null ? CombatElement.Neutral :
                Weakness(target.Source.Elemento);
            foreach (CombatElement spell in wizard.Spells)
                if (spell == weakness) return spell;
            foreach (CombatElement spell in wizard.Spells)
                if (spell == CombatElement.Neutral) return spell;
            return wizard.Spells[0];
        }

        private CombatEvent WizardAttack(CombatEnemy target, CombatElement spell)
        {
            if (target == null)
                return new CombatEvent(CurrentTick, "Mago", CombatEventKind.NoTarget,
                    null, spell, 0, WizardPosition);
            int distance = Math.Abs(target.Position - WizardPosition);
            if (distance > wizard.Range)
            {
                WizardPosition += Math.Sign(target.Position - WizardPosition);
                return new CombatEvent(CurrentTick, "Mago", CombatEventKind.Move,
                    target.Source.VariableName, spell, 0, WizardPosition);
            }
            int damage = target.Source.Elemento == "neutro" ? wizard.Damage :
                spell == Weakness(target.Source.Elemento) ? wizard.Damage * 2 : 0;
            target.Life = Math.Max(0, target.Life - damage);
            if (AllEnemiesDefeated()) Outcome = CombatOutcome.Victory;
            return new CombatEvent(CurrentTick, "Mago",
                damage == 0 ? CombatEventKind.Ineffective : CombatEventKind.Hit,
                target.Source.VariableName, spell, damage, WizardPosition);
        }

        private CombatEvent EnemyTurn(CombatEnemy enemy)
        {
            if (Math.Abs(enemy.Position - WizardPosition) > enemy.Range)
            {
                enemy.Position += Math.Sign(WizardPosition - enemy.Position);
                return new CombatEvent(CurrentTick, enemy.Source.VariableName,
                    CombatEventKind.Move, "Mago", CombatElement.Neutral, 0,
                    enemy.Position);
            }
            WizardLife = Math.Max(0, WizardLife - enemy.Damage);
            if (WizardLife == 0) Outcome = CombatOutcome.Defeat;
            return new CombatEvent(CurrentTick, enemy.Source.VariableName,
                CombatEventKind.Hit, "Mago", CombatElement.Neutral, enemy.Damage,
                enemy.Position);
        }

        private bool AllEnemiesDefeated()
        {
            foreach (CombatEnemy enemy in enemies)
                if (enemy.Life > 0) return false;
            return true;
        }

        private static CombatElement Weakness(string element)
        {
            switch (element)
            {
                case "gelo": return CombatElement.Fire;
                case "fogo": return CombatElement.Water;
                case "água": return CombatElement.Electric;
                default: return CombatElement.Neutral;
            }
        }
    }
}
