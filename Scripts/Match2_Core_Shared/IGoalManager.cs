public interface IGoalManager
{
    public int MovesLeft { get; }
    public int TotalMoves { get; }
    public bool IsGameOver { get; }
    public GoalCubeAndCount[] GoalCubeAndCounts { get; }
    public void UpdateGoal(int cubeType);
    public void UpdateMovesLeft();
}

public class GoalCubeAndCount
    {
        public int cubeType;
        public int count;
        
        public GoalCubeAndCount(int cubeType, int count)
        {
            this.cubeType = cubeType;
            this.count = count;
        }
    }