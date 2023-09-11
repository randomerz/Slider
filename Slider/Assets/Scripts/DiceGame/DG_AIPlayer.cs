using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DG_AIPlayer : DG_Player
{
    private DG_AIPersonality personality;
    public void SetPersonality(DG_AIPersonality personality)
    {
        this.personality = personality;
        charIcon = personality.CharacterIconSprite;
        GetComponent<Image>().sprite = personality.CharacterSprite;
    }


    private float bsThreshold = 0.5f;

    public override IEnumerator TakeTurn()
    {
        yield return new WaitForSeconds(1);

        yield return DecideToBetOrCallBS();
        /*
        int[] bet = DG_CurrentBet.instance.GetLowestPossibleBet();
        Debug.Log("AI Player " + playerName + " bet: " + bet[0] + ", " + bet[1]);
        DG_CurrentBet.instance.SetCurrentBet(this, bet[0], bet[1]); */
    }

    private IEnumerator DecideToBetOrCallBS()
    {
        int currBetNumDice = DG_CurrentBet.instance.NumDiceBet;
        int currBetFaceNum = DG_CurrentBet.instance.FaceNumBet;

        List<int> knownFaces = GetDiceFaces();

        float currBetProbabilityOfBeingTrue = DG_CurrentBet.instance.GetProbabilityOfCurrentBet();
     

        if (currBetProbabilityOfBeingTrue < bsThreshold)
        {
            // Call BS if the current bet's probability of being true is below the threshold
            yield return DG_GameManager.instance.BSCalled(this);
        }
        else
        {
            int faceNum = diceHolder.GetMostCommonFace();
            int numDice = DG_CurrentBet.instance.GetLowestPossibleNumDiceBetOfFace(faceNum);
            
            DG_CurrentBet.instance.SetCurrentBet(this, numDice, faceNum);
        }

        /*
        float currBetProbabilityOfBeingTrue = DG_CurrentBet.instance.GetProbabilityOfCurrentBet(knownFaces);

        // Determine the potential new bet based on player knowledge
        int newBetNumDice = currBetNumDice;  // Start with the same number of dice
        int newBetFaceNum = currBetFaceNum + 1;  // Increase the face value by 1

        // Check if we need to reset the face value and increase the number of dice
        if (newBetFaceNum > 6)
        {
            newBetFaceNum = 1;  // Reset the face value to 1
            newBetNumDice++;   // Increase the number of dice
        }

        // Calculate the likelihood of a new bet being true
        float newBetLikelihood = CalculateNewBetLikelihood(newBetNumDice, newBetFaceNum, currBetNumDice, currBetFaceNum, knownFaces);

        // Determine the threshold for calling BS (e.g., 0.5 for a 50% chance)
        float bsThreshold = 0.4f;

        if (currBetProbabilityOfBeingTrue < bsThreshold)
        {
            // Call BS if the current bet's probability of being true is below the threshold
            yield return DG_GameManager.instance.BSCalled(this);
        }
        else if (newBetLikelihood > bsThreshold)
        {
            // Place a new bet if the likelihood of a new bet being true is above the threshold
            DG_CurrentBet.instance.SetCurrentBet(this, newBetNumDice, newBetFaceNum);
        }
        else
        {
            // Otherwise, call BS
            yield return DG_GameManager.instance.BSCalled(this);
        }*/
    }

    /*
    private float CalculateNewBetLikelihood(int newBetNumDice, int newBetFaceNum, int currBetNumDice, int currBetFaceNum, List<int> knownFaces)
    {
        // Calculate the likelihood of a new bet being true based on the player's knowledge


        // Count the occurrences of each face value in the player's known dice
        Dictionary<int, int> faceCounts = new Dictionary<int, int>();
        foreach (int face in knownFaces)
        {
            if (faceCounts.ContainsKey(face))
            {
                faceCounts[face]++;
            }
            else
            {
                faceCounts[face] = 1;
            }
        }

        // Calculate the number of known dice that match the potential new bet's face value
        int matchingKnownDice = faceCounts.ContainsKey(newBetFaceNum) ? faceCounts[newBetFaceNum] : 0;

        // Calculate the number of remaining dice needed to reach the potential new bet's number
        int remainingDiceNeeded = newBetNumDice - matchingKnownDice;

        // Calculate the likelihood of getting at least that many dice with the potential new bet's face value
        float newBetLikelihood = 1.0f;
        foreach (var entry in faceCounts)
        {
            int faceValue = entry.Key;
            int count = entry.Value;

            if (faceValue == newBetFaceNum)
            {
                // Multiply by the likelihood of getting more of the potential new bet's face value
                for (int i = 0; i < remainingDiceNeeded; i++)
                {
                    newBetLikelihood *= (float)count / (knownFaces.Count + i);
                }
            }
            else if (faceValue != 1)
            {
                // Multiply by the likelihood of not getting the face value of other dice
                for (int i = 0; i < remainingDiceNeeded; i++)
                {
                    newBetLikelihood *= (float)(knownFaces.Count - count) / (knownFaces.Count + i);
                }
            }
        }

        return newBetLikelihood;
    }
    */

    public override IEnumerator ChoosePlayDirection()
    {
        yield return new WaitForSeconds(1);
        DG_GameManager.instance.currentPlayDirection = GetRandomPlayDirection();
        Debug.Log("AI Player " + playerName + " chose play direction: " + DG_GameManager.instance.currentPlayDirection);
    }

    private DGPlayDirection GetRandomPlayDirection()
    {
        int rand = Random.Range(0, 2);
        if (rand == 0)
        {
            return DGPlayDirection.ClockWise;
        }
        else
        {
            return DGPlayDirection.CounterClockWise;
        }
    }

}
