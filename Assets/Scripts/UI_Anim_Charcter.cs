using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_Anim_Charcter : MonoBehaviour {

	public List<Sprite> CharacterSprite;
	public List<Sprite> Sprites;
	public float framesPerSecond;
	SpriteRenderer rende;
	// Use this for initialization
	void Start () {
		rende = gameObject.GetComponent<SpriteRenderer>() as SpriteRenderer;
		Prep_Anim();
	}

	void Prep_Anim(){
		for(int j =0; j<CharacterSprite.Count;j++){
			Sprites.Add(CharacterSprite[j]);
		}
		for(int i =CharacterSprite.Count-2;i>-1;i--){
			Debug.Log("Add un sprite");
			Sprites.Add(CharacterSprite[i]);
		}
	}

	void Animation_Custom(){
		int index = (int)(Time.timeSinceLevelLoad * framesPerSecond);
		index = index % Sprites.Count;
		rende.sprite = Sprites[ index ];
	}
	
	// Update is called once per frame
	void Update () {
		Animation_Custom();
	}
}
