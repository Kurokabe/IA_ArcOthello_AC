using System.ComponentModel;

namespace ArcOthello_AC
{
    /// <summary>
    /// Class representing a board's piece.
    /// </summary>
    public class Piece : INotifyPropertyChanged
    {
        #region Properties
        public int X { get; private set; }
        public int Y { get; private set; }

        private Team team;

        public Team Team
        {
            get { return team; }
            set {
                team = value;
                RaisePropertyChanged("team");
            }
        }
        #endregion

        #region Constructor
        public Piece(Team team, int x, int y)
        {
            Team = team;
            this.X = x;
            this.Y = y;
        }

        public Piece(Piece p)
        {
            this.team = p.team;
            this.X = p.X;
            this.Y = p.Y;
        }
        #endregion

        public void Flip()
        {
            Team = Team == Team.Black ? Team.White : Team.Black;
        }

        public void SetTeam(Team team)
        {
            this.Team = team;
        }

        #region PropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
