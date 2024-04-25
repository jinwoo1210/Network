using TMPro;
using UnityEngine;

public class Chat : MonoBehaviour
{
    [SerializeField] Client client;

    [SerializeField] TMP_InputField inputField;
    [SerializeField] RectTransform content;

    [SerializeField] TMP_Text chatTextPrefab;

    private void Awake()
    {
        inputField.onSubmit.AddListener(SendChat);
    }

    public void AddMessage(string message)
    {
        TMP_Text newMessage = Instantiate(chatTextPrefab, content);
        newMessage.text = message;
    }

    public void SendChat(string chat)
    {
        if (client != null)     // 채팅이 null이 아닐 경우
        {
            client.SendChat(chat);
        }

        inputField.text = "";
        inputField.ActivateInputField();        // 엔터치면 포커스를 가지게, 채팅을 계속 칠수 있게 해줌
    }
}
