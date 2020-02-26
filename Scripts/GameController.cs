using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public bool debug = false;
    public bool menu = false;
    public bool rewards = true;
    public bool playerRewards = true;
    public int yinCount = 5;
    public int yangCount = 5;
    public BallAgent[] ballAgents;
    public CylinderAgent[] players;
    public GameObject area;
    public TextMeshProUGUI yinWinsText;
    public TextMeshProUGUI yangWinsText;
    public ShowThenHideText yinVictoryMessage;
    public ShowThenHideText yangVictoryMessage;

    private int yinWins;
    private int yangWins;

    private int numAgents;
    private int numPlayers;

    private void Start()
    {
        numAgents = ballAgents.Length;
        numPlayers = players.Length;
        yinWins = 0;
        yangWins = 0;

        if (!menu)
        {
            UpdateScore(yinWinsText, "Yin: ", yinWins);
            UpdateScore(yinWinsText, "Yang: ", yangWins);
        }
    }

    private void FixedUpdate()
    {
        if (yinCount <= 0)
        {
            Win("Yang");
        }
        else if (yangCount <= 0)
        {
            Win("Yin");
        }
    }

    public void Win(string winningTeam = null)
    {
        if (debug)
        {
            Debug.Log(area.name + ": " + winningTeam + " team wins.");
        }
      
        if (winningTeam == "Yin")
        {
            yinWins++;

            UpdateScore(yinWinsText, "Yin: ", yinWins);

            yinVictoryMessage.ShowThenHide();
        }
        else if (winningTeam == "Yang")
        {
            yangWins++;

            UpdateScore(yangWinsText, "Yang: ", yangWins);

            yangVictoryMessage.ShowThenHide();
        }
        else
        {
            yinWins = 0;
            yangWins = 0;

            UpdateScore(yinWinsText, "Yin: ", yinWins);
            UpdateScore(yangWinsText, "Yang: ", yangWins);
        }

        // Rewards or penalizes players based on if they won or not
        for (int i = 0; i < numPlayers; i++)
        {
            if (players[i].team == winningTeam)
            {
                players[i].AddReward(1f);
            }
            else
            {
                players[i].AddReward(-1f);
            }

            players[i].Done();
        }

        for (int i = 0; i < numAgents; i++)
        {
            if (rewards)
            {
                // Checks if an agent is on the winning team and active
                if (ballAgents[i].team == winningTeam)
                {
                    if (ballAgents[i].GetComponent<Rigidbody>().useGravity)
                    {
                        ballAgents[i].AddReward(1f);
                    }
                }
                // Penalizes the agent for losing
                else
                {
                    ballAgents[i].AddReward(-1f);
                }
            }

            ballAgents[i].Done();
        }

        yinCount = 5;
        yangCount = 5;
    }

    private void UpdateScore(TextMeshProUGUI winsText, string teamNameColon, int winCount)
    {
        winsText.text = teamNameColon + winCount;
    }
}