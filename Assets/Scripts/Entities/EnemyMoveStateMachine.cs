namespace UZSG.Entities
{
    public enum EnemyMoveStates {
        Idle, Walk, Run, Crawl,
    }
    
    public class EnemyMoveStateMachine : StateMachine<EnemyMoveStates>
    {

    }
}