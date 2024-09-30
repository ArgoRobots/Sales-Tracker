namespace Sales_Tracker.DataClasses
{
    public class SearchResult(string name, Image flag, int score)
    {
        // Properties
        private string _name = name;
        private Image _flag = flag;
        private int _score = score;

        // Getters and setters
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        public Image Flag
        {
            get => _flag;
            set => _flag = value;
        }
        public int Score
        {
            get => _score;
            set => _score = value;
        }
    }
}