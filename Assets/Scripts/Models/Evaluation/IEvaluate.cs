namespace Assets.Scripts.Models.Evaluation
{
    public interface IEvaluate
    {
        EvaluationResult Evaluate(EvaluationRules rule);
    }
}
