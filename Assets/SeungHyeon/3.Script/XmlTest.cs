using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class XmlTest : MonoBehaviour
{
    private static XmlTest _instance = null;
    public static XmlTest instance => _instance;

    string filename = "Character.xml";

    public GameObject DialogueBox;
    public Text CharacterName;
    // Start is called before the first frame update
    public Text dialogueText;
    public int dialogueindex = 0;
    public Dictionary<int, DialogueData> dialogues;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else if (_instance != this)
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        string xmlFilePath = Application.dataPath + "/StreamingAssets/" + filename;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlFilePath);

        dialogues = new Dictionary<int, DialogueData>();

        XmlNodeList dialogueNodes = xmlDoc.SelectNodes("/dialogues/dialogue");

        foreach (XmlNode dialogueNode in dialogueNodes)
        {
            int dialogueID = int.Parse(dialogueNode.Attributes["id"].Value);
            string character = dialogueNode.SelectSingleNode("character").InnerText;

            // 대화 텍스트 노드 가져오기
            XmlNodeList textNodes = dialogueNode.SelectNodes("text");

            // 대화 텍스트 저장
            List<string> texts = new List<string>();
            foreach (XmlNode textNode in textNodes)
            {
                texts.Add(textNode.InnerText);
            }

            // 대화 데이터 저장
            dialogues.Add(dialogueID, new DialogueData(character,texts));
        }
    }
    public void DisplayDialogue(int dialogueID)
    {
        if(!XmlTest.instance.DialogueBox.activeSelf)
        {
            XmlTest.instance.DialogueBox.SetActive(true);
        }
        if (dialogues.TryGetValue(dialogueID, out DialogueData dialogueData))
        {
            CharacterName.text = $"{dialogueData.Character}";
            dialogueText.text = dialogueData.Texts[dialogueindex];
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
    

