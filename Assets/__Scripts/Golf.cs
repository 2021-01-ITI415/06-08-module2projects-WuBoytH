using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Golf : MonoBehaviour {
	static public Golf S;
	static public int round = 0;

	[Header("Set in Inspector")]
	public TextAsset deckXML;
	public TextAsset layoutXML;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Vector3 layoutCenter;
	public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
	public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
	public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
	public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
	public float reloadDelay = 2f;
	public Text gameOverText, roundResultText, highScoreText;

	[Header("Set Dynamically")]
	public DeckGolf deck;
	public LayoutGolf layout;
	public List<CardGolf> drawPile;
	public Transform layoutAnchor;
	public CardGolf target;
	public List<CardGolf> table;
	public List<CardGolf> discardPile;
	public FloatingScoreGolf fsRun;

	void Awake()
	{
		S = this;
		SetUpUITexts();
	}

	void SetUpUITexts() {
		GameObject go = GameObject.Find("HighScore");
		if (go != null) {
			highScoreText = go.GetComponent<Text>();
		}
		int highScore = ScoreManagerGolf.HIGH_SCORE;
		string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
		go.GetComponent<Text>().text = hScore;
		go = GameObject.Find("GameOver");
		if (go != null) {
			gameOverText = go.GetComponent<Text>();
		}

		go = GameObject.Find("RoundResult");
		if (go != null) {
			roundResultText = go.GetComponent<Text>();
		}
		ShowResultsUI(false);
	}

	void ShowResultsUI(bool show) {
		gameOverText.gameObject.SetActive(show);
		roundResultText.gameObject.SetActive(show);
	}

	void Start() {
		ScoreboardGolf.S.score = ScoreManagerGolf.SCORE;
		deck = GetComponent<DeckGolf>();
		deck.InitDeck(deckXML.text);
		DeckGolf.Shuffle(ref deck.cards);
		layout = GetComponent<LayoutGolf>();
		layout.ReadLayoutGolf(layoutXML.text);
		drawPile = ConvertListCardsToListCardGolfs(deck.cards);
		LayoutGame();
	}

	List<CardGolf> ConvertListCardsToListCardGolfs(List<CardGolf> lCD) {
		List<CardGolf> lCP = new List<CardGolf>();
		CardGolf tCP;
		foreach (Card tCD in lCD)
		{
			tCP = tCD as CardGolf;
			lCP.Add(tCP);
		}
		return (lCP);
	}

	CardGolf Draw() {
		CardGolf cd = drawPile[0];
		drawPile.RemoveAt(0);
		return (cd);
	}

	void LayoutGame()
	{
		if (layoutAnchor == null) {
			GameObject tGO = new GameObject("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}
		CardGolf cp;
		foreach (SlotDefGolf tSD in layout.SlotDefGolfs) {
			cp = Draw();
			cp.faceUp = tSD.faceUp;
			cp.canClick = tSD.canClick;
			cp.transform.parent = layoutAnchor;
			cp.transform.localPosition = new Vector3(
				layout.multiplier.x * tSD.x,
				layout.multiplier.y * tSD.y,
				-tSD.layerID
			);
			cp.layoutID = tSD.id;
			cp.slotDefGolf = tSD;
			cp.state = eCardStateGolf.tableau;
			cp.SetSortingLayerName(tSD.layerName);

			table.Add(cp);
		}
		foreach (CardGolf tCP in table) {
			foreach( int hid in tCP.slotDefGolf.hiddenBy ) {
				cp = FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
			}	
		}
		MoveToTarget(Draw());
		UpdateDrawPile();
		FloatingScoreHandler(eScoreEventGolf.draw);
	}

	CardGolf FindCardByLayoutID (int layoutID) { 
		foreach (CardGolf tCP in table) {
			if (tCP.layoutID == layoutID) {
				return (tCP);
			}
		}
		return (null);
	}

	void SetTableauFaces() { 
		foreach( CardGolf cd in table ) {
			bool canClick = true;
			foreach ( CardGolf cover in cd.hiddenBy ) {
				if (cover.state == eCardStateGolf.tableau) {
					canClick = false;
				}
			}
			cd.canClick = canClick;
		}
	}

	void MoveToDiscard(CardGolf cd) {
		cd.state = eCardStateGolf.discard;
		discardPile.Add(cd);
		cd.transform.parent = layoutAnchor;
		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID + 0.5f);
		cd.faceUp = true;
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(-100 + discardPile.Count);			
	}

	void MoveToTarget(CardGolf cd) {
		if (target != null) MoveToDiscard(target);
		target = cd;
		cd.state = eCardStateGolf.target;
		cd.transform.parent = layoutAnchor;
		cd.transform.localPosition = new Vector3(
			layout.multiplier.x * layout.discardPile.x,
			layout.multiplier.y * layout.discardPile.y,
			-layout.discardPile.layerID
		);
		cd.faceUp = true;
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(0);
	}

	void UpdateDrawPile() {
		CardGolf cd;
		for (int i=0; i<drawPile.Count; i++) {
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;
			if (Random.Range(0, 100) < 10) drawPile[i].isGold = true;
			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(
				layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
				layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
				-layout.drawPile.layerID + 0.1f * i
			);
			cd.faceUp = false;
			cd.state = eCardStateGolf.drawpile;
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10 * i);			
		}
	}

	public void CardClicked(CardGolf cd) {
		switch (cd.state) {
			case eCardStateGolf.target:
				break;

			case eCardStateGolf.drawpile:
				MoveToDiscard(target);
				MoveToTarget(Draw());
				UpdateDrawPile();
				ScoreManagerGolf.EVENT(eScoreEventGolf.draw);
				FloatingScoreHandler(eScoreEventGolf.draw);
				break;

			case eCardStateGolf.tableau:
				bool validMatch = true;
				CardGolf tempCardRef = target;
				if (!cd.canClick) {
					validMatch = false;
				}
				if (!AdjacentRank(cd, target)) {
					validMatch = false;
				}
				if (!validMatch) return;

				table.Remove(cd);
				MoveToTarget(cd);
				SetTableauFaces();
				ScoreManagerGolf.EVENT(eScoreEventGolf.putt);
				FloatingScoreHandler(eScoreEventGolf.putt);
				break;
		}
		CheckForGameOver();
	}

	void CheckForGameOver() {
		if (table.Count==0) {
			GameOver (true);
			return;
		}
		if (drawPile.Count>0) {
			return;
		}
		foreach ( CardGolf cd in table) { 
			if (AdjacentRank(cd, target)) {
				return;
			}
		}
		GameOver(false);
	}

	void GameOver(bool won) {
		round += 1;
		int score = ScoreManagerGolf.SCORE;
		if (won) {
			gameOverText.text = "Round Over";
			roundResultText.text = "You won this round! \nRound Score: " + score;
			ShowResultsUI(true);
			ScoreManagerGolf.EVENT(eScoreEventGolf.gameWin);
			FloatingScoreHandler(eScoreEventGolf.gameWin);
		} else {
			gameOverText.text = "Game Over";
			if (round == 9) {
				if (ScoreManagerGolf.HIGH_SCORE >= score) {
					string str = "You got the high score!\nHigh score: " + score;
					roundResultText.text = str;
				}
			} else if (round < 9) {
				roundResultText.text = "Your score for Round " + round + " was: " + score;
			} else {
				roundResultText.text = "Your final score was: " + score;
			}
			ShowResultsUI(true);
			ScoreManagerGolf.EVENT(eScoreEventGolf.gameLoss);
			FloatingScoreHandler(eScoreEventGolf.gameLoss);
		}
		Invoke("ReloadLevel", reloadDelay);
	}

	void ReloadLevel() {
		if (round < 9) {
			SceneManager.LoadScene("__Golf_Scene_1");
		}
		else {
			SceneManager.LoadScene("SceneMain");
		}
	}
	
	public bool AdjacentRank(CardGolf c0, CardGolf c1) {
		if (!c0.canClick || !c1.faceUp) return (false);
		if (Mathf.Abs(c0.rank - c1.rank) == 1) {
			return (true); 
		}
		return (false);
	}

		void FloatingScoreHandler(eScoreEventGolf evt) {
		List<Vector2> fsPts;
		switch (evt) {
			case eScoreEventGolf.gameWin:
			case eScoreEventGolf.gameLoss:
				if (fsRun != null) {
					fsPts = new List<Vector2>();
					fsPts.Add(fsPosRun);
					fsPts.Add(fsPosMid2);
					fsPts.Add(fsPosEnd);
					fsRun.reportFinishTo = ScoreboardGolf.S.gameObject;
					fsRun.Init(fsPts, 0, 1);
					fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
					fsRun = null;
				}
				break;
			case eScoreEventGolf.putt:
			case eScoreEventGolf.draw:
				FloatingScoreGolf fs;
				Vector2 p0 = Input.mousePosition;
				p0.x /= Screen.width;
				p0.y /= Screen.height;
				fsPts = new List<Vector2>();
				fsPts.Add(p0);
				fsPts.Add(fsPosMid);
				fsPts.Add(fsPosRun);
				if (evt == eScoreEventGolf.draw) {
					fs = ScoreboardGolf.S.CreateFloatingScore(1, fsPts);
				}
				else {
					fs = ScoreboardGolf.S.CreateFloatingScore(-1, fsPts);
				}
				fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
				if (fsRun == null) {
					fsRun = fs;
					fsRun.reportFinishTo = null;
				}
				else {
					fs.reportFinishTo = fsRun.gameObject;
				}
				break;
		}
	}
}