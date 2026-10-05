using System;
using System.Collections.Generic;

namespace PrograMago.Domain
{
    public enum CombatElement { Neutral, Fire, Water, Electric }

    public enum CombatAction { AnalyzeTarget, SelectSpell, Attack, Cast }

    public enum CombatOutcome { InProgress, Victory, Defeat }

    public enum CombatEventKind { Move, Hit, Ineffective, NoTarget, MissingSpell, TargetAnalyzed, SpellSelected, OutOfRange, Blocked }

    public class CombatWizard
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

        protected CombatWizard(CombatWizard source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Life = source.Life;
            Damage = source.Damage;
            Range = source.Range;
            Initiative = source.Initiative;
            AttackSpeed = source.AttackSpeed;
            Spells = source.Spells;
        }

        public int Life { get; }
        public int Damage { get; }
        public int Range { get; }
        public int Initiative { get; }
        public int AttackSpeed { get; }
        public IReadOnlyList<CombatElement> Spells { get; }
        public virtual string Form => "neutro";
        public virtual CombatElement Spell => CombatElement.Neutral;

        public static CombatWizard Specialize(CombatWizard baseWizard, string form)
        {
            if (baseWizard == null) throw new ArgumentNullException(nameof(baseWizard));
            switch (form)
            {
                case "neutro": return baseWizard;
                case "piromante": return new Piromante(baseWizard);
                case "hidromante": return new Hidromante(baseWizard);
                case "eletromante": return new Eletromante(baseWizard);
                default: throw new ArgumentException("Forma elemental desconhecida.", nameof(form));
            }
        }

        public static CombatWizard FromMago(MagoState mago,
            params CombatElement[] extraSpells)
        {
            if (mago == null) throw new ArgumentNullException(nameof(mago));
            var spells = new List<CombatElement>
            {
                CombatElement.Neutral,
                CombatElement.Fire,
                CombatElement.Water,
                CombatElement.Electric
            };
            if (extraSpells != null)
                foreach (CombatElement extraSpell in extraSpells)
                    if (!spells.Contains(extraSpell)) spells.Add(extraSpell);
            return new CombatWizard(mago.Vida, mago.Dano, mago.Alcance,
                mago.Iniciativa, mago.VelocidadeAtaque, spells.ToArray());
        }
    }

    public sealed class Piromante : CombatWizard
    {
        public Piromante(CombatWizard baseWizard) : base(baseWizard) { }
        public override string Form => "piromante";
        public override CombatElement Spell => CombatElement.Fire;
    }

    public sealed class Hidromante : CombatWizard
    {
        public Hidromante(CombatWizard baseWizard) : base(baseWizard) { }
        public override string Form => "hidromante";
        public override CombatElement Spell => CombatElement.Water;
    }

    public sealed class Eletromante : CombatWizard
    {
        public Eletromante(CombatWizard baseWizard) : base(baseWizard) { }
        public override string Form => "eletromante";
        public override CombatElement Spell => CombatElement.Electric;
    }

    public sealed class CombatEnemy
    {
        internal CombatEnemy(EnemyState source, int position, bool finalEncounter = false)
        {
            Source = source;
            Life = source.Vida;
            Position = position;
            InitialPosition = position;
            bool dummy = source.Elemento == "neutro";
            Damage = dummy ? 0 : 2;
            Range = 1;
            Initiative = dummy ? 1 : 5;
            AttackSpeed = dummy ? 3 : 5;
            if (finalEncounter && !dummy)
            {
                switch (source.Elemento)
                {
                    case "gelo": Damage = 2; Range = 2; Initiative = 3; AttackSpeed = 4; break;
                    case "fogo": Damage = 1; Range = 10; Initiative = 7; AttackSpeed = 4; break;
                    case "água": Damage = 1; Range = 2; Initiative = 6; AttackSpeed = 6; break;
                }
            }
        }

        public EnemyState Source { get; }
        public int Life { get; internal set; }
        public int Position { get; internal set; }
        internal int InitialPosition { get; }
        public int Damage { get; }
        public int Range { get; }
        public int Initiative { get; }
        public int AttackSpeed { get; }
        internal int NextDueTick { get; set; }
    }

    public sealed class CombatEvent
    {
        internal CombatEvent(int tick, string actor, CombatEventKind kind,
            string target, CombatElement element, int amount, int position, int blockIndex = -1)
        {
            Tick = tick;
            Actor = actor;
            Kind = kind;
            Target = target;
            Element = element;
            Amount = amount;
            Position = position;
            BlockIndex = blockIndex;
        }

        public int Tick { get; }
        public string Actor { get; }
        public CombatEventKind Kind { get; }
        public string Target { get; }
        public CombatElement Element { get; }
        public int Amount { get; }
        public int Position { get; }
        public int BlockIndex { get; }
    }

    public sealed class CombatEngine
    {
        public const int CellCount = 16;
        private readonly CombatWizard wizard;
        private CombatStrategy strategy;
        private readonly List<CombatEnemy> enemies = new List<CombatEnemy>();
        private readonly List<CombatAction> actionOrder = new List<CombatAction>
        {
            CombatAction.AnalyzeTarget,
            CombatAction.SelectSpell,
            CombatAction.Attack
        };
        private int wizardNextDueTick;
        private readonly List<int> sourceBlocks = new List<int> { 0, 1, 2 };
        private readonly List<CombatEvent> eventsThisTick = new List<CombatEvent>();
        private int executingBlock = -1;

        public CombatEngine(CombatWizard wizard, IReadOnlyList<EnemyState> enemies,
            CombatStrategy strategy = null, bool finalEncounter = false)
        {
            this.wizard = wizard ?? throw new ArgumentNullException(nameof(wizard));
            this.strategy = strategy ?? new CombatStrategy(null, "neutro");
            if (enemies == null || enemies.Count == 0 || enemies.Count >= CellCount)
                throw new ArgumentException("A batalha precisa de inimigos.", nameof(enemies));
            for (int index = 0; index < enemies.Count; index++)
            {
                if (enemies[index] == null)
                    throw new ArgumentException("Inimigo ausente.", nameof(enemies));
                this.enemies.Add(new CombatEnemy(enemies[index], CellCount - 1 - index -
                    (finalEncounter ? 3 : 0), finalEncounter));
            }
            WizardLife = wizard.Life;
        }

        public int CurrentTick { get; private set; }
        public int WizardLife { get; private set; }
        public int WizardPosition { get; private set; }
        public string WizardForm { get; private set; } = "neutro";
        public CombatOutcome Outcome { get; private set; }
        public bool IsPaused { get; private set; }
        public IReadOnlyList<CombatEnemy> Enemies => enemies.AsReadOnly();
        public IReadOnlyList<CombatAction> ActionOrder => actionOrder.AsReadOnly();
        public IReadOnlyList<CombatEvent> EventsThisTick => eventsThisTick.AsReadOnly();

        public void ApplyDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (Outcome != CombatOutcome.InProgress) return;
            WizardLife = Math.Max(0, WizardLife - amount);
            if (WizardLife == 0) Outcome = CombatOutcome.Defeat;
        }

        public void ConfigureActions(IReadOnlyList<CombatAction> actions, IReadOnlyList<int> origins)
        {
            if (CurrentTick != 0 && !IsPaused) throw new InvalidOperationException("Pause para alterar o programa.");
            if (actions == null || origins == null || actions.Count == 0 || actions.Count > 48 || actions.Count != origins.Count)
                throw new ArgumentException("Programa de combate inválido.");
            for (int i = 0; i < actions.Count; i++)
                if (!Enum.IsDefined(typeof(CombatAction), actions[i]) || origins[i] < 0)
                    throw new ArgumentException("Comando de combate inválido.");
            actionOrder.Clear();
            sourceBlocks.Clear();
            for (int i = 0; i < actions.Count; i++) { actionOrder.Add(actions[i]); sourceBlocks.Add(origins[i]); }
        }

        public void ConfigureStrategy(CombatStrategy strategy)
        {
            if (strategy == null) throw new ArgumentNullException(nameof(strategy));
            if (CurrentTick != 0 && !IsPaused)
                throw new InvalidOperationException("Pause para alterar a Estratégia.");
            this.strategy = strategy;
        }

        public CombatEvent Tick()
        {
            eventsThisTick.Clear();
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
                eventsThisTick.Add(result);
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
            int block = sourceBlocks[fromIndex];
            sourceBlocks.RemoveAt(fromIndex);
            sourceBlocks.Insert(toIndex, block);
        }

        public void Resume()
        {
            IsPaused = false;
            eventsThisTick.Clear();
        }

        public void Restart()
        {
            WizardLife = wizard.Life;
            WizardPosition = 0;
            WizardForm = "neutro";
            CurrentTick = 0;
            wizardNextDueTick = 0;
            Outcome = CombatOutcome.InProgress;
            IsPaused = false;
            eventsThisTick.Clear();
            for (int index = 0; index < enemies.Count; index++)
            {
                enemies[index].Life = enemies[index].Source.Vida;
                enemies[index].Position = enemies[index].InitialPosition;
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
                if (enemy.Life <= 0 || enemy.Damage == 0 || enemy.NextDueTick > CurrentTick) continue;
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
            bool selected = false;
            CombatEvent result = null;
            for (int index = 0; index < actionOrder.Count && Outcome == CombatOutcome.InProgress; index++)
            {
                executingBlock = sourceBlocks[index];
                switch (actionOrder[index])
                {
                    case CombatAction.AnalyzeTarget:
                        target = NearestLivingEnemy();
                        selected = false;
                        eventsThisTick.Add(new CombatEvent(CurrentTick, "Mago", CombatEventKind.TargetAnalyzed,
                            target?.Source.VariableName, CombatElement.Neutral, 0, WizardPosition, executingBlock));
                        break;
                    case CombatAction.SelectSpell:
                        selectedSpell = SelectSpell(target);
                        selected = target != null;
                        eventsThisTick.Add(new CombatEvent(CurrentTick, "Mago", selected ? CombatEventKind.SpellSelected : CombatEventKind.NoTarget,
                            target?.Source.VariableName, selectedSpell, 0, WizardPosition, executingBlock));
                        break;
                    case CombatAction.Attack:
                        result = WizardAttack(target, selectedSpell, selected);
                        eventsThisTick.Add(result);
                        break;
                    case CombatAction.Cast:
                        target = NearestLivingEnemy();
                        selectedSpell = SelectSpell(target);
                        result = WizardAttack(target, selectedSpell, target != null);
                        eventsThisTick.Add(result);
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
            string targetElement = target == null ? "neutro" : target.Source.Elemento;
            CombatWizard specialization = CombatWizard.Specialize(
                wizard, strategy.FormFor(targetElement));
            WizardForm = specialization.Form;
            return specialization.Spell;
        }

        private CombatEvent WizardAttack(CombatEnemy target, CombatElement spell, bool selected)
        {
            if (target == null || target.Life <= 0)
                return new CombatEvent(CurrentTick, "Mago", CombatEventKind.NoTarget,
                    null, spell, 0, WizardPosition, executingBlock);
            if (!selected)
                return new CombatEvent(CurrentTick, "Mago", CombatEventKind.MissingSpell,
                    target.Source.VariableName, spell, 0, WizardPosition, executingBlock);
            bool ownsSpell = false;
            foreach (CombatElement knownSpell in wizard.Spells)
                if (knownSpell == spell) { ownsSpell = true; break; }
            if (!ownsSpell)
                return new CombatEvent(CurrentTick, "Mago", CombatEventKind.MissingSpell,
                    target.Source.VariableName, spell, 0, WizardPosition, executingBlock);
            int distance = Math.Abs(target.Position - WizardPosition);
            if (distance > wizard.Range)
            {
                WizardPosition += Math.Sign(target.Position - WizardPosition);
                return new CombatEvent(CurrentTick, "Mago", CombatEventKind.Move,
                    target.Source.VariableName, spell, 0, WizardPosition, executingBlock);
            }
            int damage = target.Source.Elemento == "neutro" ? wizard.Damage :
                spell == Weakness(target.Source.Elemento) ? wizard.Damage * 2 : 0;
            target.Life = Math.Max(0, target.Life - damage);
            if (AllEnemiesDefeated()) Outcome = CombatOutcome.Victory;
            return new CombatEvent(CurrentTick, "Mago",
                damage == 0 ? CombatEventKind.Ineffective : CombatEventKind.Hit,
                target.Source.VariableName, spell, damage, WizardPosition, executingBlock);
        }

        private CombatEvent EnemyTurn(CombatEnemy enemy)
        {
            if (Math.Abs(enemy.Position - WizardPosition) > enemy.Range)
            {
                int next = enemy.Position + Math.Sign(WizardPosition - enemy.Position);
                foreach (CombatEnemy occupant in enemies)
                    if (occupant != enemy && occupant.Life > 0 && occupant.Position == next)
                        return new CombatEvent(CurrentTick, enemy.Source.VariableName,
                            CombatEventKind.Blocked, "Mago", CombatElement.Neutral, 0, enemy.Position);
                enemy.Position = next;
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
