using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] Image staminaBar;
    [SerializeField] Image staminaBarBack;
    [SerializeField] PlayerMovement pm;
    [SerializeField] TextMeshProUGUI[] playerStats;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (pm.CurrentStamina < pm.MaxStamina)
        {
            staminaBar.fillAmount = pm.CurrentStamina / pm.MaxStamina;
            staminaBar.gameObject.SetActive(true);
            staminaBarBack.gameObject.SetActive(true);
        }
        else
        {
            staminaBar.gameObject.SetActive(false);
            staminaBarBack.gameObject.SetActive(false);
        }



        playerStats[0].text = "Horizontal Speed: " + (new Vector2 (pm.Velocity.x, pm.Velocity.z).magnitude.ToString(".0#"));
        playerStats[1].text = "Vertical Speed: " + pm.Velocity.y.ToString(".0#");
        playerStats[2].text = "Stamina: " + (int)pm.CurrentStamina + "/" + (int)pm.MaxStamina;
        playerStats[3].text = "Move State: " + pm.Move_State.ToString();
    }
}
