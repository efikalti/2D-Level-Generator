using Assets.Scripts.Enums;
using UnityEngine;

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

        public void PrintResult(string resultName = "")
        {
            Debug.Log($"Score for {resultName} is : {Score}");
        }
    }
}
