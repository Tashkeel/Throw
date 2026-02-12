using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and throws dice from a designated spawn point at regular intervals.
/// </summary>
public class DiceThrower : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The dice manager to notify when dice are spawned or cleared")]
    private DiceManager _diceManager;

    [Header("Spawning")]
    [SerializeField]
    [Tooltip("The transform position where dice will be spawned")]
    private Transform _spawnPoint;

    [SerializeField]
    [Tooltip("The die prefab to instantiate")]
    private GameplayDie _diePrefab;

    [SerializeField]
    [Tooltip("Number of dice to throw")]
    [Min(1)]
    private int _numberOfDice = 1;

    [SerializeField]
    [Tooltip("Time interval between spawning each die (in seconds)")]
    [Min(0f)]
    private float _spawnInterval = 0.2f;

    [Header("Throw Force")]
    [SerializeField]
    [Tooltip("Force applied to thrown dice in the spawn point's forward direction")]
    private float _throwForce = 5f;

    [SerializeField]
    [Tooltip("Random torque applied to dice for natural tumbling")]
    private float _randomTorque = 10f;

    private List<GameplayDie> _activeDice = new List<GameplayDie>();
    private Coroutine _throwCoroutine;
    private IReadOnlyList<InventoryDie> _handDice;

    /// <summary>
    /// Returns true if dice are currently being thrown.
    /// </summary>
    public bool IsThrowing => _throwCoroutine != null;

    /// <summary>
    /// Returns a read-only list of all active dice spawned by this thrower.
    /// </summary>
    public IReadOnlyList<GameplayDie> ActiveDice => _activeDice;

    /// <summary>
    /// Number of dice to throw. Can be modified before calling Throw().
    /// </summary>
    public int NumberOfDice
    {
        get => _numberOfDice;
        set => _numberOfDice = Mathf.Max(1, value);
    }

    /// <summary>
    /// The dice manager this thrower reports to.
    /// </summary>
    public DiceManager DiceManager
    {
        get => _diceManager;
        set => _diceManager = value;
    }

    private void OnValidate()
    {
        if (_spawnPoint == null)
        {
            _spawnPoint = transform;
        }
    }

    /// <summary>
    /// Sets the hand dice data to apply face values from InventoryDie to spawned GameplayDie.
    /// Call before Throw() to ensure enhanced dice have correct values.
    /// </summary>
    public void SetHandDice(IReadOnlyList<InventoryDie> handDice)
    {
        _handDice = handDice;
    }

    /// <summary>
    /// Throws the configured number of dice from the spawn point.
    /// </summary>
    public void Throw()
    {
        if (_diePrefab == null)
        {
            Debug.LogError("DiceThrower: No die prefab assigned.");
            return;
        }

        if (_spawnPoint == null)
        {
            Debug.LogError("DiceThrower: No spawn point assigned.");
            return;
        }

        if (_throwCoroutine != null)
        {
            StopCoroutine(_throwCoroutine);
        }

        _throwCoroutine = StartCoroutine(ThrowDiceCoroutine());
    }

    /// <summary>
    /// Throws a specific number of dice, overriding the configured amount.
    /// </summary>
    /// <param name="count">Number of dice to throw.</param>
    public void Throw(int count)
    {
        _numberOfDice = Mathf.Max(1, count);
        Throw();
    }

    /// <summary>
    /// Clears all active dice spawned by this thrower and notifies the manager.
    /// </summary>
    public void ClearDice()
    {
        if (_throwCoroutine != null)
        {
            StopCoroutine(_throwCoroutine);
            _throwCoroutine = null;
        }

        foreach (var die in _activeDice)
        {
            if (die != null)
            {
                Destroy(die.gameObject);
            }
        }

        _activeDice.Clear();
        _handDice = null;

        // Notify manager that dice have been cleared
        if (_diceManager != null)
        {
            _diceManager.ClearTrackedDice();
        }
    }

    private IEnumerator ThrowDiceCoroutine()
    {
        for (int i = 0; i < _numberOfDice; i++)
        {
            SpawnAndThrowDie(i);

            if (i < _numberOfDice - 1 && _spawnInterval > 0f)
            {
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        _throwCoroutine = null;
    }

    private void SpawnAndThrowDie(int index)
    {
        // Instantiate with random rotation for variety
        Quaternion randomRotation = Random.rotation;
        GameplayDie die = Instantiate(_diePrefab, _spawnPoint.position, randomRotation);

        // Apply face values and types from InventoryDie if available
        if (_handDice != null && index < _handDice.Count)
        {
            die.SetAllSideValues(_handDice[index].GetFaceValues());
            die.SetAllSideTypes(_handDice[index].GetFaceTypes());
        }

        _activeDice.Add(die);

        // Register with manager
        if (_diceManager != null)
        {
            _diceManager.RegisterDie(die);
        }

        // Apply throw force
        Rigidbody rb = die.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force in spawn point's forward direction
            rb.AddForce(_spawnPoint.forward * _throwForce, ForceMode.Impulse);

            // Apply random torque for natural tumbling
            Vector3 randomTorqueVector = new Vector3(
                Random.Range(-_randomTorque, _randomTorque),
                Random.Range(-_randomTorque, _randomTorque),
                Random.Range(-_randomTorque, _randomTorque)
            );
            rb.AddTorque(randomTorqueVector, ForceMode.Impulse);
        }
    }
}
