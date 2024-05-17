using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPSpecialText : MonoBehaviour
{
    /*
     * Important:
     * This script doesn't work with nested tags and requires openening <tags> to 
     * be matched with closing </tags>
     * 
     * When implementing tags you may need to wait one frame before doing stuff
     * in each coroutine
     */

    private TextMeshProUGUI m_TextMeshPro;
    private static string[] commands = {
        "jitter",
        "jittery",

        "shake",
        "shaky",

        "wave",
        "wavy",

        "var",
        "string",

        "type",
    };
    private static int[] commandHashes;
    // private List<CommandArg> commandArgs = new List<CommandArg>();

    // subscribe to TMP event manager for when text updates
    //bool hasTextChanged; 

    // idk how to best do settings
    public float jitterMultiplier = 1;
    public float shakeMultiplier = 1;
    public float waveAmplitude = 1;
    public float wavePeriod = 1;
    public float typeLoopSpeed = 0.05f;
    [Tooltip("Will delete color tags; intended for background fonts")]
    public bool ignoreColor;

    private List<Coroutine> effectCoroutines;


    private struct CommandArg
    {
        public string command;
        public int hash;
        public int start;
        public int end;

        public CommandArg(string c, int h, int s, int e)
        {
            command = c;
            hash = h;
            start = s;
            end = e;
        }
    }

    private void Awake()
    {
        // init commandHashes
        if (commandHashes == null)
        {
            commandHashes = new int[commands.Length];
            for (int i = 0; i < commands.Length; i++)
            {
                commandHashes[i] = commands[i].GetHashCode();
                //Debug.Log("Hash for '" + commands[i] + "': " + commandHashes[i]);
            }
        }


        // this is in awake now because `m_TextMeshPro.text = text;` updates the mesh
        m_TextMeshPro = GetComponent<TextMeshProUGUI>();
        if (m_TextMeshPro == null)
        {
            Debug.LogWarning("TextMeshPro Script not on object!");
        }

        // StartCoroutine(ParseText(m_TextMeshPro.text));
        effectCoroutines = new List<Coroutine>();
    }

    // moved to start so SGrid can initialize, for SaveSystem
    private void Start() 
    {
        ParseText();
    }


    /// <summary>
    /// Calls to parse special tags in the text! This takes a frame to update
    /// </summary>
    public void ParseText()
    {
        StartCoroutine(IParseText(ParseTextFirstFrame(m_TextMeshPro.text)));
    }
    
    public string ReplaceAndStripRichText(string text)
    {
        var (str, _) = ParseTextFirstFrame(text);
        return str;
    }

    private (string cleaned, List<CommandArg> cmdArgs) ParseTextFirstFrame(string text)
    {
        string cleaned = text;
        List<CommandArg> commandArgs = new();
        
        int offset = 0;

        // Parse text replacement first
        int i = 0;
        while (i < text.Length)
        {

            if (text[i] == '<')
            {
                // find if closing > exists
                int first_right_chev = text.IndexOf('>', i);
                if (first_right_chev == -1)
                {
                    i++;
                    continue;
                }

                // find command between the < >
                string command = text.Substring(i + 1, first_right_chev - i - 1);
                int commandHash = command.GetHashCode();
                
                if (!IsTextReplacement(commandHash))
                {
                    i++;
                    continue;
                } 

                // find if closing </ > exists
                int closing_tag = text.IndexOf("</" + command + ">", i);
                if (closing_tag == -1)
                {
                    i++;
                    continue;
                }

                string originalText = text.Substring(i + command.Length + 2, closing_tag - (i + command.Length + 2));
                originalText = ParseCommandReplaceText(commandHash, originalText);

                // remove tags
                text = text.Substring(0, i) + 
                       originalText + 
                       text.Substring(closing_tag + command.Length + 3);

                cleaned = text;
                // m_TextMeshPro.text = text;
            }

            i++;
        }

        // Other commands -- cannot next commands
        i = 0;
        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                // find if closing > exists
                int first_right_chev = text.IndexOf('>', i);
                if (first_right_chev == -1)
                {
                    i++;
                    continue;
                }

                // find command between the < >
                string command = text.Substring(i + 1, first_right_chev - i - 1);
                int commandHash = command.GetHashCode();
                
                if (!IsValid(commandHash))
                {
                    // lazy color solution
                    if (command[0] == '#')
                    {
                        if (ignoreColor)
                        {
                            // remove tags
                            int closing_color = text.IndexOf("</color>", i);
                            if (closing_color == -1)
                            {
                                Debug.LogError("Couldn't find closing </color> tag!");
                                continue;
                            }

                            string ot = text.Substring(i + command.Length + 2, closing_color - (i + command.Length + 2));

                            text = text.Substring(0, i) + 
                                   ot +
                                   text.Substring(closing_color + "</color>".Length + 0);
                            // m_TextMeshPro.text = text;

                            cleaned = text;
                            continue;
                        }
                        offset += 9;
                        i += 8;
                    } else if (command.Equals("/color"))
                    {
                        offset += 8;
                        i += 7;
                    } else
                    {
                        i++;
                    }
                    continue;
                } 

                // find if closing </ > exists
                int closing_tag = text.IndexOf("</" + command + ">", i);
                if (closing_tag == -1)
                {
                    i++;
                    continue;
                }

                string originalText = text.Substring(i + command.Length + 2, closing_tag - (i + command.Length + 2));
                // originalText = ParseCommandReplaceText(commandHash, originalText);

                // remove tags
                text = text.Substring(0, i) + 
                       originalText + 
                       text.Substring(closing_tag + command.Length + 3);
                // m_TextMeshPro.text = text;
                cleaned = text;

                //Debug.Log(command + " " + i + " - " + (closing_tag - command.Length - 2));
                //Debug.Log($"Hash {commandHash}");
                //ParseCommand(commandHash, i - offset, closing_tag - command.Length - 3 - offset);
                commandArgs.Add(new CommandArg(command, commandHash, i - offset, i + originalText.Length - offset - 1));// closing_tag - command.Length - 3 - offset));
            }

            i++;
        }

        return (cleaned, commandArgs);
    }

    private IEnumerator IParseText((string cleaned, List<CommandArg> cmdArgs) parseResult)
    {
        m_TextMeshPro.text = parseResult.cleaned;

        yield return null;

        foreach (CommandArg c in parseResult.cmdArgs)
        {
            ParseCommand(c.hash, c.start, c.end);
        }
    }

    private bool IsValid(int hash)
    {
        foreach (int h in commandHashes)
        {
            if (h == hash)
                return true;
        }
        return false;
    }

    private bool IsValid(string command)
    {
        foreach (string s in commands)
        {
            if (command.Equals(s))
                return true;
        }
        return false;
    }

    private bool IsTextReplacement(int hash)
    {
        // var || string
        return hash == 696029845 || hash == -983953243;
    }

    private bool IsTextReplacement(string command)
    {
        return command == "var" || command == "string";
    }

    
    /// <summary>
    /// This parses all the commands found in the string, with a massive switch statement because that's 
    /// how TextMeshPro does it, so there's probably a good reason for it.
    /// 
    /// `End` is inclusive, so for example:
    /// <tag>My text</tag>
    /// would be ParseCommand(hash, 0, 7);
    /// </summary>
    /// <param name="commandHash">The hash of the string of the command</param>
    /// <param name="start">The start index in the original string, tags removed</param>
    /// <param name="end">The end index in the original string, tags removed</param>
    /// <returns>True if a coroutine was started, else false</returns>
    private bool ParseCommand(int commandHash, int start, int end)
    {
        //Debug.Log($"Hash of Type: {"type".GetHashCode()}");
        switch (commandHash)
        {
            // jitter
            case -1623808880:
            // jittery
            case 270799001:
                effectCoroutines.Add(StartCoroutine(TMPJitter(start, end, jitterMultiplier * 2, jitterMultiplier * 2)));
                break;
            // shake
            case 371760912:
            // shaky
            case 371760908:
                effectCoroutines.Add(StartCoroutine(TMPJitter(start, end, 0, shakeMultiplier * 10)));
                break;
            // wave
            case -1966748055:
            // wavy
            case -1066070667:
                effectCoroutines.Add(StartCoroutine(Wavy(start, end, waveAmplitude * 5, wavePeriod * .5f)));
                break;
            //type
            case 1421151742:
                effectCoroutines.Add(StartCoroutine(LoopType(start, end, typeLoopSpeed)));
                break;
            default:
                return false;
        }
        return true;
    }

    private string ParseCommandReplaceText(int commandHash, string originalText)
    {
        switch (commandHash)
        {
            // var
            case 696029845:
            // string
            case -983953243:
                return GetString(originalText);
        }

        return originalText;
    }

    public void StopEffects()
    {
        foreach(var effect in effectCoroutines)
        {
            StopCoroutine(effect);
        }

        effectCoroutines.Clear();
        // commandArgs.Clear();
    }


    #region jitter-shaky

    // shamelessly stolen from TMPro's example, VertexJitter.cs

    private struct VertexAnim
    {
        public float angleRange;
        public float angle;
        public float speed;
    }

    IEnumerator TMPJitter(int start, int end, float AngleMultiplier, float CurveScale)
    {

        bool hasTextChanged;

        // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
        // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
        //m_TextMeshPro.ForceMeshUpdate();
        yield return null;

        TMP_TextInfo textInfo = m_TextMeshPro.textInfo;

        Matrix4x4 matrix;

        int loopCount = 0;
        hasTextChanged = true;

        // Create an Array which contains pre-computed Angle Ranges and Speeds for a bunch of characters.
        VertexAnim[] vertexAnim = new VertexAnim[1024];
        for (int i = 0; i < 1024; i++)
        {
            vertexAnim[i].angleRange = Random.Range(10f, 25f);
            vertexAnim[i].speed = Random.Range(1f, 3f);
        }

        // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        while (true)
        {
            // Get new copy of vertex data if the text has changed.
            if (hasTextChanged)
            {
                // Update the copy of the vertex data for the text object.
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

                hasTextChanged = false;
            }

            int characterCount = textInfo.characterCount;

            // If No Characters then just yield and wait for some text to be added
            if (characterCount < end - start)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }


            for (int i = start; i <= end; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!charInfo.isVisible)
                    continue;

                // Retrieve the pre-computed animation data for the given character.
                VertexAnim vertAnim = vertexAnim[i];

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Get the cached vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;

                // Determine the center point of each character at the baseline.
                //Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
                // Determine the center point of each character.
                Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

                // Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
                // This is needed so the matrix TRS is applied at the origin for each character.
                Vector3 offset = charMidBasline;

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;

                vertAnim.angle = Mathf.SmoothStep(-vertAnim.angleRange, vertAnim.angleRange, Mathf.PingPong(loopCount / 25f * vertAnim.speed, 1f));
                Vector3 jitterOffset = new Vector3(Random.Range(-.25f, .25f), Random.Range(-.25f, .25f), 0);

                matrix = Matrix4x4.TRS(jitterOffset * CurveScale, Quaternion.Euler(0, 0, Random.Range(-5f, 5f) * AngleMultiplier), Vector3.one);

                destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
                destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
                destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
                destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

                destinationVertices[vertexIndex + 0] += offset;
                destinationVertices[vertexIndex + 1] += offset;
                destinationVertices[vertexIndex + 2] += offset;
                destinationVertices[vertexIndex + 3] += offset;

                vertexAnim[i] = vertAnim;
            }

            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                m_TextMeshPro.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            loopCount += 1;

            yield return new WaitForSeconds(0.1f);
        }
    }
    #endregion

    #region wavy


    IEnumerator Wavy(int start, int end, float amplitude, float periodMultiplier)
    {

        bool hasTextChanged;

        // We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
        // Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
        //m_TextMeshPro.ForceMeshUpdate();
        yield return null;

        TMP_TextInfo textInfo = m_TextMeshPro.textInfo;

        int loopCount = 0;
        hasTextChanged = true;

        // Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
        TMP_MeshInfo[] cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

        while (true)
        {
            // Get new copy of vertex data if the text has changed.
            if (hasTextChanged)
            {
                // Update the copy of the vertex data for the text object.
                cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

                hasTextChanged = false;
            }

            int characterCount = textInfo.characterCount;

            // If No Characters then just yield and wait for some text to be added
            if (characterCount < end - start)
            {
                yield return new WaitForSeconds(0.25f);
                continue;
            }


            for (int i = start; i <= end; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!charInfo.isVisible)
                    continue;

                // Get the index of the material used by the current character.
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                // Get the index of the first vertex used by this text element.
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // Get the cached vertices of the mesh used by this text element (character or sprite).
                Vector3[] sourceVertices = cachedMeshInfo[materialIndex].vertices;


                // i dont know if this is the best way to do it or not lol
                Vector3 offset = Vector3.up * amplitude * Mathf.Sin(periodMultiplier * (loopCount - i));

                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] + offset;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] + offset;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] + offset;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] + offset;
            }

            // Push changes into meshes
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                m_TextMeshPro.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }

            loopCount += 1;

            // changed from 0.1f
            yield return new WaitForSeconds(0.05f);
        }
    }
    #endregion

    #region Type
    IEnumerator LoopType(int start, int end, float speed)
    {
        TMP_TextInfo textInfo = m_TextMeshPro.textInfo;
        int charIndex;
        bool allCharsVisible = false;
        while (!allCharsVisible)
        {
            allCharsVisible = true;
            for (int i = start; i <= end; i++)
            {
                if (textInfo.characterInfo[i].color.a == 0)
                {
                    allCharsVisible = false;
                }
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        while (true)
        {
            for (int i = start; i <= end; i++)
            {
                //Debug.Log($"Start: {start} End: {end}");
                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                textInfo.characterInfo[i].color.a = 0;

                TMPTextTyper.SetCharacterColor(m_TextMeshPro, textInfo, textInfo.characterInfo[i].color, i);
            }

            yield return new WaitForSeconds(speed);

            charIndex = start;
            while (charIndex <= end)
            {

                // // If No Characters then just yield and wait for some text to be added
                // if (textInfo.characterCount == 0)
                // {
                //     yield return new WaitForSeconds(textSpeed);
                //     continue;
                // }

                // Skip characters that are not visible and thus have no geometry to manipulate.
                if (!textInfo.characterInfo[charIndex].isVisible)
                {
                    charIndex++;
                    continue;
                }
                else
                {
                    textInfo.characterInfo[charIndex].color.a = 255;

                    TMPTextTyper.SetCharacterColor(m_TextMeshPro, textInfo, textInfo.characterInfo[charIndex].color, charIndex);

                }

                char currChar = textInfo.characterInfo[charIndex].character;
                charIndex++;
                yield return new WaitForSeconds(speed);
            }
        }
    }
    #endregion

    #region var-string

    private string GetString(string stringName)
    {
        return SaveSystem.Current.GetString(stringName);
    }

    #endregion

}