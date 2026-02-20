using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region VARIABLES
    [Header("Reference")] 
    [SerializeField] private RectTransform maxHealthBar;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider damageSlider;
    [SerializeField] private GameObject dashSpell;
    [SerializeField] private GameObject bulletSpell;
    [SerializeField] private GameObject laserSpell;
    [SerializeField] private TextMeshProUGUI laserAmmoText;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Internes")] 
    public float health, maxHealth, height = 25, damageDuration;
    private float healthRatio;
    public enum SpellType 
    {
        Dash,
        Bullet,
        Laser
    }
    #endregion

    #region UNITY FUNCTIONS
    void Start()
    {
        if(dashSpell != null)
        {
            dashSpell.SetActive(false);
        }

        if(bulletSpell != null)
        {
            bulletSpell.SetActive(false);
        }

        if(laserSpell != null)
        {
            laserSpell.SetActive(false);
        }

        if(laserAmmoText != null)
        {
            laserAmmoText.gameObject.SetActive(false);
        }

        if(!gameObject.CompareTag("PlayerUI"))
        {
            return;
        }
        if(SaveSystem.saveInstance != null)
        {
            SaveSystem.saveInstance.playerUIController = this;

            tutorialManager = SaveSystem.saveInstance.GetComponent<TutorialManager>();

            SaveSystem.saveInstance.UpdateUISpells(SaveSystem.saveInstance.loadSceneData);

            SetHealth(SaveSystem.saveInstance.loadSceneData.playerHealth);

            tutorialManager.ShowAdvice(0);
        }
    }
    #endregion

    #region HEALTH FUNCTIONS
    public void SetHealth(float setMaxHealth)
    {
        if(780 < setMaxHealth)
        {
            maxHealth = 780;
            health = maxHealth;
        }
        else
        {
            maxHealth = setMaxHealth;
            health = maxHealth;
        }

        maxHealthBar.sizeDelta = new Vector2(maxHealth, height);
        healthSlider.value = 1;
        damageSlider.value = 1;
    }

    public void HealthDamage(float damageReceive)
    {
        health -= damageReceive;
        if(health <= 0)
        {
            health = 0;
        }
        StartCoroutine(UpdateHealthBar());
    }

    public IEnumerator UpdateHealthBar()
    {
        healthRatio = health / maxHealth;

        healthSlider.value = healthRatio;

        yield return new WaitForSeconds(damageDuration);

        damageSlider.value = healthRatio;
        damageSlider.value = healthRatio;
    }

    public void HealthHeal(float healReceive)
    {
        health += healReceive;
        if(health >= maxHealth)
        {
            health = maxHealth;
        }

        healthRatio = health / maxHealth;
        healthSlider.value = healthRatio;
    }
    #endregion
    
    #region SPELLS
    public void EnableSpell(SpellType spellName)
    {
        switch(spellName)
        {
            case SpellType.Bullet :
                bulletSpell.SetActive(true);
                tutorialManager.ShowAdvice(2);
                break;

            case SpellType.Dash :
                dashSpell.SetActive(true);
                tutorialManager.ShowAdvice(1);
                break;

            case SpellType.Laser :
                laserSpell.SetActive(true);
                laserAmmoText.gameObject.SetActive(true);
                tutorialManager.ShowAdvice(3);
                break;
        }
    }

    public void UpdateSpell(bool spellState, SpellType spellUpdateName)
    {
        GameObject updateSpell = null;

        switch(spellUpdateName)
        {
            case SpellType.Bullet :
                updateSpell  = bulletSpell;
                break;

            case SpellType.Dash :
                updateSpell  = dashSpell;
                break;

            case SpellType.Laser :
                updateSpell  = laserSpell;
                break;
        }

        if(updateSpell == null)
        {
            Debug.LogWarning($"Spell UI introuvable : {spellUpdateName}");
            return;
        }

        Image spellImage = updateSpell.GetComponent<Image>();
        
        if(spellState)
        {
            spellImage.color = Color.white;
        }
        else
        {
            spellImage.color = Color.gray;
        }
    }

    public void UpdateLaserAmmo(int currentAmmo)
    {
        laserAmmoText.text = currentAmmo.ToString();
    }
    #endregion

    #region UI
    public void SetUIVisible(bool visible)
    {
        if(healthSlider != null)
            healthSlider.gameObject.SetActive(visible);

        if(damageSlider != null)
            damageSlider.gameObject.SetActive(visible);

        if(dashSpell != null)
            dashSpell.SetActive(visible);

        if(bulletSpell != null)
            bulletSpell.SetActive(visible);

        if(laserSpell != null)
            laserSpell.SetActive(visible);

        if(laserAmmoText != null)
            laserAmmoText.gameObject.SetActive(visible);
    }
    #endregion
}