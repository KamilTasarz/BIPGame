using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public TextMeshProUGUI text;
    public Image img;
    public GameObject canvas;

    private int index = 0;

    public bool isTutorial = true;
    private PlayerInput pi;

    private InputAction pushAction;
    private InputAction pullAction;

    public List<string> texts; 
    private void Start()
    {
        if (Instance == null)
        {

            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
        pi = GameManager.Instance.GetComponent<PlayerInput>();
        pushAction = pi.actions["pushAction"];
        pullAction = pi.actions["pullAction"];
        text.text = texts[index];
    }

    void Update()
    {
        if (pushAction.WasPressedThisFrame() || pullAction.WasPressedThisFrame())
        {
            index++;
            if (index < texts.Count)
            {
                text.text = texts[index];
                img.rectTransform.rotation.Set(0, 0, -8 + (index % 3) * 8, 0);
            } else
            {
                canvas.gameObject.SetActive(false);
                isTutorial = false;
            }
        }
    }
}
