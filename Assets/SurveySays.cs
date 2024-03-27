using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using KModkit;
using UnityEngine.UI;

using System.Text.RegularExpressions;
using Rnd = UnityEngine.Random;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
public class SurveySays : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule Modulo;
    public KMSelectable[] Buttons;
    public MeshRenderer[] ButtonMesh;
    public Material[] Materials;
    public TextAsset JSON;
    public TextMesh[] Text;
    public Light[] Light;

    static private int _moduleIdCounter = 1, dumb = 0;
    private int moduleId, idiot;

    private string ColorOrder = "0123"; // Defines order of colors on the module.
    private string RndOrder = "0123";
    private static readonly string ColorVals = "ROVJ";
    private static readonly string[] ColorNames = {"rose", "orange", "violet", "jade"};
    private static readonly string[] difficulty = {"3", "23", "2", "12", "1", "01", "0"};
    private static readonly string[] difficultyNames = {"trivial", "very easy", "easy", "medium", "hard", "very hard", "extreme"};
    private static readonly string[] repodiffs = {"trivial", "veryeasy", "easy", "medium", "hard", "veryhard", "extreme"};
    private static readonly string Alphabet = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static readonly string Chars = "0123456789EEEEEEEEEEEEAAAAAAAAAIIIIIIIIIOOOOOOOONNNNNNRRRRRRTTTTTTLLLLSSSSUUUUDDDDGGGBBCCMMPPFFHHVVWWYYKJXQZ";
    private static readonly string Vowels = "AEIOU";
    private string Input, Last, Typing, Letters;
    private bool Submission, Go = true, Solved;
    private EdgeworkModule Selected;
    private RepoModule SelectedRepo;
    private List<string> NameArray = new List<string> { };
    private List<EdgeworkModule> EdgeworkModule;
    private List<RepoModule> RepoModule;
    private WWW Fetch;

    void Awake()
    {
        moduleId = _moduleIdCounter++;
        idiot = dumb++;
        for(int i =0; i < 4; i++)
        {
            KMSelectable btn = Buttons[i];
            btn.OnInteract += delegate
            {
                if(Go && !Solved)
                Handlepress(btn);
                StartCoroutine(Sink(btn));
                return false;
            };
            btn.OnInteractEnded += delegate
            {
                StartCoroutine(Rise(btn));
            };
        }
    }

    // Use this for initialization
    void Start()
    {
        if(idiot == 0)
        {
            StartCoroutine(FetchModules());
        }
        ColorOrder = ColorOrder.ToCharArray().Shuffle().Join("");
        for(int i = 0; i < 4; i++)
        {
            ButtonMesh[i].material = Materials[int.Parse(ColorOrder[i].ToString())];
            Text[i].text = "";
            Text[i].color = Materials[int.Parse(ColorOrder[i].ToString()) + 4].color;
            Light[i].color = Materials[int.Parse(ColorOrder[i].ToString()) + 4].color;
            Light[i].enabled = false;
            float scalar = transform.lossyScale.x;
            Light[i].range *= scalar;
        }
        EdgeworkModule = JsonConvert.DeserializeObject<List<EdgeworkModule>>(JSON.ToString());
        Debug.LogFormat("[Survey Says #{0}]: I currently support {1} modules! If you want to see more, why not help add some?", moduleId, EdgeworkModule.Count());
        Generate();
    }
    void Generate()
    {
        Submission = false;
        for(int i = 0; i < 4; i++)
        {
            Text[i].text = "";
        }
        foreach (EdgeworkModule mod in EdgeworkModule)
        {
            NameArray.Add(mod.Name);
        }
        Selected = EdgeworkModule[Rnd.Range(0, EdgeworkModule.Count())];
        NameArray.Remove(Selected.Name);
        Debug.LogFormat("[Survey Says #{0}]: Today we will be surveying the module {1}.", moduleId, Selected.FullName);
        Last = "";
        Input = "";
        Submission = false;
    }

    void Handlepress(KMSelectable btn)
    {
        int i = int.Parse(ColorOrder[Array.IndexOf(Buttons, btn)].ToString());
        int a = Array.IndexOf(Buttons, btn);
        Buttons[a].AddInteractionPunch();
        if (!Submission)
            Input += i;
        else
        {
            Input += Letters[a];
            StartCoroutine(GenerateLetter(false));
        }
        if(Input == Last)
        {
            Audio.PlaySoundAtTransform("Question", Modulo.transform);
            Input = "";
            Submission = true;
            StartCoroutine(GenerateLetter(true));
        }
        if (!Submission && Input.Length == 2)
        {
            if(SelectedRepo == null)
            {
                SelectedRepo = RepoModules.Where(x => x.Name == Selected.FullName).First();
            }
            Last = Input;
            Debug.LogFormat("[Survey Says #{0}]: You pressed {1} {2}. SURVEY SAYS...", moduleId,
                ColorNames[int.Parse(Input[0].ToString())],
                ColorNames[int.Parse(Input[1].ToString())]
                );
            switch (Input)
            {
                case "00":
                    if(Selected.EdgeworkBinary[0] == '1')
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module uses batteries!", moduleId);
                        StartCoroutine(Flash("3"));
                    }
                    else
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module doesn't use batteries!", moduleId);
                        StartCoroutine(Flash("0"));
                    }
                    break;
                case "01":
                    if (Selected.EdgeworkBinary[1] == '1')
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module uses ports!", moduleId);
                        StartCoroutine(Flash("3"));
                    }
                    else
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module doesn't use ports!", moduleId);
                        StartCoroutine(Flash("0"));
                    }
                    break;
                case "02":
                    if (Selected.EdgeworkBinary[2] == '1')
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module uses indicators!", moduleId);
                        StartCoroutine(Flash("3"));
                    }
                    else
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module doesn't use indicators!", moduleId);
                        StartCoroutine(Flash("0"));
                    }
                    break;
                case "03":
                    if (Selected.EdgeworkBinary[3] == '1')
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module uses the serial number!", moduleId);
                        StartCoroutine(Flash("3"));
                    }
                    else
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module doesn't use the serial number!", moduleId);
                        StartCoroutine(Flash("0"));
                    }
                    break;
                case "10":
                    Debug.LogFormat("[Survey Says #{0}]: The module's first character is {1}!", moduleId, Selected.Name[0]);
                    int j = Array.IndexOf(Alphabet.ToCharArray(), Selected.Name[0]);
                    if (j == -1) StartCoroutine(Flash(NumberToFlash(int.Parse(Selected.Name[0].ToString()))));
                    else StartCoroutine(Flash(NumberToFlash(j)));
                    break;
                case "11":
                    int k = Selected.Name.Where(x => Vowels.Contains(x) ).Count();
                    Debug.LogFormat("[Survey Says #{0}]: The module has {1} vowels!", moduleId, k);
                    StartCoroutine(Flash(NumberToFlash(k)));
                    break;
                case "12":
                    int l = Selected.Name.Length;
                    Debug.LogFormat("[Survey Says #{0}]: The module has {1} characters!", moduleId, l);
                    StartCoroutine(Flash(NumberToFlash(l)));
                    break;
                case "13":
                    Debug.LogFormat("[Survey Says #{0}]: The module's last character is {1}!", moduleId, Selected.Name.Last());
                    int m = Array.IndexOf(Alphabet.ToCharArray(), Selected.Name.Last());
                    if (m == -1) StartCoroutine(Flash(NumberToFlash(int.Parse(Selected.Name.Last().ToString()))));
                    else StartCoroutine(Flash(NumberToFlash(m)));
                    break;
                case "20":
                    Debug.LogFormat("[Survey Says #{0}]: The module's defuser difficulty is {1}!", moduleId, difficultyNames[Array.IndexOf(repodiffs, SelectedRepo.DefuserDifficulty.ToLower())]);
                    StartCoroutine(Flash(difficulty[Array.IndexOf(repodiffs, SelectedRepo.DefuserDifficulty.ToLower())]));
                    break;
                case "21":
                    Debug.LogFormat("[Survey Says #{0}]: The module's expert difficulty is {1}!", moduleId, difficultyNames[Array.IndexOf(repodiffs, SelectedRepo.ExpertDifficulty.ToLower())]);
                    StartCoroutine(Flash(difficulty[Array.IndexOf(repodiffs, SelectedRepo.ExpertDifficulty.ToLower())]));
                    break;
                case "22":
                    Debug.LogFormat("[Survey Says #{0}]: The module's TP score is {1}!", moduleId, SelectedRepo.TwitchPlays.Score);
                    StartCoroutine(Flash(NumberToFlash((int)SelectedRepo.TwitchPlays.Score)));
                    break;
                case "23":
                    Debug.LogFormat("[Survey Says #{0}]: The module's Time Mode score is {1}!", moduleId, SelectedRepo.TimeMode.Score);
                    StartCoroutine(Flash(NumberToFlash((int)SelectedRepo.TimeMode.Score)));
                    break;
                case "30":
                    if (Selected.EdgeworkBinary[4] == '1')
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module uses battery holders!", moduleId);
                        StartCoroutine(Flash("1"));
                    }
                    else
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module doesn't use battery holders!", moduleId);
                        StartCoroutine(Flash("2"));
                    }
                    break;
                case "31":
                    if (Selected.EdgeworkBinary[5] == '1')
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module uses port plates!", moduleId);
                        StartCoroutine(Flash("1"));
                    }
                    else
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module doesn't use port plates!", moduleId);
                        StartCoroutine(Flash("2"));
                    }
                    break;
                case "32":
                    if (Selected.EdgeworkBinary[6] == '1')
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module uses modded widgets!", moduleId);
                        StartCoroutine(Flash("1"));
                    }
                    else
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module doesn't use modded widgets!", moduleId);
                        StartCoroutine(Flash("2"));
                    }
                    break;
                case "33":
                    if (Selected.EdgeworkBinary[7] == '1')
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module uses the timer (or strikes)!", moduleId);
                        StartCoroutine(Flash("1"));
                    }
                    else
                    {
                        Debug.LogFormat("[Survey Says #{0}]: The module doesn't use the timer (or strikes)!", moduleId);
                        StartCoroutine(Flash("2"));
                    }
                    break;
                default:
                    StartCoroutine(Flash("012|123"));
                    break;
            }
            Input = "";
        }
    }

    internal static List<RepoModule> LocalJson = new List<RepoModule>();
    internal static List<RepoModule> RepoModules = new List<RepoModule>();
    IEnumerator FetchModules()
    {
        yield return null;
        Debug.LogFormat("Attempting to load existing file!");
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "raw.json")))
        {
            Debug.LogFormat("Oops, local file doesn't exist. Creating...");
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "raw.json"), JSON.ToString());
        }
        string Json = File.ReadAllText(Path.Combine(Application.persistentDataPath, "raw.json"));
        LocalJson = JsonConvert.DeserializeObject<List<RepoModule>>(Json);
        Fetch = new WWW("https://ktane.timwi.de/json/raw");
        Debug.LogFormat("Attempting to reach the Repository for updates...");
        yield return Fetch;
            if (Fetch.error == null)
            {
                Debug.Log("JSON successfully fetched!");
                string Fetched = Fetch.text.Substring(16, Fetch.text.Length - 17);
                if(Application.isEditor)
                Debug.Log(Fetched);
                RepoModules = JsonConvert.DeserializeObject<List<RepoModule>>(Fetched);
                if (LocalJson != RepoModules)
                {
                    Debug.Log("Local JSON appears out of date... Updating local json!");
                    File.WriteAllText(Path.Combine(Application.persistentDataPath, "raw.json"), Fetched);
                    LocalJson = RepoModules;
                }
            }
            else
            {
                Debug.LogFormat("An error has occurred while fetching modules from the repository: {0}. Using current version.", Fetch.error);
            }
        dumb = 0;
    }
    IEnumerator Sink(KMSelectable b)
    {
        int a = Array.IndexOf(Buttons, b);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, ButtonMesh[a].transform);
        yield return null;
        float i = 0;
        while (i < 1)
        {
            yield return null;
            ButtonMesh[a].transform.localPosition = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(0, 0, -1.2f), i);
            i += Time.deltaTime * 10;
        }
    }
    IEnumerator Rise(KMSelectable b)
    {
        int a = Array.IndexOf(Buttons, b);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, ButtonMesh[a].transform);
        yield return null;
        float i = 0;
        while (i < 1)
        {
            yield return null;
            ButtonMesh[a].transform.localPosition = Vector3.Lerp(new Vector3(0, 0, -1.2f), new Vector3(0, 0, 0), i);
            i += Time.deltaTime * 10;
        }
        ButtonMesh[a].transform.localPosition = new Vector3(0, 0, 0);
    }

    string NumberToFlash(int Num)
    {
        string a = "";
        string b = Convert.ToString(Num, 2);
        b = b.PadLeft(8, '0');
        if(b.Substring(0,4) == "0000")
        {
            b = b.Substring(4, 4);
            for(int i = 0; i < 4; i++)
            {
                if(b[i] == '1')
                {
                    a += i.ToString();
                }
            }
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                if (b[i] == '1')
                {
                    a += (i%4).ToString();
                }
                if (i == 3)
                {
                    a += '|';
                }
            }
        }
        return a;
    }

    IEnumerator Flash(string flashes)
    {
        Go = false;
        yield return null;
        Audio.PlaySoundAtTransform("Question", Modulo.transform);
        RndOrder = RndOrder.ToCharArray().Shuffle().Join("");
        for(int i = 0; i < 4; i++)
        {
            int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(i+'0'));
            ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString())+4];
            Light[i].enabled = true;
            yield return new WaitForSeconds(0.06f);
        }
        yield return new WaitForSecondsRealtime(.5f);

        for(int i = 0; i < 4; i++)
        {
            Light[i].enabled = false;
            ButtonMesh[i].material = Materials[int.Parse(ColorOrder[i].ToString())];
        }
        yield return new WaitForSecondsRealtime(.25f);
        if (flashes.Contains('|'))
        {
            string flashA = flashes.Substring(0, Array.IndexOf(flashes.ToCharArray(), '|'));
            string flashB = flashes.Substring(Array.IndexOf(flashes.ToCharArray(), '|')+1, (flashes.Length - Array.IndexOf(flashes.ToCharArray(), '|')) - 1);
            Audio.PlaySoundAtTransform("Answer", Modulo.transform);
            for (int i = 0; i < flashA.Length; i++)
            {
                int j = Array.IndexOf(ColorOrder.ToCharArray(), flashA[i]);
                ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString()) + 4];
                Light[j].enabled = true;
            }
            yield return new WaitForSecondsRealtime(.7f);
            for (int i = 0; i < 4; i++)
            {
                Light[i].enabled = false;
                ButtonMesh[i].material = Materials[int.Parse(ColorOrder[i].ToString())];
            }
            yield return new WaitForSecondsRealtime(.3f);
            Audio.PlaySoundAtTransform("Answer", Modulo.transform);
            for (int i = 0; i < flashB.Length; i++)
            {
                int j = Array.IndexOf(ColorOrder.ToCharArray(), flashB[i]);
                ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString()) + 4];
                Light[j].enabled = true;
            }
            yield return new WaitForSecondsRealtime(.7f);
            for (int i = 0; i < 4; i++)
            {
                Light[i].enabled = false;
                ButtonMesh[i].material = Materials[int.Parse(ColorOrder[i].ToString())];
            }
        }
        else
        {
            Audio.PlaySoundAtTransform("Answer", Modulo.transform);
            for (int i = 0; i < flashes.Length; i++)
            {
                int j = Array.IndexOf(ColorOrder.ToCharArray(), flashes[i]);
                ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString())+4];
                Light[j].enabled = true;
            }
            yield return new WaitForSecondsRealtime(.7f);
            for (int i = 0; i < 4; i++)
            {
                Light[i].enabled = false;
                ButtonMesh[i].material = Materials[int.Parse(ColorOrder[i].ToString())];
            }
        }

        Go = true;
    }

    IEnumerator GenerateLetter(bool first)
    {
        yield return null;
        Letters = "";
        if (first)
        {
            Letters += Selected.Name[0];
            while(Letters.Length < 4)
            { 
                char a = Chars[Rnd.Range(0, Chars.Length)];
                if (!Letters.Contains(a))
                {
                    Letters += a;
                    continue;
                }
            }
        }
        else
        {
            if (Input != Selected.Name)
            {
                NameArray = NameArray.Where(x => x.StartsWith(Input)).ToList();
                NameArray = NameArray.Where(x => x.Length >= Input.Length).ToList();
                NameArray = NameArray.Shuffle();
                if (Selected.Name.StartsWith(Input))
                    Letters += Selected.Name[Input.Length];
                while (Letters.Length < 4)
                {
                    foreach (string str in NameArray)
                    {
                        if (str.Length > Input.Length && !Letters.Contains(str[Input.Length]))
                        {
                            Letters += str[Input.Length];
                            if (Letters.Length == 4)
                            {
                                break;
                            }
                        }
                    }
                    while (Letters.Length < 4)
                    {
                        char a = Chars[Rnd.Range(0, Chars.Length)];
                        if (!Letters.Contains(a))
                        {
                            Letters += a;
                            continue;
                        }
                    }
                }
            }
            if (Input == Selected.Name || (Selected.Name.StartsWith(Input) && Input.Length >= 5 && NameArray.Count == 0))
            {
                Audio.PlaySoundAtTransform("Solve", Modulo.transform);
                Debug.LogFormat("[Survey Says #{0}]: {1} unambiguously spells out {2}! Module solved.", moduleId, Input, Selected.FullName);
                Solved = true;
                for (int k = 0; k < 4; k++)
                {
                    int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(k + '0'));
                    ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString()) + 4];
                    Text[j].text = "";
                    Light[j].enabled = true;
                    yield return new WaitForSeconds(0.02f);
                }
                yield return new WaitForSecondsRealtime(.1f);
                for (int l = 0; l < 4; l++)
                {
                    int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(l + '0'));
                    ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString())];
                    Text[j].text = "!";
                    Light[j].enabled = false;
                    yield return new WaitForSeconds(0.02f);
                }
                Modulo.HandlePass();
                yield break;
            }
            else if (!Selected.Name.StartsWith(Input) && (NameArray.Count < 1 || (NameArray[0].StartsWith(Input) && Input.Length >= 5)))
            {
                Debug.LogFormat("[Survey Says #{0}]: {1} doesn't spell {2}... Strike! Regenerating.", moduleId, Input, Selected.FullName);
                for (int k = 0; k < 4; k++)
                {
     
                    int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(k + '0'));
                    ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString()) + 4];
                    Text[j].text = "";
                    Light[j].enabled = true;
                    yield return new WaitForSeconds(0.02f);
                }
                yield return new WaitForSecondsRealtime(.1f);
                Audio.PlaySoundAtTransform("Wrong", Modulo.transform);
                for (int l = 0; l < 4; l++)
                {
                    
                    int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(l + '0'));
                    ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString())];
                    Text[j].text = "X";
                    Light[j].enabled = false;
                    yield return new WaitForSeconds(0.02f);
                }
                Modulo.HandleStrike();
                yield return new WaitForSeconds(0.5f);
                for (int k = 0; k < 4; k++)
                {
                    int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(k + '0'));
                    ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString()) + 4];
                    Text[j].text = "";
                    Light[j].enabled = false;
                    yield return new WaitForSeconds(0.02f);
                }
                yield return new WaitForSecondsRealtime(.1f);
                for (int l = 0; l < 4; l++)
                {
                    Light[l].enabled = false;
                    int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(l + '0'));
                    ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString())];
                    yield return new WaitForSeconds(0.02f);
                }
                Generate();
                yield break;
            }
        }
        if(!first)Audio.PlaySoundAtTransform("Answer", Modulo.transform);
        Letters = Letters.ToCharArray().Shuffle().Join("");
        Go = false;
        yield return null;
        RndOrder = RndOrder.ToCharArray().Shuffle().Join("");
        for (int k = 0; k < 4; k++)
        {
            int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(k + '0'));
            ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString()) + 4];
            Text[j].text = "";
            Light[j].enabled = true;
            yield return new WaitForSeconds(0.02f);
        }
        yield return new WaitForSecondsRealtime(.1f);
        if(first) yield return new WaitForSecondsRealtime(.4f);
        for (int l = 0; l < 4; l++)
        {
            int j = Array.IndexOf(RndOrder.ToCharArray(), Convert.ToChar(l + '0'));
            ButtonMesh[j].material = Materials[int.Parse(ColorOrder[j].ToString())];
            Text[j].text = Letters[j].ToString();
            Light[j].enabled = false;
            yield return new WaitForSeconds(0.02f);
        }
        Go = true;
        Debug.LogFormat("[Survey Says #{0}]: For letter {1}, I am showing the letters {2}.", moduleId, Input.Length+1, Letters);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
public class EdgeworkModule
{
    public string Name;
    public string EdgeworkBinary;
    public string FullName;
}
public class RepoModule
{
    public string Name;
    public string DefuserDifficulty;
    public string ExpertDifficulty;
    public Twitch TwitchPlays;
    public TimeM TimeMode;
}
public class Twitch
{
    public double Score { get; set; }
}
public class TimeM
{
    public double Score { get; set; }
}
