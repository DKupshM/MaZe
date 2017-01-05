using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Pathfinding;
using System.Collections;
using System;

public class Menu : MonoBehaviour
{

	public Image usePlayerButton;

	public int width = 101;
	public int height = 101;
	public int ballCount = 2;
	public bool usePlayer = true;

	public string main;
	public string menu;

	void OnAwake ()
	{
		DontDestroyOnLoad (this);
	}

	void Start ()
	{
		SetUsePlayerColor ();
	}

	private void SetUsePlayerColor ()
	{
		if (usePlayer) {
			usePlayerButton.color = Color.green;
		} else {
			usePlayerButton.color = Color.red;
		}
	}

	public void UsePlayer ()
	{
		usePlayer = !usePlayer;
		SetUsePlayerColor ();
	}

	public void SetBallCount (String ballCount)
	{
		int temp = 0;
		try {
			temp = int.Parse (ballCount);
		} catch (Exception e) {
			Debug.Log (ballCount);
			Debug.Log (e);
			return;
		}
		this.ballCount = temp;
	}


	public void SetWidth (String width)
	{
		int temp = 0;
		try {
			temp = int.Parse (width);
		} catch (Exception e) {
			Debug.Log (width);
			Debug.Log (e);
			return;
		}
		// No Even Numbers
		if (temp % 2 == 0)
			temp++;
		this.width = temp;
	}

	public void SetHeight (String height)
	{
		int temp = 0;
		try {
			temp = int.Parse (height);
		} catch (Exception e) {
			Debug.Log (e);
			return;
		}
		// No Even Numbers
		if (temp % 2 == 0)
			temp++;
		this.height = temp;
	}

	public void StartGame ()
	{
		StartCoroutine (SetUpGame ());
	}

	public IEnumerator SetUpGame ()
	{
		AsyncOperation sync = SceneManager.LoadSceneAsync (main, LoadSceneMode.Additive);
		sync.allowSceneActivation = false;
		while (!sync.isDone) {
			if (sync.progress == .9f) {
				sync.allowSceneActivation = true;
				SceneManager.MoveGameObjectToScene (this.gameObject, SceneManager.GetSceneByName (main));
			}
			yield return null;
		}

		UpdateVariables ();
		SceneManager.UnloadScene (menu);
		FinishSetUp ();
		Destroy (this.gameObject);
	}

	private void UpdateVariables ()
	{
		GameManager manager = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ();
		manager.mazeConfig.width = width;
		manager.mazeConfig.height = height;
		manager.ballConfig.ballCount = ballCount;
		manager.ballConfig.usePlayer = usePlayer;
		//manager.SetUpGame ();
	}

	private void FinishSetUp ()
	{
		GameManager manager = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameManager> ();
		//manager.StartGame ();
	}
}