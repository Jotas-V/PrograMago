using System.Collections.Generic;
using PrograMago.Domain;

namespace PrograMago.Presentation
{
    public interface IArenaView
    {
        void SetClassDeclared(bool hasDeclaredClass);

        void CommitSilhouette();

        void ShowMago(MagoState mago);

        void ShowEnemies(IReadOnlyList<EnemyState> enemies);

        void Reset();
    }
}
