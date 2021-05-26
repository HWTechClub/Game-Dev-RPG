using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalTree : MonoBehaviour
{

    string start = "X";

    LTreeRule[] rules = { new LTreeRule("X", "F+[[X]-X]-F[-FX]+X"), new LTreeRule("F", "FF") };

    float angle = 25f;

    int iterations = 5;

    public string CreateSentence () {
        string sentence = start;

        for (int i = 0; i < iterations; i++) {
            string currentIteration = sentence;

            sentence = "";

            for (int j = 0; j < currentIteration.Length; j++) {
                sentence +=  UseRule (currentIteration.Substring(j, 1));              
            }
        }

        Debug.Log(sentence);

        return sentence;
    }

    void CreateTree(string sentence) {

    }

    string UseRule(string letter) {
        foreach (LTreeRule rule in rules)
        {
            if (rule.from == letter) {
                return rule.to;
            }
        }

        return letter;
    }

    private void Start()
    {
        CreateSentence();
    }

}

public struct LTreeRule {

    public string from;
    public string to;

    public LTreeRule(string from, string to) {
        this.from = from;
        this.to = to;
    }
    
}
