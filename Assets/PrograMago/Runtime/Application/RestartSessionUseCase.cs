using System;
using PrograMago.Domain;

namespace PrograMago.Application
{
    public sealed class RestartSessionUseCase
    {
        private readonly LearningSession session;

        public RestartSessionUseCase(LearningSession session)
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public bool Execute()
        {
            session.Restart();
            return session.HasDeclaredClass;
        }
    }
}
