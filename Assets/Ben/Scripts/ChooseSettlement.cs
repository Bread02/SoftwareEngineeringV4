using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseSettlement : MonoBehaviour
{
    [SerializeField] private Material origColour, takenColour, hoverOverColour;
    public bool settlementTaken;

    [Header("Other Scripts")]
    private TerrainAssigner terrainAssigner;
    private TurnManager turnManager;
    private WarningText warningText;
    private GameObject makeTradeScript;

    // 0 if unclaimed
    public int playerClaimedBy;

    [Header("Adjacent Objects")]
    // needed for robbers
    public List<GameObject> adjacentTiles = new List<GameObject>();

    // a settlemust MUST be joined by an adjacent road (unless it is 1st turn)
    // a settlement MUST NEVER be built adjacent to another settlement.
    public List<GameObject> adjacentRoads = new List<GameObject>();
    public List<GameObject> adjacentSettlements = new List<GameObject>();

    [Header("Player Color")]
    public Material red;
    public Material blue;
    public Material orange;
    public Material white;

    [Header("Contains Port Features")]
    public bool isImprovedHarbor = false;
    public bool isBrickHarbor = false;
    public bool isLumberHarbor = false;
    public bool isWoolHarbor = false;
    public bool isOreHarbor = false;
    public bool isGrainHarbor = false;

    public bool isCity = false;
    public Mesh cityMesh;
    public bool inCityBuildMode;

    public bool adjacentRoadCheck;


    [Header("Audio")]
    public AudioManager audioManager;

    public void Awake()
    {
        FindOtherScripts();
        turnManager.allSettlementBuildSites.Add(this.gameObject);

    }

    private void FindOtherScripts()
    {
        turnManager = GameObject.Find("TurnManager").GetComponent<TurnManager>();
        terrainAssigner = GameObject.Find("TileHolder").GetComponent<TerrainAssigner>();
        makeTradeScript = GameObject.FindGameObjectWithTag("MakeTrade");
        warningText = GameObject.Find("PlayerWarningBox").GetComponent<WarningText>();
    }

    private void Start()
    {
        settlementTaken = false;
        //game object must be active first so it can be found. Set active as this is the trade GUI and should only show when trade starts.
        AdjacentTiles();
        FindAdjacentSettlements();
        FindAdjacentRoads();

    }

    public void ChangeToCity()
    {
        this.gameObject.GetComponent<MeshFilter>().mesh = cityMesh;
        isCity = true;

        // interact with player manager to add claimed city
        PlayerManager playerManager = turnManager.ReturnCurrentPlayer();
        playerManager.playerOwnedCities.Add(this.gameObject);
        inCityBuildMode = false;
        makeTradeScript.GetComponent<MakeTrade>().SetCityBought(false);
    }

    // this is for BUILDING RULE ADHERENCE
    public void FindAdjacentSettlements()
    {
        foreach(GameObject settleSite in turnManager.allSettlementBuildSites)
        {
            if(this.gameObject != settleSite)
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

    // this is for BUILDING RULE ADHERENCE
    public void FindAdjacentRoads()
    {
        foreach (GameObject road in turnManager.allRoadBuildSites)
        {
            if (this.gameObject != road)
            {
                if (Vector3.Distance(this.gameObject.transform.position, road.transform.position) < 0.7f)
                {
                    adjacentRoads.Add(road);
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
        if (makeTradeScript.GetComponent<MakeTrade>().GetSettlementBought())
        {
            this.gameObject.GetComponent<Renderer>().enabled = true;
        }
        else if (!makeTradeScript.GetComponent<MakeTrade>().GetSettlementBought() && !settlementTaken)
        {
            this.gameObject.GetComponent<Renderer>().enabled = false;
        }

        // change all to city mesh
        if(Input.GetKeyDown(KeyCode.W))
        {
            ChangeToCity();
        }

        
        if (makeTradeScript.GetComponent<MakeTrade>().GetCityBought() && settlementTaken)
        {
            inCityBuildMode = true;
        }

        /*
        else if (!makeTradeScript.GetComponent<MakeTrade>().GetCityBought() && settlementTaken)
        {
            this.gameObject.GetComponent<Renderer>().enabled = false;
        }
        */
    }

    // find tiles adjacent to this settlement
    // find all objects within 1 unit of this settlement
    // all objects within 1 unit are 'adjacent'.
    // this is for ROBBER FUNCTIONALITY
    public void AdjacentTiles()
    {
        foreach(GameObject tile in terrainAssigner.tileList)
        {
            if(Vector3.Distance(this.gameObject.transform.position, tile.gameObject.transform.position) < 1)
            {
                if(!adjacentTiles.Contains(tile))
                {
                    adjacentTiles.Add(tile);
                }
            }
        }
    }


    // check the settlement is not already taken, and check there are no adjacent built settlements.
    // IF NOT IN OPENING SEQUENCE THE SETTLEMENT MUST BE BUILT ON A ROAD.
    private void OnMouseDown()
    {
        adjacentRoadCheck = false; // false until proven.

        Debug.Log("Choose settlement mouse down");
        // In the case of cities, do this alternate option
        if(settlementTaken && inCityBuildMode)
        {
            Debug.Log("Choose settlement mouse down 2");
            if (playerClaimedBy == turnManager.playerToPlay)
            {
                Debug.Log("Choose settlement mouse dow 3");
                // build city
                ChangeToCity();
                audioManager.PlaySound("build");
                PlayerManager playerManager = turnManager.ReturnCurrentPlayer();
                string playerColor = playerManager.GetPlayerColor();
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
                return;
            }
            else
            {
                warningText.WarningTextBox("You do not own this settlement");
                return;
            }
        }


        // if not in setup phase, check an adjacent player owned road is present
        if (turnManager.isSetUpPhase == false)
        {
            foreach(GameObject adjacentRoad in adjacentRoads)
            {
                if(adjacentRoad.GetComponent<ChooseBorder>().playerClaimedBy == turnManager.playerToPlay)
                {
                    adjacentRoadCheck = true;
                }
            }

            if (!adjacentRoadCheck)
            {
                StartCoroutine(warningText.WarningTextBox("No adjacent road to build settlement"));
                return;
            }
        }

        //Can only interact with this point when the user has bought a settlement!
        if (this.gameObject.GetComponent<Renderer>().enabled)
        {
            foreach (GameObject adjacentSettlement in adjacentSettlements)
            {
                if (adjacentSettlement.GetComponent<ChooseSettlement>().settlementTaken)
                {
                    Debug.Log("CANNOT BUILD SETTLEMENT. ADJACENT SETTLEMENT ALREADY CLAIMED");
                    StartCoroutine(warningText.WarningTextBox("Cannot build settlement. Adjacent settlement already claimed."));
                    return;
                }
            }

            if (!settlementTaken)
            {
                Debug.Log("settlement taken");
                Debug.Log("value of isSetUpPhase in turnManager is: " + turnManager.isSetUpPhase);

                //   this.gameObject.GetComponent<Renderer>().material = takenColour;
                settlementTaken = true;
                playerClaimedBy = turnManager.playerToPlay;

                // add to playerManager of correct player.
                PlayerManager playerManager = turnManager.ReturnCurrentPlayer();
                playerManager.playerOwnedSettlements.Add(this.gameObject);
                string playerColor = playerManager.GetPlayerColor();


                // give this player an improved port if this is a port hex.
                if (isImprovedHarbor)
                {
                    playerManager.ownsImprovedHarbor = true;
                }
                if (isBrickHarbor)
                {
                    playerManager.ownsBrickHarbor = true;
                }
                if (isLumberHarbor)
                {
                    playerManager.ownsLumberHarbor = true;
                }
                if (isWoolHarbor)
                {
                    playerManager.ownsWoolHarbor = true;
                }
                if (isGrainHarbor)
                {
                    playerManager.ownsGrainHarbor = true;
                }
                if (isOreHarbor)
                {
                    playerManager.ownsOreHarbor = true;
                }


                //Play Audio Queue
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
                    Debug.Log("siiiu");
                    turnManager.roadAndSettlementPlacedSetUpCounter++;
                }
                makeTradeScript.GetComponent<MakeTrade>().SetSettlementBought(false);

                foreach (GameObject adjacentTile in adjacentTiles)
                {
                    adjacentTile.GetComponent<TerrainHex>().adjacentSettlements.Add(this.gameObject);
                }
            }
        }
    }

    private void OnMouseExit()
    {
        if (inCityBuildMode && settlementTaken)
        {
            string playerColor = turnManager.ReturnPlayerManagerByNumber(playerClaimedBy).GetPlayerColor();
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
        }


        if (!settlementTaken)
        {
            this.gameObject.GetComponent<Renderer>().material = origColour;
        }
    }

    private void OnMouseEnter()
    {
        if (inCityBuildMode && settlementTaken)
        {
            this.gameObject.GetComponent<Renderer>().material = hoverOverColour;
        }

        if (!settlementTaken)
        {
            this.gameObject.GetComponent<Renderer>().material = hoverOverColour;
        }
    }
}
