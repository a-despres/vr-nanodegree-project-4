using UnityEngine;
using System.Collections;

public class GameLogic : MonoBehaviour {

	public GameObject player;
	public GameObject eventSystem;
	public GameObject startUI, restartUI;
	public GameObject startPoint, playPoint, restartPoint;
	public GameObject[] puzzleSpheres;
	public GameObject sparkleEffect;

	public int puzzleLength = 5; // How many times we light up. This is the difficulty factor. The longer it is the more you have to memorize in-game.
	public float puzzleSpeed = 1f; // How many seconds between puzzle display pulses
	private int[] puzzleOrder; // For now let's have 5 orbs

	private int currentDisplayIndex = 0; // Temporary variable for storing the index when displaying the pattern
	public bool currentlyDisplayingPattern = true;
	public bool playerWon = false;

	private int currentSolveIndex = 0; // Temporary variable for storing the index that the player is solving for in the pattern.
	private GameObject sparkle;

	public GameObject failAudioHolder;

	// Use this for initialization
	void Start () {
		puzzleOrder = new int[puzzleLength]; // Se the size of our array to the declared puzzle length.
		generatePuzzleSequence (); // Generate the puzzle sequence for this playthrough.
	}

	// Update is called once per frame
	void Update () {
		
	}

	public void playerSelection (GameObject sphere) {
		if (playerWon != true) { // If the player hasn't won yet
			int selectedIndex = 0;

			// Get the index of the selected object
			for (int i = 0; i < puzzleSpheres.Length; i++) { // Go through the puzzleSpheres array
				if (puzzleSpheres [i] == sphere) { // If the object we have matches this index, we're good
					Debug.Log ("Look's like we hit a sphere: " + i);
					selectedIndex = i;
				}
			}
			solutionCheck (selectedIndex, sphere); // Check if it's correct
		}
	}

	public void solutionCheck (int playerSelectionIndex, GameObject currentSphere) { // We check whether or not the passed index matches the solution index
		if (playerSelectionIndex == puzzleOrder [currentSolveIndex]) { // Check if the index of the object the player passed is the same as the current solve index in our solution array
			currentSolveIndex++;
			Debug.Log("Correct! You've solved " + currentSolveIndex + " out of " + puzzleLength);
			sparkle = Instantiate(sparkleEffect, currentSphere.transform.position, Quaternion.identity, currentSphere.transform);
			Destroy (sparkle, 1.0f);
			if (currentSolveIndex >= puzzleLength) {
				puzzleSuccess ();
			}
		} else {
			puzzleFailure ();
		}
	}	

	public void startPuzzle () { //Begin the puzzle sequence
		// Generate a random number one through five, save it in an array. Do this n times.
		// Step through the array for displaying the puzzle, and checking puzzle failure or succcess.
		startUI.SetActive (false);
		eventSystem.SetActive (false);
		iTween.MoveTo (player,
			iTween.Hash (
				"position", playPoint.transform.position,
				"time", 3,
				"easetype", "linear"
			)
		);
		CancelInvoke ("displayPattern");
		InvokeRepeating ("displayPattern", 3, puzzleSpeed); // Start running through the displaypattern function
		currentSolveIndex = 0; // Set our puzzle index at 0
	}

	void displayPattern () { // Invoked repeating.
		currentlyDisplayingPattern = true; // Let us know we're displaying the pattern.
		eventSystem.SetActive (false); // Disable gaze input events while we are displaying the pattern.

		if (currentlyDisplayingPattern == true) { // If we are not finished displaying the pattern
			if (currentDisplayIndex < puzzleOrder.Length) { // If we haven't reached the end of the puzzle
				Debug.Log (puzzleOrder[currentDisplayIndex] + " at index: " + currentDisplayIndex);
				puzzleSpheres [puzzleOrder [currentDisplayIndex]].GetComponent<lightUp> ().patternLightUp (puzzleSpeed); // Light up the sphere at the proper index. For now we keep it lit up the same amount of time as our interval, but could adjust this to be less.
				currentDisplayIndex++; // Move one further up.
			} else {
				Debug.Log ("End of puzzle display");
				currentlyDisplayingPattern = false; // Let us know we're done displaying the pattern
				currentDisplayIndex = 0;
				CancelInvoke (); // Stop the pattern display. May be better to use coroutines for this but oh well.
				eventSystem.SetActive (true); // Enable gaze input when we aren't displaying the pattern.
			}
		}
	}

	public void generatePuzzleSequence () {
		int tempReference;
		for (int i = 0; i < puzzleLength; i++) { // Do this as many times as necessary for puzzle length
			tempReference = Random.Range(0, puzzleSpheres.Length); // Generate a random reference number for our puzzle spheres
			puzzleOrder [i] = tempReference; // Set the current index to our randomly generated reference number
		}
	}
			
	public void resetPuzzle () { //Reset the puzzle sequence
		print("resetPuzzle called");
		iTween.MoveTo (player,
			iTween.Hash (
				"position", playPoint.transform.position,
				"time", 3,
				"easetype", "linear",
				"oncomplete", "resetGame",
				"oncompletetarget", this.gameObject
			)
		);

		restartUI.SetActive (false);
	}

	public void resetGame () {
		restartUI.SetActive (false);
		startUI.SetActive (true);
		playerWon = false;
		generatePuzzleSequence (); // Generate the puzzle sequence for this playthrough.
		startPuzzle ();
	}

	public void puzzleFailure () { // Do this when the player gets it wrong
		Debug.Log("You've failed, resetting Puzzle");
		failAudioHolder.GetComponent<GvrAudioSource> ().Play ();
		currentSolveIndex = 0;
		startPuzzle ();
	}
		
	public void puzzleSuccess () { //Do this when the player gets it right
		iTween.MoveTo (player, 
			iTween.Hash (
				"position", restartPoint.transform.position, 
				"time", 3, 
				"easetype", "linear",
				"oncomplete", "finishingFlourish",
				"oncompletetarget", this.gameObject
			)
		);
	}

	public void finishingFlourish () { // A nice visual flouish when the player wins
		//this.GetComponent<AudioSource>().Play(); // Play the success audio
		restartUI.SetActive (true);
		playerWon = true;
	}
}
