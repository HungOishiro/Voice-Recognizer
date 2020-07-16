using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class Manager : MonoBehaviour
{
    //API
    public string[] keywords = new string[] { };
    public ConfidenceLevel confidenceLevel = ConfidenceLevel.Low;
    KeywordRecognizer recognizer;
    public string word = "";

    //Source
    public AudioSource[] audioSource;
    public List<AudioClip> audioClip;

    //Pos
    public GameObject[] objPos;
    private int index;
    private float timeMove = 0.5f;
    private float timeRot = 0.1f;
    private float timePerFrame = 0.05f;

    //Check
    private bool isPlaying = false;
    private bool checkInfo = false;
    private bool canSpeak = true;
    private int count = 0;
    private int point = 0;
    private float time = 80;
    private float timeStart;
    private bool start = false;
    private bool checkRand = false;
    private bool closeUILoss = false;
    int[] checkQuestion = new int[8];

    //UI
    public GameObject UIStart, UIInformation, UIQuestion, UIDoor, UIStatus, UIEnd;
    public Text textPoint, textTime, textWorL, textResult;
    public Text[] answerText = new Text[3];

    //Answer Yes and No
    private string[] keywordsY = new string[] { "Monitor", "Office Chair", "Laptop", "Desk lamp",
                                                "Coffee mug", "Shelf", "Printer", "Stair case" };
    private string[] keywordsN = new string[] {"Picture", "Mouse", "Sofa", "Desk", "Book", "Keyboard",
                                                "Light", "Stone", "Spoon", "Knife", "Batman", "Ball", "Paper", "Box", "Wood", "Joker"};

    private void Recognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        if (!canSpeak)
        {
            word = "";
        }
        else
        {
            word = args.text;
        }
        
    }

    private void OnApplicationQuit()
    {
        if (recognizer != null && recognizer.IsRunning)
        {
            recognizer.OnPhraseRecognized -= Recognizer_OnPhraseRecognized;
            recognizer.Stop();
        }
    }

    void Start()
    {
        if (keywords != null)
        {
            Debug.Log("RUN");
            recognizer = new KeywordRecognizer(keywords, confidenceLevel);
            recognizer.OnPhraseRecognized += Recognizer_OnPhraseRecognized;
            recognizer.Start();
        }

        textPoint.text = "0";
    }

    void FixedUpdate()
    {
        AllowToSpeak();

        Menu();

        WinOrLoss();

        Handle();

        TimeReducing();

        GoOrNot();

        textPoint.text = point.ToString();
    }

    private void AllowToSpeak()
    {
        if (audioSource[1].isPlaying && !audioSource[0].isPlaying)
        {
            Debug.Log("Can Speak");
            canSpeak = true;
        }

        else if (audioSource[1].isPlaying && audioSource[0].isPlaying)
        {
            canSpeak = false;
        }

        else
        {
            Debug.Log("Can Speak");
            canSpeak = true;
        }
    }

    private void Menu()
    {
        if (!isPlaying)
        {
            if (word == "Start")
            {
                if (!checkInfo)
                {
                    isPlaying = true;
                    Debug.Log("Start");
                    UIStart.SetActive(false);
                    UIDoor.SetActive(true);
                    StartCoroutine(Wait(0, 0));
                    StartCoroutine(Wait(1, 12));
                    word = "";
                }
            }
            if (word == "Information")
            {
                checkInfo = true;
                UIStart.SetActive(false);
                UIInformation.SetActive(true);
            }
            if (word == "Main menu")
            {
                checkInfo = false;
                UIInformation.SetActive(false);
                UIStart.SetActive(true);
            }
        }

        if (word == "Quit")
        {
            Debug.Log("Quit");
            StartCoroutine(Wait(8, 0));
            StartCoroutine(Quit());
            word = "";
        }
    }

    private void WinOrLoss()
    {
        if (!closeUILoss)
            GameLoss();
        else
        {
            UIQuestion.SetActive(false);
            if (word == "Restart")
            {
                Restart();
                word = "";
            }
        }
    }

    private void Handle()
    {
        if (start)
        {
            for(int i = 0; i < 16; i++)
            {
                if (i < 8)
                {
                    if (word == keywordsY[index])
                    {
                        RandomQuestion();
                        
                        if (!closeUILoss)
                        {
                            StartCoroutine(MoveCam());
                            StartCoroutine(Wait(13, 0));
                            StartCoroutine(Wait(5, 0.5f));
                            StartCoroutine(Wait(3, 3));
                            UIQuestion.SetActive(false);
                            StartCoroutine(SetActiveUIQuestion(2, UIQuestion, true));
                        }

                        point += 10;
                        word = "";
                    }
                }
                if (word == keywordsN[i])
                {
                    StartCoroutine(Wait(14, 0));
                    StartCoroutine(Wait(4, 0.5f));
                    point -= 7;
                    word = "";
                }
            }
        }
    }

    private void TimeReducing()
    {
        if (time >= 0 && start)
        {
            time -= 0.02f;
            textTime.text = Mathf.Round(time) + "s";
        }
        else
        {
            textTime.text = "0s";
        }
    }

    //Check Robot or Not
    private void GoOrNot()
    {
        
        if (word == "Absolutely")
        {
            if (!start)
            {
                StartCoroutine(Wait(2, 0));
                UIDoor.SetActive(false);
                UIQuestion.SetActive(true);
                UIStatus.SetActive(true);
                RandomQuestion();
                StartCoroutine(MoveCam());
                StartCoroutine(Wait(3, 4));
                timeStart = Time.time;
                start = true;
            }
            word = "";
        }

        if (word == "Yes" || word == "Definitely")
        {
            if (!start)
            {
                count++;
                if (count < 3)
                {
                    StartCoroutine(Wait(12, 0));
                }
                else if (count == 3)
                {
                    Debug.Log("You are a Robot");
                    StartCoroutine(Wait(10, 0));
                    StartCoroutine(Quit());
                }
                word = "";
            }
        }
    }

    private void RandomQuestion()
    {
        int i = Random.Range(0, 8);
        index = i;
        checkRand = false;
        
        if (checkQuestion[i] == 1)
        {
            for (int j = 0; j < 8; j++)
            {
                if (checkQuestion[j] != 1)
                {
                    checkRand = true;
                }                
            }
            if (checkRand)
            {
                RandomQuestion();
            }
            else
            {
                GameWin();
            }
        }
        else
        {
            RandomAnswer(i);
            checkQuestion[i] = 1;
        }
    }

    private void RandomAnswer(int i)
    {
        keywords = new string[] { keywordsY[i] };
        int r = Random.Range(0, 2);
        answerText[r].text = keywordsY[i];
        if (r == 0)
        {
            answerText[1].text = keywordsN[Random.Range(0, 16)];
            answerText[2].text = keywordsN[Random.Range(0, 16)];
        }
        if (r == 1)
        {
            answerText[0].text = keywordsN[Random.Range(0, 16)];
            answerText[2].text = keywordsN[Random.Range(0, 16)];
        }
        if (r == 2)
        {
            answerText[0].text = keywordsN[Random.Range(0, 16)];
            answerText[1].text = keywordsN[Random.Range(0, 16)];
        }
    }

    private void GameLoss()
    {
        if (Time.time == 80 + Mathf.Round(timeStart))
        {
            if (!closeUILoss)
            {
                UIQuestion.SetActive(false);
                UIStatus.SetActive(false);
                UIEnd.SetActive(true);
                textWorL.text = "LOSER";
                textResult.text = "YOUR POINT IS: " + point;
                StartCoroutine(Wait(6, 0));
                start = false;
            }
            
        }
        else if (Time.time >= 80 + Mathf.Round(timeStart))
        {
            if (word == "Restart")
            {
                Restart();
                word = "";
            }
        }
    }

    private void GameWin()
    {
        closeUILoss = true;
        UIStatus.SetActive(false);        
        UIEnd.SetActive(true);
        textWorL.text = "WINNER";
        textResult.text = "YOUR POINT IS: " + point;
        StartCoroutine(Wait(7, 0));
        start = false;
        
    }

    private void Restart()
    {
        timeStart = 0;
        StartCoroutine(Wait(9, 0));
        index = 8;
        StartCoroutine(MoveCam());
        StartCoroutine(Wait(0, 3));
        StartCoroutine(Wait(1, 15));
        UIEnd.SetActive(false);
        UIDoor.SetActive(true);
        point = 0;
        time = 80;
        closeUILoss = false;
        for (int j = 0; j < 8; j++)
        {
            if (checkQuestion[j] == 1)
            {
                checkQuestion[j] = 0;
            }
        }
        GoOrNot();
    }

    IEnumerator Quit()
    {
        yield return new WaitForSeconds(3);
        Application.Quit();
    }

    IEnumerator MoveCam()
    {
        float timeCam = 0;
        while (this.transform.position != objPos[index].transform.position)
        {
            yield return new WaitForSeconds(timePerFrame);
            this.transform.position = Vector3.MoveTowards(this.transform.position, objPos[index].transform.position, timeMove);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, objPos[index].transform.rotation, timeCam);
            timeCam += timeRot;
        }
    }

    IEnumerator Wait(int i, float j)
    {

        yield return new WaitForSeconds(j);
        audioSource[0].PlayOneShot(audioClip[i], 1);
        
    }

    IEnumerator SetActiveUIQuestion(int i, GameObject ui, bool j)
    {
        yield return new WaitForSeconds(i);
        ui.SetActive(j);
    }
}
