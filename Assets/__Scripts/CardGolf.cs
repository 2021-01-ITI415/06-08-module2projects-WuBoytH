using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardStateGolf {
    drawpile,
    tableau,
    target,
    discard
}

public class CardGolf : Card
{

    [Header("Set Dynamically: CardGolf")]
    public eCardStateGolf state = eCardStateGolf.drawpile;
    public List<CardGolf> hiddenBy = new List<CardGolf>();
    public int layoutID;
    public bool canClick;
    public SlotDefGolf slotDefGolf;

    override public void OnMouseUpAsButton() {
        Golf.S.CardClicked(this);
        base.OnMouseUpAsButton();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
