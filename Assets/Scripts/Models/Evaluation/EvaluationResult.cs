using Assets.Scripts.Enums;

namespace Assets.Scripts.Models.Evaluation
{
    public class EvaluationResult
    {
        // TODO Remove Status if not needed
        public readonly EvaluationResultStatus Status;

        public readonly int Score;

        public EvaluationResult(int score)
        {
            Score = score;
        }
    }
}
