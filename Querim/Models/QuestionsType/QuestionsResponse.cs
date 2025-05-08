namespace Querim.Models.QuestionsType
{
    public class QuestionsResponse
    {
        public List<TrueFalseQuestion> TrueFalseQuestions { get; set; } = new();
        public List<MultipleChoiceQuestion> MultipleChoiceQuestions { get; set; } = new();
    }
}
