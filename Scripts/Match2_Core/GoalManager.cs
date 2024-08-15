public class GoalManager : IGoalManager
{
    public int MovesLeft { get; private set; }
    public int TotalMoves { get; private set; }
    public GoalCubeAndCount[] GoalCubeAndCounts { get; private set; }
    public bool IsGameOver { get; private set; }

    public GoalManager(int totalMoves, GoalCubeAndCount[] goalCubeAndCounts)
    {
        TotalMoves = totalMoves;
        MovesLeft = totalMoves;
        this.GoalCubeAndCounts = goalCubeAndCounts;
    }

    public void UpdateGoal(int cubeType)
    {
        for (int i = 0; i < GoalCubeAndCounts.Length; i++)
        {
            if (GoalCubeAndCounts[i].cubeType == cubeType && GoalCubeAndCounts[i].count != 0)
            {
                GoalCubeAndCounts[i].count--;
            }
        }

        if (GoalCubeAndCounts[0].count == 0 && GoalCubeAndCounts[1].count == 0)
        {
            IsGameOver = true;
        }
    }

    public void UpdateMovesLeft()
    {
        MovesLeft--;
        if (MovesLeft == 0)
        {
            IsGameOver = true;
        }
    }
}