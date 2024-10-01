using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinishDetector : MonoBehaviour
{
    public int _finishCount = 0;
    public float jumpForce = 500f;
    [SerializeField] private Rigidbody playerRigidbody;

    public GameObject _panelVictory;
    private CarController _carController;
    public TextMeshProUGUI _scoreTxt;
    public TextMeshProUGUI _numberTourTxt;
    public int _numberTour = 0;




    private void Start()
    {
        _panelVictory.SetActive(false);

        _carController = FindObjectOfType<CarController>();

        _numberTour = 0;
        UpdateTourText();
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("FinishLine"))
        {

            _finishCount++;

            if (_finishCount >= 2)
            {
                _numberTour++;
                OnPlayerWin(col.gameObject);

            }
        }
    }



    private void OnPlayerWin(GameObject player)
    {

        if (playerRigidbody != null)
        {
            // Applique une force vers le haut pour faire sauter le joueur
            playerRigidbody.AddForce(Vector3.up * jumpForce + Vector3.forward * jumpForce);
            PanelVictory();
            UpdateTourText();

        }
        else
        {
            Debug.LogWarning("Pas de Rigidbody trouvé sur le joueur !");
        }
    }

    private void PanelVictory()
    {
        _panelVictory.SetActive(true);
        _carController.GetScore();
        _scoreTxt.text = Mathf.RoundToInt(_carController._score).ToString();


    }

    void UpdateTourText()
    {
        _numberTourTxt.text = Mathf.RoundToInt(_numberTour).ToString() + "/1 tour";

    }


}
