using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerController player1;
    [SerializeField] private PlayerController player2;
    [SerializeField] private Slider _HPBar1;
    [SerializeField] private Slider _HPBar2;
    [SerializeField] private Slider _TPBar1;
    [SerializeField] private Slider _TPBar2;
    [SerializeField] private TMP_Text _HPnum1;
    [SerializeField] private TMP_Text _HPnum2;
    [SerializeField] private TMP_Text _TPnum1;
    [SerializeField] private TMP_Text _TPnum2;
    [SerializeField] private GameSettings settings;

    private void Start()
    {
        if (player1 == null)
        {
            player1 = GetComponentInParent<PlayerController>(); // auto-find if possible
        }

        if (_HPBar1 != null && player1 != null && _TPBar1 != null)
        {
            _HPBar1.maxValue = player1.HP; // set initial range for both HP and TP
            _TPBar1.maxValue = 20;
            _HPBar1.value = player1.HP;
            _TPBar1.value = player1.TP;
        }

        if (player2 == null) //just rince and repeat, there probably is a better way but I'm lazy :)
        {
            player2 = GetComponentInParent<PlayerController>();
        }

        if (_HPBar2 != null && player2 != null && _TPBar2 != null)
        {
            _HPBar2.maxValue = player2.HP;
            _TPBar2.maxValue = 20;
            _HPBar2.value = player2.HP;
            _TPBar2.value = player2.TP;
        }
    }

    private void Update()
    {
        if (player1 != null && _HPBar1 != null && _TPBar1 != null && _HPnum1 != null && _TPnum1 != null)
        {
            _HPBar1.value = player1.HP; // always read live value from player
            _TPBar1.value = player1.TP;
            _HPnum1.text = player1.HP + "/" + settings.startingHP;
            _TPnum1.text = player1.TP + "/20";
        }
        if (player2 != null && _HPBar2 != null && _TPBar2 != null && _HPnum2 != null && _TPnum2 != null)
        {
            _HPBar2.value = player2.HP;
            _TPBar2.value = player2.TP;
            _HPnum2.text = player2.HP + "/" + settings.startingHP;
            _TPnum2.text = player2.TP + "/20";
        }
    }
}
