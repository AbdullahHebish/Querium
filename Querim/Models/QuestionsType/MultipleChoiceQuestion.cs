namespace Querim.Models.QuestionsType
{
    public class MultipleChoiceQuestion
    {
        public string Text { get; set; }
        public List<string> Options { get; set; } = new();
        public string CorrectOption { get; set; }
    }
}
