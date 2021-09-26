namespace QFighterPolice.Models
{
    public class AnsweredQuestion
    {
        public AnsweredQuestion(Question question, string answer)
        {
            Question = question;
            Answer = answer;
        }

        public Question Question { get; }
        public string Answer { get; }
    }
}
