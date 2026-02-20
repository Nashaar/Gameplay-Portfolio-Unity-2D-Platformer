using System;
using UnityEngine;

public class BadelinePhaseController : MonoBehaviour
{  
    #region VARIABLES
    [SerializeField] private Health badelineHealth;
    
    public enum Phase
    {
        Phase1,
        Phase2,
        Phase3
    }
    public Phase CurrentPhase { get; private set; } = Phase.Phase1;

    public event Action<Phase> OnPhaseChanged;
    #endregion

    #region UNITY FUNCTIONS
    // Update is called once per frame
    void Update()
    {
        UpdatePhase();
    }
    #endregion 

    #region LOGIC
    private void UpdatePhase()
    {
        float healthPercent = badelineHealth.currentHealth / (float)badelineHealth.maxHealth;

        Phase newPhase = CurrentPhase;

        if(healthPercent > 0.66f)
        {
            newPhase = Phase.Phase1;
        }
        else if(healthPercent > 0.33f)
        {
            newPhase = Phase.Phase2;
        }
        else
        {
            newPhase = Phase.Phase3;
        }

        if(newPhase != CurrentPhase)
        {
            CurrentPhase = newPhase;
            
            OnPhaseChanged?.Invoke(CurrentPhase);
        }
    }
    #endregion
}
