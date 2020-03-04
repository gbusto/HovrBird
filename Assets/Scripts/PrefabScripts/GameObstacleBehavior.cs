using UnityEngine;

public class GameObstacleBehavior : MonoBehaviour
{
    public ObstaclePrefab op;
    public int _id;
    public bool disableColliders; // Whether or not colliders should be disabled
    public bool active; // Whether or not the Update function should be called on the object

    public bool cleanup; // True when the object should be destroyed

    public void InitGameObstacle(ObstaclePrefab op, bool disableColliders = false)
    {
        this.op = op;
        _id = op._id;
        this.disableColliders = disableColliders;
        active = true;
    }
}
