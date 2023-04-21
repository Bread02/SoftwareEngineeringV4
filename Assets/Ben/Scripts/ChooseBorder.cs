using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseBorder : MonoBehaviour
{
    
    private GameObject makeTradeScript;

    [Header("Other Scripts")]
 //   private TerrainAssigner terrainAssigner;
    private TurnManager turnManager;
    private WarningText warningText;

    [Header("Player Color")]
    public Material red;
    public Material blue;
    public Material orange;
    public Material white;
    [SerializeField] private Material origColour, takenColour, hoverOverColour;

    [Header("Adjacent Objects")]
    public List<GameObject> adjacentRoads = new List<GameObject>();
    public List<GameObject> adjacentSettlements = new List<GameObject>();

    [Header("Bools")]
    private bool borderTaken;

    [Header("Longest Road Check")]
    public int borderLengthCheck;

    [Header("Audio")]
    public AudioManager audioManager;


    // works out who currently has the longest road.
    public void CheckMaxRoadLength()
    {

    }

    private void Awake()
    {
        turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        turnManager.allRoadBuildSites.Add(this.gameObject);
        makeTradeScript = GameObject.FindGameObjectWithTag("MakeTrade");
        warningText = GameObject.Find("PlayerWarningBox").GetComponent<WarningText>();
    }

    private void Start()
    {
        FindAdjacentSettlements();
        FindAdjacentRoads();
        borderTaken = false;
        //game object must be active first so it can be found. Set active as this is the trade GUI and should only show when trade starts.
    }

    private void FindAdjacentSettlements()
    {
        foreach (GameObject settleSite in turnManager.allSettlementBuildSites)
        {
            if (this.gameObject != settleSite)
            {
                if (Vector3.Distance(this.gameObject.transform.position, settleSite.transform.position) < 0.7f)
                {
                    adjacentSettlements.Add(settleSite);
                }
            }
            else
            {
                // do nothing
            }
        }
    }

    private void FindAdjacentRoads()
    {
        foreach (GameObject settleSite in turnManager.allRoadBuildSites)
        {
            if (this.gameObject != settleSite)
            {
                if (Vector3.Distance(this.gameObject.transform.position, settleSite.transform.position) < 0.7f)
                {
                    adjacentRoads.Add(settleSite);
                }
            }
            else
            {
                // do nothing
            }
        }
    }


    private void Update()
    {

        
        if (makeTradeScript.GetComponent<MakeTrade>().GetRoadBought())
        {
            this.gameObject.GetComponent<Renderer>().enabled = true;
        }
        else if (!makeTradeScript.GetComponent<MakeTrade>().GetRoadBought() && !borderTaken)
        {
            this.gameObject.GetComponent<Renderer>().enabled = false;
        }
        
    }


    // Roads can ONLY be built adjacent to other roads OR settlements.
    // Check if a player settlement is adjacent to this road OR another road is adjacent to this road (of same player), if YES, allow building.
    private void OnMouseDown()
    {
        //Can only interact with border when the user has bought a road!
        if (this.gameObject.GetComponent<Renderer>().enabled)
        {
            // false until otherwise proven
            bool adjacentSettlementPresent = false;
            bool adjacentRoadPresent = false;
            // roads must connect to a settlement or to an adjacent road.
            foreach (GameObject adjacentSettlement in adjacentSettlements)
            {
                if (adjacentSettlement.GetComponent<ChooseSettlement>().settlementTaken)
                {
                    adjacentSettlementPresent = true;
                    //           Debug.Log("Adjacent settlement present for road");

                }
            }

            // IF IN STARTING PHASE IT MUST BE CONNECTED TO A VILLAGE WITH NO ADJACENT ROADS.
            foreach (GameObject adjacentRoad in adjacentRoads)
            {
                if (adjacentRoad.GetComponent<ChooseBorder>().borderTaken)
                {
                    adjacentRoadPresent = true;
                    Debug.Log("Adjacent road present for road");

                }
            }

            if (!turnManager.allPlayersBuiltStart && adjacentRoadPresent)
            {
                StartCoroutine(warningText.WarningTextBox("EACH SETTLEMENT NEEDS A STARTING ROAD"));
                return;
            }

            if (!adjacentRoadPresent && !adjacentSettlementPresent)
            {
                Debug.Log("CANNOT BUILD ROAD. NO ADJACENT ROAD OR SETTLEMENT PRESENT");
                StartCoroutine(warningText.WarningTextBox("CANNOT BUILD ROAD. NO ADJACENT ROAD OR SETTLEMENT PRESENT."));
                return;
            }


            if (!borderTaken)
            {
                borderTaken = true;
                makeTradeScript.GetComponent<MakeTrade>().SetRoadBought(false);

                PlayerManager playerManager = turnManager.ReturnCurrentPlayer();
                playerManager.playerOwnedRoads.Add(this.gameObject);
                string playerColor = playerManager.GetPlayerColor();
                //   Debug.Log("PLayer color: " + playerColor);

                //Play sound queue
                audioManager.PlaySound("build");

                // get color of player to turn settlement into
                switch (playerColor)
                {
                    case "red":
                        this.gameObject.GetComponent<Renderer>().material = red;
                        break;
                    case "blue":
                        this.gameObject.GetComponent<Renderer>().material = blue;
                        break;
                    case "white":
                        this.gameObject.GetComponent<Renderer>().material = white;
                        break;
                    case "orange":
                        this.gameObject.GetComponent<Renderer>().material = orange;
                        break;
                    default:
                        Debug.LogError("Color ISSUE. Unacceptable string for color");
                        this.gameObject.GetComponent<Renderer>().material = takenColour;
                        break;

                }

                if (turnManager.isSetUpPhase)
                {
                    turnManager.roadAndSettlementPlacedSetUpCounter++;
                }
            }
        }
    }

    private void OnMouseExit()
    {
        if (!borderTaken)
        {
            this.gameObject.GetComponent<Renderer>().material = origColour;
        }
    }

    private void OnMouseEnter()
    {
        if (!borderTaken)
        {
            this.gameObject.GetComponent<Renderer>().material = hoverOverColour;
        }
    }
}
