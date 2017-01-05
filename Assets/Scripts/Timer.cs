using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Timer : MonoBehaviour
{

	private Text text;
	private float startTime;


	void Start ()
	{
		text = GetComponent<Text> ();
		startTime = Time.time;	
	}
	
	// Update is called once per frame
	void Update ()
	{
		int currentTime = (int)(Time.time - startTime); 
		if (currentTime < 60) {
			text.text = "" + currentTime;
		} else {
			int minutes = currentTime / 60;
			int seconds = currentTime % 60;
			if (minutes < 60) {
				int hours = minutes / 60;
				minutes %= 60;
				text.text = hours + ":" + FormatTime (minutes) + ":" + FormatTime (seconds);
			}
			text.text = minutes + ":" + FormatTime (seconds); 
		}
	}

	private string FormatTime (int time)
	{
		if (time < 10) {
			return "0" + time;
		} else
			return time + "";
	}
}
