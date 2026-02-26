using UnityEngine;
using R3;

public class PlayerHpUI : MonoBehaviour
{
    public GameObject[] hpUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player.HP
            .Subscribe(hp =>
            {
                Debug.Log(hp);
                for (int i = 0; i < hpUI.Length; i++)
                {
                    hpUI[i].SetActive(i < hp);
                }

            }).AddTo( this );
    }
}
