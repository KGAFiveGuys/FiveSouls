using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class XmlTest : MonoBehaviour
{
    public Text CharacterName;
    // Start is called before the first frame update
    public Text dialogueText;

    private Dictionary<int, DialogueData> dialogues;

    private void Start()
    {
        string xmlFilePath = "C:/Users/KGA/Desktop/gitHub/FiveSouls/Assets/SeungHyeon/Resources/Character.xml";
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlFilePath);

        dialogues = new Dictionary<int, DialogueData>();

        XmlNodeList dialogueNodes = xmlDoc.SelectNodes("/dialogues/dialogue");

        foreach (XmlNode dialogueNode in dialogueNodes)
        {
            int dialogueID = int.Parse(dialogueNode.Attributes["id"].Value);
            string character = dialogueNode.SelectSingleNode("character").InnerText;

            // ��ȭ �ؽ�Ʈ ��� ��������
            XmlNodeList textNodes = dialogueNode.SelectNodes("text");

            // ��ȭ �ؽ�Ʈ ����
            List<string> texts = new List<string>();
            foreach (XmlNode textNode in textNodes)
            {
                texts.Add(textNode.InnerText);
            }

            // ��ȭ ������ ����
            dialogues.Add(dialogueID, new DialogueData(character,texts));
        }
        // ���÷� ù ��° ��ȭ ǥ��
        DisplayDialogue(1);
    }
    void DisplayDialogue(int dialogueID)
    {
        if (dialogues.TryGetValue(dialogueID, out DialogueData dialogueData))
        {
            CharacterName.text = $"{dialogueData.Character}";
            dialogueText.text = dialogueData.Texts[0];
        }
        else
        {
            dialogueText.text = "Dialogue not found";
        }
    }
}
public class DialogueData
{
    public string Character { get; private set;}
    public List<string> Texts { get; private set; }

    public DialogueData(string character, List<string> texts)
    {
        Character = character;
        Texts = texts;
    }
}
    

